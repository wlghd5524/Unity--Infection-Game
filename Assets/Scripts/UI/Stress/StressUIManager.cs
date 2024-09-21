using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

// ProfileWindowManager
public class StressUIManager : MonoBehaviour
{
    public static StressUIManager Instance;

    //public TextMeshProUGUI allStressText;       // 전체 스트레스 수치
    //public TextMeshProUGUI doctorStressText;    // 의사
    //public TextMeshProUGUI nurseStressText;     // 간호사
    //public TextMeshProUGUI outpatientStressText;// 외래환자
    //public TextMeshProUGUI inpatientStressText; // 내래환자

    public GameObject showIndividualPanel;      // 해당 NPC 정보 패널
    public TextMeshProUGUI npcNameText;         // NPC 이름 Text
    public Image npcAvatarImage;
    public TextMeshProUGUI individualStressText;// 개인스트레스 수치
    public TextMeshProUGUI npcProtectionRateText; // 개인 감염방지 확률 (합)

    public CircularProgressBar circularPrograssBar; // 원형 프로그래스 바
    public Button toggleRestingButton; // 휴식 상태를 변경하는 버튼
    public TextMeshProUGUI restingStatusText; // 휴식 상태 텍스트
    private ProfileWindow profileWindow; // ProfileWindow 인스턴스

    private Person currentNPC;
    private StressController currentStress;
    private bool isActive;

    private float allStress;
    private int count = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        //allStressText = Assign(allStressText, "AllStress");
        //doctorStressText = Assign(doctorStressText, "DoctorStress");
        //nurseStressText = Assign(nurseStressText, "NurseStress");
        //outpatientStressText = Assign(outpatientStressText, "OutPatientStress");
        //inpatientStressText = Assign(inpatientStressText, "InPatientStress");
        showIndividualPanel = Assign(showIndividualPanel, "ShowIndividualPanel");
        npcNameText = Assign(npcNameText, "NPCName");
        npcAvatarImage = Assign(npcAvatarImage, "NPCAvatarImage");
        individualStressText = Assign(individualStressText, "IndividualStressText");
        npcProtectionRateText = Assign(npcProtectionRateText, "NPCProtectionRateText");
        circularPrograssBar = Assign(circularPrograssBar, "CirclePrograssBar");
        toggleRestingButton = Assign(toggleRestingButton, "ToggleRestingButton"); // 버튼 할당
        restingStatusText = Assign(restingStatusText, "RestingStatusText"); // 휴식 상태 텍스트 할당
        profileWindow = FindObjectOfType<ProfileWindow>(); // ProfileWindow 인스턴스 찾기


        toggleRestingButton.onClick.AddListener(OnToggleRestingButtonClick); // 버튼 클릭 이벤트 추가

        circularPrograssBar.SetProgress(0.5f);
    }
    private void Update()
    {
        if (currentNPC != null)
        {
            UpdateIndividual();
        }
    }
    // 자동 할당 코드
    private T Assign<T>(T obj, string objectName) where T : Object
    {
        if (obj == null)
        {
            GameObject foundObject = GameObject.Find(objectName);
            if (foundObject != null)
            {
                if (typeof(Component).IsAssignableFrom(typeof(T))) obj = foundObject.GetComponent(typeof(T)) as T;
                else if (typeof(GameObject).IsAssignableFrom(typeof(T))) obj = foundObject as T;
            }
            if (obj == null) Debug.LogError($"{objectName} 를 찾을 수 없습니다.");
        }
        return obj;
    }

    public void ShowStressUI(int personID)
    {
        // 현재 NPC 설정
        currentNPC = PersonManager.Instance.GetPerson(personID);

        if (currentNPC != null)
        {
            // 개인 스트레스 패널 활성화
            showIndividualPanel.SetActive(true);
            isActive = true;
            UpdateRestingStatus();


            if (currentNPC.role == Role.Doctor || currentNPC.role == Role.Nurse)
            {
                toggleRestingButton.gameObject.SetActive(true); // 버튼 활성화
                restingStatusText.gameObject.SetActive(true); // 텍스트 활성화
                UpdateRestingStatus();
            }
            else
            {
                toggleRestingButton.gameObject.SetActive(false); // 버튼 비활성화
                restingStatusText.gameObject.SetActive(false); // 텍스트 비활성화
            }

            // StressController 설정
            currentStress = currentNPC.GetComponent<StressController>();
            if (currentStress == null)
            {
                Debug.LogError("currentNPC does not have a StressController component.");
                isActive = false;
                showIndividualPanel.SetActive(false);
            }
            // NPC 보호율 텍스트 업데이트
            npcProtectionRateText.text = $"+ {currentNPC.GetTotalProtectionRate():F2}%";
        }
        else
        {
            Debug.LogError("currentNPC is null.");
            isActive = false;
            showIndividualPanel.SetActive(false);
        }
    }

    private void OnToggleRestingButtonClick()
    {
        if (currentNPC != null)
        {
            bool wasResting = currentNPC.IsResting; // 현재 상태를 저장
            currentNPC.ToggleRestingState();
            UpdateRestingStatus();
            RefreshProfiles();
            if (wasResting)
            {
                profileWindow.ShowInventoryForPerson(currentNPC.ID);
            }

        }
    }

    private void RefreshProfiles()
    {
        if (profileWindow != null)
        {
            profileWindow.RefreshProfiles();
        }
    }

    private void UpdateRestingStatus()
    {
        restingStatusText.text = currentNPC.IsResting ? "휴식 중" : "근무 중";
        toggleRestingButton.GetComponentInChildren<TextMeshProUGUI>().text = "교대";
    }

    public void UpdateIndividual()
    {
        // 개인 스트레스 텍스트 업데이트
        individualStressText.text = $"{currentNPC.GetStressLevel():F1}%";

        // NPC 정보 텍스트 업데이트
        npcNameText.text = $"{currentNPC.Job} {currentNPC.Name}";
        npcAvatarImage.sprite = currentNPC.AvatarSprite;
        float progress = currentStress.stressLevel;
        circularPrograssBar.SetProgress(progress / 100);
        if (progress <= 40)
        {
            circularPrograssBar.SetColor(Color.green);

        }
        else if (progress > 40 && progress <= 75)
        {
            circularPrograssBar.SetColor(Color.yellow);
        }
        else
        {
            circularPrograssBar.SetColor(Color.red);
        }

        // NPC 보호율 텍스트 업데이트
        npcProtectionRateText.text = $"+ {currentNPC.GetTotalProtectionRate():F2}%";
    }


    // 스트레스 표시 업데이트
    public void UpdateStressTexts()
    {
        //allStressText.text = $"All : {StressManager.Instance.GetAverageStress():F1}";
        //doctorStressText.text = $"Doctor: {StressManager.Instance.GetAverageStressByRole(NurseRole.Doctor):F1}";
        //nurseStressText.text = $"Nurse: {StressManager.Instance.GetAverageStressByRole(NurseRole.Nurse):F1}";
        //outpatientStressText.text = $"Outpatient: {StressManager.Instance.GetAverageStressByRole(NurseRole.Outpatient):F1}";
        //inpatientStressText.text = $"Inpatient: {StressManager.Instance.GetAverageStressByRole(NurseRole.Inpatient):F1}";
    }
}