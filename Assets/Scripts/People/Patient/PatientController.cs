﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.AI;
using System.IO;

public class PatientController : NPCController
{
    // 웨이포인트 관련 변수
    public int waypointIndex = 0;

    public List<NavMeshPath> navMeshPaths = new List<NavMeshPath>();

    // 상태 플래그
    public bool isQuarantined = false;
    public bool isFollowingNurse = false;
    public bool isWaitingForDoctor = false;
    public bool isWaitingForNurse = false;
    public bool isExiting = false;

    public bool officeSignal = false;
    public bool nurseSignal = false;
    public bool doctorSignal = false;

    public GameObject nurse;
    public QuarantineRoom nPRoom;
    //public MonthlyReportUI //monthlyReportUI;     //MonthlyReportUI 스크립트

    private IconManager iconManager;       // 진오 추가

    public BedWaypoint bedWaypoint;
    public int prevWaypointIndex = -1;
    public bool isLayingDown = false;

    public Transform waypointsTransform;

    public bool excutedHC = false;  //입원 시간 재는 코루틴 실행 여부
    public bool excutedQC = false;  //격리 시간 재는 코루틴 실행 여부

    public ProfileWindow profileWindow;
    public void Activate()
    {
        do
        {
            ward = Random.Range(0, 4);
            waypointsTransform = Managers.NPCManager.waypointDictionary[(ward, "OutpatientWaypoints")];
            wardComponent = waypointsTransform.gameObject.GetComponentInParent<Ward>();
        }
        while (wardComponent.isClosed);
        // 첫 번째 웨이포인트 추가
        AddWaypoint(waypointsTransform, $"CounterWaypoint (0)");

    }

    // 진오 추가
    private void Start()
    {
        profileWindow = FindObjectOfType<ProfileWindow>();
        iconManager = GetComponent<IconManager>();
        personComponent = GetComponent<Person>();
        //입원 환자 웨이포인트 추가
        if (personComponent.role == Role.Inpatient)
        {
            AddInpatientWaypoints();

        }
        if (personComponent.role == Role.EmergencyPatient)
        {
            StartCoroutine(EmergencyPatientMove());
        }
    }

    private void Update()
    {
        // 애니메이션
        Managers.NPCManager.UpdateAnimation(agent, animator);
        if (!isLayingDown)
        {
            Managers.NPCManager.PlayWakeUpAnimation(animator);
            agent.radius = 0.175f;
        }
        if (isExiting)
        {
            return;
        }
        if (isQuarantined)
        {
            if (isLayingDown)
            {
                if(nPRoom.bedNum >= 8)
                {
                    transform.eulerAngles = new Vector3(0, 90, 0);
                }
                else
                {
                    transform.eulerAngles = new Vector3(0, -90, 0);

                }
            }
            else
            {
                StartCoroutine(QuarantineMove());

            }
        }
        if(isWaiting)
        {
            return;
        }
        if (isQuarantined && excutedQC)
        {
            excutedQC = false;
            StartCoroutine(QuarantineTimeCounter());
        }
        if (personComponent.role == Role.Inpatient)
        {
            if (!excutedHC && Managers.PatientCreator.startSignal)
            {
                excutedHC = true;
                StartCoroutine(HospitalizationTimeCounter());
            }

            if (isLayingDown)
            {
                agent.radius = 0.000001f;
                if (bedWaypoint.bedGameObject.transform.parent.eulerAngles == new Vector3(0, 0, 0))
                {
                    transform.eulerAngles = new Vector3(0, -180, 0);
                }
                else
                {
                    transform.eulerAngles = Vector3.zero;
                }
            }
            if (isWaitingForNurse || isQuarantined)
            {
                return;
            }
            // 목적지에 도착했는지 확인
            if (Managers.NPCManager.isArrived(agent))
                StartCoroutine(InpatientMove());
        }

        if (personComponent.role == Role.Outpatient)
        {
            // 애니메이션 업데이트
            //Managers.NPCManager.UpdateAnimation(agent, animator);

            // 대기 중이면 이동 처리하지 않음
            if (isWaitingForNurse)
            {
                return;
            }
            if (isWaitingForNurse || isQuarantined)
            {
                return;
            }
            // 목적지에 도착했는지 확인
            if (Managers.NPCManager.isArrived(agent))
            {
                if (waypointIndex == 4 && !isWaitingForNurse && !isFollowingNurse && !isQuarantined)
                {
                    // 모든 웨이포인트를 방문했으면 비활성화
                    Managers.ObjectPooling.DeactivatePatient(gameObject);
                    Managers.PatientCreator.numberOfOutpatient--;
                    return;
                }
                else
                {
                    // 다음 웨이포인트로 이동
                    StartCoroutine(OutpatientMove());
                }
            }
        }
        if (personComponent.role == Role.EmergencyPatient)
        {
            if (isLayingDown)
            {
                agent.radius = 0;
                if (bedWaypoint.bedGameObject.transform.parent.eulerAngles == new Vector3(0, 270, 0))
                {
                    transform.eulerAngles = new Vector3(0, 180, 0);
                }
                else
                {
                    transform.eulerAngles = Vector3.zero;
                }
                //Managers.NPCManager.PlayLayDownAnimation(animator);
            }
        }
    }

