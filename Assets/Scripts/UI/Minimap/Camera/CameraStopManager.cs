using System.Collections.Generic;
using UnityEngine;

public class CameraStopManager : MonoBehaviour
{
    public static CameraStopManager Instance { get; private set; } // 싱글톤 인스턴스

    public CameraHandler cameraHandler; // CameraHandler 스크립트
    public List<GameObject> uiPanels; // UI 패널 리스트
    void Start()
    {
        if (cameraHandler == null)
        {
            cameraHandler = FindObjectOfType<CameraHandler>();
            if (cameraHandler == null)
            {
                Debug.LogError("CameraHandler not found!");
            }
        }
    }


    void Update()
    {
        HandleUIAndCameraControl();
    }

    private void HandleUIAndCameraControl()
    {
        bool anyPanelActive = false;
        foreach (GameObject uiPanel in uiPanels)
        {
            if (uiPanel.activeSelf)
            {
                anyPanelActive = true;
                break;
            }
        }
        cameraHandler.enabled = !anyPanelActive;
    }
}
