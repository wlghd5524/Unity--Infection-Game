using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GraphManager : MonoBehaviour
{
    public Transform graphContainer;    // 그래프를 그릴 부모 객체

    public void DrawGraph(List<float> scores)
    {
        RectTransform graphRectTransform = graphContainer.GetComponent<RectTransform>();
        float graphWidth = graphRectTransform.sizeDelta.x;  // 그래프 영역의 넓이(5060.209f;)
        float graphHeight = graphRectTransform.sizeDelta.y; // 그래프 영역의 높이(2190.399f;)

        float yMax = 100f;                                   // y축 최댓값(100%)

        float xSpacing = graphWidth / (scores.Count - 1);    // 점 간의 x 간격 계산
        float graphW = graphWidth / 2 * (-1);
        float graphH = graphHeight / 2 * (-1);

        Vector2 previousPointPosition = Vector2.zero;

        // 점과 선 생성
        for (int i = 0; i < scores.Count; i++)
        {
            float xPosition = i * xSpacing;                         // x축 간격 
            float yPosition = (scores[i] / yMax) * graphHeight;     // 점의 y 위치

            Vector2 currentPointPosition = new Vector2(xPosition + graphW, yPosition + graphH);
            //CreatePoint(currentPointPosition);   

            // 이전 점과 현재 점 사이에 선 그리기
            if (i > 0)
            {
                CreateLine(previousPointPosition, currentPointPosition);
            }

            previousPointPosition = currentPointPosition;   // 이전 점 위치 업데이트
        }
    }

    // 점 생성하는 메소드
    void CreatePoint(Vector2 anchoredPosition)
    {
        GameObject point = Instantiate(Resources.Load<GameObject>("Graph/bigDot"), graphContainer);
        point.GetComponent<RectTransform>().anchoredPosition = anchoredPosition;
    }

    // 선 생성하는 메소드
    void CreateLine(Vector2 start, Vector2 end)
    {
        GameObject line = Instantiate(Resources.Load<GameObject>("Graph/blueLine"), graphContainer);
        RectTransform lineRectTransform = line.GetComponent<RectTransform>();

        Vector2 direction = (end - start).normalized;          // 선의 방향
        float distance = Vector2.Distance(start, end);         // 점 사이의 거리

        lineRectTransform.sizeDelta = new Vector2(distance, lineRectTransform.sizeDelta.y * 1.5f); // 선 길이 조정
        lineRectTransform.anchoredPosition = start + (end - start) / 2;  // 두 점의 중앙에 선 배치
        lineRectTransform.rotation = Quaternion.Euler(0, 0, GetAngleFromVector(direction));  // 각도에 맞게 선 회전
    }

    // 두 점 사이의 각도 계산하기
    float GetAngleFromVector(Vector2 dir)
    {
        return Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
    }
}
