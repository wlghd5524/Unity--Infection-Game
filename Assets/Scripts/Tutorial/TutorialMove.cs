using UnityEngine;

public class TutorialMove : TutorialBase
{
    private bool movedUp = false;
    private bool movedDown = false;
    private bool movedLeft = false;
    private bool movedRight = false;

    public float moveDistance = 5.0f; // 이동해야 할 거리
    private Vector3 initialPosition;

    private CameraHandler cameraHandler;

    public override void Enter()
    {
        // 카메라 핸들러를 찾음
        cameraHandler = FindObjectOfType<CameraHandler>();
        if (cameraHandler == null)
        {
            Debug.LogError("CameraHandler not found!");
            return;
        }

        // 상태 초기화
        movedUp = false;
        movedDown = false;
        movedLeft = false;
        movedRight = false;

        // 카메라 초기 위치 저장
        initialPosition = cameraHandler.mainCamera.transform.position;

        // 튜토리얼 시작 안내
        Debug.Log("Move the camera in all directions (W, A, S, D) by " + moveDistance + " units to continue.");
    }

    public override void Execute(TutorialController controller)
    {
        if (cameraHandler == null) return;

        Vector3 currentPosition = cameraHandler.mainCamera.transform.position;
        Vector3 distanceMoved = currentPosition - initialPosition;

        // 상하좌우 이동 감지 - 설정한 이동 거리 이상 이동했을 때만 true
        if (!movedUp && distanceMoved.z <= -moveDistance)
        {
            movedUp = true;
            Debug.Log("Moved up enough.");
        }
        if (!movedDown && distanceMoved.z >= moveDistance)
        {
            movedDown = true;
            Debug.Log("Moved down enough.");
        }
        if (!movedLeft && distanceMoved.x >= moveDistance)
        {
            movedLeft = true;
            Debug.Log("Moved left enough.");
        }
        if (!movedRight && distanceMoved.x <= -moveDistance)
        {
            movedRight = true;
            Debug.Log("Moved right enough.");
        }

        // 모든 방향으로 설정된 거리만큼 이동했을 때 다음 튜토리얼로 이동
        if (movedUp && movedDown && movedLeft && movedRight)
        {
            Debug.Log("All directions moved enough. Proceeding to the next tutorial.");
            controller.SetNextTutorial();
        }
    }

    public override void Exit()
    {
        // 튜토리얼 종료 시의 로직을 여기에 추가할 수 있습니다.
    }
}
