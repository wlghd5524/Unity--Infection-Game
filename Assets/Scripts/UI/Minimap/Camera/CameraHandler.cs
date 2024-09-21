using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraHandler : MonoBehaviour
{
    // 뷰 모드 열거형: TopView(탑뷰), DiagonalView(대각선뷰)
    public enum ViewMode { TopView, DiagonalView }
    public ViewMode currentViewMode = ViewMode.TopView; // 현재 뷰 모드

    // 카메라 이동 관련 설정
    public Camera mainCamera; // Main 카메라

    public float rotationSpeed = 5.0f; // 회전 속도
    public float zoomSpeed = 3.0f; // 줌 속도
    public float minZoomDistance = 2.0f; // 최소 줌 거리
    public float maxZoomDistance = 20.0f; // 최대 줌 거리
    public float moveSpeed = 3.0f; // 카메라 이동 속도
    public float edgeMoveSpeed = 5.0f; // 화면 가장자리에서 카메라 이동 속도
    public float minYPosition = 15.0f; // 카메라의 y축 최소 위치
    public float minFOV = 15.0f; // 최소 줌 (FOV)
    public float maxFOV = 90.0f; // 최대 줌 (FOV)
    public float edgeBoundary = 10.0f; // 가장자리 감지 범위 (픽셀)

    // 거리 제한을 위한 설정
    public Vector3 centerPosition; // 중심 위치
    public float maxXDistanceFromCenter = 50.0f; // 중심으로부터의 최대 X 거리
    public float maxZDistanceFromCenter = 50.0f; // 중심으로부터의 최대 Z 거리

    private Vector3 initialPosition; // 초기 위치
    private Quaternion initialRotation; // 초기 회전

    void Start()
    {
        if (mainCamera == null) // Main 카메라 할당 x
        {
            Debug.LogError("Main Camera is not assigned!");
            return;
        }
        initialPosition = mainCamera.transform.position; // 초기 위치 설정
        initialRotation = mainCamera.transform.rotation; // 초기 회전 설정
    }

    void Update()
    {
        HandleMouseZoom(); // 마우스 줌 처리
        HandleEdgeMovement(); // 화면 가장자리 이동 처리
        LimitCameraPosition(); // 카메라 위치 제한 로직 추가
    }

    // 마우스 줌 처리
    void HandleMouseZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel"); // 마우스 스크롤 입력
        mainCamera.fieldOfView -= scroll * zoomSpeed; // 줌 조정
        mainCamera.fieldOfView = Mathf.Clamp(mainCamera.fieldOfView, minFOV, maxFOV); // 줌 제한
    }

    // 화면 가장자리 이동 처리
    void HandleEdgeMovement()
    {
        Vector3 direction = Vector3.zero; // 이동 방향 초기화

        if (currentViewMode == ViewMode.TopView) // 탑뷰 모드일 때
        {
            if (Input.mousePosition.x >= Screen.width - edgeBoundary)
            {
                direction += Vector3.left; // 왼쪽으로 이동
            }
            if (Input.mousePosition.x <= edgeBoundary)
            {
                direction += Vector3.right; // 오른쪽으로 이동
            }
            if (Input.mousePosition.y >= Screen.height - edgeBoundary)
            {
                direction += Vector3.back; // 뒤로 이동
            }
            if (Input.mousePosition.y <= edgeBoundary)
            {
                direction += Vector3.forward; // 앞으로 이동
            }
        }
        else if (currentViewMode == ViewMode.DiagonalView) // 쿼터뷰 모드일 때
        {
            if (Input.mousePosition.x >= Screen.width - edgeBoundary)
            {
                direction += (Vector3.left + Vector3.forward).normalized; // 왼쪽 상단 대각선으로 이동
            }
            if (Input.mousePosition.x <= edgeBoundary)
            {
                direction += (Vector3.right + Vector3.back).normalized; // 오른쪽 하단 대각선으로 이동
            }
            if (Input.mousePosition.y >= Screen.height - edgeBoundary)
            {
                direction += (Vector3.back + Vector3.left).normalized; // 왼쪽 하단 대각선으로 이동
            }
            if (Input.mousePosition.y <= edgeBoundary)
            {
                direction += (Vector3.forward + Vector3.right).normalized; // 오른쪽 상단 대각선으로 이동
            }
        }

        if (direction != Vector3.zero)
        {
            Vector3 newPosition = mainCamera.transform.position + direction * edgeMoveSpeed * Time.deltaTime; // 새로운 위치 계산
            newPosition.y = mainCamera.transform.position.y; // Y 값 고정
            mainCamera.transform.position = newPosition; // 새로운 위치로 이동
        }
    }

    // 카메라 이동 거리 제한
    void LimitCameraPosition()
    {
        Vector3 position = mainCamera.transform.position;

        if (currentViewMode == ViewMode.TopView) // 탑뷰 모드일 때
        {
            float minX = centerPosition.x - maxXDistanceFromCenter; // 최소 X 위치
            float maxX = centerPosition.x + maxXDistanceFromCenter; // 최대 X 위치
            float minZ = centerPosition.z - maxZDistanceFromCenter; // 최소 Z 위치
            float maxZ = centerPosition.z + maxZDistanceFromCenter; // 최대 Z 위치

            position.x = Mathf.Clamp(position.x, minX, maxX); // X 위치 제한
            position.z = Mathf.Clamp(position.z, minZ, maxZ); // Z 위치 제한
        }
        else if (currentViewMode == ViewMode.DiagonalView) // 쿼터뷰 모드일 때
        {
            Vector3 offsetFromCenter = position - centerPosition; // 중심으로부터의 오프셋 계산
            Vector2 rotatedOffset = RotateVector2(new Vector2(offsetFromCenter.x, offsetFromCenter.z), -45f); // 오프셋 회전

            rotatedOffset.x = Mathf.Clamp(rotatedOffset.x, -maxXDistanceFromCenter, maxXDistanceFromCenter); // X 오프셋 제한
            rotatedOffset.y = Mathf.Clamp(rotatedOffset.y, -maxZDistanceFromCenter, maxZDistanceFromCenter); // Y 오프셋 제한

            rotatedOffset = RotateVector2(rotatedOffset, 45f); // 오프셋을 다시 회전

            position.x = centerPosition.x + rotatedOffset.x; // 새로운 X 위치 계산
            position.z = centerPosition.z + rotatedOffset.y; // 새로운 Z 위치 계산
        }

        mainCamera.transform.position = position; // 새로운 위치로 이동
    }

    // 2D 벡터 회전
    Vector2 RotateVector2(Vector2 v, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad; // 각도를 라디안으로 변환
        float sin = Mathf.Sin(radians); // 사인 계산
        float cos = Mathf.Cos(radians); // 코사인 계산

        float tx = v.x;
        float ty = v.y;

        return new Vector2(cos * tx - sin * ty, sin * tx + cos * ty); // 회전된 벡터 반환
    }

    // 뷰 모드 설정
    public void SetViewMode(ViewMode mode)
    {
        currentViewMode = mode; // 뷰 모드 업데이트
    }

    // 중심 위치 설정
    public void SetCenterPosition(Vector3 newCenterPosition)
    {
        centerPosition = newCenterPosition; // 중심 위치 업데이트
    }
}
