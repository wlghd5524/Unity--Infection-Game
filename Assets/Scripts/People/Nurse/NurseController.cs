using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
public enum NurseRole
{
    ER,
    Ward,
    InpateintWard
}
public class NurseController : NPCController
{
    public NurseRole role;
    public bool isWorking = false; // 간호사가 일하는 중인지 여부
    public bool isRest = false;
    public bool isWaitingAtDoctorOffice = false;
    public DoctorController doctor;

    //public GameObject targetPatient; // 타겟 환자
    public GameObject chair;

    // Start는 첫 프레임 업데이트 전에 호출됩니다.
    void Start()
    {

    }

    // Update는 매 프레임 호출됩니다.
    void Update()
    {

        // 애니메이션 업데이트
        Managers.NPCManager.UpdateAnimation(agent, animator);

        if (isWaiting || isRest)
        {
            return; // 기다리는 중이면 리턴
        }

        if (Managers.NPCManager.isArrived(agent))
        {
            if (role == NurseRole.Ward)
            {
                if (isWorking)
                    return;


                if (!isWorking)
                {

                    StartCoroutine(WardNurseMove()); // 다음 작업을 위해 대기 후 이동


                }

            }
            else if (role == NurseRole.ER)
            {
                StartCoroutine(ERNurseMove());
            }
            else if (role == NurseRole.InpateintWard)
            {
                StartCoroutine(InpatientWardNurseMove());
            }
        }


    }

    // 환자에게 이동
    public IEnumerator GoToPatient(GameObject patientGameObject)
    {
        PatientController targetPatientController = patientGameObject.GetComponent<PatientController>();
        isWorking = true; // 일하는 중으로 설정
        Managers.NPCManager.PlayWakeUpAnimation(animator);
        yield return new WaitForSeconds(1.0f);
        Vector3 targetPatientPosition = Managers.NPCManager.GetPositionInFront(transform, patientGameObject.transform, 0.5f); // 환자 앞의 임의 위치 계산
        agent.SetDestination(targetPatientPosition); // 에이전트 목적지 설정
        //targetPatient = patientGameObject; // 타겟 환자 설정

        yield return new WaitUntil(() => !agent.pathPending);
        yield return new WaitUntil(() => agent.remainingDistance == 0 && agent.speed == 0);

        if (targetPatientController.animator.GetBool("Sleeping"))
        {
            Managers.NPCManager.PlayWakeUpAnimation(targetPatientController.animator);
            yield return new WaitForSeconds(5.0f);
        }

        Managers.NPCManager.FaceEachOther(gameObject, patientGameObject); // 간호사와 환자가 서로를 바라보게 설정

        targetPatientController.nurseSignal = true; // 환자에게 간호사가 도착했음을 알림
                                                    //targetPatientController.nurse = gameObject; // 간호사 설정

    }

    // 음압실로 이동
    public IEnumerator GoToNegativePressureRoom(GameObject patientGameObject)
    {
        PatientController targetPatientController = patientGameObject.GetComponent<PatientController>();
        isWorking = true; // 일하는 중으로 설정
        Managers.NPCManager.PlayWakeUpAnimation(animator);
        yield return new WaitForSeconds(1.0f);
        Vector3 targetPatientPosition = Managers.NPCManager.GetPositionInFront(transform, patientGameObject.transform, 0.5f); // 환자 앞의 임의 위치 계산
        agent.SetDestination(targetPatientPosition); // 에이전트 목적지 설정
        //targetPatient = patientGameObject; // 타겟 환자 설정

        yield return new WaitUntil(() => !agent.pathPending);
        yield return new WaitUntil(() => agent.remainingDistance == 0 && agent.speed == 0);

        if (targetPatientController.animator.GetBool("Sleeping"))
        {
            Managers.NPCManager.PlayWakeUpAnimation(targetPatientController.animator);
            yield return new WaitForSeconds(5.0f);
        }

        Managers.NPCManager.FaceEachOther(gameObject, patientGameObject); // 간호사와 환자가 서로를 바라보게 설정

        targetPatientController.nurseSignal = true; // 환자에게 간호사가 도착했음을 알림
        //targetPatientController.nurse = gameObject; // 간호사 설정
        agent.speed = targetPatientController.agent.speed - 0.5f;
        targetPatientController.StartCoroutine(targetPatientController.FollowNurse(gameObject));
        Transform parentTransform = GameObject.Find("Waypoints").transform;

        for (int i = 0; i < 14; i++)
        {
            NPRoom nPRoom = parentTransform.Find("N-PRoom (" + i + ")").GetComponent<NPRoom>(); // 음압실 웨이포인트 찾기
            if (nPRoom.isEmpty)
            {
                targetPatientController.nPRoom = nPRoom;
                nPRoom.isEmpty = false;
                agent.SetDestination(nPRoom.GetRandomPointInRange()); // 음압실로 이동
                break;
            }
        }
        // 격리실이 남아있지 않을 때
        if (targetPatientController.nPRoom == null)
        {
            targetPatientController.StartCoroutine(targetPatientController.ExitHospital());
        }
        else
        {
            yield return new WaitUntil(() => Managers.NPCManager.isArrived(agent));
            targetPatientController.StartCoroutine(targetPatientController.WaitForQuarantine());
            Managers.NPCManager.FaceEachOther(gameObject, targetPatientController.gameObject);
            yield return new WaitForSeconds(3);
            agent.speed += 0.5f;
            isWorking = false;
            targetPatientController.isFollowingNurse = false;
            targetPatientController.isQuarantined = true;
            targetPatientController.ward = -1;
            targetPatientController.wardComponent = null;
        }
    }


