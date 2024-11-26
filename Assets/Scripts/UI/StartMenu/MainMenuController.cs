using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    public Button startLoginButton;
    public Button startSignUpButton;
    public Button startExitButton;
    public Button startOptionButton;

    public GameObject loginCanvas;
    public GameObject signupCanvas;
    public GameObject settingWindow;
    public GameObject exitGameCanvas;

    public float hoverScale = 1.1f;     // 확대 비율
    public float scaleDuration = 0.1f;  // 확대/축소 애니메이션 시간
    private bool isPopupActive = false; // 팝업 활성 상태를 확인하는 변수

    private void Start()
    {
        startLoginButton = Assign(startLoginButton, "StartLoginButton");
        startSignUpButton = Assign(startSignUpButton, "StartSignUpButton");
        startExitButton = Assign(startExitButton, "StartExitButton");
        startOptionButton = Assign(startOptionButton, "StartOptionButton");
        loginCanvas = Assign(loginCanvas, "LoginCanvas");
        signupCanvas = Assign(signupCanvas, "SignUpCanvas");
        settingWindow = Assign(settingWindow, "SettingWindow");
        exitGameCanvas = Assign(exitGameCanvas, "ExitGameCanvas");

        // Canvas를 올바른 순서로 배치
        loginCanvas.transform.SetAsLastSibling();
        signupCanvas.transform.SetAsLastSibling();
        exitGameCanvas.transform.SetAsLastSibling();

        startLoginButton.onClick.AddListener(()=> OnButtonClicked(OnStartGameClicked));
        startSignUpButton.onClick.AddListener(()=> OnButtonClicked(OnAccountCreationClicked));
        startExitButton.onClick.AddListener(() => OnButtonClicked(OnQuitGameClicked));
        startOptionButton.onClick.AddListener(() => OnButtonClicked(OnSettingsClicked));
    }

    void OnButtonClicked(System.Action buttonAction)
    {
        if (!isPopupActive)
            buttonAction.Invoke();
        else
            return;
    }

    // 자동 할당 코드
    private T Assign<T>(T obj, string objectName) where T : Object
    {
        if (obj == null)
        {
            GameObject foundObject = GameObject.Find(objectName);
            if (foundObject != null)
            {
                if (typeof(Component).IsAssignableFrom(typeof(T))) obj = foundObject.GetComponent(typeof(T)) as T;
                else if (typeof(GameObject).IsAssignableFrom(typeof(T))) obj = foundObject as T;
            }
            if (obj == null) Debug.LogError($"{objectName} 를 찾을 수 없습니다.");
        }
        return obj;
    }

    // 로그인 메뉴 클릭
    private void OnStartGameClicked()
    {
        //Debug.Log("Start Game Clicked");
        BtnSoundManager.Instance.PlayButtonSound();
        loginCanvas.SetActive(true);    // 로그인 창 실행
        isPopupActive = true; // 팝업 활성 상태로 설정
    }

    // 회원가입 메뉴 클릭
    private void OnAccountCreationClicked()
    {
        //Debug.Log("Account Creation Clicked");
        BtnSoundManager.Instance.PlayButtonSound();
        signupCanvas.SetActive(true);  // 회원가입 창 실행
        isPopupActive = true; // 팝업 활성 상태로 설정
    }

    // 설정 메뉴 클릭
    private void OnSettingsClicked()
    {
        //Debug.Log("Settings Clicked");
        BtnSoundManager.Instance.PlayButtonSound();
        settingWindow.SetActive(true);// 설정 창 실행
        isPopupActive = true;
    }

    // 게임종료 메뉴 클릭
    private void OnQuitGameClicked()
    {
        //Debug.Log("Quit Game Clicked");
        BtnSoundManager.Instance.PlayButtonSound();
        exitGameCanvas.SetActive(true); // 게임종료 확인 창 실행
        isPopupActive = true; // 팝업 활성 상태로 설정
    }

    // ExitGame 팝업 종료 버튼
    // private 인 isPopupActive 관리 위해 여기서 사용
    public void ClosePopup(GameObject popupCanvas)
    {
        popupCanvas.SetActive(false);
        BtnSoundManager.Instance.PlayButtonSound();
        isPopupActive = false; // 팝업 비활성 상태로 설정
    }

    // 팝업 해제되면 메인메뉴 버튼 다시 클릭 가능하도록 사용
    public void EnableMenuInteraction()
    {
        isPopupActive = false;
    }
}
