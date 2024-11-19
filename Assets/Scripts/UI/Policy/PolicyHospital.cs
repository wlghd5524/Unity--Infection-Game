using System.Collections;
using System.Collections.Generic;
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

    Ward ward;
    ResearchDBManager researchDBManager;
    string[] wards = new string[] { "내과 1", "내과 2", "외과 1", "외과 2", "입원병동1", "입원병동2", "입원병동3", "입원병동4" };
    bool[] isClosed = new bool[8];
    bool[] isDisinfected = new bool[8];

    void Start()
    {
        ward = FindObjectOfType<Ward>();
        researchDBManager = FindObjectOfType<ResearchDBManager>();
        updateButton = GameObject.Find("UpdateButton").GetComponent<Button>();
        monthlyReportUI = FindObjectOfType<MonthlyReportUI>();
        currentMoneyManager = FindObjectOfType<CurrentMoney>();
        moneyText = GameObject.Find("MoneyText").GetComponent<TextMeshProUGUI>();

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

            //소독 버튼을 비활성화 상태로 초기화
            disinfectionButton[index].interactable = false;
            isClosed[index] = false;       // 모든 병동을 열림 상태로 저장
            isDisinfected[index] = false;  // 모든 병동을 소독 안 한 상태로 저장

            closingButton[index].onClick.RemoveAllListeners(); // 기존 리스너 제거
            disinfectionButton[index].onClick.RemoveAllListeners();

            // 폐쇄 버튼 클릭 시 처리
            closingButton[index].onClick.AddListener(() =>
            {
                Debug.Log($"PolicyHospital: {index}");
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
                BtnSoundManager.Instance.PlayButtonSound();
            });
        }

        updateButton.onClick.AddListener(() => { OnUpdateCount(); BtnSoundManager.Instance.PlayButtonSound(); });
        UpdateWardCountsPeriodically();
    }

    // 폐쇄 버튼 클릭 시 처리(true가 폐쇄)
    void ToggleColsing(int index)
    {
        isClosed[index] = !isClosed[index];
        Debug.Log($"PolicyHospital: {isClosed[index]}");
        closingOutline[index].color = isClosed[index] ? HexColor("#DC0004") : HexColor("#CED4DA");       // 폐쇄 시 빨간 테두리 이미지 
        UpdateWardCounts();

        // 소독을 안 했다면
        if (!isDisinfected[index])
        {
            disinfectionButton[index].interactable = isClosed[index]; // 소독 버튼 활성화 관리
            disinfectionText[index].text = isClosed[index] ? "소독 가능" : "";
        }

        PrintButtonState(1, index, isClosed[index]);             // DB에 폐쇄 상태 저장
    }

    // 소독 버튼 클릭 시 소독 상태 업데이트
    void ToggleDisinfection(int index)
    {
        if (isClosed[index] && !isDisinfected[index])
        {
            // 소독 중일 때 비활성화하여 추가 클릭을 방지
            disinfectionButton[index].interactable = false;
            disinfectionOutline[index].color = HexColor("#00FF37");    // 소독 시 초록색 테두리
            disinfectionText[index].text = "소독 중...";

            // 소독 시간 대기 후 완료 텍스트로 전환
            StartCoroutine(DisinfectionTimer(index));
        }
    }

    IEnumerator DisinfectionTimer(int index)
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
        isDisinfected[index] = true;
        //disinfectionText[index].text = "소독 완료";
        disinfectionOutline[index].color = HexColor("#CED4DA");
        PrintButtonState(2, index, true); // 소독 완료 상태를 DB에 저장
        disinfectionButton[index].interactable = false;
        monthlyReportUI.AddExpenseDetail("소독", 500);
        currentMoneyManager.CurrentMoneyGetter -= 300;
        moneyText.text = $"{currentMoneyManager.CurrentMoneyGetter:N0}Sch";

        // 소독 기능 재사용까지 30초 텀 두기
        float cooldownTime = 30f;
        float cooldownElapsed = 0f;
        while (cooldownElapsed < cooldownTime)
        {
            cooldownElapsed += Time.unscaledDeltaTime;
            float remainingCooldown = Mathf.Ceil(cooldownTime - cooldownElapsed);
            disinfectionText[index].text = $"소독 완료\n{remainingCooldown}초";

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
        Debug.Log($"PolicyHospital: {toggleType}.{wardNumber}.{toggleState}");

        researchDBManager.AddResearchData(ResearchDBManager.ResearchMode.patient, toggleType, wardNumber, toggleState);
    }

    // 버튼을 누르면 병동 데이터 업데이트
    void OnUpdateCount()
    {
        UpdateWardCountsPeriodically();
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
            if (i >= 0 && i <= 3)
            {
                if (isClosed[i])
                    closingText[i].text = "의사 x0\n간호사 x0\n외래환자 x0";
                else if (wardCounts.ContainsKey(wards[i]))
                {
                    // 병동이 열려있을 때 병동 정보를 출력
                    var wardInfo = wardCounts[wards[i]];
                    closingText[i].text = $"의사 x{wardInfo.doctorCount}\n간호사 x{wardInfo.nurseCount}\n외래환자 x{wardInfo.outpatientCount}";
                }
            }
            else
            {
                if (isClosed[i])
                    closingText[i].text = "의사 x0\n간호사 x0\n내원환자 x0";
                else if (wardCounts.ContainsKey(wards[i]))
                {
                    // 병동이 열려있을 때 병동 정보를 출력
                    var wardInfo = wardCounts[wards[i]];
                    closingText[i].text = $"의사 x{wardInfo.doctorCount}\n간호사 x{wardInfo.nurseCount}\n내원환자 x{wardInfo.inpatientCount}";
                }
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
}
