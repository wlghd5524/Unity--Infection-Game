using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform[] floorPositions; // 각 층의 위치를 나타내는 트랜스폼 배열
    public Camera mainCamera; // 메인 카메라
    public string[] floorLayerNames; // 각 층별 내부 레이어 이름 배열
    public string[] stairsLayerNames; // 각 층별 계단 레이어 이름 배열
    private CameraHandler cameraHandler; // CameraHandler 인스턴스

    // 기본값을 저장할 변수들
    private float defaultZoomSpeed;
    private float defaultEdgeMoveSpeed;
    private float defaultMaxXDistanceFromCenter;
    private float defaultMaxZDistanceFromCenter;

    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main; // mainCamera가 할당되지 않은 경우 기본 메인 카메라로 설정
        }

        cameraHandler = mainCamera.GetComponent<CameraHandler>(); // CameraHandler 컴포넌트 찾기
        if (cameraHandler == null)
        {
            Debug.LogError("CameraHandler not found on the main camera!");
        }

        // 바꿀 CameraHandler의 기본값 저장
        defaultZoomSpeed = cameraHandler.zoomSpeed;
        defaultEdgeMoveSpeed = cameraHandler.edgeMoveSpeed;
        defaultMaxXDistanceFromCenter = cameraHandler.maxXDistanceFromCenter;
        defaultMaxZDistanceFromCenter = cameraHandler.maxZDistanceFromCenter;

        MoveToFloor(8); // 초기 층 옥상(Loof)로 설정
    }

    // 특정 층으로 이동하는 메서드
    public void MoveToFloor(int floorIndex)
    {
        if (floorIndex < 0 || floorIndex >= floorPositions.Length)
        {
            Debug.LogError("Invalid floor index"); // 유효하지 않은 층 인덱스 오류 출력
            return;
        }

        mainCamera.transform.position = floorPositions[floorIndex].position; // 메인 카메라 위치 설정
        mainCamera.transform.rotation = floorPositions[floorIndex].rotation; // 메인 카메라 회전 설정

        // 중심 위치 업데이트
        if (cameraHandler != null)
        {
            cameraHandler.SetCenterPosition(floorPositions[floorIndex].position);
        }

        int layerMask = 0; // 기본 레이어 포함하지 않도록 0으로 초기화

        // 층 레이어 설정
        if (floorIndex < floorLayerNames.Length)
        {
            int floorLayer = LayerMask.NameToLayer(floorLayerNames[floorIndex]);
            if (floorLayer != -1)
            {
                layerMask |= 1 << floorLayer; // 층 레이어 추가
                //Debug.Log($"Setting layer mask for floor {floorIndex + 1}: {floorLayerNames[floorIndex]}");
            }
            else
            {
                //Debug.LogWarning($"Layer not found: {floorLayerNames[floorIndex]}"); // 층 레이어를 찾지 못한 경우 경고 출력
            }
        }

        // 계단의 레이어도 해당 층 내부와 같이 보이게 하는 로직
        // 해당 층의 계단 레이어 추가
        int stairsLayerIndex = floorIndex / 2; // 각 층의 좌측과 우측이 번갈아 나오므로 2로 나눈 몫이 해당 층의 계단 레이어 인덱스가 됩니다.
        if (stairsLayerIndex < stairsLayerNames.Length)
        {
            int stairsLayer = LayerMask.NameToLayer(stairsLayerNames[stairsLayerIndex]);
            if (stairsLayer != -1)
            {
                layerMask |= 1 << stairsLayer; // 계단 레이어 추가
                //Debug.Log($"Stairs layer added to culling mask: {stairsLayerNames[stairsLayerIndex]}");
            }
            else
            {
                //Debug.LogWarning($"Stairs layer not found: {stairsLayerNames[stairsLayerIndex]}"); // 계단 레이어를 찾지 못한 경우 경고 출력
            }
        }


        //Debug.Log($"Camera culling mask set to: {mainCamera.cullingMask}");

        int uiLayer = LayerMask.NameToLayer("Default");
        if (uiLayer != -1)
        {
            layerMask |= 1 << uiLayer;
        }
        else
        {
            Debug.LogWarning("UI layer not found");
        }
        mainCamera.cullingMask = layerMask; // 카메라 컬링 마스크 설정

        ///////////////////////// 옥상 뷰 때 카메라 움직임 설정값 변경하는 곳
        if (cameraHandler != null)
        {
            // 옥상 뷰(floorIndex = 8)일 때만 카메라 핸들러 설정 변경
            if (floorIndex == 8)
            {
                cameraHandler.zoomSpeed = 20.0f;
                cameraHandler.edgeMoveSpeed = 32.0f;
                cameraHandler.maxXDistanceFromCenter = 80.0f;
                cameraHandler.maxZDistanceFromCenter = 80.0f;
                // 추가로 변경할 설정들은 여기에 더 넣기
            }
            else
            {
                // 다른 층일 때 기본 설정 복구
                cameraHandler.zoomSpeed = defaultZoomSpeed;
                cameraHandler.edgeMoveSpeed = defaultEdgeMoveSpeed;
                cameraHandler.maxXDistanceFromCenter = defaultMaxXDistanceFromCenter;
                cameraHandler.maxZDistanceFromCenter = defaultMaxZDistanceFromCenter;
            }
        }
    }
}
