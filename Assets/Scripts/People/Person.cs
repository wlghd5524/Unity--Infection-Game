using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//플레이어의 상태(감염병)를 나타내는 enum
//Stage1은 접촉성 감염병
//Stage2은 비접촉성(범위형) 감염병
public enum InfectionState
{
    Normal,
    CRE,
    Covid
}
public enum Role
{
    Doctor,
    Nurse,
    Outpatient,
    Inpatient,
    EmergencyPatient
}
public struct ItemInfo
{
    public bool isPurchased;
    public bool isEquipped;
    public int price;
    public float protectionRate;
    public float stressIncreaseValue; // 스트레스 증가 값

    public ItemInfo(bool isPurchased, bool isEquipped, int price, float protectionRate, float stressIncreaseValue)
    {
        this.isPurchased = isPurchased;
        this.isEquipped = isEquipped;
        this.price = price;
        this.protectionRate = protectionRate;
        this.stressIncreaseValue = stressIncreaseValue;
    }
}
public class Person : MonoBehaviour
{
    public InfectionState status = InfectionState.Normal;
    public int infectionResistance = 0;
    public Role role;

    public bool isImmune;
    private CapsuleCollider coll;
    private bool isWaiting;

    public PatientController patientController;
    public delegate void InfectionStateChanged(InfectionState newStatus);
    public event InfectionStateChanged OnInfectionStateChanged;


    public int ID { get; private set; }
    public string Name { get; private set; }
    public string Job { get; private set; }
    public bool IsResting { get; private set; }
    public Dictionary<string, ItemInfo> Inventory { get; private set; }
    public Sprite AvatarSprite { get; private set; } // 추가된 필드
    public bool IsMale { get; private set; } // 성별 필드 추가


    public void Initialize(int id, string name, string job, bool isResting, Role role, List<(string itemName, int itemPrice, float protectionRate, float stressIncreaseValue)> inventory)
    {
        ID = id;
        Name = name;
        Job = job;
        IsResting = isResting;
        this.role = role;
        Inventory = new Dictionary<string, ItemInfo>();
        foreach (var (itemName, itemPrice, protectionRate, stressIncreaseValue) in inventory)
        {
            Inventory[itemName] = new ItemInfo(false, false, itemPrice, protectionRate, stressIncreaseValue); // 기본 구매 및 착용 상태는 false
        }

        // 성별 랜덤 설정
        IsMale = Random.Range(0, 2) == 0;
        AssignName();

        // AvatarSprite가 null인 경우에만 로드
        if (AvatarSprite == null)
        {
            string genderFolder = IsMale ? "Man" : "Woman";
            Sprite[] avatars = Resources.LoadAll<Sprite>($"Sprites/Avatars/{genderFolder}");
            if (avatars != null && avatars.Length > 0)
            {
                AvatarSprite = avatars[Random.Range(0, avatars.Length)];
            }
        }
    }
    void Start()
    {
        Transform ballTransform = transform.Find("IsInfection");
        coll = GetComponent<CapsuleCollider>();

        patientController = GetComponent<PatientController>();

        // Renderer 컴포넌트 얻기
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            // NPCManager에 NPC 등록
            NPCManager.Instance.RegisterNPC(gameObject, this, renderer);
        }
    }
    void Update()
    {

        //감염병 종류에 따라 감염 범위 설정
        if (status == InfectionState.CRE)
        {
            coll.radius = 0.3f;
        }
        else if (status == InfectionState.Covid)
        {
            coll.radius = 1.0f;
        }
        else if (status == InfectionState.Normal)
        {
            coll.radius = 0.2f;
        }
        if (isWaiting)
        {
            return;
        }
        if (status != InfectionState.Normal)
        {
            //ballRenderer.enabled = true;
        }
        else
        {
            //ballRenderer.enabled = false;
        }

        //착용하고 있는 보호 장비에 따라 감염 저항성 설정

    }
    public void AssignName()
    {
        Name = NameList.GetUniqueName(role, IsMale);
    }

    public void ResetInventory()
    {
        List<string> keys = new List<string>(Inventory.Keys);
        foreach (var key in keys)
        {
            Inventory[key] = new ItemInfo(false, false, Inventory[key].price, Inventory[key].protectionRate, Inventory[key].stressIncreaseValue);
        }
    }
    public void ToggleRestingState()
    {
        IsResting = !IsResting;
        if (IsResting)
        {
            ResetInventory();
        }
    }
    public void ResetPerson()
    {
        role = Role.Outpatient;
        status = InfectionState.Normal;
        // 필요한 다른 속성들도 초기화
    }


    public void ChangeStatus(InfectionState infection)
    {
        gameObject.GetComponent<NPCController>().wardComponent.infectedNPC++;
        StartCoroutine(IncubationPeriod(infection));
        if (Random.Range(0, 100) <= 30)
        {
            StartCoroutine(SelfRecovery());
        }
    }

    public IEnumerator SelfRecovery()
    {
        yield return new WaitForSeconds(Random.Range(70f, 80f));
        Debug.Log("자가 면역을 가져서 더 이상 감염되지 않음");
        status = InfectionState.Normal;
        isImmune = true;
    }
    public void Recover()
    {
        status = InfectionState.Normal;
    }
    private IEnumerator IncubationPeriod(InfectionState infection)
    {
        status = infection;
        isWaiting = true;
        yield return new WaitForSeconds(5);
        isWaiting = false;
        OnInfectionStateChanged?.Invoke(infection); // 이벤트 호출
    }

    public void EquipItem(string itemName)
    {
        if (Inventory.ContainsKey(itemName) && Inventory[itemName].isPurchased)
        {
            Inventory[itemName] = new ItemInfo(Inventory[itemName].isPurchased, true, Inventory[itemName].price, Inventory[itemName].protectionRate, Inventory[itemName].stressIncreaseValue);
        }
    }

    public void UnequipItem(string itemName)
    {
        if (Inventory.ContainsKey(itemName))
        {
            Inventory[itemName] = new ItemInfo(Inventory[itemName].isPurchased, false, Inventory[itemName].price, Inventory[itemName].protectionRate, Inventory[itemName].stressIncreaseValue);
        }
    }

    public float GetTotalProtectionRate()
    {
        float totalProtectionRate = 0f;
        foreach (var item in Inventory.Values)
        {
            if (item.isEquipped)
            {
                totalProtectionRate += item.protectionRate;
            }
        }
        return totalProtectionRate;
    }
}
