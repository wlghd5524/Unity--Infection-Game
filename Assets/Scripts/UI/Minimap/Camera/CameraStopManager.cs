using System.Collections.Generic;
using UnityEngine;

public class CameraStopManager : MonoBehaviour
{
    public CameraHandler cameraHandler; // CameraHandler 스크립트
    public List<GameObject> uiPanels; // 인스펙터에서 할당할 UI 패널 리스트

    void Start()
    {
        if (cameraHandler == null)
        {
            cameraHandler = FindObjectOfType<CameraHandler>(); // CameraHandler 자동 할당
        }
    }

    void Update()
    {
        // UI 패널 리스트에서 하나라도 활성화된 것이 있으면 CameraHandler를 비활성화
        bool anyPanelActive = false;
        foreach (GameObject uiPanel in uiPanels)
        {
            if (uiPanel.activeSelf)
            {
                anyPanelActive = true;
                break;
            }
        }

        cameraHandler.enabled = !anyPanelActive; // UI 패널이 하나라도 활성화되면 CameraHandler 비활성화, 아니면 활성화
    }
}
