using UnityEngine;

[RequireComponent(typeof(DialogSystem))]
public class TutorialDialog : TutorialBase
{
	// 캐릭터들의 대사를 진행하는 DialogSystem
	private	DialogSystem dialogSystem;

	public override void Enter()
	{
		dialogSystem = GetComponent<DialogSystem>();

		// 튜토리얼이 시작되면 게임 일시정지
		TutorialController controller = FindObjectOfType<TutorialController>();
        if ((controller != null))
        {
			controller.PauseGame();	// 게임 일시정지
        }

        dialogSystem.Setup();
	}

	public override void Execute(TutorialController controller)
	{
		// 현재 분기에 진행되는 대사 진행
		bool isCompleted = dialogSystem.UpdateDialog();

		// 현재 분기의 대사 진행이 완료되면
		if ( isCompleted == true )
		{
			// 게임을 다시 시작
			controller.ResumeGame();

			// 다음 튜토리얼로 이동
			controller.SetNextTutorial();
		}
	}

	public override void Exit()
	{
	}
}

