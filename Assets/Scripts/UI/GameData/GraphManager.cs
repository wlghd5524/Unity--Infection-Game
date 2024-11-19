﻿using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;
using UnityEngine.XR;

public class GraphManager : MonoBehaviour
{
    public Transform graphContainer;    // 그래프를 그릴 부모 객체
    public Toggle totalToggle, doctorToggle, nurseToggle, inpatientToggle, outpatientToggle, emergencyToggle, icuToggle;

    public GameObject feedbackGraphPanel;
    public Transform feedgraphContainer;    // 피드백 그래프를 그릴 부모 객체
    public Button feedBackButton;
    public Button backButton;
    public Button graphCloseButton;
    public Button feedbackCloseButton;

    public Transform feedbackContainer;
    public GameDataManager gameDataManager;
    public TextMeshProUGUI feedbackText;
    public float spacing = 50f;     // 프리팹 간의 간격 설정

    Dictionary<string, List<GameObject>> graphLines = new Dictionary<string, List<GameObject>>();

    void Start()
    {
        graphContainer = GameObject.Find("graphContainer").transform;
        totalToggle = GameObject.Find("TotalToggle").GetComponent<Toggle>();
        doctorToggle = GameObject.Find("DoctorlToggle").GetComponent<Toggle>();
        nurseToggle = GameObject.Find("NurseToggle").GetComponent<Toggle>();
        inpatientToggle = GameObject.Find("InpatientToggle").GetComponent<Toggle>();
        outpatientToggle = GameObject.Find("OutpatientToggle").GetComponent<Toggle>();
        emergencyToggle = GameObject.Find("EmergencyToggle").GetComponent<Toggle>();
        icuToggle = GameObject.Find("IcuToggle").GetComponent<Toggle>();
        feedbackGraphPanel = GameObject.Find("FeedbackGraphPanel");
        feedgraphContainer = GameObject.Find("FeedgraphContainer").transform;
        feedBackButton = GameObject.Find("FeedBackButton").GetComponent<Button>();
        backButton = GameObject.Find("BackButton").GetComponent<Button>();
        graphCloseButton = GameObject.Find("GraphCloseButton").GetComponent<Button>();
        feedbackCloseButton = GameObject.Find("FeedbackCloseButton").GetComponent<Button>();
        feedbackContainer = GameObject.Find("FeedbackContainer").transform;
        gameDataManager = FindObjectOfType<GameDataManager>();
        feedbackText = GameObject.Find("FeedbackText").GetComponent<TextMeshProUGUI>();

        // 토글별 그래프 리스트 초기화
        graphLines["total"] = new List<GameObject>();
        graphLines["doctor"] = new List<GameObject>();
        graphLines["nurse"] = new List<GameObject>();
        graphLines["inpatients"] = new List<GameObject>();
        graphLines["outpatients"] = new List<GameObject>();
        graphLines["emergencyPatients"] = new List<GameObject>();
        graphLines["icuPatients"] = new List<GameObject>();

        // 각 토글에 이벤트 리스너 추가
        totalToggle.onValueChanged.AddListener(isOn => ToggleGraphVisibility("total", isOn));
        doctorToggle.onValueChanged.AddListener(isOn => ToggleGraphVisibility("doctor", isOn));
        nurseToggle.onValueChanged.AddListener(isOn => ToggleGraphVisibility("nurse", isOn));
        inpatientToggle.onValueChanged.AddListener(isOn => ToggleGraphVisibility("inpatients", isOn));
        outpatientToggle.onValueChanged.AddListener(isOn => ToggleGraphVisibility("outpatients", isOn));
        emergencyToggle.onValueChanged.AddListener(isOn => ToggleGraphVisibility("emergencyPatients", isOn));
        icuToggle.onValueChanged.AddListener(isOn => ToggleGraphVisibility("icuPatients", isOn));

        feedbackGraphPanel.SetActive(false);

        feedBackButton.onClick.AddListener(OpenFeedback);
        backButton.onClick.AddListener(CloseFeedback);
        graphCloseButton.onClick.AddListener(QuitGame);
        feedbackCloseButton.onClick.AddListener(QuitGame);
    }

