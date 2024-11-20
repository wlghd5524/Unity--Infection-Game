using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TutorialButtonAutoPress : TutorialBase
{
    [SerializeField] private Image targetImage; // 목표로 하는 Image
    [SerializeField] private Canvas targetCanvas; // 목표 버튼이 포함된 Canvas
    [SerializeField] private int elevatedSortingOrder = 10; // 튜토리얼 동안 사용할 높은 sortingOrder 값

    private int originalSortingOrder; // 원래 sortingOrder 값을 저장

    public override void Enter()
    {
        if (targetCanvas != null)
        {
            // 원래 sortingOrder 값을 저장하고 높은 값으로 설정
            originalSortingOrder = targetCanvas.sortingOrder;
            targetCanvas.sortingOrder = elevatedSortingOrder;
        }

        if (targetImage != null)
        {
            // RaycastTarget이 켜져있는지 확인하고 클릭 가능하게 만듦
            if (targetImage.raycastTarget)
            {
                // 자동으로 클릭하는 효과를 만들기 위해 직접적으로 클릭 이벤트를 처리
                SimulateClickOnImage();
            }
        }
        else
        {
            Debug.LogError("Target Image is not assigned in the Inspector.");
        }
    }

    public override void Execute(TutorialController controller)
    {
        // 다음 튜토리얼로 넘어가는 작업
        controller.SetNextTutorial();
    }

    public override void Exit()
    {
        // 필요시 종료 시 추가 로직 작성 가능
        // 원래 sortingOrder 값으로 되돌리기
        if (targetCanvas != null)
        {
            targetCanvas.sortingOrder = originalSortingOrder;
        }
    }

    private void SimulateClickOnImage()
    {
        // 클릭 효과를 시뮬레이션
        ExecuteEvents.Execute(targetImage.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);

        // 클릭 후 다음 튜토리얼로 진행
        Execute(FindObjectOfType<TutorialController>());
    }
}
