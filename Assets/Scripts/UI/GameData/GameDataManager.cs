using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using MyApp.UserManagement;
using Unity.VisualScripting;
using System.Data;
using System.Linq;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance { get; private set; }

    public string userName;
    public string userId;
    public string currentDate;

    public Toggle feedbackTotalToggle, feedbackDoctorToggle, feedbackNurseToggle, feedbackInpatientToggle, feedbackOutpatientToggle, feedbackEmergencyToggle, feedbackIcuToggle;

    private string doctorScore = "";    //감염률 데이터
    private string nurseScore = "";
    private string inpatientsScore = "";
    private string outpatientsScore = "";
    private string emergencyPatientsScore = "";
    private string icuPatientsScore = "";
    private string totalScores = "";

    public List<float> infectionRates = new List<float>();          // 10초 단위 감염률
    Dictionary<string, float> infectionRoleDictionary = new Dictionary<string, float>();
    public List<float> doctorInfectionRates = new List<float>();
    public List<float> nurseInfectionRates = new List<float>();
    public List<float> inpatientsRates = new List<float>();
    public List<float> outpatientsRates = new List<float>();
    public List<float> emergencyPatientsRates = new List<float>();
    public List<float> icuPatientsRates = new List<float>();

    private List<float> averagedInfectionRates = new List<float>();  // 하루 단위 감염률
    private List<float> avgDoctorRate = new List<float>();
    private List<float> avgNurseRate = new List<float>();
    private List<float> avgInpatientsRate = new List<float>();
    private List<float> avgOutpatientsRate = new List<float>();
    private List<float> avgEmergencyPatientsRate = new List<float>();
    private List<float> avgIcuPatientsRate = new List<float>();

    public bool[] difference20More = new bool[15];     // 차이가 20 이상인 인덱스 저장 => 피드백 문자열에 추가
    public Dictionary<int, string> feedbackContent = new Dictionary<int, string>();

    GameObject scoreGraphCanvas;
    private DateTime gameStartTime;
    public Transform graphContainer;
    public Transform feedbackContainer;
    public GameObject gameClearPanel;
    public Button gameClearNextButton;
    public TextMeshProUGUI selectedLevel;
    List<string> steps = new List<string>();
    TextMeshProUGUI feedbackText;

    private string urlUpdateData = "http://220.69.209.164:3333/update_game_score";
    private int pointsPerMinute = 6;        // 1분 동안의 감염률 평균 내기(10초*6)
    public string originalContent;

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
        graphContainer = GameObject.Find("graphContainer").transform;
        feedbackContainer = GameObject.Find("FeedgraphContainer").transform;
        feedbackTotalToggle = GameObject.Find("FeedbackTotalToggle").GetComponent<Toggle>();
        feedbackDoctorToggle = GameObject.Find("FeedbackDoctorlToggle").GetComponent<Toggle>();
        feedbackNurseToggle = GameObject.Find("FeedbackNurseToggle").GetComponent<Toggle>();
        feedbackInpatientToggle = GameObject.Find("FeedbackInpatientToggle").GetComponent<Toggle>();
        feedbackOutpatientToggle = GameObject.Find("FeedbackOutpatientToggle").GetComponent<Toggle>();
        feedbackEmergencyToggle = GameObject.Find("FeedbackEmergencyToggle").GetComponent<Toggle>();
        feedbackIcuToggle = GameObject.Find("FeedbackIcuToggle").GetComponent<Toggle>();
        gameClearPanel = GameObject.Find("GameClearPanel");
        gameClearNextButton = GameObject.Find("GameClearNextButton").GetComponent<Button>();
        selectedLevel = GameObject.Find("SelectedLevel").GetComponent<TextMeshProUGUI>();
        feedbackText = GameObject.Find("FeedbackText").GetComponent<TextMeshProUGUI>();

        for (int i = 0; i < 15; i++)
            difference20More[i] = false;

        // 각 토글에 이벤트 리스너 추가
        feedbackTotalToggle.onValueChanged.AddListener(isOn => ToggleFeedbackVisibility("total", isOn));
        feedbackDoctorToggle.onValueChanged.AddListener(isOn => ToggleFeedbackVisibility("doctor", isOn));
        feedbackNurseToggle.onValueChanged.AddListener(isOn => ToggleFeedbackVisibility("nurse", isOn));
        feedbackInpatientToggle.onValueChanged.AddListener(isOn => ToggleFeedbackVisibility("inpatients", isOn));
        feedbackOutpatientToggle.onValueChanged.AddListener(isOn => ToggleFeedbackVisibility("outpatients", isOn));
        feedbackEmergencyToggle.onValueChanged.AddListener(isOn => ToggleFeedbackVisibility("emergencyPatients", isOn));
        feedbackIcuToggle.onValueChanged.AddListener(isOn => ToggleFeedbackVisibility("icuPatients", isOn));

        //게임 클리어창 설정
        gameClearPanel.SetActive(false);
        gameClearNextButton.onClick.AddListener(() =>
        {
            gameClearPanel.SetActive(false);
            GraphSourceChangeInt();             // 그래프 생성
        });
    }

    // 초기 데이터 삽입 (Flask API로 전송)
    public void InsertInitialData()
    {
        currentDate = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        WWWForm form = new WWWForm();
        form.AddField("user_id", userId);
        form.AddField("user_name", userName);
        form.AddField("doctorRate", " ");
        form.AddField("nurseRate", " ");
        form.AddField("inpatientsRate", " ");
        form.AddField("outpatientsRate", " ");
        form.AddField("emergencyPatientsRate", " ");
        form.AddField("icuPatientsRate", " ");
        form.AddField("totalRate", " ");
        form.AddField("playingDate", currentDate);

        StartCoroutine(PostRequest(urlUpdateData, form));
        StartCoroutine(SaveInfectionRateFor15Minutes());
    }

    // 15분 동안 10초 간격으로 감염률 리스트에 저장
    private IEnumerator SaveInfectionRateFor15Minutes()
    {
        float totalTime = 900f;      // 전체 실행 시간 (초)
        float interval = 10f;        // 간격 (초)
        float elapsedTime = 0f;      // 경과 시간 (초)
        int dataCount = 0;           // 현재까지 모은 데이터 개수  

        while (elapsedTime < totalTime)
        {
            yield return YieldInstructionCache.WaitForSeconds(interval); 

            float infectionRate = InfectionManager.Instance.GetOverallInfectionRate(Ward.wards);  // 현재 감염률 가져오기
            infectionRates.Add(infectionRate);

            // 감염률이 80%를 초과할 시 게임 오버
            if (infectionRate > 80)
            {
                Time.timeScale = 0;
                scoreGraphCanvas.SetActive(true);
                GraphSourceChangeInt();
                OneClearManager.Instance.CloseDisinfectionMode();
                SavePassStage("np");
                yield break;
            }

            // 역할별 감염률 리스트 저장
            infectionRoleDictionary = InfectionManager.Instance.GetInfectionRateByRole();
            doctorInfectionRates.Add(infectionRoleDictionary["doctor"] * 100);
            nurseInfectionRates.Add(infectionRoleDictionary["nurse"] * 100);
            inpatientsRates.Add(infectionRoleDictionary["inpatients"] * 100);
            outpatientsRates.Add(infectionRoleDictionary["outpatients"] * 100);
            icuPatientsRates.Add(infectionRoleDictionary["icuPatients"] * 100);
            emergencyPatientsRates.Add(infectionRoleDictionary["emergencyPatients"] * 100);

            dataCount++;

            // 6개의 데이터를 모으면 평균 계산하고 DB에 업데이트
            if (dataCount == pointsPerMinute)
            {
                CalculateAndSaveAverage(infectionRates, "totalScores", averagedInfectionRates);
                CalculateAndSaveAverage(doctorInfectionRates, "doctorScore", avgDoctorRate);
                CalculateAndSaveAverage(nurseInfectionRates, "nurseScore", avgNurseRate);
                CalculateAndSaveAverage(inpatientsRates, "inpatientsScore", avgInpatientsRate);
                CalculateAndSaveAverage(outpatientsRates, "outpatientsScore", avgOutpatientsRate);
                CalculateAndSaveAverage(icuPatientsRates, "icuPatientsScore", avgIcuPatientsRate);
                CalculateAndSaveAverage(emergencyPatientsRates, "emergencyPatientsScore", avgEmergencyPatientsRate);

                dataCount = 0; // 데이터 수 초기화
            }

            elapsedTime += interval;   
        }

        // 게임 클리어 시
        Time.timeScale = 0;                 
        scoreGraphCanvas.SetActive(true);
        gameClearPanel.SetActive(true);
        SavePassStage("p");
        yield break;
    }

    // 스테이지 클리어 기록 DB에 저장
    void SavePassStage(string pnp)
    {
        string id = AuthManager.Instance.id;
        string name = UserManager.Instance.GetNameById(id);
        List<string> steps = UserManager.Instance.GetUserStep(id);
        string clearLevel = $"{pnp}: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}";

        string updatedEasySteps = selectedLevel.text == "Easy" ? $"{steps[0]}, {clearLevel}" : steps[0];
        string updatedNormalSteps = selectedLevel.text == "Normal" ? $"{steps[1]}, {clearLevel}" : steps[1];
        string updatedHardSteps = selectedLevel.text == "Hard" ? $"{steps[2]}, {clearLevel}" : steps[2];

        UserManager.Instance.AddUser(id, name, AuthManager.Instance.password, 1, updatedEasySteps, updatedNormalSteps, updatedHardSteps);
    }

    // 평균 계산 및 DB 업데이트 함수
    private void CalculateAndSaveAverage(List<float> ratesList, string scoreType, List<float> avgList)
    {
        if (ratesList.Count < pointsPerMinute) return;

        List<float> lastSixData = ratesList.GetRange(ratesList.Count - pointsPerMinute, pointsPerMinute);
        float average = CalculateAverage(lastSixData);
        avgList.Add(average);

        // 차이가 20 이상인 경우
        float previousAverage = avgList.Count > 0 ? avgList[avgList.Count - 1] : 0f;
        float difference = Mathf.Abs(average - previousAverage);
        if (difference >= 20)
        {
            difference20More[avgList.Count] = true;
        }

        if (scoreType == "totalScores")
        {
            FindTimeInResearchRecords(avgList.Count);
        }

        switch (scoreType)
        {
            case "totalScores":
                totalScores += average.ToString("F0") + ",";
                break;
            case "doctorScore":
                doctorScore += average.ToString("F0") + ",";
                break;
            case "nurseScore":
                nurseScore += average.ToString("F0") + ",";
                break;
            case "inpatientsScore":
                inpatientsScore += average.ToString("F0") + ",";
                break;
            case "outpatientsScore":
                outpatientsScore += average.ToString("F0") + ",";
                break;
            case "emergencyPatientsScore":
                emergencyPatientsScore += average.ToString("F0") + ",";
                break;
            case "icuPatientsScore":
                icuPatientsScore += average.ToString("F0") + ",";
                break;
        }

        UpdateDatabaseInfectionScore();     // DB 업데이트
    }

    // 평균 계산하는 함수
    private float CalculateAverage(List<float> chunk)
    {
        float sum = 0f;
        foreach (float value in chunk) sum += value;
        return sum / chunk.Count;
    }

    // 해당 구간 동안 진행된 연구 데이터 출력하기 (index: 1~15)
    private void FindTimeInResearchRecords(int index)
    {
        ResearchDBManager researchManager = ResearchDBManager.Instance;

        // 이전 로그를 기반으로 현재 로그 초기화
        if (index > 1 && feedbackContent.ContainsKey(index - 2))
        {
            feedbackContent[index - 1] = feedbackContent[index - 2];
            RemoveFeedbackByKeyword(index);     // 필요없는 문장 제거
        }
        else
            feedbackContent[index - 1] = " "; // 첫날이거나 이전 로그가 없는 경우


        foreach (ResearchDBManager.ResearchMode mode in Enum.GetValues(typeof(ResearchDBManager.ResearchMode)))
        {
            List<string> recordList = researchManager.researchRecords[mode]
                .Where(record => int.Parse(record.Split('.')[0]) == index) // 날짜 필터링
                .ToList();

            for (int i = 0; i < recordList.Count; i++)
            {
                string[] parts = recordList[i].Split('.');  // 데이터 분할 (day.btnNum.targetNum.toggleIsOn.currentMoment)
                int day = int.Parse(parts[0]);              // day 값 추출
                int btnNum = int.Parse(parts[1]);           // 연구 항목 번호
                int targetNum = int.Parse(parts[2]);        // 타겟 번호
                int toggleState = int.Parse(parts[3]);      // 상태 값 (1 또는 0)
                string currentMoment = parts[4];            // 시간 값 (mm:ss 형식)
                string feedback = "";

                // 비활성화된 연구버튼에 대한 로그는 피드백에서 삭제
                if (toggleState == 0)
                {
                    RemoveFeedback(index, btnNum, targetNum, toggleState, mode);
                    continue;
                }

                // 피드백 생성
                switch (mode)
                {
                    case ResearchDBManager.ResearchMode.gear:
                        feedback = GetGearResearchFeedback(btnNum, targetNum, toggleState);
                        break;
                    case ResearchDBManager.ResearchMode.patient:
                        feedback = GetPatientResearchFeedback(btnNum, targetNum, toggleState);
                        break;
                    case ResearchDBManager.ResearchMode.research:
                        feedback = GetAdvancedResearchFeedback(btnNum, targetNum, toggleState);
                        break;
                }

                if (!feedbackContent[index - 1].Contains(feedback))
                {
                    feedbackContent[index - 1] += $"{feedback} ({currentMoment})\n";
                }
            }
        }
    }

    void RemoveFeedback(int index, int btnNum, int targetNum, int toggleState, ResearchDBManager.ResearchMode mode)
    {
        if (!feedbackContent.ContainsKey(index - 1)) return;

        string[] lines = feedbackContent[index - 1].Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        string updatedContent = "";

        foreach (var line in lines)
        {
            if (mode == ResearchDBManager.ResearchMode.gear)
            {
                if (line.Contains($"{GetGearResearchFeedback(btnNum, targetNum, 1)}"))
                    continue;
            }
            else if (mode == ResearchDBManager.ResearchMode.patient)
            {
                if (line.Contains($"{GetPatientResearchFeedback(btnNum, targetNum, 1)}"))
                    continue;
            }

            updatedContent += line + "\n";
        }

        feedbackContent[index - 1] = updatedContent;
    }

    void RemoveFeedbackByKeyword(int index)
    {
        if (!feedbackContent.ContainsKey(index - 1)) return;

        string[] lines = feedbackContent[index - 1].Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        string updatedContent = "";

        foreach (var line in lines)
        {
            if (line.Contains("소독") || line.Contains("연구") || line.Contains("백신") || line.Contains("치료제"))
                continue;

            updatedContent += line + "\n";
        }

        feedbackContent[index - 1] = updatedContent;
    }

    // Gear Research 모드의 피드백 생성
    private string GetGearResearchFeedback(int btnNum, int target, int toggleState)
    {
        string[] gearItems = { "Dental 마스크", "일회용 장갑", "N95 마스크", "라텍스 장갑", "의료용 고글", "의료용 헤어캡", "AP 가운", "Level C" };
        string[] gearTarget = { "의사", "간호사", "외래 환자", "입원 환자", "응급 환자" };

        if (btnNum >= 1 && btnNum <= 8 && target >= 1 && target <= 5)
        {
            string itemName = gearItems[btnNum - 1];
            string targetName = gearTarget[target - 1];
            return $"{itemName} {(toggleState == 1 ? "사용" : "미사용")}({targetName})";
        }

        return "";
    }

    // Patient Research 모드의 피드백 생성
    private string GetPatientResearchFeedback(int btnNum, int target, int toggleState)
    {
        string[] patientItems = { "폐쇄", "소독" };
        string[] patientTarget = { "내과1", "내과2", "외과1", "외과2", "입원병동1", "입원병동2", "입원병동3", "입원병동4" };

        if (btnNum >= 1 && btnNum <= 2)
        {
            string itemName = patientItems[btnNum - 1];
            string tartgetName = patientTarget[target - 1];
            return $"{itemName} {(toggleState == 1 ? "진행" : "미진행")}({tartgetName})";
        }
        return "";
    }

    // Advanced Research 모드의 피드백 생성
    private string GetAdvancedResearchFeedback(int btnNum, int target, int toggleState)
    {
        string[] advancedItems = { "연구", "백신", "치료제" };
        string[] advancedTarget = { "내과1", "내과2", "외과1", "외과2", "입원병동1", "입원병동2", "입원병동3", "입원병동4", "응급실", "중환자실" };

        if (btnNum >= 1 && btnNum <= advancedItems.Length)
        {
            string itemName = advancedItems[btnNum - 1];

            if (target >= 1 && target <= advancedTarget.Length)
            {
                string tartgetName = advancedTarget[target - 1];

                if (btnNum == 1)
                {
                    return $"{itemName} 진행({tartgetName})";
                }
                else
                {
                    return $"{itemName} {toggleState}개 사용";
                }
            }
            else
            {
                Debug.LogWarning($"GetAdvancedResearchFeedback: target 값이 유효하지 않습니다 (target: {target})");
            }
        }
        return "";
    }

    // 토글이 true인 직군의 피드백만 feedbackText에 출력
    public void ToggleFeedbackVisibility(string role, bool isVisible)
    {
        if (feedbackText == null)
            feedbackText = GameObject.Find("FeedbackText").GetComponent<TextMeshProUGUI>();

        UpdateTotalToggle(role);    // 하위 토글 상태에 따른 전체토글 값 관리

        // 전체 토글이 켜져 있으면 모든 내용을 표시
        if (feedbackTotalToggle.isOn)
        {
            feedbackText.text = originalContent;
            return;
        }

        string filteredContent = "";
        string[] dayBlocks = originalContent.Split(new[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < dayBlocks.Length; i++)
        {
            string[] lines = dayBlocks[i].Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            if (difference20More[i])
            {
                filteredContent += $"{i + 1}DAY - 감염률 급상승!\n";
            }
            else
            {
                filteredContent += $"{i + 1}DAY\n";
            }

            foreach (string line in lines)
            {
                if (line.StartsWith($"{i + 1}DAY")) continue;

                if (feedbackDoctorToggle.isOn && line.Contains("의사"))
                    filteredContent += $"{line}\n";
                if (feedbackNurseToggle.isOn && line.Contains("간호사"))
                    filteredContent += $"{line}\n";
                if (feedbackInpatientToggle.isOn && (line.Contains("입원 환자") || line.Contains("입원병동")))
                    filteredContent += $"{line}\n";
                if (feedbackOutpatientToggle.isOn && (line.Contains("외래 환자") || line.Contains("내과") || line.Contains("외과")))
                    filteredContent += $"{line}\n";
                if (feedbackEmergencyToggle.isOn && (line.Contains("응급 환자") || line.Contains("응급실")))
                    filteredContent += $"{line}\n";
                if (feedbackIcuToggle.isOn && line.Contains("중환자실"))
                    filteredContent += $"{line}\n";
            }

            filteredContent += "\n";
        }

        feedbackText.text = filteredContent.Trim();
    }

    // 모든 토글의 상태가 변경될 때 전체 토글의 상태를 업데이트
    public void UpdateTotalToggle(string role)
    {
        if (role == "total")
        {
            bool onTotalToggle = feedbackTotalToggle.isOn;

            feedbackDoctorToggle.SetIsOnWithoutNotify(onTotalToggle);
            feedbackNurseToggle.SetIsOnWithoutNotify(onTotalToggle);
            feedbackInpatientToggle.SetIsOnWithoutNotify(onTotalToggle);
            feedbackOutpatientToggle.SetIsOnWithoutNotify(onTotalToggle);
            feedbackEmergencyToggle.SetIsOnWithoutNotify(onTotalToggle);
            feedbackIcuToggle.SetIsOnWithoutNotify(onTotalToggle);
        }
        else
        {
            bool allOn = feedbackDoctorToggle.isOn &&
                        feedbackNurseToggle.isOn &&
                        feedbackInpatientToggle.isOn &&
                        feedbackOutpatientToggle.isOn &&
                        feedbackEmergencyToggle.isOn &&
                        feedbackIcuToggle.isOn;

            feedbackTotalToggle.SetIsOnWithoutNotify(allOn);
        }
    }

    // DB에 최종 감염 데이터 정보 업그레이드(1일~ 15일 데이터)
    void UpdateDatabaseInfectionScore()
    {
        WWWForm form = new WWWForm();
        form.AddField("user_id", userId);
        form.AddField("user_name", userName);
        form.AddField("doctorRate", string.IsNullOrEmpty(doctorScore) ? "0" : doctorScore);
        form.AddField("nurseRate", string.IsNullOrEmpty(nurseScore) ? "0" : nurseScore);
        form.AddField("inpatientsRate", string.IsNullOrEmpty(inpatientsScore) ? "0" : inpatientsScore);
        form.AddField("outpatientsRate", string.IsNullOrEmpty(outpatientsScore) ? "0" : outpatientsScore);
        form.AddField("emergencyPatientsRate", string.IsNullOrEmpty(emergencyPatientsScore) ? "0" : emergencyPatientsScore);
        form.AddField("icuPatientsRate", string.IsNullOrEmpty(icuPatientsScore) ? "0" : icuPatientsScore);
        form.AddField("totalRate", string.IsNullOrEmpty(totalScores) ? "0" : totalScores);
        form.AddField("playingDate", currentDate);

        //Debug.Log($"DB: Sending Data: user_id={userId}, user_name={userName}, infectionScore={infectionScores}, playingDate ={currentDate}");
        StartCoroutine(PostRequest(urlUpdateData, form));
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
        }
    }

    // 문장열 형태의 감염 데이터 정수화
    private void GraphSourceChangeInt()
    {
        FindObjectOfType<GraphManager>().DrawGraph(infectionRates, "total", graphContainer);
        FindObjectOfType<GraphManager>().DrawGraph(doctorInfectionRates, "doctor", graphContainer);
        FindObjectOfType<GraphManager>().DrawGraph(nurseInfectionRates, "nurse", graphContainer);
        FindObjectOfType<GraphManager>().DrawGraph(inpatientsRates, "inpatients", graphContainer);
        FindObjectOfType<GraphManager>().DrawGraph(outpatientsRates, "outpatients", graphContainer);
        FindObjectOfType<GraphManager>().DrawGraph(emergencyPatientsRates, "emergencyPatients", graphContainer);
        FindObjectOfType<GraphManager>().DrawGraph(icuPatientsRates, "icuPatients", graphContainer);
    }
}