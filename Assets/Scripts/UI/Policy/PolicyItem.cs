using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Linq;



class PolicyItemInfo
{
    public string[] itemInfos = new string[]
    {
        "Dental 마스크|1|비말 차단 + 미세입자 방어|3중 구조로 이루어진 일회용 의료 마스크입니다.\n주로 의료 환경에서 감염 예방을 위해 사용되며,\n착용감이 뛰어나고 장시간 사용에도 불편함이 적습니다.|N95 마스크와 중복 착용 할 수 없습니다.",
        "일회용 장갑|2|비말 및 오염 방지|의료 환경에서 감염 예방을 위해 사용되는 일회용 장갑으로\n피부 오염을 방지하며, 사용이 간편하고 다양한 크기로\n제공되어 의료 종사자들이 효율적으로 사용할 수 있습니다.|라텍스 장갑과 중복 착용할 수 없습니다.",
        "N95 마스크|3|미세 입자 및 비말 차단|고효율 필터로 미세 입자와 비말을 효과적으로\n차단하는 의료용 마스크로, 높은 밀착성과 편안한\n착용감을 제공하여 감염 예방에 필수적입니다.|Dental 마스크와 중복 착용할 수 없습니다.",
        "라텍스 장갑|4|세균 및 바이러스 차단|밀착성이 뛰어난 라텍스 소재로 세균 및 바이러스로부터\n손을 보호하며, 내구성이 우수하여 의료 환경에서\n사용하기 적합한 장갑입니다.|일회용 장갑과 중복 착용할 수 없습니다.",
        "의료용 고글|5|눈 보호 및 비말 차단|의료 현장에서 눈을 보호하고 비말로부터 안전하게\n유지하는 의료용 보호 고글로, 김 서림 방지 처리가 되어\n시야 확보에 용이합니다.| ",
        "의료용 헤어캡|6|머리카락 오염 방지|의료 환경에서 머리카락을 감싸 비말과 오염으로부터\n보호하는 헤어캡으로, 신축성이 좋아 다양한 머리 크기에\n맞게 착용 가능합니다.| ",
        "AP 가운|7|전신 감염 방어|비말과 체액으로부터 전신을 보호하는 방수 의료용 가운으로,\n내구성이 뛰어나며 의료 종사자의 안전을 보장합니다.| ",
        "Level C|8|고위험 감염 보호|Level C 보호 장비로 고위험 환경에서 의료 종사자를\n전신 감염으로부터 보호하며, 편리한 착용과\n빠른 탈의가 가능하여 긴급 상황에 적합합니다.|이 보호구를 착용하면 다른 보호구는 장착 해제됩니다."
    };
}

public class PolicyItem : MonoBehaviour
{
    public static PolicyItem Instance { get; private set; } // 싱글톤 인스턴스

    public NurseController nurses;
    public bool isAllItemsEquipped = false;

    public GameObject itemInfoPrefab;
    public Transform itemScrollViewContent;
    private PolicyItemInfo policyItemInfo = new PolicyItemInfo();
    public Button doctorButton, nurseButton, isolationNurseButton, patientButton;

    private string selectedJob; // 선택된 직업 이름
    private Button currentlySelectedButton; // 현재 선택된 버튼


    private string[] doctorJobs = { "Doctor", "Nurse", "QuarantineNurse" }; // 모든 아이템 착용 가능 직업

    private Dictionary<string, Dictionary<string, bool>> equippedStatesByJob = new Dictionary<string, Dictionary<string, bool>>(); // 직업별 아이템 상태 저장
    private string[] requireItem = { "N95 마스크", "라텍스 장갑", "의료용 고글", "AP 가운" };
    private readonly Dictionary<string, string[]> mutuallyExclusiveItems = new Dictionary<string, string[]>
    {
    { "Dental 마스크", new[] { "N95 마스크" } },
    { "N95 마스크", new[] { "Dental 마스크" } },
    { "일회용 장갑", new[] { "라텍스 장갑" } },
    { "라텍스 장갑", new[] { "일회용 장갑" } }
    };


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
        ConfigureGridLayout();
        AdjustScrollSpeed();
        InitializeEquippedStates();
        CreateItemEntriesForAllJobs();

