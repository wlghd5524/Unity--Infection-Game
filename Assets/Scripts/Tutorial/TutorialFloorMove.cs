using UnityEngine;
using System.Collections;

public class TutorialFloorMove : TutorialBase
{
    private CameraController cameraController;
    private bool isFloorChanged = false;

    [SerializeField] private Canvas targetCanvas;  // 목표 버튼이 포함된 Canvas
    [SerializeField] private int elevatedSortingOrder = 10; // 튜토리얼 동안 사용할 높은 sortingOrder 값

    private int originalSortingOrder; // 원래 sortingOrder 값을 저장

    public override void Enter()
    {
        // CameraController 인스턴스를 찾음
        cameraController = FindObjectOfType<CameraController>();
        if (cameraController == null)
        {
            Debug.LogError("CameraController not found!");
            return;
        }

        Debug.Log("Wait for the floor index to change to 2.");

        // Canvas의 원래 sortingOrder 값을 저장하고 높은 값으로 설정
        if (targetCanvas != null)
        {
            originalSortingOrder = targetCanvas.sortingOrder;
            targetCanvas.sortingOrder = elevatedSortingOrder;
        }
    }

    public override void Execute(TutorialController controller)
    {
        // floorIndex가 2로 바뀌는지 확인
        if (cameraController.currentFloorIndex == 2 && !isFloorChanged)
        {
            isFloorChanged = true;

            cameraController.StartCoroutine(controller.Delay());
        }
    }

    public override void Exit()
    {
        // 원래 sortingOrder 값으로 되돌리기
        if (targetCanvas != null)
        {
            targetCanvas.sortingOrder = originalSortingOrder;
        }

        // 상태 초기화
        isFloorChanged = false;
    }
}
