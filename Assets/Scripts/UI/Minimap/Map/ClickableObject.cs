using UnityEngine;
using UnityEngine.SceneManagement;

public class ClickableObject : MonoBehaviour
{
    private Color originalColor;
    private Color highlightColor = new Color(0.5f, 0.5f, 1.0f, 1.0f); // 연한 파랑색

    public int floorIndex;
    public string floorName; // 각 오브젝트에 원하는 층 이름을 할당

    private CameraController cameraController;
    private CameraSwitcher cameraSwitcher;

    void Start()
    {
        // 원래 색상을 저장합니다.
        originalColor = GetComponent<Renderer>().material.color;
        cameraController = FindObjectOfType<CameraController>();
        cameraSwitcher = FindObjectOfType<CameraSwitcher>();
    }

    public void OnMouseDown()
    {
        Debug.Log("OnMouseDown called on " + gameObject.name + " with parent: " + transform.parent.name);

        // 부모 오브젝트의 이름을 가져옵니다
        string parentName = transform.parent.name;

        // 부모 이름에 따라 다른 메시지를 출력하고 카메라 위치를 전환
        switch (parentName)
        {
            case "1f left":
                Debug.Log("1층 왼쪽이 클릭되었습니다!");
                ChangeFloor("응급실");
                break;
            case "1f right":
                Debug.Log("1층 오른쪽이 클릭되었습니다!");
                ChangeFloor("중환자실");
                break;
            case "2f left":
                Debug.Log("2층 왼쪽이 클릭되었습니다!");
                ChangeFloor("내과 1");
                break;
            case "2f right":
                Debug.Log("2층 오른쪽이 클릭되었습니다!");
                ChangeFloor("내과 2");
                break;
            case "3f left":
                Debug.Log("3층 왼쪽이 클릭되었습니다!");
                ChangeFloor("외과 3");
                break;
            case "3f right":
                Debug.Log("3층 오른쪽이 클릭되었습니다!");
                ChangeFloor("외과 4");
                break;
            case "4f left":
                Debug.Log("4층 왼쪽이 클릭되었습니다!");
                ChangeFloor("소아과");
                break;
            case "4f right":
                Debug.Log("4층 오른쪽이 클릭되었습니다!");
                ChangeFloor("산부인과");
                break;
            case "rooftop":
                Debug.Log("옥상이 클릭되었습니다!");
                ChangeFloor("옥상");
                break;
            default:
                Debug.Log(parentName + "가 클릭되었습니다!");
                break;
        }
    }

    public void ChangeFloor(string floorName)
    {
        if (cameraController != null)
        {
            cameraController.MoveToFloor(floorIndex);   // 카메라를 특정 층으로 이동시키고 레이어를 설정
        }
        if (cameraSwitcher != null)
        {
            cameraSwitcher.SwitchFloor(floorIndex);     // 층을 전환하고 새로운 뷰를 설정
        }
        UIManager.Instance.UpdateCurrentFloorText(floorName);
    }

    public void OnMouseEnter()
    {
        // 마우스가 오브젝트 위에 올라갔을 때 색상을 변경합니다
        ChangeColor(transform.parent, highlightColor);
        UIManager.Instance.ShowObjectName(floorName); // 지정된 층 이름 표시
    }

    public void OnMouseExit()
    {
        // 마우스가 오브젝트에서 벗어났을 때 원래 색상으로 복원합니다
        ChangeColor(transform.parent, originalColor);
        UIManager.Instance.HideObjectName(); // 이름 숨기기
    }

    void ChangeColor(Transform parent, Color color)
    {
        // 부모 오브젝트의 모든 자식들의 색상을 변경합니다
        Renderer[] renderers = parent.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.material.color = color;
        }
    }
}
