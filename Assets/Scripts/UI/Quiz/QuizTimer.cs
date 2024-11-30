using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuizTimer : MonoBehaviour
{
    public static QuizTimer Instance {  get; private set; }
    public TextMeshProUGUI timerText;
    public Image shadowImage;

    float timeRemaining;
    bool isTiming;            
    string currentLevelName;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else 
            Destroy(gameObject);

        timerText = GameObject.Find("TimerText").GetComponent<TextMeshProUGUI>();
        shadowImage = GameObject.Find("TimerShadow").GetComponent<Image>();
    }

    void Update()
    {
        if (isTiming && timeRemaining > 0)
        {
            timeRemaining -= Time.unscaledDeltaTime;

            if (timeRemaining <= 0)
            {
                isTiming = false;
                timeRemaining = 0;
                QuizManager.Instance.NextQuestionDelay();
            }

            UpdateTimerUI();
        }
    }

    // 타이머 시작하는 함수
    public void StartQuizTimer(string levellName)
    {
        currentLevelName = levellName;
        timeRemaining = LevelByTimerDuration(currentLevelName);
        isTiming = true;
    }

    // 타이머 시간을 고정시킨은 함수(코루틴 진행될 때)
    public void ResetTimerText(string currentLevel)
    {
        isTiming = false;
        timeRemaining = LevelByTimerDuration(currentLevel);
        UpdateTimerUI();
    }

    // 레벨별 타이머 시간
    float LevelByTimerDuration(string levelName)
    {
        switch (levelName)
        {
            case "LevelButton1":
                return 10f;
            case "LevelButton2":
                return 20f;
            case "LevelButton3":
                return 30f;
        }

        return -1;
    }

    // 타이머 UI 업데이트 메서드
    private void UpdateTimerUI()
    {
        timerText = GameObject.Find("TimerText").GetComponent<TextMeshProUGUI>();
        timerText.text = Mathf.Ceil(timeRemaining).ToString();

        // Shadow 영역 업데이트
        float fillAmount = timeRemaining / LevelByTimerDuration(currentLevelName);
        shadowImage = GameObject.Find("TimerShadow").GetComponent<Image>();
        shadowImage.fillAmount = fillAmount;
    }
}
