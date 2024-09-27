using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class MonthlyReportUI : MonoBehaviour
{
    public GameObject monthlyReportCanvas;  // 월정산 UI 캔버스
    //public TextMeshProUGUI moneyInfo;       // 현재 금액
    public TextMeshProUGUI nowMoney;        // 월정산 이후 잔여 금액
    public TextMeshProUGUI incomeMoney;     // 한달 동안의 총 수입 금액
    public TextMeshProUGUI expenseMoney;    // 한달 동안의 총 지출 금액
    public TextMeshProUGUI netIncomeMoney;  // 순수익 금액
    public Image exitButton;                // 월정산 나가기 버튼
    public CurrentMoney currentMoneyManager;    //CurrentMoney 스크립트

    private int month = 1;
    private int totalIncome = 0;
    private int totalExpense = 0;

    private void Start()
    {
        monthlyReportCanvas = Assign(monthlyReportCanvas, "MonthlyReportCanvas");
        //moneyInfo = Assign(moneyInfo, "MoneyInfo");
        nowMoney = Assign(nowMoney, "NowMoney");
        incomeMoney = Assign(incomeMoney, "IncomeMoney");
        expenseMoney = Assign(expenseMoney, "ExpenseMoney");
        netIncomeMoney = Assign(netIncomeMoney, "NetIncomeMoney");
        exitButton = Assign(exitButton, "ExitButton");
        currentMoneyManager = Assign(currentMoneyManager, "CurrentMoneyManager");

        if (exitButton != null)
        {
            EventTrigger trigger = exitButton.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerClick;
            entry.callback.AddListener((eventData) => { CloseMonthlyReport(); });
            trigger.triggers.Add(entry);
        }
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
            else
                Debug.Log($"{objectName} 를 성공적으로 찾았습니다.");
        }
        return obj;
    }

    // 지출 금액 추가 메서드
    public void AddExpense(int amount)
    {
        //Debug.Log($"AddExpense 호출됨 - amount: {amount}");
        totalExpense += amount;
        UpdateExpenseText();
    }

    // 수입 금액 추가 메서드
    public void AddIncome(int amount)
    {
        //Debug.Log($"AddIncome 호출됨 - amount: {amount}");
        totalIncome += amount;
        UpdateIncomeText();
    }

    // 지출 금액 텍스트 갱신
    private void UpdateExpenseText()
    {
        //Debug.Log($"UpdateExpenseText 호출됨 - totalExpense: {totalExpense}");
        expenseMoney.text = $"{totalExpense:N0} 원";
        UpdateNetIncomeText();
    }

    // 수입 금액 텍스트 갱신
    private void UpdateIncomeText()
    {
        //Debug.Log($"UpdateIncomeText 호출됨 - totalIncome: {totalIncome}");
        incomeMoney.text = $"{totalIncome:N0} 원";
        UpdateNetIncomeText();
    }

    // 순수익 텍스트 갱신
    private void UpdateNetIncomeText()
    {
        int netIncome = totalIncome - totalExpense;
        netIncomeMoney.text = $"{netIncome:N0} 원";
    }

    // 현재 금액 업데이트 메서드
    public void UpdateNowMoney()
    {
        nowMoney.text = $"{currentMoneyManager.CurrentMoneyGetter:N0} 원";
        /*string moneyTextValue = moneyInfo.text;

        if (int.TryParse(moneyTextValue.Trim().Replace(",", ""), out int currentMoney))
        {
            nowMoney.text = $"{currentMoney:N0} 원";
        }
        else
        {
            Debug.LogError("Failed to parse money amount.");
        }*/
    }


    // 월 정산 표시 메서드
    public void ShowMonthlyReport()
    {
        Debug.Log("월 정산을 처리합니다.");
        if (monthlyReportCanvas != null)
        {
            UpdateNowMoney(); // 현재 금액 업데이트
            monthlyReportCanvas.SetActive(true); // 월 정산 캔버스 활성화
            Time.timeScale = 0; // 게임 일시 정지
        }
    }

    // 월 정산 캔버스 닫기 메서드
    private void CloseMonthlyReport()
    {
        if (monthlyReportCanvas != null)
        {
            monthlyReportCanvas.SetActive(false); // 월 정산 캔버스 비활성화
            month++;
            Time.timeScale = 1; // 게임 재개
        }

        // 월 정산 데이터 초기화
        ResetMonthlyReport();
    }

    // 월 정산 데이터 초기화 메서드
    private void ResetMonthlyReport()
    {
        totalIncome = 0;
        totalExpense = 0;
        UpdateIncomeText();
        UpdateExpenseText();
        UpdateNetIncomeText();
    }
}
