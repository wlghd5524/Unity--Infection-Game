using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance { get; private set; }

    public string userName;
    public string userId;
    public string currentDate;

    private string infectionScores = "";                   //감염률 데이터

    private List<float> infectionRates = new List<float>();          // 감염률 데이터를 저장할 리스트
    private List<float> averagedInfectionRates = new List<float>();  // 감염률 하루 단위로 저장한 리스트 
    
    GameObject scoreGraphCanvas;
    private DateTime gameStartTime;

    private string urlUpdateData = "http://220.69.209.164:3333/update_game_score";
    private int pointsPerMinute = 6;  

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        scoreGraphCanvas = GameObject.Find("ScoreGraphCanvas");
        gameStartTime = DateTime.Now;  // 게임 시작 시간 저장
    }

    // 초기 데이터 삽입 (Flask API로 전송)
    public void InsertInitialData()
    {
        currentDate = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        WWWForm form = new WWWForm();
        form.AddField("user_id", userId);
        form.AddField("user_name", userName);
        form.AddField("infectionScore", " ");  // Start with 0 infection rate
        form.AddField("playingDate", currentDate);

        Debug.Log($"DB: {userId}, {userName}, {infectionScores}, {currentDate}");

        StartCoroutine(PostRequest(urlUpdateData, form));  // Send the initial data to the server
        StartCoroutine(SaveInfectionRateFor15Minutes());
    }

    private IEnumerator SaveInfectionRateFor15Minutes()
    {
        float totalTime = 900f;     // 전체 실행 시간 (초)
        //float totalTime = 30f;
        float interval = 10f;        // 간격 (초)
        float elapsedTime = 0f;      // 경과 시간 (초)
        int dataCount = 0;

        while (elapsedTime < totalTime)
        {
            yield return new WaitForSeconds(interval);              // 10초 대기
            //SaveInfectionScoreRoutine();
            float infectionRate = InfectionManager.Instance.GetOverallInfectionRate(Ward.wards);  // 현재 감염률 가져오기
            infectionRates.Add(infectionRate);
            dataCount++;

            // 6개의 데이터를 모으면 평균 계산하고 DB에 업데이트(=1분치 데이터)
            if (dataCount == pointsPerMinute)
            {
                CalculateAndSaveAverage();
                dataCount = 0;
            }

            elapsedTime += interval;                                // 경과 시간 업데이트
        }

        CalculateAndSaveAverage();          // 마지막 데이터 처리
        Time.timeScale = 0;                 // 게임 일시 정지
        scoreGraphCanvas.SetActive(true);   // 그래프 UI 출력
        GraphSourceChangeInt();             // 게임 종료 시 그래프 생성
    }

    // 평균 계산 및 DB 업데이트 함수
    public void CalculateAndSaveAverage()
    {
        List<float> lastSixData = infectionRates.GetRange(infectionRates.Count - pointsPerMinute, pointsPerMinute);  // 마지막 6개 데이터 가져오기
        float average = CalculateAverage(lastSixData);  // 평균 계산

        // 평균 비교
        float previousAverage = averagedInfectionRates.Count > 0 ? averagedInfectionRates[averagedInfectionRates.Count - 1] : 0f;
        float difference = Mathf.Abs(average - previousAverage);

        if (difference >= 20)  // 차이가 20 이상인 경우
        {
            Debug.Log($"20이상계산하기) 차이가 20 이상인 평균들: 이전 평균 = {previousAverage}, 현재 평균 = {average}");

            // 평균 인덱스를 기준으로 시간 계산
            DateTime startTime = gameStartTime.AddSeconds(averagedInfectionRates.Count * 60);
            DateTime endTime = startTime.AddSeconds(60);

            string formattedStartTime = startTime.ToString("mm:ss");
            string formattedEndTime = endTime.ToString("mm:ss");

            Debug.Log($"20이상계산하기) 시간 범위: {formattedStartTime}~ {formattedEndTime}");

            // ResearchDBManager의 각 연구 기능 리스트에서 버튼 누른 시간 찾기
            FindTimeInResearchRecords(formattedStartTime, formattedEndTime);
        }

        averagedInfectionRates.Add(average);    // 평균 리스트에 추가

        // 감염률을 문자열로 변환하여 DB 업데이트
        infectionScores += average.ToString("F0") + ",";
        UpdateDatabaseInfectionScore();     // DB 업데이트
    }

    // 평균 계산하는 함수
    private float CalculateAverage(List<float> chunk)
    {
        float sum = 0f;
        foreach (float value in chunk)
        {
            sum += value;
        }
        return sum / chunk.Count;
    }

    // ResearchDBManager의 researchRecords 딕셔너리에서 시간 찾기
    private void FindTimeInResearchRecords(string startTime, string endTime)
    {
        ResearchDBManager researchManager = ResearchDBManager.Instance;

        // 모든 모드에 대해 시간 찾기
        foreach (ResearchDBManager.ResearchMode mode in Enum.GetValues(typeof(ResearchDBManager.ResearchMode)))
        {
            List<string> recordList = researchManager.researchRecords[mode];  // 현재 모드의 리스트 가져오기

            for (int i = 0; i < recordList.Count; i++)
            {
                string recordTime = recordList[i];

                // recordTime을 DateTime으로 변환 (mm:ss 형식으로 저장되어 있음)
                //DateTime pressTime = DateTime.ParseExact(recordTime, "mm:ss", null);

                // 시간 범위 내에 있는지 확인
                if (IsTimeInRange(recordTime, startTime, endTime))
                {
                    Debug.Log($"20이상계산하기) {mode} 리스트의 {i}번째 인덱스에서 시간 {recordTime}을 찾았습니다.");
                }
                else
                {
                    Debug.Log($"20이상계산하기) 못 찾음. {recordTime}은 {startTime} ~ {endTime} 사이에 없다.");
                }
            }
        }
    }

    // 주어진 시간 범위에 포함되는지 확인하는 함수
    private bool IsTimeInRange(string recordTime, string startTime, string endTime)
    {
        try
        {
            // 시간을 TimeSpan으로 변환
            TimeSpan recordTimeSpan = TimeSpan.ParseExact(recordTime, "mm\\:ss", null);
            TimeSpan startSpan = TimeSpan.ParseExact(startTime, "mm\\:ss", null);
            TimeSpan endSpan = TimeSpan.ParseExact(endTime, "mm\\:ss", null);

            return (recordTimeSpan >= startSpan && recordTimeSpan <= endSpan);
        }
        catch (FormatException e)
        {
            Debug.LogError($"시간 파싱 오류: {e.Message}, recordTime: {recordTime}");
            return false;
        }
    }

    // DB에 최종 감염 데이터 정보 업그레이드(1일~ 15일 데이터)
    void UpdateDatabaseInfectionScore()
    {
        WWWForm form = new WWWForm();
        form.AddField("user_id", userId);
        form.AddField("user_name", userName);
        form.AddField("infectionScore", infectionScores);
        form.AddField("playingDate", currentDate);

        Debug.Log($"DB: Sending Data: user_id={userId}, user_name={userName}, infectionScore={infectionScores}, playingDate ={currentDate}");

        StartCoroutine(PostRequest(urlUpdateData, form));  // Send the request to update the infection data
    }

    // Send a POST request to the Flask API
    IEnumerator PostRequest(string url, WWWForm form)
    {
        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error sending request: " + www.error);
            }
            else
            {
                Debug.Log("Request successfully sent: " + www.downloadHandler.text);
            }
        }
    }

    // 문장열 형태의 감염 데이터 정수화
    private void GraphSourceChangeInt()
    {
        FindObjectOfType<GraphManager>().DrawGraph(infectionRates);  // Draw the graph with infectionRates
    }
}