    public IEnumerator QuarantineMove()
    {
        isWaiting = true;
        yield return new WaitForSeconds(2.0f);
        if (isQuarantined)
        {
            if (!isLayingDown)
            {
                agent.SetDestination(nPRoom.GetBedPoint());
                yield return new WaitUntil(() => Managers.NPCManager.isArrived(agent));
                if (isLayingDown)
                {
                    if (nPRoom.bedNum >= 8)
                    {
                        transform.eulerAngles = new Vector3(0, 90, 0);
                    }
                    else
                    {
                        transform.eulerAngles = new Vector3(0, -90, 0);

                    }
                }
                isLayingDown = true;
                Managers.NPCManager.PlaySittingAnimation(animator);
                yield return new WaitForSeconds(0.5f);
                Managers.NPCManager.PlayLayDownAnimation(animator);
                yield return new WaitForSeconds(2.0f);
            }
            yield break;
        }
        isWaiting = false;
    }

    // 다음 웨이포인트로 이동하는 코루틴
    public IEnumerator OutpatientMove()
    {
        isWaiting = true;
        yield return new WaitForSeconds(1.5f);
        if (waypointIndex > 0 && waypoints[waypointIndex - 1] is DoctorOffice docOffice)
        {
            DoctorController targetDoctor = docOffice.doctor.GetComponent<DoctorController>();
            targetDoctor.patient = gameObject;
            targetDoctor.outpatientSignal = true;
            yield return new WaitForSeconds(1.0f);
            yield return new WaitUntil(() => doctorSignal);
            Managers.NPCManager.FaceEachOther(docOffice.doctor, gameObject);
            yield return new WaitForSeconds(1.5f);

        }





        // 현재 웨이포인트 인덱스에 따라 다음 웨이포인트 추가
        AddOutpatientWaypoint();

        if (waypointIndex < waypoints.Count)
        {

            // 의사 사무실인 경우 대기
            if (waypoints[waypointIndex] is DoctorOffice doctorOffice)
            {
                StartCoroutine(WaitForDoctorOffice(doctorOffice));
            }
            else if (waypointIndex > 0 && waypoints[waypointIndex - 1] is DoctorOffice doc)
            {
                DoctorController doctorController = doc.doctor.GetComponent<DoctorController>();
                doc.isEmpty = true;

                // 진료비 수입 증가 임시 위치
                // 진료비 : 10000
                MoneyManager.Instance.IncreaseMoney(MoneyManager.MedicalFee);
                //monthlyReportUI = FindObjectOfType<MonthlyReportUI>();
                //monthlyReportUI.AddIncome(MoneyManager.MedicalFee);

                doctorController.outpatientSignal = false;
            }

            // 다음 웨이포인트로 이동
            if (!isWaitingForDoctor)
            {
                // 현재 웨이포인트가 DoctorOffice일 경우
                if (waypointIndex > 0 && waypoints[waypointIndex - 1] is DoctorOffice)
                {
                    bool isFound = false;
                    BedWaypoint nextBed = null;
                    foreach (Ward ward in Ward.wards)
                    {
                        foreach (BedWaypoint bed in ward.beds)
                        {
                            if (bed.patient == null && 4 <= ward.num && ward.num <= 7)
                            {
                                nextBed = bed;
                                nextBed.patient = gameObject;
                                isFound = true;
                                break;
                            }
                        }
                        if (isFound)
                        {
                            break;
                        }
                    }

                    // 랜덤으로 입원 혹은 퇴원
                    int random = Random.Range(0, 101);
                    //입원 환자로 전환
                    if (random >= 30 && isFound)
                    {
                        Debug.Log("외래 환자 -> 입원 환자");
                        bedWaypoint = nextBed;
                        isLayingDown = false;
                        waypoints.Clear();
                        
                        officeSignal = false;
                        doctorSignal = false;
                        wardComponent.outpatients.Remove(this);
                        profileWindow.RemoveProfile(personComponent.ID);
                        Managers.PatientCreator.numberOfOutpatient--;
                        ward = bedWaypoint.ward;
                        wardComponent = Managers.NPCManager.waypointDictionary[(ward, "InpatientWaypoints")].GetComponentInParent<Ward>();
                        waypointsTransform = Managers.NPCManager.waypointDictionary[(ward, "InpatientWaypoints")];
                        wardComponent.inpatients.Add(this);
                        profileWindow.AddInpatientProfile(gameObject);
                        Managers.PatientCreator.numberOfInpatient++;
                        doctorSignal = false;
                        AddInpatientWaypoints();
                        personComponent.role = Role.Inpatient;
                        agent.SetDestination(bedWaypoint.GetBedPoint());
                        //yield return new WaitUntil(() => Managers.NPCManager.isArrived(agent));
                        waypointIndex = -1;
                        isWaiting = false;
                        yield break;
                    }
                    else
                    {
                        nextBed.patient = null;
                        iconManager.IsIcon();
                    }
                }

                if (waypoints[waypointIndex] is DoctorOffice DO)
                {
                    agent.SetDestination(waypoints[waypointIndex++].GetMiddlePointInRange());
                }
                else
                {
                    agent.SetDestination(waypoints[waypointIndex++].GetSampledPosition());
                }

            }

        }
        isWaiting = false;
    }

