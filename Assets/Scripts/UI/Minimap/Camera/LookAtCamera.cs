using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    private Camera mainCamera;
    private CameraViewManager.CameraViewMode currentViewMode;
    private CameraHandler cameraHandler; // CameraHandler 참조 변수

    void Start()
    {
        mainCamera = Camera.main; // 메인 카메라를 찾아서 참조합니다.
        if (mainCamera != null)
        {
            cameraHandler = mainCamera.GetComponent<CameraHandler>(); // CameraHandler 참조 가져오기
        }

        if (cameraHandler != null)
        {
            currentViewMode = (CameraViewManager.CameraViewMode)cameraHandler.currentViewMode; // CameraHandler의 currentViewMode로 초기화
        }
        else
        {
            Debug.LogError("CameraHandler component not found on the main camera.");
        }

        AdjustUIRotation(); // 초기 회전 설정
    }

    void Update()
    {
        // GameManager에서 현재 뷰 모드 가져오기
        currentViewMode = CameraViewManager.Instance.currentViewMode;

        // UI 회전 조정
        AdjustUIRotation();
    }

    void AdjustUIRotation()
    {
        Vector3 direction = (mainCamera.transform.position - transform.position).normalized;

        if (currentViewMode == CameraViewManager.CameraViewMode.TopView)
        {
            direction.x = 0; // X축 방향을 무시합니다.
            if (direction != Vector3.zero)
            {
                Quaternion rotation = Quaternion.LookRotation(direction);

                // 오브젝트가 뒤집히지 않도록 회전값을 조정
                Vector3 eulerAngles = rotation.eulerAngles;
                eulerAngles.y = 0; // X축 회전을 고정
                eulerAngles.z = 0; // Z축 회전을 고정

                transform.rotation = Quaternion.Euler(eulerAngles);
            }
        }
        else if (currentViewMode == CameraViewManager.CameraViewMode.QuarterView)
        {
            Quaternion rotation = Quaternion.LookRotation(direction);
            transform.rotation = rotation;
        }
    }
}
