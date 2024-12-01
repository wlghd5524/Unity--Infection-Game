using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuizCooltime : MonoBehaviour
{
    public static QuizCooltime Instance { get; private set; }
    public TextMeshProUGUI cooltimeText;
    public Image fillImage;

    Dictionary<string, Coroutine> activeCoroutines = new Dictionary<string, Coroutine>();
    public static int maxCooltime = 60;
    public string currentName;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);

        cooltimeText = GameObject.Find("TextCoolTime").GetComponent<TextMeshProUGUI>();
        fillImage = GameObject.Find("Shadow").GetComponent<Image>();
    }


    public void ShowCooltimePanel(string levelName)
    {
        // 기존 코루틴이 있으면 정지
        if (activeCoroutines.ContainsKey(levelName))
        {
            StopCoroutine(activeCoroutines[levelName]);
            activeCoroutines.Remove(levelName);
        }

        // 새로운 코루틴 실행
        activeCoroutines[levelName] = StartCoroutine(CooltimeCoroutine(levelName));
    }

    IEnumerator CooltimeCoroutine(string levelName)
    {
        while (QuizManager.Instance.cooldownTimers[levelName] > 0)
        {
            QuizManager.Instance.cooldownTimers[levelName] -= Time.unscaledDeltaTime;

            if (QuizManager.Instance.cooldownTimers[levelName] <= 0)
                QuizManager.Instance.cooldownTimers[levelName] = 0;

            UpdateCooltimeUI();
            yield return null;
        }

        QuizManager.Instance.coolTimePanel.SetActive(false);
        QuizManager.Instance.questCount[levelName].Clear();
        activeCoroutines.Remove(levelName);
        QuizManager.Instance.OnLevelButtonClicked(QuizManager.Instance.clickbtn);
    }

    void UpdateCooltimeUI()
    {
        float currentCooldown = QuizManager.Instance.cooldownTimers[currentName];

        // 남은 시간 계산
        int hours = Mathf.FloorToInt(currentCooldown / 3600F);
        int minutes = Mathf.FloorToInt((currentCooldown - hours * 3600) / 60F);
        int seconds = Mathf.RoundToInt(currentCooldown - hours * 3600 - minutes * 60);

        // UI 업데이트
        cooltimeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        fillImage.fillAmount = currentCooldown / maxCooltime;
    }
}