    public IEnumerator InpatientMove()
    {

        if (prevWaypointIndex == 1)
        {
            waypoints[1].isEmpty = true;
        }

        isWaiting = true;

        if (isQuarantined)
        {
            yield return new WaitForSeconds(2.0f);
            agent.SetDestination(nPRoom.GetBedPoint());
            isWaiting = false;
            yield break;
        }

        float random = Random.Range(0, 101);
        if (random <= 70)
        {
            if (prevWaypointIndex == 0)
            {
                yield return new WaitForSeconds(Random.Range(4.5f, 5.5f));
                isWaiting = false;
                yield break;
            }

            agent.SetDestination(bedWaypoint.GetBedPoint());
            yield return new WaitUntil(() => Managers.NPCManager.isArrived(agent));
            if (isExiting)
            {
                yield break;
            }
            if (bedWaypoint.bedGameObject.transform.parent.eulerAngles == new Vector3(0, 0, 0))
            {
                transform.eulerAngles = new Vector3(0, -180, 0);
            }
            else
            {
                transform.eulerAngles = Vector3.zero;
            }
            isLayingDown = true;
            Managers.NPCManager.PlaySittingAnimation(animator);

            yield return new WaitForSeconds(0.5f);


            Managers.NPCManager.PlayLayDownAnimation(animator);

            yield return new WaitForSeconds(2.0f);
            waypoints[0].isEmpty = false;
            prevWaypointIndex = 0;
            yield return new WaitForSeconds(2.0f);
        }
        else if (random <= 80 && waypoints[1].isEmpty)
        {
            if (prevWaypointIndex == 0)
            {
                waypoints[0].isEmpty = true;
                Managers.NPCManager.PlayWakeUpAnimation(animator);
                yield return new WaitForSeconds(5.0f);
                isLayingDown = false;
            }
            waypoints[1].isEmpty = false;

            agent.SetDestination(waypoints[1].GetSampledPosition());

            yield return new WaitUntil(() => Managers.NPCManager.isArrived(agent));
            prevWaypointIndex = 1;
        }
        else if (random <= 90)
        {
            if (prevWaypointIndex == 0)
            {
                waypoints[0].isEmpty = true;
                Managers.NPCManager.PlayWakeUpAnimation(animator);
                yield return new WaitForSeconds(5.0f);
                isLayingDown = false;
            }

            agent.SetDestination(waypoints[2].GetSampledPosition());

            yield return new WaitUntil(() => Managers.NPCManager.isArrived(agent));
            prevWaypointIndex = 2;
        }
        else
        {
            if (prevWaypointIndex == 0)
            {
                waypoints[0].isEmpty = true;
                Managers.NPCManager.PlayWakeUpAnimation(animator);
                yield return new WaitForSeconds(5.0f);
                isLayingDown = false;
            }

            agent.SetDestination(waypoints[Random.Range(3, 5)].GetSampledPosition());

            yield return new WaitUntil(() => Managers.NPCManager.isArrived(agent));

            prevWaypointIndex = 3;
        }
        yield return new WaitForSeconds(Random.Range(6f, 10f));
        isWaiting = false;
    }

