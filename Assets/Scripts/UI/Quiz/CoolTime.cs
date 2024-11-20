using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CoolTime : MonoBehaviour
{
    public TMP_Text cooltimeText;
    public Image fillImage;
    private float maxCooldown;
    float startTime;        // 쿨타임 시작 시간
    public string currentLevelName;
    RandomQuest randomQuest;
    private System.Action onCooldownComplete;  //쿨타임이 끝나면 호출
    Coroutine _co;

    private void Start()
    {
        cooltimeText = GameObject.Find("TextCoolTime").GetComponent<TextMeshProUGUI>();
        fillImage = GameObject.Find("Shadow").GetComponent<Image>();
        randomQuest = FindObjectOfType<RandomQuest>();
    }

    public void StartCooldown(float remainingCooldown, float maxCooldown, System.Action onCooldownComplete)
    {
        this.maxCooldown = maxCooldown; 
        this.onCooldownComplete = onCooldownComplete;
        randomQuest.cooldownTimers[currentLevelName] = remainingCooldown;
        startTime = Time.realtimeSinceStartup; 
        UpdateCooltimeUI();
        gameObject.SetActive(true); 
        if (_co != null)
        {
            StopCoroutine(_co);
        }
        _co = StartCoroutine(CooldownCoroutine());
    }

    //시간이 남아있으면 매 프레임마다 currentCooldown을 감소시킴
    //쿨타임 끝나면 onCooldownCompete 호출
    private IEnumerator CooldownCoroutine()
    {
        float endTime = startTime + randomQuest.cooldownTimers[currentLevelName]; 

        while (Time.realtimeSinceStartup < endTime)
        {
            randomQuest.cooldownTimers[currentLevelName] = endTime - Time.realtimeSinceStartup; 
            UpdateCooltimeUI();
            yield return null;
        }

        randomQuest.cooldownTimers[currentLevelName] = 0;
        UpdateCooltimeUI();
        onCooldownComplete?.Invoke();  //null이 아니면 호출해서 등록된 메서드 실행
        gameObject.SetActive(false);  
    }

    //쿨타임 UI 업데이트
    private void UpdateCooltimeUI()
    {
        if (cooltimeText != null && fillImage != null)
        {
            float currentCooldown = randomQuest.cooldownTimers[currentLevelName];
            // 남은 시간 계산
            int hours = Mathf.FloorToInt(currentCooldown / 3600F);
            int minutes = Mathf.FloorToInt((currentCooldown - hours * 3600) / 60F);
            int seconds = Mathf.FloorToInt(currentCooldown - hours * 3600 - minutes * 60);

            // 텍스트 업데이트
            cooltimeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

            // 이미지 업데이트
            fillImage.fillAmount = currentCooldown / maxCooldown;
        }
    }

}
