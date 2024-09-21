using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    public Camera mainCamera; // 메인 카메라
    public CameraHandler cameraHandler; // 카메라 핸들러 스크립트

    [System.Serializable]
    public struct FloorView
    {
        public Transform topViewTransform; // 탑뷰 트랜스폼
        public Transform quarterViewTransform; // 쿼터뷰 트랜스폼
    }

    public FloorView[] floorViews; // 각 층의 뷰 설정
    private int currentFloorIndex = 0; // 현재 선택된 층 인덱스
    private bool isTopView = true; // 현재 뷰 모드 (탑뷰 여부)

    void Start()
    {
        if (cameraHandler == null)  // CameraHandler가 할당 안되어있으면
        {
            Debug.LogError("CameraHandler is not assigned!");
            return;
        }

        SwitchFloor(8); // 처음 층 Loof
        SetView(); // 초기 뷰 설정
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            ToggleView(); // 'T' 키를 눌러 뷰 모드 토글
        }
    }

    // 층 전환
    public void SwitchFloor(int floorIndex)
    {
        if (floorIndex < 0 || floorIndex >= floorViews.Length)  // 유효한 층 Index가 아니라면
        {
            Debug.LogError("Invalid floor index");
            return;
        }

        currentFloorIndex = floorIndex; // 현재 층으로 설정
        SetView(); // 새로운 층에 대한 뷰 설정
    }

    // 뷰 모드 토글 (탑뷰 <-> 쿼터뷰)
    public void ToggleView()
    {
        isTopView = !isTopView;
        SetView(); // 뷰 모드 변경
    }

    // 현재 뷰 설정
    void SetView()
    {
        if (isTopView)
        {
            SetTopView(); // 탑뷰 설정
            cameraHandler.SetViewMode(CameraHandler.ViewMode.TopView); // 카메라 핸들러에 탑뷰 모드 설정
        }
        else
        {
            SetQuarterView(); // 쿼터뷰 설정
            cameraHandler.SetViewMode(CameraHandler.ViewMode.DiagonalView); // 카메라 핸들러에 쿼터뷰 모드 설정
        }

        // 중심 위치 업데이트
        Vector3 newCenterPosition = isTopView ? floorViews[currentFloorIndex].topViewTransform.position : floorViews[currentFloorIndex].quarterViewTransform.position;
        cameraHandler.SetCenterPosition(newCenterPosition); // 카메라 핸들러에 중심 위치 설정
    }

    // 탑뷰 설정
    void SetTopView()
    {
        mainCamera.transform.position = floorViews[currentFloorIndex].topViewTransform.position; // 메인 카메라의 위치를 탑뷰 위치로 설정
        mainCamera.transform.rotation = floorViews[currentFloorIndex].topViewTransform.rotation; // 메인 카메라의 회전을 탑뷰 회전으로 설정
    }

    // 쿼터뷰 설정
    void SetQuarterView()
    {
        mainCamera.transform.position = floorViews[currentFloorIndex].quarterViewTransform.position; // 메인 카메라의 위치를 쿼터뷰 위치로 설정
        mainCamera.transform.rotation = floorViews[currentFloorIndex].quarterViewTransform.rotation; // 메인 카메라의 회전을 쿼터뷰 회전으로 설정
    }
}
