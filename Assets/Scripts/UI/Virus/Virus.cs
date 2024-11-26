using UnityEngine;

public class Virus : MonoBehaviour
{    
    public float infectionProbability = 0.1f;  //감염 확률(50%)

    private float lifetime = 5;              //병원체 기본 수명 5초
    private InfectionStatus infectionState;   //감염 상태

    public static float virusLifetime = 5f;                    //병원체 수명
    public static float virusDropProbability = 0.05f;          //병원체가 생성될 확률
    public static float checkInterval = 1f;                  //감염 체크 간격
    public static int currentGameLevel_1 = 1;

    // 병원체의 수명 관리
    public void SetLifetime(float time)
    {
        lifetime = time;
        Destroy(gameObject, lifetime);  //lifetime 후에 병원체 게임 오브젝트 파괴
    }

    // 병원체 감염 상태
    public void SetInfectionState(InfectionStatus State)
    {
        infectionState = State;
    }

    //병원체가 사람과 충돌할 때
    void OnTriggerEnter(Collider other)
    {
        Person person = other.GetComponent<Person>();  //충돌한 오브젝트 확인
        if (person != null && person.infectionStatus == InfectionStatus.Normal)
        {
            int random = Random.Range(0, 101);
            //감염되는 사람의 감염 저항성을 고려하여 감염 확률 계산
            if (random + InfectionController.GetFinalProtectionRate(person) <= Managers.Infection.infectionProbability)
            {
                Debug.Log(random + " || " + InfectionController.GetFinalProtectionRate(person) + "||" + Managers.Infection.infectionProbability);
                //other.GetComponent<NPCController>().wardComponent.infectedNPC++;
                person.ChangeStatus(infectionState);
            }
        }
    }

    //소독 기능 
    public void Disinfect()
    {
        Destroy(gameObject);
        //Debug.Log("병원체 하나를 소독했습니다.");  
    }
}