    public IEnumerator EmergencyPatientMove()
    {
        agent.SetDestination(bedWaypoint.GetBedPoint());
        yield return new WaitForSeconds(2.0f);
        yield return new WaitUntil(() => Managers.NPCManager.isArrived(agent));
        DoctorController.ERwaitingList.Add(this);
        bedWaypoint.isEmpty = false;
        isLayingDown = true;
        Managers.NPCManager.PlaySittingAnimation(animator);
        yield return new WaitForSeconds(0.5f);

        Managers.NPCManager.PlayLayDownAnimation(animator);
        yield return new WaitForSeconds(5.0f);

        yield return new WaitUntil(() => doctorSignal);


        bool isFound = false;
        BedWaypoint nextBed = null;
        foreach (Ward ward in Ward.wards)
        {
            foreach (BedWaypoint bed in ward.beds)
            {
                if (bed.patient == null && 4 <= ward.num && ward.num <= 7)
                {
                    nextBed = bed;
                    nextBed.patient = gameObject;
                    isFound = true;
                    break;
                }
            }
            if (isFound)
            {
                break;
            }
        }

        // 랜덤으로 입원 혹은 퇴원
        int random = Random.Range(0, 101);
        // 퇴원
        if (random <= 30 || !isFound)
        {
            nextBed.isEmpty = true;
            nextBed.patient = null;
            AddWaypoint(waypointsTransform, $"Counter");
            AddWaypoint(Managers.NPCManager.gatewayTransform, $"Gateway ({Random.Range(0, 2)})");
            int stayDuration = Random.Range(5, 10);
            yield return new WaitForSeconds(stayDuration);

            Managers.NPCManager.PlayWakeUpAnimation(animator);
            yield return new WaitForSeconds(5.0f);
            isLayingDown = false;

            agent.SetDestination(waypoints[1].GetRandomPointInRange());

            yield return new WaitUntil(() => Managers.NPCManager.isArrived(agent));
            yield return new WaitForSeconds(2.0f);


            agent.SetDestination(waypoints[2].GetRandomPointInRange());

            yield return new WaitUntil(() => Managers.NPCManager.isArrived(agent));
            Managers.ObjectPooling.DeactivatePatient(gameObject);
            Managers.PatientCreator.numberOfEmergencyPatient--;
        }
        //입원 환자로 전환
        else
        {
            Managers.NPCManager.PlayWakeUpAnimation(animator);
            yield return new WaitForSeconds(5.0f);
            bedWaypoint = nextBed;
            agent.SetDestination(bedWaypoint.GetBedPoint());
            isLayingDown = false;
            waypoints.Clear();
            wardComponent.emergencyPatients.Remove(this);
            profileWindow.RemoveProfile(personComponent.ID);

            Managers.PatientCreator.numberOfEmergencyPatient--;
            ward = bedWaypoint.ward;
            wardComponent = Managers.NPCManager.waypointDictionary[(ward, "InpatientWaypoints")].GetComponentInParent<Ward>();
            waypointsTransform = Managers.NPCManager.waypointDictionary[(ward, "InpatientWaypoints")];
            doctorSignal = false;
            AddInpatientWaypoints();

            wardComponent.inpatients.Add(this);
            profileWindow.AddInpatientProfile(gameObject);
            Managers.PatientCreator.numberOfInpatient++;
            yield return new WaitUntil(() => Managers.NPCManager.isArrived(agent));
            yield return new WaitForSeconds(2.0f);

            personComponent.role = Role.Inpatient;
        }
    }


