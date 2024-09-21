using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System;

public class StressController : MonoBehaviour
{
    public float stressLevel = 0f;
    public float stressIncreaseRate = 2f;
    public float stressDecreaseRate = 2f;
    public event Action<float> OnStressLevelChanged;

    private Person person;
    private float timer = 0f;
    private bool isStressActive = false; // 스트레스 활성화 여부

    private void Start()
    {
        person = GetComponent<Person>();

        // stressLevel을 0에서 20 사이의 랜덤 정수 값으로 초기화
        //stressLevel = UnityEngine.Random.Range(0, 21);
    }

    IEnumerator UpdateEverySecond()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            UpdateStress();
        }
    }

    public void StartStress()
    {
        if (!isStressActive)
        {
            isStressActive = true;
            StartCoroutine(UpdateEverySecond());
        }
    }

    // 스트레스 조절 함수
    private void UpdateStress()
    {
        // 활성화 안된 의사를 무시
        if (person == null)
        {
            Debug.LogError("Person 객체가 null입니다.", gameObject);
            return;
        }

        // 활성화 안된 의사를 무시
        if (person.Inventory == null)
        {
            //Debug.LogWarning("Inventory가 null입니다. Stress를 계산하지 않습니다.", gameObject);
            return;
        }

        float stressChange = 0f;

        // 직업에 따른 스트레스 조건
        switch (person.role)
        {
            case Role.Doctor:
                // 의사 스트레스 증가 로직
                // 환자 받을 시 증가 로직은 DoctorOffice.cs에서 관리
                // 착용 아이템 확인
                foreach (var item in person.Inventory.Values)
                {
                    if (item.isEquipped)
                    {
                        stressChange += item.stressIncreaseValue;   // 착용된 아이템마다 다른 스트레스 증가
                                                                    // 아이템 별 스트레스 수치 조정은 InfoWindow.cs의 List에 있음 
                    }
                }
                break;
            case Role.Nurse:
                // 간호사 스트레스 증가 로직
                break;
            case Role.Outpatient:
                // 외래환자 스트레스 증가 로직
                // 착용 아이템 확인
                foreach (var item in person.Inventory.Values)
                {
                    if (item.isEquipped)
                    {
                        stressChange += item.stressIncreaseValue;   // 착용된 아이템마다 다른 스트레스 증가
                                                                    // 아이템 별 스트레스 수치 조정은 InfoWindow.cs의 List에 있음 
                    }
                }
                break;
            case Role.Inpatient:
                // 입원환자 스트레스 증가 로직
                break;
            default:
                Debug.Log("role is null");
                break;
        }

        UpdateStressLevel(stressChange);
    }

    public void UpdateStressLevel(float amount)
    {
        stressLevel += amount;
        stressLevel = Mathf.Clamp(stressLevel, 0f, 100f); // 스트레스 수치를 0~100 사이로 제한
        OnStressLevelChanged?.Invoke(stressLevel); // 이벤트 호출
    }
}
