using System.Threading;
using UnityEngine;
using UnityEngine.iOS;

public class MinimapRaycaster : MonoBehaviour
{
    public static MinimapRaycaster Instance { get; private set; }

    public Camera minimapCamera; // 미니맵 카메라
    public MinimapController minimapController; // 미니맵 컨트롤러
    public RectTransform minimapRectTransform; // 미니맵 UI의 RectTransform
    private GameObject lastHighlighted = null; // 마지막으로 하이라이트된 오브젝트
    private Ray currentRay; // 현재 레이
    private bool isRayActive = false; // 레이 활성화 여부
    public float rayLength = 100f; // 레이 길이
    public LayerMask clickableLayer; // 클릭 가능한 오브젝트의 레이어

    private bool externalHighlightActive = false; // 외부 하이라이트가 활성화되었는지 여부
    private string currentHighlightedFloorName = ""; // 현재 하이라이트된 층 이름

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);  // 중복된 인스턴스를 방지
        }
    }

    void Update()
    {
        if (minimapController.IsLargeMapOpen) // 미니맵 패널이 활성화된 상태에서만
        {
            HandleMinimapInteraction(); // 미니맵 상호작용 처리
        }
        else if (externalHighlightActive)
        {
            FloorManager.HighlightFloor(currentHighlightedFloorName);
        }
        else
        {
            ResetHighlight(); // 하이라이트 초기화
        }
    }

    // 외부에서 하이라이트 활성화 상태를 설정하는 함수
    public void SetExternalHighlightActive(bool active, string floorName = "")          // 외부에서 사용법 = MinimapRaycaster.Instance.SetExternalHighlightActive(true, "응급실")
    {
        externalHighlightActive = active;
        currentHighlightedFloorName = floorName;
        Debug.Log(externalHighlightActive + currentHighlightedFloorName);

        if (!externalHighlightActive)
        {
            Debug.Log("해제");
            ResetHighlight();
        }
    }

    // 미니맵 상호작용을 처리하는 메서드
    void HandleMinimapInteraction()
    {
        Vector3 mousePosition = Input.mousePosition;
        Vector2 localPoint;

        // 마우스 포지션을 미니맵 UI의 로컬 포인트로 변환
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(minimapRectTransform, mousePosition, null, out localPoint))
        {
            // 로컬 포인트를 미니맵 카메라의 뷰포트 포인트로 변환
            Vector2 viewportPoint = new Vector2(
                (localPoint.x / minimapRectTransform.rect.width) + 0.5f,
                (localPoint.y / minimapRectTransform.rect.height) + 0.5f
            );

            // 뷰포트 포인트를 사용하여 레이캐스트
            Ray ray = minimapCamera.ViewportPointToRay(viewportPoint);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, rayLength, clickableLayer))
            {
                GameObject hitObject = hit.collider.gameObject;

                FloorManager.FloorInfo floorInfo = GetFloorInfoFromHit(hitObject);
                if (floorInfo != null)
                {
                    HandleHighlight(hitObject); // 하이라이트 처리
                    HandleClick(floorInfo); // 클릭 처리
                }
                else
                {
                    Debug.Log("Hit object is not clickable: " + hitObject.name);
                }
            }
            else
            {
                ResetHighlight(); // 하이라이트 초기화
            }
        }
    }

    // 하이라이트 처리
    void HandleHighlight(GameObject hitObject)
    {
        if (lastHighlighted != hitObject)
        {
            if (lastHighlighted != null)
            {
                FloorManager.HighlightFloor(null); // 기존 하이라이트 해제
            }

            FloorManager.FloorInfo floorInfo = GetFloorInfoFromHit(hitObject);
            if (floorInfo != null)
            {
                FloorManager.HighlightFloor(floorInfo.floorName); // 새로운 오브젝트의 색상 변경
                UIManager.Instance.ShowObjectName(floorInfo.floorName); // 지정된 층 이름 표시
                lastHighlighted = hitObject; // 마지막 하이라이트된 오브젝트 업데이트
            }
        }
    }

    // 층 클릭 시 작동
    void HandleClick(FloorManager.FloorInfo floorInfo)
    {
        if (Input.GetMouseButtonDown(0) && minimapController.IsLargeMapOpen)
        {
            Debug.Log("Mouse button down detected on: " + floorInfo.floorName);
            FindObjectOfType<FloorManager>().ChangeFloor(floorInfo); // 층 이동 처리
            BtnSoundManager.Instance.PlayButtonSound();
        }
    }

    // 층 정보 얻기
    FloorManager.FloorInfo GetFloorInfoFromHit(GameObject hitObject)
    {
        FloorManager floorManager = FindObjectOfType<FloorManager>();
        if (floorManager != null)
        {
            foreach (var floor in floorManager.floors)
            {
                if (floor.floorObject == hitObject)
                {
                    return floor;
                }
            }
        }
        return null;
    }

    // 층 색상 초기화
    void ResetHighlight()
    {
        if (lastHighlighted != null)
        {
            FloorManager.HighlightFloor(null); // 마지막 하이라이트된 오브젝트의 색상 초기화
            UIManager.Instance.HideObjectName(); // 이름 숨기기
            lastHighlighted = null;
        }
        isRayActive = false; // 레이 비활성화
    }

    // 미니맵에 Ray를 잘 쏘고 있나 확인용
    void OnDrawGizmos()
    {
        if (isRayActive)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(currentRay.origin, currentRay.direction * rayLength); // 현재 레이의 길이만큼 빨간색으로 그림
        }
    }
}
