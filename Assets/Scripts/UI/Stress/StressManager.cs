using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;

public class StressManager : MonoBehaviour
{
    public static StressManager Instance;

    private List<Person> allPeople = new List<Person>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        StartCoroutine(ExecuteEverySecond());
    }

    // 씬에 존재하는 활성화 상태 캐릭터를 가입시킴
    public void RegisterPerson(Person person)
    {
        if (!allPeople.Contains(person))    // 사람 리스트에 해당 person이 포함되어 있지 않는 상태면 가입
        {
            allPeople.Add(person);
        }
    }

    // 게임 접속 버튼 클릭 시 호출될 메서드
    public void StartStressForAll()
    {
        foreach (var person in allPeople)
        {
            StressController stressController = person.GetComponent<StressController>();
            if (stressController != null)
            {
                stressController.StartStress();
            }
        }
    }

    // 전체 스트레스 평균 얻기
    public float GetAverageStress()
    {
        if (allPeople.Count == 0) return 0f;
        return allPeople.Average(p => p.GetStressLevel());
    }

    // 병동 별 의사 스트레스 평균 얻는 메서드
    public float GetAverageDoctorStressByWard(Ward ward)
    {
        if (ward.doctors.Count == 0) return 0f;
        float totalStress = 0f;
        foreach (var doctor in ward.doctors)
        {
            totalStress += doctor.GetComponent<StressController>().stressLevel;
        }
        return totalStress / ward.doctors.Count;
    }

    // 병동 별 간호사 스트레스 평균 얻는 메서드
    public float GetAverageNurseStressByWard(Ward ward)
    {
        if (ward.nurses.Count == 0) return 0f;
        float totalStress = 0f;
        foreach (var nurse in ward.nurses)
        {
            totalStress += nurse.GetComponent<StressController>().stressLevel;
        }
        return totalStress / ward.nurses.Count;
    }

    // 직업 별 스트레스 평균 얻기
    public float GetAverageStressByRole(Role role)
    {
        var peopleInRole = allPeople.Where(p => p.role == role).ToList();
        if (peopleInRole.Count == 0) return 0f;
        return peopleInRole.Average(p => p.GetStressLevel());
    }

    // 모든 스트레스 수치 업데이트
    public void UpdateAllStressLevels(float amount)
    {
        foreach (var person in allPeople)
        {
            StressController stressController = person.GetComponent<StressController>();
            if (stressController != null)
            {
                stressController.UpdateStressLevel(amount);
            }
        }
    }

    IEnumerator ExecuteEverySecond()
    {
        // 주기적으로 UI 업데이트 (예: 1초마다)
        while (true)
        {
            StressUIManager.Instance.UpdateStressTexts();
            yield return new WaitForSeconds(0.5f);
        }
    }
}