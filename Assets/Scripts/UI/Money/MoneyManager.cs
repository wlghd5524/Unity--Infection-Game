using TMPro;
using UnityEngine;

public class MoneyManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static MoneyManager Instance { get; private set; }
    public CurrentMoney currentMoneyManager;        //CurrentMoney 스크립트

    //public TextMeshProUGUI moneyText; // 게임 재화 Text (Inspector에서 할당)
    // 병원 의료비 설정 (일단 임의값)
    public static int MedicalFee = 1;            // 진료비
    public static int HospitalizationFee = 30;    // 입원비 한달 비용
    public static int SurgeryFee = 20;            // 수술비

    private void Awake()
    {
        // 싱글톤 패턴 구현: Instance 설정 및 중복 방지
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 이 오브젝트는 씬 전환 시에도 파괴되지 않음
        }
        else
        {
            Destroy(gameObject); // 중복된 인스턴스가 생기면 파괴
        }
    }

    private void Start()
    {
        currentMoneyManager = Assign(currentMoneyManager, "CurrentMoneyManager");
    }

    public void IncreaseMoney(int amount)
    {
        currentMoneyManager.CurrentMoneyGetter += amount;
        /*string moneyTextValue = moneyText.text;

        if (int.TryParse(moneyTextValue.Trim().Replace(",", ""), out int currentMoney))
        {
            currentMoney += amount;
            moneyText.text = $"{currentMoney:N0}";
            Debug.Log($"Increased money by {amount}. New total: {currentMoney}");
        }
        else
        {
            Debug.LogError("Failed to parse money amount.");
        }*/
    }

    // 오브젝트 자동 할당
    private T Assign<T>(T obj, string objectName) where T : Object
    {
        if (obj == null)
        {
            GameObject foundObject = GameObject.Find(objectName);
            if (foundObject != null)
            {
                if (typeof(Component).IsAssignableFrom(typeof(T)))
                    obj = foundObject.GetComponent(typeof(T)) as T;
                else if (typeof(GameObject).IsAssignableFrom(typeof(T)))
                    obj = foundObject as T;
            }
            if (obj == null)
                Debug.LogError($"{objectName} 를 찾을 수 없습니다.");
        }
        return obj;
    }
}