    // 다음 웨이포인트 추가 메서드
    private void AddOutpatientWaypoint()
    {
        switch (waypointIndex)
        {
            case 0:
                AddWaypoint(waypointsTransform, $"CounterWaypoint (0)");
                break;
            case 1:
                AddWaypoint(waypointsTransform, $"SofaWaypoint (0)");
                break;
            case 2:
                if (waypoints.Count < 3)
                {
                    AddWaypoint(waypointsTransform, $"Doctor'sOffice (0)");
                }
                break;
            case 3:
                AddWaypoint(Managers.NPCManager.gatewayTransform, $"Gateway ({Random.Range(0, 2)})");
                break;
        }
    }

    // 의사 사무실 대기 코루틴
    private IEnumerator WaitForDoctorOffice(DoctorOffice doctorOffice)
    {
        isWaitingForDoctor = true;
        while (!officeSignal)
        {
            yield return new WaitForSeconds(1.0f);
        }
        doctorOffice.isEmpty = false;
        isWaitingForDoctor = false;
    }

    //간호사가 올 때까지 대기 코루틴
    public IEnumerator WaitForNurse()
    {
        isWaitingForNurse = true;
        agent.isStopped = true;
        yield return new WaitUntil(() => nurseSignal);
        agent.ResetPath();
        nurseSignal = false;
        agent.isStopped = false;
        isWaitingForNurse = false;
    }

    // 웨이포인트 추가 메서드
    private void AddWaypoint(Transform parentTransform, string childName)
    {
        Transform waypointTransform = parentTransform.Find(childName);
        if (waypointTransform != null)
        {
            Waypoint comp = waypointTransform.gameObject.GetComponent<Waypoint>();
            if (comp is DoctorOffice)
            {
                // 의사 사무실 선택 로직
                SelectDoctorOffice(parentTransform);
            }
            else
            {
                if (!waypoints.Contains(comp))
                {
                    waypoints.Add(comp);
                }
            }
        }
        else
        {
            Debug.LogWarning($"Can't find waypoint: {childName}");
        }
    }

    // 의사 사무실 선택 메서드
    private void SelectDoctorOffice(Transform parentTransform)
    {
        // 가능한 의사 사무실 목록 생성
        Dictionary<DoctorOffice, int> countDic = new Dictionary<DoctorOffice, int>();
        for (int i = 0; i < 10; i++)
        {
            DoctorOffice doctorOffice = parentTransform.Find("Doctor'sOffice (" + i + ")").gameObject.GetComponent<DoctorOffice>();
            GameObject searchedDoctor = doctorOffice.doctor;
            if (doctorOffice.doctor == null)
            {
                continue;
            }
            DoctorController doctorController = searchedDoctor.GetComponent<DoctorController>();
            if (doctorController == null || doctorController.isResting)
            {
                continue;
            }
            int patientCount = doctorController.patientCount + doctorOffice.waitingQueue.Count;
            if (!countDic.ContainsKey(doctorOffice))
            {
                countDic.Add(doctorOffice, patientCount);
            }
        }

        // 최적의 의사 사무실 선택
        if (countDic.Count > 0)
        {
            DoctorOffice searchedOffice = countDic
                .OrderBy(kvp => kvp.Value)
                .FirstOrDefault().Key;
            searchedOffice.waitingQueue.Enqueue(this);
            waypoints.Add(searchedOffice);
            countDic.Clear();
        }
        else
        {
            Debug.LogError("의사를 찾을 수 없습니다.");
        }
    }
    public IEnumerator FollowNurse(GameObject nurse)
    {
        this.nurse = nurse;
        isFollowingNurse = true;
        while (isFollowingNurse)
        {
            isLayingDown = false;
            agent.SetDestination(nurse.transform.position);
            yield return new WaitForSeconds(0.1f);
        }
        agent.ResetPath();
    }

