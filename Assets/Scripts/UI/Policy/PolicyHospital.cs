using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PolicyHospital : MonoBehaviour
{
    public Button[] closingButton = new Button[8];
    public Button[] disinfectionButton = new Button[8];
    public Button updateButton;

    public TextMeshProUGUI[] disinfectionText = new TextMeshProUGUI[8];
    public TextMeshProUGUI[] closingText = new TextMeshProUGUI[8];

    public Image[] closingOutline = new Image[8];
    public Image[] disinfectionOutline = new Image[8];

    public MonthlyReportUI monthlyReportUI;
    public CurrentMoney currentMoneyManager;
    public TMP_Text moneyText;

    public GameObject questDisfectCanvas;
    public TextMeshProUGUI disinfectQuest;
    public Button[] disinfectAnswers;
    public GameObject disWrongPanel;
    public GameObject disCorrectPanel;
    public Button disinfectXButton;

    Ward ward;
    ResearchDBManager researchDBManager;
    string[] wards = new string[] { "내과 1", "내과 2", "외과 1", "외과 2", "입원병동1", "입원병동2", "입원병동3", "입원병동4" };
    bool[] isClosed = new bool[8];
    bool[] isDisinfected = new bool[8];
    int nowIndex;

    // 퀴즈 질문
    string[] questions = {
        "바이러스와 세균을 효과적으로 제거하기 위해 가장 많이 사용되는 소독제는 무엇인가요?",
        "손 소독에 적합한 알코올의 농도는 얼마인가요?",
        "세균 소독 시 가장 효과적인 소독 시간과 방법은 무엇인가요?",
        "소독제를 사용할 때 가장 주의해야 할 사항은 무엇인가요?",
        "세균성 감염을 예방하기 위해 자주 소독해야 하는 의료기구는 무엇인가요?",
        "바이러스를 사멸시키기 위해 손 소독제의 주요 활성 성분으로 가장 적합한 것은 무엇인가요?",
        "의료 시설에서 사용하는 소독제로 가장 흔히 사용되는 화학 물질은 무엇인가요?",
        "알코올 소독제가 바이러스와 세균을 죽이는 원리는 무엇인가요?",
        "의료기구 소독 시 가장 흔히 사용하는 열처리 방법은 무엇인가요?",
        "의료 환경에서 표면 소독 시 효과적인 방법은 무엇인가요?"
    };

    // 각 질문에 대한 선택지
    string[,] choices = {
        { "알코올 70%", "식염수", "증류수", "글리세린" },
        { "30%", "50%", "70%", "100%" },
        { "10초 동안 소독제 뿌리기", "30초 동안 문지르기", "1분 동안 자연 건조", "5분 동안 젖은 상태 유지" },
        { "희석하지 않고 사용하기", "환기가 잘 되는 곳에서 사용하기", "다른 소독제와 혼합하여 사용하기", "소독제를 마시지 않도록 주의하기" },
        { "청진기", "주사기", "혈압계", "전자 체온계" },
        { "벤잘코늄 클로라이드", "차아염소산 나트륨", "에탄올", "과산화수소" },
        { "염산", "차아염소산 나트륨", "설탕", "바세린" },
        { "세포벽을 파괴한다", "세포 내부로 침투하여 DNA를 변형시킨다", "세포의 수분을 증발시킨다", "세포막의 단백질을 변성시킨다" },
        { "자외선 소독", "고압 증기 멸균 (오토클레이브)", "알코올 침지", "냉동 처리" },
        { "물로만 닦기", "마른 천으로 닦기", "소독제 뿌린 후 문지르기", "자연건조 후 소독제 닦아내기" }
    };

    // 정답 배열 
    int[] correctAnswers = { 0, 2, 1, 1, 0, 2, 1, 3, 1, 2 };

    void Start()
    {
        ward = FindObjectOfType<Ward>();
        researchDBManager = FindObjectOfType<ResearchDBManager>();
        updateButton = GameObject.Find("UpdateButton").GetComponent<Button>();
        monthlyReportUI = FindObjectOfType<MonthlyReportUI>();
        currentMoneyManager = FindObjectOfType<CurrentMoney>();
        moneyText = GameObject.Find("MoneyText").GetComponent<TextMeshProUGUI>();
        questDisfectCanvas = GameObject.Find("QuestDisinfectCanvas");
        disinfectQuest = GameObject.Find("DisinfectQuest").GetComponent<TextMeshProUGUI>();
        disWrongPanel = GameObject.Find("DisWrongPanel");
        disCorrectPanel = GameObject.Find("DisCorrectPanel");
        disinfectXButton = GameObject.Find("DisinfectXButton").GetComponent<Button>();

        disWrongPanel.SetActive(false);
        disCorrectPanel.SetActive(false);

        for (int i = 0; i < closingButton.Length; i++)
        {
            int index = i;

            //자동할당
            closingButton[index] = GameObject.Find($"ClosingButton{index}").GetComponent<Button>();
            disinfectionButton[index] = GameObject.Find($"DisinfectionButton{index}").GetComponent<Button>();
            closingText[index] = GameObject.Find($"ClosingText{index}").GetComponent<TextMeshProUGUI>();
            disinfectionText[index] = GameObject.Find($"DisinfectionText{index}").GetComponent<TextMeshProUGUI>();
            closingOutline[index] = GameObject.Find($"ClosingOutline{index}").GetComponent<Image>();
            disinfectionOutline[index] = GameObject.Find($"DisinfectionOutline{index}").GetComponent<Image>();

            //초기화
            disinfectionButton[index].interactable = false;
            isClosed[index] = false;       // 모든 병동을 열림 상태로 저장
            isDisinfected[index] = false;  // 모든 병동을 소독 안 한 상태로 저장

            closingButton[index].onClick.RemoveAllListeners(); 
            disinfectionButton[index].onClick.RemoveAllListeners();

            // 폐쇄 버튼 클릭 시 처리
            closingButton[index].onClick.AddListener(() =>
            {
                BtnSoundManager.Instance.PlayButtonSound();
                if (!isClosed[index])
                {
                    Ward.wards[index].CloseWard();
                }
                else
                {
                    Ward.wards[index].OpenWard();
                }
                ToggleColsing(index);
            });

            // 소독 버튼 클릭 시 처리
            disinfectionButton[index].onClick.AddListener(() =>
            {
                ToggleDisinfection(index);
                nowIndex = index;
                BtnSoundManager.Instance.PlayButtonSound();
            });
        }

        //선택지 버튼(DisinfectAnswerButton1~ DisinfectAnswerButton4)
        disinfectAnswers = Enumerable.Range(1, 4)
            .Select(i => GameObject.Find($"DisinfectAnswerButton{i}").GetComponent<Button>())
            .ToArray();

        disinfectXButton.onClick.AddListener(() => { questDisfectCanvas.SetActive(false); BtnSoundManager.Instance.PlayButtonSound(); });

        updateButton.onClick.AddListener(() => { UpdateWardCountsPeriodically(); BtnSoundManager.Instance.PlayButtonSound(); });
        UpdateWardCountsPeriodically();
    }

    // 폐쇄 버튼 클릭 시 처리(true가 폐쇄)
    void ToggleColsing(int index)
    {
        isClosed[index] = !isClosed[index];
        closingOutline[index].color = isClosed[index] ? HexColor("#DC0004") : HexColor("#CED4DA");       // 폐쇄 시 빨간 테두리 이미지 
        UpdateWardCounts();

        // 소독 진행 중에 폐쇄를 끄면 소독도 중지
        if (!isClosed[index] && isDisinfected[index])
        {
            isDisinfected[index] = false;
        }
        disinfectionButton[index].interactable = isClosed[index]; 
        disinfectionText[index].text = isClosed[index] ? "소독 가능" : "";

        PrintButtonState(1, index, isClosed[index]);             // DB에 폐쇄 상태 저장
    }

    // 소독 버튼 클릭 시 소독 상태 업데이트
    void ToggleDisinfection(int index)
    {
        var wardCounts = GetStaffAndOutpatientCounts();

        if (isClosed[index] && !isDisinfected[index])
        {
            // 병동의 인원 수 확인
            var wardInfo = wardCounts[wards[index]];
            if (wardInfo.doctorCount == 0 && wardInfo.nurseCount == 0 && wardInfo.outpatientCount == 0 && wardInfo.inpatientCount == 0)
            {
                OpenDisinfectQuiz();   
            }
            else{
                disinfectionText[index].text = "소독 불가: 병동에 인원이 있습니다.";
            } 
        }
    }

    public void StartWardDisinfection(int index)
    {
        // 소독 중일 때 비활성화하여 추가 클릭을 방지
        disinfectionButton[index].interactable = false;
        disinfectionOutline[index].color = HexColor("#00FF37");    // 소독 시 초록색 테두리
        disinfectionText[index].text = "소독 중...";
        isDisinfected[index] = true;

        // 소독 타이머 시작
        StartCoroutine(DisinfectionTimer(index, "소독 완료")); 
    }

    public IEnumerator DisinfectionTimer(int index, string state)
    {
        if (isDisinfected[index])
        {
            float elapsedTime = 0f;
            float disinfectionTime = 30f; // 소독 시간 30초

            while (elapsedTime < disinfectionTime)
            {
                // 소독 중에 폐쇄 모드 중지 시
                if (!isClosed[index])
                {
                    disinfectionOutline[index].color = HexColor("#CED4DA");
                    yield break;
                }

                elapsedTime += Time.unscaledDeltaTime;
                float remainingTime = Mathf.Ceil(disinfectionTime - elapsedTime);
                disinfectionText[index].text = $"소독 중\n{remainingTime}초";
                yield return null;
            }

            // 소독 완료 처리
            disinfectionOutline[index].color = HexColor("#CED4DA");
            PrintButtonState(2, index, true); // 소독 완료 상태를 DB에 저장
            disinfectionButton[index].interactable = false;
            monthlyReportUI.AddExpenseDetail("소독", 500);
            currentMoneyManager.CurrentMoneyGetter -= 300;
            moneyText.text = $"{currentMoneyManager.CurrentMoneyGetter:N0}Sch";
        }

        // 소독 기능 재사용까지 30초 텀 두기
        float cooldownTime = 30f;
        float cooldownElapsed = 0f;
        while (cooldownElapsed < cooldownTime)
        {
            cooldownElapsed += Time.unscaledDeltaTime;
            float remainingCooldown = Mathf.Ceil(cooldownTime - cooldownElapsed);
            disinfectionText[index].text = $"{state}\n{remainingCooldown}초";

            yield return null;
        }

        isDisinfected[index] = false;
        disinfectionButton[index].interactable = isClosed[index];
        disinfectionText[index].text = isClosed[index] ? "소독 가능" : "";
    }

    //DB 데이터 만들기
    void PrintButtonState(int toggleType, int wardIndex, bool isOn)
    {
        int toggleState = isOn ? 1 : 0;
        int wardNumber = wardIndex + 1; // 병동 번호 1부터 시작
        researchDBManager.AddResearchData(ResearchDBManager.ResearchMode.patient, toggleType, wardNumber, toggleState);
    }

    // 병동별 의사, 간호사, 외래환자 수 1초마다 업데이트
    void UpdateWardCountsPeriodically()
    {
        for (int i = 0; i < closingButton.Length; i++)
        {
            UpdateWardCounts();
        }
    }

    // 토글 상태별 데이터 출력
    void UpdateWardCounts()
    {
        var wardCounts = GetStaffAndOutpatientCounts();     

        for (int i = 0; i < closingButton.Length; i++)
        {
            var wardInfo = wardCounts[wards[i]];
            if (i >= 0 && i <= 3)
            {
                if (isClosed[i] && wardInfo.doctorCount == 0 && wardInfo.nurseCount == 0 && wardInfo.outpatientCount == 0 && wardInfo.inpatientCount == 0)
                    closingText[i].text = "의사 x0\n간호사 x0\n외래환자 x0";
                else if (wardCounts.ContainsKey(wards[i]))
                    closingText[i].text = $"의사 x{wardInfo.doctorCount}\n간호사 x{wardInfo.nurseCount}\n외래환자 x{wardInfo.outpatientCount}";
            }
            else  //내원병동인 경우
            {
                if (isClosed[i] && wardInfo.doctorCount == 0 && wardInfo.nurseCount == 0 && wardInfo.outpatientCount == 0 && wardInfo.inpatientCount == 0)
                    closingText[i].text = "의사 x0\n간호사 x0\n내원환자 x0";
                else if (wardCounts.ContainsKey(wards[i]))
                    closingText[i].text = $"의사 x{wardInfo.doctorCount}\n간호사 x{wardInfo.nurseCount}\n내원환자 x{wardInfo.inpatientCount}";
            }
        }
    }

    // 병동별 의사, 간호사, 외래환자 데이터 수집
    public Dictionary<string, (int doctorCount, int nurseCount, int outpatientCount, int inpatientCount)> GetStaffAndOutpatientCounts()
    {
        Dictionary<string, (int doctorCount, int nurseCount, int outpatientCount, int inpatientCount)> wardCounts = new Dictionary<string, (int, int, int, int)>();

        foreach (Ward ward in Ward.wards)
        {
            if (ward.num >= 0 && ward.num <= 7)
            {
                int doctorCount = ward.doctors.Count;
                int nurseCount = ward.nurses.Count;
                int outpatientCount = ward.outpatients.Count;
                int inpatientCount = ward.inpatients.Count;

                //Debug.Log($"Ward: {ward.WardName}, Doctors: {doctorCount}, Nurses: {nurseCount}, Outpatients: {outpatientCount}");
                wardCounts.Add(ward.WardName, (doctorCount, nurseCount, outpatientCount, inpatientCount));
            }
        }

        return wardCounts;
    }

    // 헥사값 컬러 반환( 코드 순서 : RGBA )
    public static Color HexColor(string hexCode)
    {
        Color color;
        if (ColorUtility.TryParseHtmlString(hexCode, out color))
        {
            return color;
        }

        Debug.LogError("[UnityExtension::HexColor]invalid hex code - " + hexCode);
        return Color.white;
    }

    //소독 퀴즈 시작
    public void OpenDisinfectQuiz()
    {
        questDisfectCanvas.SetActive(true);

        int randomIndex = UnityEngine.Random.Range(0, questions.Length);

        //질문 텍스트 설정
        disinfectQuest.text = questions[randomIndex];

        //선택지 텍스트 설정 & 정답 체크
        for (int i = 0; i < disinfectAnswers.Length; i++)
        {
            disinfectAnswers[i].GetComponentInChildren<TextMeshProUGUI>().text = choices[randomIndex, i];
            int answerIndex = i;
            disinfectAnswers[i].onClick.RemoveAllListeners();      //이전에 설정된 이벤트가 있다면 초기화
            disinfectAnswers[i].onClick.AddListener(() => OnAnswerSelected(answerIndex, randomIndex));
        }
    }

    //정답 체크
    void OnAnswerSelected(int selectedAnswerIndex, int questionIndex)
    {
        if (selectedAnswerIndex == correctAnswers[questionIndex])
        {
            StartCoroutine(ShowCorrectPanel());
        }
        else
        {
            StartCoroutine(ShowDisWrongPanel());
        }
    }

    //정답 패널 생성
    IEnumerator ShowCorrectPanel()
    {
        disCorrectPanel.SetActive(true);
        yield return YieldInstructionCache.WaitForSecondsRealtime(1.3f);
        disCorrectPanel.SetActive(false);
        questDisfectCanvas.SetActive(false);
        StartWardDisinfection(nowIndex);
    }

    //오답 패널 생성
    IEnumerator ShowDisWrongPanel()
    {
        disWrongPanel.SetActive(true);
        yield return YieldInstructionCache.WaitForSecondsRealtime(1.3f);
        disWrongPanel.SetActive(false);
        questDisfectCanvas.SetActive(false);
        disinfectionButton[nowIndex].interactable = false;
        StartCoroutine(DisinfectionTimer(nowIndex, "소독 대기"));
    }
}
