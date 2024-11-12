using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RadialProgressbars : MonoBehaviour
{
    private float hospitalInfectionRate;        // 병원 내 감염률
    private float hospitalStressAverageRate;    // 병원 내 평균 스트레스 지수

    public TextMeshProUGUI hospitalInfectionRateText;
    public TextMeshProUGUI hospitalStressAverageRateText;

    public Image hospitalInfectionRateBar;
    public Image hospitalStressAverageRateBar;

    private bool isUpdating = false; // 코루틴이 실행 중인지 체크하는 변수
    private bool isTenseBGMPlaying = false; // 긴박한 BGM 재생 여부

    private float updateInterval = 1f; // 업데이트 간격
    private float timer = 0f;

    private AudioManager audioManager;

    // Start is called before the first frame update
    void Start()
    {
        hospitalInfectionRateText = Assign(hospitalInfectionRateText, "HospitalInfectionRateText");
        hospitalStressAverageRateText = Assign(hospitalStressAverageRateText, "HospitalStressAverageRateText");
        hospitalInfectionRateBar = Assign(hospitalInfectionRateBar, "HospitalInfectionRateBar");
        hospitalStressAverageRateBar = Assign(hospitalStressAverageRateBar, "HospitalStressAverageRateBar");

        hospitalInfectionRateText.text = "0.0%";
        hospitalStressAverageRateText.text = "0.0%";
        hospitalInfectionRateBar.fillAmount = 0;
        hospitalStressAverageRateBar.fillAmount = 0;

        audioManager = FindObjectOfType<AudioManager>();
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

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= updateInterval)
        {
            UpdateProgressBars();
            timer = 0f;
        }
    }

    void UpdateProgressBars()
    {
        hospitalInfectionRate = InfectionManager.Instance.GetOverallInfectionRate(Ward.wards);
        hospitalInfectionRateText.text = $"{hospitalInfectionRate:F1}%";
        hospitalInfectionRateBar.fillAmount = hospitalInfectionRate / 100;

        // 감염률이 50%를 넘으면 긴박한 배경음악 재생
        if (hospitalInfectionRate >= 50)
        {
            if (!isTenseBGMPlaying)
            {
                audioManager.SwitchToTenseBGM();
                isTenseBGMPlaying = true;
            }
        }
        // 감염률이 40% 이하로 떨어지면 일반 배경음악으로 복귀
        else if (hospitalInfectionRate <= 40 && isTenseBGMPlaying)
        {
            audioManager.SwitchToNormalBGM();
            isTenseBGMPlaying = false;
        }
    }
}
