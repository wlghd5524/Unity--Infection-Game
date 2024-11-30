using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ObjectActivator : MonoBehaviour
{
    public GameObject questParentCanvas;
    public Button activateButton;         // 활성화할 버튼
    public Button quizCloseButton;        //퀴즈창 닫기 버튼
    public Canvas questCanvas;            //퀴즈창

    void Start()
    {
        questParentCanvas = GameObject.Find("QuestCanvas");
        questCanvas = questParentCanvas.GetComponent<Canvas>();
        activateButton = GameObject.Find("QuizStartButton").GetComponent<Button>();
        activateButton = gameObject.GetComponent<Button>();
        quizCloseButton = GameObject.Find("XButton").GetComponent <Button>();

        // 초기 상태 설정: 퀴즈창을 보이지 않게 설정
        SetQuestCanvasVisible(false);

        if (activateButton != null)
        {
            activateButton.onClick.AddListener(() => { OnActivateButtonClick(); BtnSoundManager.Instance.PlayButtonSound(); OneClearManager.Instance.CloseDisinfectionMode(); });
        }

        if (quizCloseButton != null)
        {
            quizCloseButton.onClick.AddListener(() => { OnDisactivateButtonClick(); BtnSoundManager.Instance.PlayButtonSound(); });
        }
    }

    public void OnActivateButtonClick()
    {
        SetQuestCanvasVisible(true);

        // 레벨 1을 기본으로 설정
        QuizManager.Instance.OnLevelButtonClicked(0);
    }

    //QuestCanvas가 비활성화돼도 게임 로직이나 쿨타임 패널은 정상 유지
    public void OnDisactivateButtonClick()
    {
        SetQuestCanvasVisible(false);

        // 카운트다운 타이머 멈추기
        QuizTimer.Instance.ResetTimerText("LevelButton1");
    }

    private void SetQuestCanvasVisible(bool isVisible)
    {
        questCanvas.enabled = isVisible;

        CanvasGroup canvasGroup = questCanvas.gameObject.GetComponent<CanvasGroup>();
        canvasGroup.interactable = isVisible;
        canvasGroup.blocksRaycasts = isVisible;
    }
}
