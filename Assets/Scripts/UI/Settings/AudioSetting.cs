using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor; // 에디터 관련 네임스페이스
#endif

public class AudioSetting : MonoBehaviour
{
    public AudioManager audioManager;
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    private bool isUpdatingSlider = false; // 슬라이더 값이 업데이트 중인지 여부를 체크하는 플래그
    public Button quitButton; // 게임 종료 버튼 추가

    void Start()
    {
        // 슬라이더 및 오디오 매니저 자동 할당
        audioManager = Assign(audioManager, "AudioManager");
        masterVolumeSlider = Assign(masterVolumeSlider, "MasterVolume");
        musicVolumeSlider = Assign(musicVolumeSlider, "BGMVolume");
        sfxVolumeSlider = Assign(sfxVolumeSlider, "SfxVolume");
        quitButton = Assign(quitButton, "QuitButton"); // 게임 종료 버튼 자동 할당
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

    void OnEnable()
    {
        // 슬라이더와 토글 초기화
        InitializeSlidersAndToggles();

        // 이벤트 리스너 등록
        RegisterListeners();
    }

    void OnDisable()
    {
        // 이벤트 리스너 제거
        UnregisterListeners();
    }

    private void InitializeSlidersAndToggles()
    {
        masterVolumeSlider.value = SnapValue(PlayerPrefs.GetFloat("MasterVolume", 100.0f));
        musicVolumeSlider.value = SnapValue(PlayerPrefs.GetFloat("BGMVolume", 100.0f));
        sfxVolumeSlider.value = SnapValue(PlayerPrefs.GetFloat("SfxVolume", 100.0f));
    }

    private void RegisterListeners()
    {
        masterVolumeSlider.onValueChanged.AddListener(SnapAndSetMasterVolume);
        musicVolumeSlider.onValueChanged.AddListener(SnapAndSetMusicVolume);
        sfxVolumeSlider.onValueChanged.AddListener(SnapAndSetSfxVolume);
        // 게임 종료 버튼 클릭 이벤트 등록
        quitButton.onClick.AddListener(OnQuitButtonClicked);
    }

    private void UnregisterListeners()
    {
        masterVolumeSlider.onValueChanged.RemoveListener(SnapAndSetMasterVolume);
        musicVolumeSlider.onValueChanged.RemoveListener(SnapAndSetMusicVolume);
        sfxVolumeSlider.onValueChanged.RemoveListener(SnapAndSetSfxVolume);
        // 게임 종료 버튼 클릭 이벤트 제거
        quitButton.onClick.RemoveListener(OnQuitButtonClicked);
    }

    // 마스터 볼륨 설정 함수
    void SnapAndSetMasterVolume(float value)
    {
        if (isUpdatingSlider) return;

        isUpdatingSlider = true;

        masterVolumeSlider.value = value;
        PlayerPrefs.SetFloat("MasterVolume", value);

        audioManager.SetMasterVolume(value);

        isUpdatingSlider = false;
    }

    void SnapAndSetMusicVolume(float value)
    {
        if (isUpdatingSlider) return;

        isUpdatingSlider = true;

        musicVolumeSlider.value = value;
        PlayerPrefs.SetFloat("BGMVolume", musicVolumeSlider.value);

        audioManager.SetMusicVolume(musicVolumeSlider.value);

        isUpdatingSlider = false;
    }

    void SnapAndSetSfxVolume(float value)
    {
        if (isUpdatingSlider) return;

        isUpdatingSlider = true;

        sfxVolumeSlider.value = value;
        PlayerPrefs.SetFloat("SfxVolume", sfxVolumeSlider.value);

        audioManager.SetSfxVolume(sfxVolumeSlider.value);

        isUpdatingSlider = false;
    }

    // 게임 종료 버튼 클릭 시 호출되는 함수
    void OnQuitButtonClicked()
    {
        BtnSoundManager.Instance.PlayButtonSound();
#if UNITY_EDITOR
        EditorApplication.isPlaying = false; // 에디터에서 플레이 모드 종료
#else
        Debug.Log("Quit button clicked");
        Application.Quit(); // 빌드된 게임에서 게임 종료
#endif
    }

    // 슬라이더 값을 스냅 간격으로 조정하는 함수
    private float SnapValue(float value)
    {
        return value;
    }
}
