using UnityEngine;

public class TutorialKeyPress : TutorialBase
{
    [SerializeField] private KeyCode keyToPress = KeyCode.Space; // 사용자에게 입력받을 키

    private bool isKeyPressed = false;

    public override void Enter()
    {
        isKeyPressed = false;
    }

    public override void Execute(TutorialController controller)
    {
        // 사용자가 지정된 키를 누르면
        if (Input.GetKeyDown(keyToPress))
        {
            isKeyPressed = true;
            Debug.Log("Key pressed: " + keyToPress.ToString());

            // 다음 튜토리얼로 이동
            controller.SetNextTutorial();
        }
    }

    public override void Exit()
    {
    }
}
