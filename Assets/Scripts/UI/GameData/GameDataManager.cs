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

    private string infectionScores = "0,";                   //감염률 데이터

    private List<float> infectionRates = new List<float>();          // 감염률 데이터를 저장할 리스트
    private List<float> averagedInfectionRates = new List<float>();  // 감염률 하루 단위로 저장한 리스트 
    
    GameObject scoreGraphCanvas;

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
    }

    // 초기 데이터 삽입 (Flask API로 전송)
    public void InsertInitialData()
    {
        currentDate = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        WWWForm form = new WWWForm();
        form.AddField("user_id", userId);
        form.AddField("user_name", userName);
        form.AddField("infectionScore", "0, ");  // Start with 0 infection rate
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

        while (elapsedTime < totalTime)
        {
            yield return new WaitForSeconds(interval);              // 10초 대기
            //SaveInfectionScoreRoutine();
            float infectionRate = InfectionManager.Instance.GetOverallInfectionRate(Ward.wards);  // 현재 감염률 가져오기
            infectionRates.Add(infectionRate);
            elapsedTime += interval;                                // 경과 시간 업데이트
        }

        CalculateAveragedInfectionRates();
    }

    // 평균 감염률 계산 함수
    public void CalculateAveragedInfectionRates()
    {
        averagedInfectionRates.Clear();

        for (int i = 0; i < infectionRates.Count; i += pointsPerMinute)
        {
            if (i + pointsPerMinute <= infectionRates.Count)
            {
                List<float> chunk = infectionRates.GetRange(i, pointsPerMinute);  // Get the next 6 items
                float average = CalculateAverage(chunk);                          // Calculate the average
                averagedInfectionRates.Add(average);                              // Add the average to the list
            }
        }

        foreach (var averagedInfectionRate in averagedInfectionRates)
        {
            infectionScores += averagedInfectionRate.ToString("F0") + ",";  // 리스트 -> 문자열
        }

        UpdateDatabaseInfectionScore();                         // DB에 업데이트

        // 15분 후 그래프 그리기 시작
        scoreGraphCanvas.SetActive(true);                       //그래프 UI 출력
        GraphSourceChangeInt();                                 // 게임 종료 시 그래프 생성
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