    // 대기 후 랜덤 웨이포인트로 이동 코루틴
    public IEnumerator WardNurseMove()
    {
        isWaiting = true; // 기다리는 중으로 설정
        yield return new WaitForSeconds(1.5f);
        if (isWorking)
        {
            isWaiting = false;
            yield break;
        }
        if (waypoints.Count > 0)
        {
            int roleNum = num % 16;
            if (8 <= roleNum && roleNum <= 13)  //진료실 앞 대기 간호사들
            {
                for (int i = 4; i <= 13; i++)
                {
                    if (waypoints[i] is NurseWaitingPoint nurseWaitingPoint && nurseWaitingPoint.doctorOffice.doctor != null)
                    {
                        DoctorController doctorController = nurseWaitingPoint.doctorOffice.doctor.GetComponent<DoctorController>();
                        if (nurseWaitingPoint.isEmpty && !doctorController.isResting && isWaitingAtDoctorOffice == false)
                        {
                            doctorController.nurse = gameObject;
                            nurseWaitingPoint.isEmpty = false;
                            isWaitingAtDoctorOffice = true;
                            agent.SetDestination(waypoints[i].GetMiddlePointInRange());
                            //yield return new WaitUntil(() => Managers.NPCManager.isFindPath(agent));
                            yield return new WaitUntil(() => Managers.NPCManager.isArrived(agent));
                            transform.eulerAngles = nurseWaitingPoint.doctorOffice.doctor.transform.eulerAngles;
                        }
                        if (doctorController.nurse != null)
                        {
                            NurseController nurseController = doctorController.nurse.GetComponent<NurseController>();
                            if (!doctorController.waypoints[1].isEmpty)
                            {
                                yield return new WaitForSeconds(3);
                                nurseController.agent.SetDestination(doctorController.nurse.GetComponent<NurseController>().waypoints[i + 10].GetMiddlePointInRange());
                                //yield return new WaitUntil(() => Managers.NPCManager.isFindPath(agent));
                                yield return new WaitUntil(() => Managers.NPCManager.isArrived(agent));
                                nurseController.gameObject.transform.LookAt(doctorController.gameObject.transform);
                            }
                            else
                            {
                                nurseController.agent.SetDestination(doctorController.nurse.GetComponent<NurseController>().waypoints[i].GetMiddlePointInRange());
                                //yield return new WaitUntil(() => Managers.NPCManager.isFindPath(agent));
                                yield return new WaitUntil(() => Managers.NPCManager.isArrived(agent));

                                nurseController.gameObject.transform.eulerAngles = nurseWaitingPoint.doctorOffice.doctor.GetComponent<DoctorController>().chair.transform.eulerAngles;

                            }
                        }

                    }
                }
            }
            else if (0 <= roleNum && roleNum <= 7)  //카운터에 있는 간호사들
            {
                if (animator.GetBool("Sitting"))
                {
                    isWaiting = false;
                    yield break;
                }
                if (chair.transform.parent.parent.parent.parent.eulerAngles == new Vector3(0, 0, 0))
                {
                    agent.SetDestination(new Vector3(chair.transform.position.x, chair.transform.position.y, chair.transform.position.z - 0.5f));
                }
                else
                {
                    agent.SetDestination(new Vector3(chair.transform.position.x, chair.transform.position.y, chair.transform.position.z + 0.5f));
                }
                yield return new WaitUntil(() => Managers.NPCManager.isArrived(agent));
                if (chair.transform.parent.parent.parent.parent.eulerAngles == new Vector3(0, 0, 0))
                {
                    transform.eulerAngles = new Vector3(0, 180, 0);
                }
                else
                {
                    transform.eulerAngles = new Vector3(0, 0, 0);
                }
                Managers.NPCManager.PlaySittingAnimation(animator);
            }

            else
            {
                agent.SetDestination(waypoints[Random.Range(24, 27)].GetRandomPointInRange()); // 랜덤 웨이포인트로 이동
            }
        }
        isWaiting = false; // 기다리는 중 해제
    }

