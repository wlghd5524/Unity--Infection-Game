using System.Collections.Generic;
using UnityEngine;

public class FloorManager : MonoBehaviour
{
    [System.Serializable]
    public class FloorInfo
    {
        public GameObject floorObject;  // 층을 나타내는 오브젝트
        public int floorIndex;          // 층 인덱스
        public string floorName;        // 층 이름
    }

    public static FloorManager Instance { get; private set; }  // 싱글톤 인스턴스 생성

    public List<FloorInfo> floors = new List<FloorInfo>();  // 모든 층 정보를 담은 리스트
    private Color highlightColor = new Color(0.5f, 0.5f, 1.0f, 1.0f); // 연한 파랑색
    private Dictionary<GameObject, Color[]> originalColors = new Dictionary<GameObject, Color[]>(); // 각 층의 원래 색상 저장

    private CameraController cameraController;
    private ProfileWindow profileWindow;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);  // 이미 인스턴스가 존재하면 중복 생성 방지
            return;
        }
    }

    void Start()
    {
        profileWindow = FindObjectOfType<ProfileWindow>();
        cameraController = FindObjectOfType<CameraController>();

        // 각 층의 원래 색상 저장
        foreach (FloorInfo floor in floors)
        {
            Renderer renderer = floor.floorObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                Color[] colors = new Color[renderer.materials.Length];
                for (int i = 0; i < renderer.materials.Length; i++)
                {
                    colors[i] = renderer.materials[i].color;
                }
                originalColors.Add(floor.floorObject, colors);
            }
        }
    }

    public void OnFloorClicked(GameObject clickedObject)
    {
        // 클릭된 층을 찾고 해당 층으로 이동
        foreach (FloorInfo floor in floors)
        {
            if (floor.floorObject == clickedObject)
            {
                Debug.Log($"{floor.floorName}이 클릭되었습니다!");
                ChangeFloor(floor);
                profileWindow.UpdateButtonTexts(floor.floorName);
                break;
            }
        }
    }

    public void ChangeFloor(FloorInfo floor)
    {
        if (cameraController != null)
        {
            cameraController.MoveToFloor(floor.floorIndex);  // 카메라를 특정 층으로 이동시키고 레이어를 설정
        }
        UIManager.Instance.UpdateCurrentFloorText(floor.floorName);
    }

    public static void HighlightFloor(string floorName)
    {
        // 드롭다운에서 선택된 층 이름에 맞는 오브젝트의 색상 변경
        if (Instance != null)
        {
            foreach (FloorInfo floor in Instance.floors)
            {
                if (floor.floorName == floorName)
                {
                    Instance.ChangeColor(floor.floorObject, Instance.highlightColor);
                }
                else
                {
                    Instance.RestoreOriginalColor(floor.floorObject);
                }
            }
        }
    }

    void ChangeColor(GameObject obj, Color color)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            foreach (Material mat in renderer.materials)
            {
                mat.color = color;
            }
        }
    }

    void RestoreOriginalColor(GameObject obj)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null && originalColors.ContainsKey(obj))
        {
            Color[] colors = originalColors[obj];
            for (int i = 0; i < renderer.materials.Length; i++)
            {
                renderer.materials[i].color = colors[i];
            }
        }
    }

    // 드롭다운에서 선택된 층을 반영할 수 있는 함수 (층 이름 사용하면 해당 층 색상 변경 가능), null 사용하면 다시 색상 초기화
    //public static void OnFloorDropdownChanged(string selectedFloorName)
    //{
    //    HighlightFloor(selectedFloorName);
    //    MinimapRaycaster.Instance.SetExternalHighlightActive(true, selectedFloorName);
    //    Debug.Log("하이라이트" + selectedFloorName);
    //}
}
