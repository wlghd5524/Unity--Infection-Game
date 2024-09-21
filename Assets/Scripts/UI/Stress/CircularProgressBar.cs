using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CircularProgressBar : MonoBehaviour
{
    public Image progressBar; // 프로그래스 바 이미지
    public float progress = 0f; // 진행도 (0에서 1 사이의 값)

    void Start()
    {
        progressBar = Assign(progressBar, "PrograssBar");
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

    private void Update()
    {
        // 진행도를 이미지에 반영
        progressBar.fillAmount = progress;
    }

    // 진행도를 설정하는 함수
    public void SetProgress(float value)
    {
        progress = Mathf.Clamp01(value); // 0에서 1 사이로 값 제한
    }
    public void SetColor(Color color)
    {
        progressBar.color = color;
    }
}