    public IEnumerator InpatientWardNurseMove()
    {
        isWaiting = true;
        yield return new WaitForSeconds(2.0f);
        int roleNum = num % 12;
        if (0 <= roleNum && roleNum <= 7)
        {
            if (animator.GetBool("Sitting"))
            {
                isWaiting = false;
                yield break;
            }
            if (chair.transform.parent.parent.parent.eulerAngles == new Vector3(0, 0, 0))
            {
                agent.SetDestination(new Vector3(chair.transform.position.x, chair.transform.position.y, chair.transform.position.z - 0.5f));
            }
            else
            {
                agent.SetDestination(new Vector3(chair.transform.position.x, chair.transform.position.y, chair.transform.position.z + 0.5f));
            }
            yield return new WaitUntil(() => Managers.NPCManager.isArrived(agent));
            if (chair.transform.parent.parent.parent.eulerAngles == new Vector3(0, 0, 0))
            {
                transform.eulerAngles = new Vector3(0, 180, 0);
            }
            else
            {
                transform.eulerAngles = new Vector3(0, 0, 0);
            }
            Managers.NPCManager.PlaySittingAnimation(animator);
        }
        else if (8 <= roleNum && roleNum <= 11)
        {
            int random = Random.Range(4, waypoints.Count);
            if (!waypoints[random].isEmpty && waypoints[random] is BedWaypoint bed && bed.patient != null)
            {
                PatientController targetInpatientController = bed.patient.GetComponent<PatientController>();
                targetInpatientController.StartCoroutine(targetInpatientController.WaitForNurse());
                agent.SetDestination(bed.transform.position);
                yield return new WaitUntil(() => Managers.NPCManager.isArrived(agent));
                //Managers.NPCManager.FaceEachOther(bed.patient, gameObject);
                if (bed.patient == null)
                {
                    isWaiting = false;
                    yield break;
                }
                transform.LookAt(bed.patient.transform);
                if (Random.Range(0, 101) <= 50)
                {
                    if (targetInpatientController.isLayingDown)
                    {
                        Managers.NPCManager.WakeUpAndSittingAndTalking(targetInpatientController.animator);
                        yield return new WaitForSeconds(4.0f);
                        Managers.NPCManager.PlayLayDownAnimation(targetInpatientController.animator);
                    }

                }
                else
                {
                    yield return new WaitForSeconds(2.0f);
                }
                targetInpatientController.nurseSignal = true;
            }
            else
            {
                agent.SetDestination(waypoints[Random.Range(4, 8)].GetRandomPointInRange());
            }
        }

        isWaiting = false;
    }


    public IEnumerator ERNurseMove()
    {
        isWaiting = true;
        yield return new WaitForSeconds(2.0f);
        if (isWorking)
        {
            isWaiting = false;
            yield break;
        }
        if (waypoints.Count > 0)
        {
            if (0 <= num && num <= 5) //중앙 카운터 간호사들
            {
                if (chair.transform.parent.parent.eulerAngles == new Vector3(0, 0, 0))
                {
                    agent.SetDestination(new Vector3(chair.transform.position.x, chair.transform.position.y, chair.transform.position.z - 0.5f));
                }
                else
                {
                    agent.SetDestination(new Vector3(chair.transform.position.x, chair.transform.position.y, chair.transform.position.z + 0.5f));
                }
                yield return new WaitUntil(() => Managers.NPCManager.isArrived(agent));

                if (chair.transform.parent.parent.eulerAngles == new Vector3(0, 0, 0))
                {
                    transform.eulerAngles = new Vector3(0, 180, 0);
                }
                else
                {
                    transform.eulerAngles = new Vector3(0, 0, 0);
                }
                Managers.NPCManager.PlaySittingAnimation(animator);
                yield return new WaitForSeconds(2.0f);

            }
            else if (6 <= num && num <= 8) //병원 입구 쪽 카운터 간호사들
            {
                agent.SetDestination(new Vector3(chair.transform.position.x, chair.transform.position.y, chair.transform.position.z - 0.5f));
                yield return new WaitUntil(() => Managers.NPCManager.isArrived(agent));
                transform.eulerAngles = new Vector3(0, 180, 0);
                Managers.NPCManager.PlaySittingAnimation(animator);
                yield return new WaitForSeconds(2.0f);
            }
            else //환자 보러다니는 간호사들
            {
                while (doctor.isWorking)
                {
                    agent.SetDestination(doctor.transform.position - doctor.transform.forward * 0.5f);
                    yield return new WaitForSeconds(0.1f);
                }
                int random = Random.Range(0, waypoints.Count);
                if (!waypoints[random].isEmpty && waypoints[random] is BedWaypoint bed)
                {
                    agent.SetDestination(waypoints[random].GetMiddlePointInRange());
                }
                else
                {
                    agent.SetDestination(waypoints[0].GetRandomPointInRange());
                }
                if (doctor.isWorking)
                {
                    yield break;
                }
                yield return new WaitUntil(() => Managers.NPCManager.isArrived(agent));
            }
        }
        isWaiting = false;
    }
}
