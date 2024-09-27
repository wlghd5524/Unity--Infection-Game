using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

// 로그인 및 회원가입 관련 기능 매니저
public class AuthManager : MonoBehaviour
{
    // 로그인 관련 객체들
    public TMP_InputField loginIdInputField;
    public TMP_InputField loginUsernameInputField;
    public Image loginButton;
    public Image loginCloseButton;
    public TMP_Text loginMessageText;

    // 회원가입 관련 객체들
    public TMP_InputField signupIdInputField;
    public TMP_InputField signupUsernameInputField;
    public Image signupButton;
    public Image signupCloseButton;
    public TMP_Text signupMessageText;

    public GameObject loginPopup;  // 로그인 팝업창
    public GameObject signupPopup; // 회원가입 팝업창
    public MainMenuController mainMenuController;

    public TutorialController tutorialController;

    private string id;
    private string username;

    private enum AuthMode { Login, SignUp }
    private AuthMode currentMode;

    private void Start()
    {
        // 오브젝트 자동 할당
        loginIdInputField = Assign(loginIdInputField, "LoginIDInputField");
        loginUsernameInputField = Assign(loginUsernameInputField, "LoginUserNameInputField");
        loginButton = Assign(loginButton, "LoginButton");
        loginCloseButton = Assign(loginCloseButton, "LoginCloseButton");
        loginMessageText = Assign(loginMessageText, "LoginMessageText");
        signupIdInputField = Assign(signupIdInputField, "SignUpIDInputField");
        signupUsernameInputField = Assign(signupUsernameInputField, "SignUpUserNameInputField");
        signupButton = Assign(signupButton, "SignUpButton");
        signupCloseButton = Assign(signupCloseButton, "SignUpCloseButton");
        signupMessageText = Assign(signupMessageText, "SignUpMessageText");
        loginPopup = Assign(loginPopup, "LoginCanvas");
        signupPopup = Assign(signupPopup, "SignUpCanvas");
        mainMenuController = Assign(mainMenuController, "MainMenuCanvas");
        tutorialController = Assign(tutorialController, "TutorialController");

        // 로그인 이벤트 트리거 추가
        AddEventTrigger(loginCloseButton, (data) => OnBackButtonClicked(loginPopup));
        AddEventTrigger(loginButton, (data) => OnAuthButtonClicked(AuthMode.Login));

        // 회원가입 이벤트 트리거 추가
        AddEventTrigger(signupCloseButton, (data) => OnBackButtonClicked(signupPopup));
        AddEventTrigger(signupButton, (data) => OnAuthButtonClicked(AuthMode.SignUp));

        currentMode = AuthMode.Login; // 초기 모드를 로그인으로 설정
        InitializePopup(); // 초기화
    }

    // 오브젝트 자동 할당
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

