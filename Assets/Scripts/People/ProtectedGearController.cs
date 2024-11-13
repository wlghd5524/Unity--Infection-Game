using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ProtectedGearController : MonoBehaviour
{
    public Animator animator;
    public StandingState standingState = StandingState.Standing;
    public NavMeshAgent agent;
    public GameObject parentObject;
    public SkinnedMeshRenderer meshRenderer;

    // Update is called once per frame
    void FixedUpdate()
    {
        gameObject.transform.rotation = parentObject.transform.rotation;
        // 애니메이션
        Managers.NPCManager.UpdateAnimation(agent, animator);
    }
}
