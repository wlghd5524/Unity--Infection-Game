using UnityEngine;
using UnityEngine.UI;

public class TutorialButtonPress : TutorialBase
{
    [SerializeField] private Button buttonToPress;
    private bool isButtonPressed = false;
    private bool tutorialCompleted = false;

    public override void Enter()
    {
        // 상태 초기화
        isButtonPressed = false;
        tutorialCompleted = false;

        // 버튼에 클릭 이벤트 리스너 추가
        if (buttonToPress != null)
        {
            buttonToPress.onClick.AddListener(OnButtonPressed);
        }
        else
        {
            Debug.LogError("Button not assigned in the inspector!");
        }
    }

    public override void Execute(TutorialController controller)
    {
        // 버튼이 눌리고 튜토리얼이 아직 완료되지 않은 경우에만 실행
        if (isButtonPressed && !tutorialCompleted)
        {
            tutorialCompleted = true;
            controller.SetNextTutorial();
        }
    }

    public override void Exit()
    {
        // 리스너 제거 및 상태 초기화
        if (buttonToPress != null)
        {
            buttonToPress.onClick.RemoveListener(OnButtonPressed);
        }
        isButtonPressed = false;
        tutorialCompleted = false;
    }

    private void OnButtonPressed()
    {
        if (!tutorialCompleted)
        {
            isButtonPressed = true;
            Debug.Log("Button pressed!");
        }
    }
}
