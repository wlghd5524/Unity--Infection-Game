using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DividerCollisionHandler : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Outpatient 태그를 가진 오브젝트와 충돌했는지 확인
        if (other.CompareTag("Outpatient") || other.CompareTag("Inpatient"))
        {
            PatientController Outpatient = other.GetComponent<PatientController>();

            if (Outpatient != null)
            {
                if (Outpatient.isFollowingNurse == false)
                {
                    int waypointIndex = Outpatient.waypointIndex;

                    // LayerChangeManager의 ChangeLayerBasedOnCollider 메서드 호출
                    Managers.LayerChanger.ChangeLayerBasedOnCollider(other.gameObject, gameObject, waypointIndex);
                }
                else
                {
                    Managers.LayerChanger.ChangeLayerBasedOnCollider(other.gameObject, gameObject, 4);
                }
            }
        }
        else
        {
            Debug.LogWarning($"Colliding object {other.gameObject.name} does not have an OutpatientController component.");
        }

        // Nurse 태그를 가진 오브젝트와 충돌했는지 확인
        if (other.CompareTag("Nurse"))
        {
            NurseController Nurse = other.GetComponent<NurseController>();

            if (Nurse != null)
            {
                if (Nurse.isWorking == false)
                {
                    Managers.LayerChanger.ChangeLayerBasedOnCollider(other.gameObject, gameObject, 1);
                }
                else
                {
                    Managers.LayerChanger.ChangeLayerBasedOnCollider(other.gameObject, gameObject, 4);
                }

            }
            else
            {
                Debug.LogWarning($"Colliding object {other.gameObject.name} does not have an OutpatientController component.");
            }
        }
    }
}
