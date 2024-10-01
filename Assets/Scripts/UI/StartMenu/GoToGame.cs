using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 게임 시작 시 UI 및 카메라 설정을 담당하는 스크립트
public class GoToGame : MonoBehaviour
{
    public static GoToGame Instance { get; private set; }

    public GameObject startItems;
    public GameObject profileWindowUI;
    public GameObject infoWindowUI;
    public GameObject settingButtonIcon;
    public GameObject minimapCanvas;
    public CalendarManager calendarManager;
    //public GameObject researchCanvas;
    public GameObject disinfectionCanvas;
    public GameObject newsTickerCanvas;
    public GameObject researchPanel;
    public GameObject researchMenuCanvas;

    public ProfileWindow profileWindow;

    //public Transform cameraTransform;
    //public Transform targetTransform;

    public bool isStart = false;
    void Awake()
    {
        GameObject hospital = GameObject.Find("Hospital");
        Vector3 hospitalPosition = hospital.transform.position;
        hospitalPosition = new Vector3(-hospitalPosition.x, hospitalPosition.y, hospitalPosition.z);
        hospital.transform.position = hospitalPosition;

        startItems = Assign(startItems, "StartUI");
        profileWindowUI = Assign(profileWindowUI, "ProfileCanvas");
        infoWindowUI = Assign(infoWindowUI, "InfoCanvas");
        settingButtonIcon = Assign(settingButtonIcon, "SettingButton");
        minimapCanvas = Assign(minimapCanvas, "MinimapCanvas");
        //researchCanvas = Assign(researchCanvas, "ResearchCanvas");
        disinfectionCanvas = Assign(disinfectionCanvas, "DisinfectionCanvas");
        newsTickerCanvas = Assign(newsTickerCanvas, "NewsTickerCanvas");
        calendarManager = FindObjectOfType<CalendarManager>();
        profileWindow = FindObjectOfType<ProfileWindow>();
        researchPanel = Assign(researchPanel, "ResearchPanel");
        researchMenuCanvas = Assign(researchMenuCanvas, "ResearchMenuCanvas");

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
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




    // 게임 시작 시 설정
    public void StartGame()
    {
        //// 카메라 위치 변경
        //if (cameraTransform == null)
        //{
        //    cameraTransform = Camera.main.transform;
        //}
        //MovingCamera(targetTransform.position, targetTransform.rotation);
        startItems.SetActive(false);
        profileWindowUI.SetActive(true);
        infoWindowUI.SetActive(true);
        settingButtonIcon.SetActive(true);
        minimapCanvas.SetActive(true);
        //researchCanvas.SetActive(true);
        disinfectionCanvas.SetActive(true);
        newsTickerCanvas.SetActive(true);
        researchMenuCanvas.SetActive(true);
        researchPanel.SetActive(false);
        //calendarManager.StartCalendar();
        profileWindow.UpdateButtonTexts("응급실");
        //Managers.PatientCreator.startSignal = true; // => 게임 시작 전에 환자 생성 막는 코드입니다 patient 코드가 사라져서 오류가 뜹니다
        isStart = true;
        Debug.Log("Start!!!");
    }

    //public void MovingCamera(Vector3 newPosition, Quaternion newRotation)
    //{
    //    cameraTransform.position = newPosition;
    //    cameraTransform.rotation = newRotation;
    //}
}
