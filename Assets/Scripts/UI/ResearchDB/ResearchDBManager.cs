using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class ResearchDBManager : MonoBehaviour
{
    public static ResearchDBManager Instance { get; private set; }

    public string userNum;
    public string userName;
    public string currentDate;

    // 메뉴 모드에 따라 버튼(누름, 누른 시간) 기록을 저장할 목록
    public Dictionary<ResearchMode, List<string>> researchRecords = new Dictionary<ResearchMode, List<string>>()
    {
        {ResearchMode.medical, new List<string>() },
        {ResearchMode.patient, new List<string>() },
        {ResearchMode.hospital, new List<string>() }
    };

    private string urlUpdateResearch = "http://220.69.209.164:3333/update_research";

    public enum ResearchMode { medical, patient, hospital }
    private ResearchMode currentMode;

    private void Awake()
    {
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

    private void Start()
    {
        currentDate = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }

    // 리스트 생성 및 DB 저장
    public void AddResearchData(ResearchMode mode)
    {
        string currentMoment = System.DateTime.Now.ToString("mm:ss");
        researchRecords[mode].Add(currentMoment);    // 각 연구 메뉴별로 연구 버튼을 누른 시간 저장
        SendResearchDataToServer();                  // DB 전송
    }

    // POST 요청으로 서버에 데이터 보내기
    public void SendResearchDataToServer()
    {
        WWWForm form = new WWWForm();
        form.AddField("userNum", userNum);
        form.AddField("userName", userName);
        form.AddField("medicalResearch", FormatDataForDB(researchRecords[ResearchMode.medical]));
        form.AddField("patientResearch", FormatDataForDB(researchRecords[ResearchMode.patient]));
        form.AddField("hospitalResearch", FormatDataForDB(researchRecords[ResearchMode.hospital]));
        form.AddField("playingDate", currentDate);

        //Debug.Log($"ResearchDB: {userNum}, {userName}, {FormatDataForDB(researchRecords[ResearchMode.medical])}, {FormatDataForDB(researchRecords[ResearchMode.patient])}, {FormatDataForDB(researchRecords[ResearchMode.hospital])}, {currentDate}");

        StartCoroutine(PostRequest(urlUpdateResearch, form));
    }

    // DB에 보낼 수 있게 리스트->문자열 형변환
    private string FormatDataForDB(List<string> records)
    {
        if (records != null && records.Count > 0)
        {
            return string.Join(", ", records);
        }
        return " ";
    }

    // Send a POST request to the Flask API
    IEnumerator PostRequest(string url, WWWForm form)
    {
        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error sending request: " + www.error);
            }
            else
            {
                Debug.Log("Request successfully sent: " + www.downloadHandler.text);
            }
        }
    }
}
