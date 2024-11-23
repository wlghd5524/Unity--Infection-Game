using UnityEngine;

public class Virus : MonoBehaviour
{    
    public float infectionProbability = 0.1f;  //감염 확률(50%)

    private float lifetime = 5;              //바이러스 기본 수명 5초
    private InfectionState infectionState;   //감염 상태

    public static float virusLifetime = 5f;                    //바이러스 수명
    public static float virusDropProbability = 0.05f;          //바이러스가 생성될 확률
    public static float checkInterval = 1f;                  //감염 체크 간격
    public static int currentGameLevel_1 = 1;

    // 바이러스의 수명 관리
    public void SetLifetime(float time)
    {
        lifetime = time;
        Destroy(gameObject, lifetime);  //lifetime 후에 바이러스 게임 오브젝트 파괴
    }

    // 바이러스 감염 상태
    public void SetInfectionState(InfectionState State)
    {
        infectionState = State;
    }

    //바이러스가 사람과 충돌할 때
    void OnTriggerEnter(Collider other)
    {
        Person person = other.GetComponent<Person>();  //충돌한 오브젝트 확인
        if (person != null && person.status == InfectionState.Normal)
        {
            if (Random.value < infectionProbability)
            {
                person.ChangeStatus(infectionState);
                //Debug.Log($"바이러스에 의해 감염됨 (상태: {infectionState})");
            }
            //else { Debug.Log($"바이러스에 의해 감염되지 않음 (상태: {infectionState})"); }
        }
    }

    //소독 기능 
    public void Disinfect()
    {
        Destroy(gameObject);
        //Debug.Log("바이러스 하나를 소독했습니다.");  
    }
}