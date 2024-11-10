using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public enum StandingState
{
    Standing,
    Sitting,
    LayingDown
}

public class NPCController : MonoBehaviour
{
    public Animator animator;
    public StandingState standingState = StandingState.Standing;
    public NavMeshAgent agent;
    public List<Waypoint> waypoints = new List<Waypoint>();
    public Ward wardComponent;
    public int ward;

    public bool isWaiting = false;

    public Person personComponent;
    public InfectionController infectionController;

    public int num;

    private void Awake()
    {
        // 컴포넌트 초기화
        animator = GetComponent<Animator>();
        infectionController = GetComponent<InfectionController>();
        agent = GetComponent<NavMeshAgent>();
        agent.avoidancePriority = Random.Range(0, 100);
        agent.speed = Random.Range(3.0f, 5.0f);
        personComponent = GetComponent<Person>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (standingState != StandingState.Standing)
        {
            return;
        }
        if (other.gameObject.layer == gameObject.layer)
        {
            return;
        }
        if (!other.CompareTag("Floor"))
        {
            return;
        }
        gameObject.layer = other.gameObject.layer;
        // 자식 오브젝트도 레이어 변경
        foreach (Transform child in gameObject.GetComponentsInChildren<Transform>(true))
        {
            child.gameObject.layer = other.gameObject.layer;
        }
    }
}