    private void Update()
    {
        // Tab 키 처리
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            // 현재 포커스된 입력 필드 확인
            Selectable current = EventSystem.current.currentSelectedGameObject?.GetComponent<Selectable>();

            // 다음으로 포커스할 입력 필드 선택
            Selectable next = null;
            if (current != null)
            {
                next = current.FindSelectableOnDown();
                if (next == null) // 마지막 필드라면 첫번째 필드로 이동
                {
                    next = FindFirstInputField();
                }
            }
            else
            {
                // 포커스된 필드가 없다면 첫번째 필드로 이동
                next = FindFirstInputField();
            }

            // 다음 입력 필드로 포커스 이동
            next?.Select();
        }
    }

    // 첫번째 입력 필드를 찾는 함수
    private Selectable FindFirstInputField()
    {
        if (currentMode == AuthMode.Login)
        {
            return loginIdInputField;
        }
        else if (currentMode == AuthMode.SignUp)
        {
            return signupIdInputField;
        }
        return null;
    }

    // 팝업창에서 뒤로가기 클릭
    private void OnBackButtonClicked(GameObject popup)
    {
        Debug.Log("Back Button Clicked");
        InitializePopup(); // 팝업 초기화 (적혀있는 내용 삭제)
        popup.SetActive(false);
        mainMenuController.EnableMenuInteraction(); // 메뉴 버튼 상호작용 가능하게 설정

    }

    // 로그인/회원가입 메뉴 버튼 클릭 시
    private void OnAuthButtonClicked(AuthMode mode)
    {
        currentMode = mode;
        Debug.Log($"{mode} Button Clicked");

        // 입력된 ID 및 이름 가져오기
        if (mode == AuthMode.Login)
        {
            id = loginIdInputField.text;
            username = loginUsernameInputField.text;
        }
        else if (mode == AuthMode.SignUp)
        {
            id = signupIdInputField.text;
            username = signupUsernameInputField.text;
        }

        // ID 입력창 비어있을 때 예외처리
        if (string.IsNullOrEmpty(id))
        {
            GetCurrentMessageText().text = "사원번호를 입력해주세요.";
            GetCurrentMessageText().color = Color.red;
            return;
        }

        // 이름 입력창 비어있을 때 예외처리
        if (string.IsNullOrEmpty(username))
        {
            GetCurrentMessageText().text = "이름을 입력해주세요.";
            GetCurrentMessageText().color = Color.red;
            return;
        }

        // 로그인/회원가입 실행
        if (mode == AuthMode.Login)
        {
            HandleLogin();
        }
        else if (mode == AuthMode.SignUp)
        {
            HandleSignUp();
        }
    }

    // 메시지 표시
    private void DisplayMessage(string message, Color color)
    {
        TMP_Text messageText = GetCurrentMessageText();
        messageText.text = message;
        messageText.color = color;
    }

    // 로그인 정보 일치 검사
    private void HandleLogin()
    {
        if (UserManager.Instance.ValidateUser(id, username))
        {
            StartCoroutine(LoginSuccessCoroutine());
        }
        else
        {
            DisplayMessage("사용자 정보가 존재하지 않습니다.", Color.red);
        }
    }

    // 회원가입 처리
    private void HandleSignUp()
    {
        if (!IsValidID(id))
        {
            DisplayMessage("유효한 사원번호 형식이 아닙니다.\n(숫자만 가능)", Color.red);
            return;
        }

        if (!IsValidUsername(username))
        {
            DisplayMessage("유효한 이름 형식이 아닙니다.\n(한글, 영어만 가능)", Color.red);
            return;
        }

        if (UserManager.Instance.IsIDExists(id))
        {
            DisplayMessage("이미 존재하는 사원번호입니다.", Color.red);
            return;
        }

        if (UserManager.Instance.IsUsernameExists(username))
        {
            DisplayMessage("이미 존재하는 이름입니다.", Color.red);
            return;
        }

        UserManager.Instance.AddUser(id, username);
        DisplayMessage("회원가입 성공!\n로그인 화면으로 이동해주세요.", Color.green);
    }

    // ID 유효성 검사
    private bool IsValidID(string id)
    {
        return id.Length >= 6 && id.Length <= 10 && UserManager.Instance.IsValidID(id);
    }

    // 이름 유효성 검사
    private bool IsValidUsername(string username)
    {
        return username.Length >= 2 && username.Length <= 11 && UserManager.Instance.IsValidUsername(username);
    }

    // AuthMode에 따라 해당 팝업창이 로그인 창인지 회원가입 창인지 알려주는 텍스트
    private TMP_Text GetCurrentMessageText()
    {
        // 최근 모드가 로그인 모드면 "로그인", 아니면 "회원가입"
        return currentMode == AuthMode.Login ? loginMessageText : signupMessageText;
    }

    // 포인터 클릭 이벤트 부여
    private void AddEventTrigger(Image image, UnityEngine.Events.UnityAction<BaseEventData> action)
    {
        EventTrigger trigger = image.gameObject.GetComponent<EventTrigger>() ?? image.gameObject.AddComponent<EventTrigger>();
        var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
        entry.callback.AddListener(action);
        trigger.triggers.Add(entry);
    }

    // 팝업창 내부에 있는 정보 초기화
    private void InitializePopup()
    {
        loginIdInputField.text = "";
        loginUsernameInputField.text = "";
        loginMessageText.text = "";
        signupIdInputField.text = "";
        signupUsernameInputField.text = "";
        signupMessageText.text = "";
    }

    // 로그인 성공 시 텀 두는 코루틴 (4초경과)
    private IEnumerator LoginSuccessCoroutine()
    {
        for (int i = 3; i >= 0; i--)
        {
            if (i == 0)
            {
                loginMessageText.text = "접속 중입니다..";
                loginMessageText.color = Color.green;
                yield return new WaitForSeconds(1);
            }
            else
            {
                loginMessageText.text = $"로그인 성공! {i}초 후에 접속됩니다..";
                loginMessageText.color = Color.green;
                yield return new WaitForSeconds(1);
            }
        }
        GoToGame.Instance.StartGame();
        tutorialController.TutorialCheck();
    }
}
