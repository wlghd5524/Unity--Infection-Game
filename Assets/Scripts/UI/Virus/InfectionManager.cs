using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class InfectionManager
{
    private static InfectionManager _instance = new InfectionManager();
    public static InfectionManager Instance { get { return _instance; } }
    public Button allClearButton;


    public int infectionProbability = 10;

    //스테이지 감염병 종류에 따른 감염 확률 매핑
    private Dictionary<int, float> probabilityMapping = new Dictionary<int, float>();

    //유니티에서 테스트를 위한 감염 확률 변수 (테스트 단계에서만 사용, 배포 단계에선 제외)
    public int stage1InfectionProbability = 100;
    public int stage2InfectionProbability = 120;

    public void Init()
    {
        infectionProbability = stage1InfectionProbability;
        probabilityMapping.Add(1, stage1InfectionProbability);
        probabilityMapping.Add(2, stage2InfectionProbability);

        allClearButton = Assign(allClearButton, "AllClearButton");

        allClearButton.onClick.AddListener(() => { DisinfectAllViruses(); BtnSoundManager.Instance.PlayButtonSound(); });
    }
    public void UpdateInfectionProbability()
    {
        if (Managers.Stage.stage == 1)
        {
            infectionProbability = stage1InfectionProbability;
        }
        else if (Managers.Stage.stage == 2)
        {
            infectionProbability = stage2InfectionProbability;
        }
    }

    // 병동 별 감염률 계산
    public float GetInfectionRate(Ward ward)
    {
        if (ward.totalOfNPC == 0)
        {
            return 0f; // 병동에 NPC가 없을 경우 감염률은 0으로 설정
        }
        return (ward.infectedNPC / ward.totalOfNPC) * 100; // 감염률을 퍼센트로 계산
    }

    // 병원 전체 감염률 계산
    public float GetOverallInfectionRate(List<Ward> wards)
    {

        if (wards == null || wards.Count == 0)
        {
            return 0f; // 병동 목록이 없거나 병동이 하나도 없으면 감염률 0%
        }

        float totalInfected = 0f;
        float totalNPCs = 0f;

        foreach (var ward in wards)
        {
            totalInfected += ward.doctors.Count(d => d.infectionController.isInfected)
                + ward.nurses.Count(n => n.infectionController.isInfected)
                + ward.outpatients.Count(o => o.infectionController.isInfected)
                + ward.inpatients.Count(i => i.infectionController.isInfected)
                + ward.emergencyPatients.Count(e => e.infectionController.isInfected)
                + ward.icuPatients.Count(c => c.infectionController.isInfected);
            totalNPCs += ward.totalOfNPC;
        }

        if (totalNPCs == 0)
        {
            return 0f; // 병원 전체에 NPC가 없으면 감염률 0%
        }
        //Debug.Log($"감염률 계산 : {totalInfected} / {totalNPCs} = {totalInfected / totalNPCs} ");
        return (totalInfected / totalNPCs) * 100f; // 전체 감염률을 퍼센트로 계산
    }

    public Dictionary<string, float> GetInfectionRateByRole()
    {
        Dictionary<string, float> infectionRateByRoleDictionary = new Dictionary<string, float>();
        float[] numberOfInfectedNPC = new float[6];
        float[] numberOfNPC = new float[6];

        foreach (Ward ward in Ward.wards)
        {
            foreach (PatientController patient in ward.inpatients)
            {
                if (patient.personComponent.infectionStatus != InfectionStatus.Normal)
                {
                    numberOfInfectedNPC[0]++;
                }
                numberOfNPC[0]++;
            }
            foreach (PatientController patient in ward.outpatients)
            {
                if (patient.personComponent.infectionStatus != InfectionStatus.Normal)
                {
                    numberOfInfectedNPC[1]++;
                }
                numberOfNPC[1]++;

            }
            foreach (PatientController patient in ward.emergencyPatients)
            {
                if (patient.personComponent.infectionStatus != InfectionStatus.Normal)
                {
                    numberOfInfectedNPC[2]++;
                }
                numberOfNPC[2]++;

            }
            foreach (PatientController patient in ward.icuPatients)
            {
                if (patient.personComponent.infectionStatus != InfectionStatus.Normal)
                {
                    numberOfInfectedNPC[3]++;
                }
                numberOfNPC[3]++;

            }

            foreach (NurseController nurse in ward.nurses)
            {
                if (nurse.personComponent.infectionStatus != InfectionStatus.Normal)
                {
                    numberOfInfectedNPC[4]++;
                }
                numberOfNPC[4]++;
            }

            foreach (DoctorController doctor in ward.doctors)
            {
                if (doctor.personComponent.infectionStatus != InfectionStatus.Normal)
                {
                    numberOfInfectedNPC[5]++;
                }
                numberOfNPC[5]++;
            }
        }
        // 역할 이름 배열
        string[] roles = { "inpatients", "outpatients", "emergencyPatients", "icuPatients", "nurse", "doctor" };

        // 반복문을 사용해 사전 업데이트
        for (int i = 0; i < roles.Length; i++)
        {
            float infectionRate = (numberOfNPC[i] != 0) ? (float)numberOfInfectedNPC[i] / numberOfNPC[i] : 0f;
            infectionRateByRoleDictionary[roles[i]] = infectionRate;
        }

        return infectionRateByRoleDictionary;
    }


    //감염 여부 체크
    public bool CheckInfection(int infectionResistance)
    {
        int random = Random.Range(0, infectionProbability);  //현재 감염 확률 내에서 난수 설정
        int totalRandom = Random.Range(0, 101);
        return random - infectionResistance >= totalRandom;  //감염 여부 반환
    }

    //모든 바이러스 소독
    public void DisinfectAllViruses()
    {
        Virus[] viruses = Object.FindObjectsOfType<Virus>();
        foreach (Virus virus in viruses)
        {
            virus.Disinfect();
        }
        Debug.Log("모든 바이러스 소독 완료"); //수정
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
}
