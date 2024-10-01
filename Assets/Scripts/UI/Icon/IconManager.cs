using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class IconManager : MonoBehaviour
{
    public GameObject iconCanvas;
    public Sprite smileSprite;
    public Sprite[] symptomSprites;

    private PatientController patientController;
    private Coroutine symptomCoroutine;
    private Person person;
    private Image infectionIcon;

    private void Awake()
    {
        if (iconCanvas != null)
        {
            iconCanvas.SetActive(false);

            Transform infectionIconTransform = iconCanvas.transform.Find("InfectionIcon");
            if (infectionIconTransform != null)
            {
                infectionIcon = infectionIconTransform.GetComponent<Image>();
                if (infectionIcon == null)
                {
                    Debug.LogError("InfectionIcon 오브젝트에서 Image 컴포넌트를 찾을 수 없습니다.");
                }
            }
        }

        person = GetComponent<Person>();
        if (person == null)
        {
            Debug.LogError("Person component not found on the GameObject.");
        }
        else
        {
            person.OnInfectionStateChanged += HandleInfectionStateChanged; // 이벤트 구독
        }

        infectionIcon.sprite = smileSprite;

    }

    private void OnDestroy()
    {
        if (person != null)
        {
            person.OnInfectionStateChanged -= HandleInfectionStateChanged; // 이벤트 구독 해제
        }
    }

    private void HandleInfectionStateChanged(InfectionState newStatus)
    {
        if ((newStatus == InfectionState.CRE || newStatus == InfectionState.Covid) && (person.role == Role.Outpatient || person.role == Role.Inpatient))
        {
            if (symptomCoroutine != null)
            {
                StopCoroutine(symptomCoroutine);
            }
            symptomCoroutine = StartCoroutine(ShowRandomSymptomIcon());
            Debug.Log("증상 발현!");
            if (Random.Range(0, 100) <= 30)
            {
                Debug.Log("증상 발견!");
                QuarantineManager targetQuarantineManager = person.gameObject.GetComponent<QuarantineManager>();

                if (person.gameObject.CompareTag("Outpatient") || person.gameObject.CompareTag("Inpatient") || person.gameObject.CompareTag("EmergencyPatient"))
                {
                    PatientController patientController = person.gameObject.GetComponent<PatientController>();
                    Transform parentTransform = Managers.NPCManager.waypointDictionary[(9, "NurseWaypoints")];
                    for (int i = 0; i < 14; i++)
                    {
                        NPRoom nPRoom = parentTransform.Find("N-PRoom (" + i + ")").GetComponent<NPRoom>(); // 음압실 웨이포인트 찾기
                        if (nPRoom.isEmpty)
                        {
                            patientController.nPRoom = nPRoom;
                            nPRoom.isEmpty = false;

                            break;
                        }
                    }
                    // 격리실이 남아있지 않을 때
                    if (patientController.nPRoom == null)
                    {
                        patientController.StartCoroutine(patientController.ExitHospital());
                    }
                    else
                    {
                        GameObject closestNurse = targetQuarantineManager.SearchNurse(person.gameObject.transform.position);
                        try
                        {
                            if (patientController.isFollowingNurse || patientController.isQuarantined || patientController.isWaitingForNurse || patientController.isExiting || patientController.isWaitingForDoctor)
                            {
                                Debug.Log("이미 격리중인 환자입니다.");
                            }
                            else if (closestNurse == null)
                            {
                                Debug.Log("근처에 간호사가 없습니다.");
                                return;
                            }
                            else
                            {
                                // 음압실 데려가기
                                targetQuarantineManager.Quarantine(closestNurse);
                                Debug.Log("증상 발견으로 인한 격리 조치 중!");
                            }
                        }
                        catch (System.Exception ex)
                        {
                            Debug.LogError($"예외 발생: {ex.Message}");
                        }
                    }
                }
            }
            else
            {
                Debug.Log("증상 미발견...");
            }

        }
        else
        {
            if (symptomCoroutine != null)
            {
                StopCoroutine(symptomCoroutine);
                symptomCoroutine = null;
                iconCanvas.gameObject.SetActive(false); // Canvas 비활성화
            }
        }
    }

    private void Start()
    {
        patientController = GetComponent<PatientController>();
        if (patientController == null)
        {
            Debug.LogError("OutpatientController를 찾을 수 없습니다.");
        }
    }

    public void IsIcon()
    {
        StartCoroutine(ShowIcon());
    }

    private IEnumerator ShowIcon()
    {
        if (iconCanvas != null)
        {
            iconCanvas.SetActive(true);
            yield return new WaitForSeconds(2f);
            iconCanvas.SetActive(false);
        }
    }

    private IEnumerator ShowRandomSymptomIcon()
    {
        while ((person.status == InfectionState.CRE || person.status == InfectionState.Covid) && person.role == Role.Outpatient)
        {
            int randomIndex = Random.Range(0, symptomSprites.Length);

            infectionIcon.sprite = symptomSprites[randomIndex];
            iconCanvas.gameObject.SetActive(true); // Canvas 활성화

            yield return new WaitForSeconds(2); // 아이콘을 2초 동안 표시

            iconCanvas.gameObject.SetActive(false); // Canvas 비활성화

            yield return new WaitForSeconds(5); // 2초 대기
        }
    }

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