    public void DrawGraph(List<float> scores, string role, Transform container)
    {
        RectTransform graphRectTransform = container.GetComponent<RectTransform>();
        float graphWidth = graphRectTransform.sizeDelta.x;
        float graphHeight = graphRectTransform.sizeDelta.y;
        float yMax = 80f;  // y축 최댓값
        float xSpacing = graphWidth / (scores.Count - 1);    
        Vector2 previousPointPosition = Vector2.zero;

        // 선 생성
        for (int i = 0; i < scores.Count; i++)
        {
            float xPosition = i * xSpacing;                        
            float yValue = float.IsNaN(scores[i]) ? 0f : scores[i];
            float yPosition = (yValue / yMax) * graphHeight;

            yPosition = Mathf.Min(yPosition, graphHeight);  // y값은 최대 80
            Vector2 currentPointPosition = new Vector2(xPosition + graphWidth / 2 * (-1), yPosition - graphHeight / 2);

            // 이전 점과 현재 점 사이에 선 그리기
            if (i > 0)
            {
                var line = CreateLine(previousPointPosition, currentPointPosition, role, container);
                graphLines[role].Add(line);
            }

            previousPointPosition = currentPointPosition;   // 이전 점 위치 업데이트
        }
    }

    // 선 생성하는 메소드
    GameObject CreateLine(Vector2 start, Vector2 end, string role, Transform container)
    {
        GameObject linePrefab = Resources.Load<GameObject>($"Graph/{role}Line");
        GameObject line = Instantiate(linePrefab, container);
        RectTransform lineRectTransform = line.GetComponent<RectTransform>();

        Vector2 direction = (end - start).normalized;          // 선의 방향
        float distance = Vector2.Distance(start, end);         // 점 사이의 거리

        lineRectTransform.sizeDelta = new Vector2(distance, lineRectTransform.sizeDelta.y); // 선 길이 조정  * 1.5f
        lineRectTransform.anchoredPosition = start + (end - start) / 2;  // 두 점의 중앙에 선 배치

        // 회전 각도 계산
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        lineRectTransform.rotation = Quaternion.Euler(0, 0, angle);

        return line;
    }

    // 토글 상태에 따라 그래프 라인 표시/숨김
    void ToggleGraphVisibility(string role, bool isVisible)
    {
        foreach (var line in graphLines[role])
        {
            line.SetActive(isVisible);
        }
    }

    void OpenFeedback()
    {
        feedbackGraphPanel.SetActive(true);

        DrawGraph(GameDataManager.Instance.infectionRates, "total", feedgraphContainer);
        DrawGraph(GameDataManager.Instance.doctorInfectionRates, "doctor", feedgraphContainer);
        DrawGraph(GameDataManager.Instance.nurseInfectionRates, "nurse", feedgraphContainer);
        DrawGraph(GameDataManager.Instance.inpatientsRates, "inpatients", feedgraphContainer);
        DrawGraph(GameDataManager.Instance.outpatientsRates, "outpatients", feedgraphContainer);
        DrawGraph(GameDataManager.Instance.emergencyPatientsRates, "emergencyPatients", feedgraphContainer);
        DrawGraph(GameDataManager.Instance.icuPatientsRates, "icuPatients", feedgraphContainer);

        string str = "";

        for (int i = 0; i < 15; i++)
        {
            // 제목 설정
            if (GameDataManager.Instance.difference20More[i])
            {
                str += $"{i + 1}DAY - 감염률 급상승!\n";
            }
            else
            {
                str += $"{i + 1}DAY\n";
            }

            // 내용 설정
            if (gameDataManager.feedbackContent.ContainsKey(i))
            {
                string[] lines = gameDataManager.feedbackContent[i].Split('\n');

                foreach (string line in lines)
                {
                    if (!string.IsNullOrWhiteSpace(line)) // 빈 줄은 무시
                    {
                        str += $"- {line}\n";
                    }
                }
            }
            else
                str += $"No research done\n";

            str += '\n';
        }        
        feedbackText.text = str;

        //원본 백업
        gameDataManager.originalContent = feedbackText.text;

        //  레이아웃 재구성
        feedbackContainer.gameObject.SetActive(false);
        LayoutRebuilder.ForceRebuildLayoutImmediate(feedbackContainer as RectTransform);
        feedbackContainer.gameObject.SetActive(true);
    }

    void CloseFeedback()
    {
        feedbackGraphPanel.SetActive(false);
    }

    void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
