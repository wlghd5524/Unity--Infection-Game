using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class TutorialProfileClick : TutorialBase
{
    [SerializeField] private Canvas targetCanvas;  // 클릭 대상이 포함된 Canvas
    [SerializeField] private int elevatedSortingOrder = 10; // 튜토리얼 동안 사용할 높은 sortingOrder 값

    private bool isClicked = false;
    private int originalSortingOrder; // 원래 sortingOrder 값을 저장

    public override void Enter()
    {
        // Canvas의 원래 sortingOrder 값을 저장하고 높은 값으로 설정
        if (targetCanvas != null)
        {
            originalSortingOrder = targetCanvas.sortingOrder;
            targetCanvas.sortingOrder = elevatedSortingOrder;
        }

        Debug.Log("Click on any profile to proceed.");
    }

    public override void Execute(TutorialController controller)
    {
        // 마우스 클릭을 감지하여 EventSystem을 사용해 UI 클릭 확인
        if (Input.GetMouseButtonDown(0))
        {
            // UI 클릭 감지
            if (EventSystem.current.IsPointerOverGameObject())
            {
                // 클릭된 UI 오브젝트 감지
                PointerEventData pointerData = new PointerEventData(EventSystem.current)
                {
                    position = Input.mousePosition
                };

                // RaycastAll을 사용하여 모든 UI 오브젝트 검사
                var results = new System.Collections.Generic.List<RaycastResult>();
                EventSystem.current.RaycastAll(pointerData, results);

                foreach (var result in results)
                {
                    // 클릭된 오브젝트에서 ProfileClickHandler 또는 부모 오브젝트에서 ProfileClickHandler 찾기
                    ProfileClickHandler profileClickHandler = result.gameObject.GetComponentInParent<ProfileClickHandler>();
                    if (profileClickHandler != null && !isClicked)
                    {
                        Debug.Log($"Profile clicked: {profileClickHandler.personID}");
                        isClicked = true;
                        StartCoroutine(controller.Delay());  // 클릭 후 딜레이 처리
                    }
                    else
                    {
                        Debug.Log("Clicked object does not have ProfileClickHandler.");
                    }
                }
            }
        }
    }

    public override void Exit()
    {
        // 원래 sortingOrder 값으로 되돌리기
        if (targetCanvas != null)
        {
            targetCanvas.sortingOrder = originalSortingOrder;
        }
    }
}