    public IEnumerator ExitHospital()
    {
        isExiting = true;
        if (isWaiting)
        {
            if (personComponent.role == Role.Outpatient)
            {
                StopCoroutine(OutpatientMove());
                isWaiting = false;
                agent.ResetPath();
            }
            else if (personComponent.role == Role.Inpatient)
            {
                StopCoroutine(InpatientMove());
                isWaiting = false;
                agent.ResetPath();
            }
        }

        Debug.Log($"{gameObject.name} 퇴원 시작");


        Managers.NPCManager.PlayWakeUpAnimation(animator);
        yield return new WaitForSeconds(5.0f);

        isLayingDown = false;

        //if (personComponent.role != Role.Outpatient)
        //{
        //    agent.SetDestination(waypoints[Random.Range(3, 5)].GetSampledPosition());
        //    yield return new WaitUntil(() => Managers.NPCManager.isArrived(agent));
        //    yield return new WaitForSeconds(2.0f);
        //}

        if (personComponent.role == Role.Inpatient)
        {
            wardComponent.inpatients.Remove(this);
            Managers.PatientCreator.numberOfInpatient--;
        }
        else if (personComponent.role == Role.Outpatient)
        {
            wardComponent.outpatients.Remove(this);
            Managers.PatientCreator.numberOfOutpatient--;
        }
        else if (personComponent.role == Role.EmergencyPatient)
        {
            wardComponent.emergencyPatients.Remove(this);
            Managers.PatientCreator.numberOfEmergencyPatient--;
        }
        profileWindow.RemoveProfile(personComponent.ID);

        if (bedWaypoint != null)
        {
            bedWaypoint.isEmpty = true;
            bedWaypoint.patient = null;
        }
        bedWaypoint = null;

        if(agent.hasPath)
        {
            agent.ResetPath();
        }

        //if (ward >= 6 && ward <= 7)
        //{
        //    agent.SetDestination(Managers.NPCManager.passPointTransform.GetChild(0).GetComponent<Waypoint>().GetSampledPosition());
        //    yield return new WaitUntil(() => Managers.NPCManager.isArrived(agent));
        //}
        //else if (ward >= 4 && ward <= 5)
        //{
        //    agent.SetDestination(Managers.NPCManager.passPointTransform.GetChild(1).GetComponent<Waypoint>().GetSampledPosition());


        //    yield return new WaitUntil(() => Managers.NPCManager.isArrived(agent));
        //}
        //else if (ward >= 2 && ward <= 3)
        //{
        //    agent.SetDestination(Managers.NPCManager.passPointTransform.GetChild(2).GetComponent<Waypoint>().GetSampledPosition());


        //    yield return new WaitUntil(() => Managers.NPCManager.isArrived(agent));
        //}

        agent.SetDestination(Managers.NPCManager.gatewayTransform.Find("Gateway (" + Random.Range(0, 2) + ")").GetComponent<Waypoint>().GetSampledPosition());


        yield return new WaitUntil(() => Managers.NPCManager.isArrived(agent));


        Managers.ObjectPooling.DeactivatePatient(gameObject);
        
    }

    private void AddInpatientWaypoints()
    {
        waypoints.Add(bedWaypoint);
        waypoints[0].isEmpty = true;
        waypoints.Add(bedWaypoint.toilet);
        waypoints.Add(waypointsTransform.Find("SofaWaypoint").gameObject.GetComponent<Waypoint>());
        waypoints.Add(waypointsTransform.Find("CounterWaypoint (0)").gameObject.GetComponent<Waypoint>());
        waypoints.Add(waypointsTransform.Find("CounterWaypoint (1)").gameObject.GetComponent<Waypoint>());
    }
    public IEnumerator HospitalizationTimeCounter()
    {
        yield return new WaitForSeconds(Random.Range(20f, 100f));

        StartCoroutine(ExitHospital());
    }
    public IEnumerator QuarantineTimeCounter()
    {
        yield return new WaitForSeconds(70);
        if(isWaiting)
        {
            StopCoroutine(QuarantineMove());
        }
        isQuarantined = false;
        Managers.NPCManager.PlayWakeUpAnimation(animator);
        yield return new WaitForSeconds(5.0f);
        AutoDoorWaypoint[] inFrontOfAutoDoor = nPRoom.transform.GetComponentsInChildren<AutoDoorWaypoint>();
        agent.SetDestination(inFrontOfAutoDoor[1].GetMiddlePointInRange());
        yield return new WaitUntil(() => Managers.NPCManager.isArrived(agent));
        inFrontOfAutoDoor[0].quarantineRoom.GetComponent<Animator>().SetBool("IsOpened", true);
        yield return new WaitForSeconds(1.0f);

        agent.SetDestination(inFrontOfAutoDoor[0].GetMiddlePointInRange());
        yield return new WaitUntil(() => Managers.NPCManager.isArrived(agent));
        inFrontOfAutoDoor[0].quarantineRoom.GetComponent<Animator>().SetBool("IsOpened", false);
        agent.SetDestination(waypoints[0].GetSampledPosition());

        nPRoom.isEmpty = true;
        nPRoom = null;
        isWaiting = false;
        StartCoroutine(ExitHospital());
    }
}