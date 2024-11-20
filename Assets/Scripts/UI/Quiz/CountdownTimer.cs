using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CountdownTimer : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    public Image shadowImage;

    private float maxDuration;        // 최대 타이머 시간
    private float timeRemaining;      // 남은 시간
    private bool isTiming;            // 타이머 진행 중인지 확인
    private System.Action onTimerEnd; // 타이머 종료 시 호출될 콜백

    void Start()
    {
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
                OnTimerEnd();
            }

            UpdateTimerUI();
        }
    }

    // 타이머 시작 메서드
    public void StartTimer(float duration, System.Action timerEndCallback)
    {
        maxDuration = duration; // 최대 시간 설정
        timeRemaining = duration;
        onTimerEnd = timerEndCallback;
        isTiming = true;
        UpdateTimerUI();
    }

    // 타이머 멈추기 메서드 추가
    public void StopTimer()
    {
        isTiming = false;
    }

    public void ResetTimer()
    {
        timeRemaining = maxDuration;
        UpdateTimerUI();
    }

    public void SetTimerText(string text)
    {
        if (timerText != null)
        {
            timerText.text = text;
        }
    }

    // 타이머 UI 업데이트 메서드
    private void UpdateTimerUI()
    {
        timerText = GameObject.Find("TimerText").GetComponent<TextMeshProUGUI>();
        timerText.text = Mathf.Ceil(timeRemaining).ToString();

        // Shadow 영역 업데이트
        float fillAmount = timeRemaining / maxDuration;
        shadowImage = GameObject.Find("TimerShadow").GetComponent<Image>();
        shadowImage.fillAmount = fillAmount;
    }

    // 타이머 종료 시 호출될 메서드
    private void OnTimerEnd()
    {
        onTimerEnd?.Invoke();
    }
}
