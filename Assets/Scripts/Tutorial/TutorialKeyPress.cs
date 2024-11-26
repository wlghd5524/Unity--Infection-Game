using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TutorialKeyPress : TutorialBase
{
    [SerializeField] private KeyCode keyToPress; // 사용자에게 입력받을 키
    [SerializeField] private GameObject KeyImage;        // 키 이미지 UI
    [SerializeField] private Toggle keyCheckToggle; // 체크박스

    private bool isKeyPressed = false;
    private CameraHandler cameraHandler;


    public override void Enter()
    {
        // 카메라 핸들러를 찾음
        cameraHandler = FindObjectOfType<CameraHandler>();

        isKeyPressed = false;

        // UI 초기 상태 설정
        KeyImage.SetActive(true);
        keyCheckToggle.isOn = false;
    }

    public override void Execute(TutorialController controller)
    {
        // 사용자가 지정된 키를 누르면, 그리고 키가 아직 눌리지 않은 경우에만 처리
        if (Input.GetKeyDown(keyToPress))
        {
            if (!isKeyPressed) // 카메라 전환 막기
                cameraHandler.isTutorialKeyActive = true;
            isKeyPressed = true;
            keyCheckToggle.isOn = true;

            controller.SetNextTutorial();
        }
    }

    public override void Exit()
    {
        // UI 요소 비활성화
        KeyImage.SetActive(false);
        keyCheckToggle.isOn = false;
        cameraHandler.isTutorialKeyActive = false;
    }
}