        doctorButton.onClick.AddListener(() => HandleJobButtonClick("Doctor", doctorButton));
        nurseButton.onClick.AddListener(() => HandleJobButtonClick("Nurse", nurseButton));
        isolationNurseButton.onClick.AddListener(() => HandleJobButtonClick("QuarantineNurse", isolationNurseButton));
        patientButton.onClick.AddListener(() => HandleJobButtonClick("Patient", patientButton));
    }

    private void InitializeEquippedStates()
    {
        // 의사, 간호사, 격리 간호사
        foreach (var jobName in doctorJobs)
        {
            equippedStatesByJob[jobName] = new Dictionary<string, bool>();
            foreach (var itemInfo in policyItemInfo.itemInfos)
            {
                string itemName = itemInfo.Split('|')[0];
                equippedStatesByJob[jobName][itemName] = false; // 초기 상태: 미장착
            }
        }

        // 환자 (Outpatient, Inpatient, EmergencyPatient, ICUPatient)
        equippedStatesByJob["Patient"] = new Dictionary<string, bool>();
        foreach (var itemInfo in policyItemInfo.itemInfos)
        {
            string itemName = itemInfo.Split('|')[0];
            // 환자는 Dental 마스크와 N95 마스크만 허용
            if (itemName == "Dental 마스크" || itemName == "N95 마스크")
            {
                equippedStatesByJob["Patient"][itemName] = false; // 초기 상태: 미장착
            }
        }
    }

    public void IsAllItemsEquipped(string itemName)
    {
        if (selectedJob != "QuarantineNurse") return;

        // QuarantineNurse의 모든 필수 아이템이 착용되었는지 확인
        isAllItemsEquipped = requireItem.All(itemName =>
            equippedStatesByJob.ContainsKey(selectedJob) &&
            equippedStatesByJob[selectedJob].ContainsKey(itemName) &&
            equippedStatesByJob[selectedJob][itemName]);

        if (isAllItemsEquipped)
            Debug.Log("이잉 기모띠");
    }


    private void CreateItemEntriesForAllJobs()
    {
        ShowItemsForJob("Doctor"); // 초기화 시 의사 직업 아이템 표시
    }

    private List<string> GetAllowedItemsForJob(string jobName)
    {
        if (doctorJobs.Contains(jobName))
        {
            // 모든 아이템 허용
            return policyItemInfo.itemInfos.Select(info => info.Split('|')[0]).ToList();
        }
        else if (jobName == "Patient")
        {
            // 환자는 특정 아이템만 허용
            return new List<string> { "Dental 마스크", "N95 마스크" };
        }

        return new List<string>();
    }


    void ConfigureGridLayout()
    {
        GridLayoutGroup gridLayoutGroup = itemScrollViewContent.GetComponent<GridLayoutGroup>();
        if (gridLayoutGroup != null)
        {
            gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayoutGroup.constraintCount = 1;
            gridLayoutGroup.cellSize = new Vector2(1850, 400);
            gridLayoutGroup.spacing = new Vector2(30, 30);
            gridLayoutGroup.padding = new RectOffset(60, 60, 30, 30);
        }
    }

    void AdjustScrollSpeed()
    {
        ScrollRect scrollRect = itemScrollViewContent.GetComponentInParent<ScrollRect>();
        if (scrollRect != null)
        {
            scrollRect.scrollSensitivity = 120.0f;
        }
    }

    private void HandleJobButtonClick(string jobName, Button clickedButton)
    {
        // 이전 선택된 버튼 상태 초기화
        if (currentlySelectedButton != null)
        {
            ResetButtonAppearance(currentlySelectedButton);
        }

        // 현재 선택된 버튼 강조
        HighlightButton(clickedButton);
        currentlySelectedButton = clickedButton;

        // 해당 직업에 맞는 아이템 표시
        ShowItemsForJob(jobName);
    }

    private void HighlightButton(Button button)
    {
        ColorBlock colors = button.colors;
        // 강조 색상: #CCCCCC (연한 회색)
        Color highlightColor = new Color(204f / 255f, 204f / 255f, 204f / 255f); // RGB 값을 255로 나눈 값

        colors.normalColor = highlightColor; // 강조 색상
        colors.selectedColor = highlightColor; // 클릭 시 색상
        colors.highlightedColor = highlightColor; // 하이라이트 색상
        button.colors = colors;
    }

    private void ResetButtonAppearance(Button button)
    {
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white; // 기본 흰색
        colors.selectedColor = Color.white;
        colors.highlightedColor = Color.white;
        button.colors = colors;
    }

    private void CreateItemEntry(string[] itemDetails)
    {
        GameObject itemInstance = Instantiate(itemInfoPrefab, itemScrollViewContent);
        itemInstance.name = $"ItemInfoPrefab_{itemDetails[0]}";

        string itemName = itemDetails[0];
        string itemPrice = itemDetails[1];
        string itemEffect = itemDetails[2];
        string itemInformation = itemDetails[3];
        string itemMoreInfo = itemDetails[4];

        Image itemIcon = itemInstance.transform.Find("ItemImageSlot/ItemIcon").GetComponent<Image>();
        TextMeshProUGUI itemNameText = itemInstance.transform.Find("ItemNameSlot/ItemName").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI itemPriceText = itemInstance.transform.Find("ItemInfoSlot/ItemPrice").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI itemEffectText = itemInstance.transform.Find("ItemInfoSlot/ItemEffect").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI itemInformationText = itemInstance.transform.Find("ItemInfoSlot/ItemInfomation").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI itemMoreInfoText = itemInstance.transform.Find("ItemInfoSlot/ItemMore").GetComponent<TextMeshProUGUI>();
        Sprite itemSprite = Resources.Load<Sprite>($"Sprites/ItemSprites/{itemName}");
        Button itemEquipmentButton = itemInstance.transform.Find("ItemEquipmentButton").GetComponent<Button>();

        TextMeshProUGUI itemEquipmentText = itemEquipmentButton.transform.Find("ItemEquipmentText").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI itemIsEquipText = itemInstance.transform.Find("ItemEquipment/ItemIsEquipText").GetComponent<TextMeshProUGUI>();

        itemNameText.text = itemName;
        itemPriceText.text = itemPrice + " SCH(인당)";
        itemEffectText.text = itemEffect;
        itemInformationText.text = itemInformation;
        itemMoreInfoText.text = itemMoreInfo;
        itemIcon.sprite = itemSprite;

        // 초기 장착 상태 업데이트
        UpdateItemUI(itemName, itemEquipmentText, itemIsEquipText);

        // 아이템 착용/해제 버튼 클릭 이벤트
        itemEquipmentButton.onClick.AddListener(() =>
        {
            ToggleItemEquippedState(itemName);
            UpdateItemUI(itemName, itemEquipmentText, itemIsEquipText);
            UpdateNPCEquipment(itemName, isEquipped: equippedStatesByJob[selectedJob][itemName]);
            HandleMutuallyExclusiveItems(itemName); // 중복 착용 해제 로직 호출
            IsAllItemsEquipped(itemName);
        });
    }

    private void ToggleItemEquippedState(string itemName)
    {
        if (equippedStatesByJob[selectedJob].ContainsKey(itemName))
        {
            equippedStatesByJob[selectedJob][itemName] = !equippedStatesByJob[selectedJob][itemName];
        }
    }

    private void UpdateItemUI(string itemName, TextMeshProUGUI equipmentText, TextMeshProUGUI equipStatusText)
    {
        if (equippedStatesByJob[selectedJob].ContainsKey(itemName) && equippedStatesByJob[selectedJob][itemName])
        {
            equipmentText.text = "장착 해제";
            equipStatusText.text = "장착 중";
        }
        else
        {
            equipmentText.text = "장착";
            equipStatusText.text = "장착 안함";
        }
    }

    private void UpdateNPCEquipment(string itemName, bool isEquipped)
    {
        List<Person> persons = PersonManager.Instance.GetAllPersons();
        foreach (Person person in persons)
        {
            if (person.role == Role.Nurse)
            {
                NurseController nurseController = person.GetComponent<NurseController>();
                if (nurseController.isQuarantineNurse && selectedJob == "QuarantineNurse")
                {
                    if (person.Inventory.TryGetValue(itemName, out Item item))
                    {
                        item.isEquipped = isEquipped;
                        person.UpdateInfectionResistance(); // NPC 방어율 업데이트
                    }
                }
                else if (!nurseController.isQuarantineNurse && selectedJob == "Nurse")
                {
                    if (person.Inventory.TryGetValue(itemName, out Item item))
                    {
                        item.isEquipped = isEquipped;
                        person.UpdateInfectionResistance(); // NPC 방어율 업데이트
                    }
                }
            }
            else if (person.role == GetRoleFromJobName(selectedJob))
            {
                if (person.Inventory.TryGetValue(itemName, out Item item))
                {
                    item.isEquipped = isEquipped;
                    person.UpdateInfectionResistance(); // NPC 방어율 업데이트
                }
            }
        }
    }

    private void HandleMutuallyExclusiveItems(string itemName)
    {
        if (mutuallyExclusiveItems.ContainsKey(itemName))
        {
            foreach (var exclusiveItem in mutuallyExclusiveItems[itemName])
            {
                if (equippedStatesByJob[selectedJob].ContainsKey(exclusiveItem) && equippedStatesByJob[selectedJob][exclusiveItem])
                {
                    equippedStatesByJob[selectedJob][exclusiveItem] = false;
                    UpdateNPCEquipment(exclusiveItem, isEquipped: false);
                    Transform otherItem = itemScrollViewContent.Find($"ItemInfoPrefab_{exclusiveItem}");
                    if (otherItem != null)
                    {
                        TextMeshProUGUI otherEquipmentText = otherItem.Find("ItemEquipmentButton/ItemEquipmentText").GetComponent<TextMeshProUGUI>();
                        TextMeshProUGUI otherEquipStatusText = otherItem.Find("ItemEquipment/ItemIsEquipText").GetComponent<TextMeshProUGUI>();
                        UpdateItemUI(exclusiveItem, otherEquipmentText, otherEquipStatusText);
                    }
                }
            }
        }
        UpdateNPCEquipment(itemName, isEquipped: equippedStatesByJob[selectedJob][itemName]);
    }

    private Role GetRoleFromJobName(string jobName)
    {
        return jobName switch
        {
            "Doctor" => Role.Doctor,
            "Outpatient" => Role.Outpatient,
            "Inpatient" => Role.Inpatient,
            "EmergencyPatient" => Role.EmergencyPatient,
            "ICUPatient" => Role.ICUPatient,
            "Nurse" => Role.Nurse,
            "QuarantineNurse" => Role.Nurse,
            _ => throw new System.ArgumentException("Invalid job name")
        };
    }

    private void ShowItemsForJob(string jobName)
    {
        selectedJob = jobName;

        foreach (Transform child in itemScrollViewContent)
        {
            Destroy(child.gameObject);
        }

        List<string> allowedItems = GetAllowedItemsForJob(jobName);

        foreach (string itemInfo in policyItemInfo.itemInfos)
        {
            string[] itemDetails = itemInfo.Split('|');
            string itemName = itemDetails[0];

            if (allowedItems.Contains(itemName))
            {
                CreateItemEntry(itemDetails);
            }
        }
    }

}

