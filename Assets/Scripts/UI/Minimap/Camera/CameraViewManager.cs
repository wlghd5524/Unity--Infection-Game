using UnityEngine;

public class CameraViewManager : MonoBehaviour
{
    public static CameraViewManager Instance;
    public enum CameraViewMode { TopView, QuarterView };
    public CameraViewMode currentViewMode;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            SwitchViewMode();
        }
    }

    private void SwitchViewMode()
    {
        if (currentViewMode == CameraViewMode.TopView)
        {
            currentViewMode = CameraViewMode.QuarterView;
        }
        else
        {
            currentViewMode = CameraViewMode.TopView;
        }

        Debug.Log($"Global view mode switched to: {currentViewMode}");
    }
}
