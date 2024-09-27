using System.Collections.Generic;
using UnityEngine;

public class MaskController : MonoBehaviour
{
    public Material maskMaterial;

    public float maskX;
    public float maskY;
    public float maskWidth;
    public float maskHeight;

    public List<Vector4> maskPositions;

    void Start()
    {
        // 초기 마스크 설정
        if (maskPositions != null && maskPositions.Count > 0)
        {
            UpdateMask(0);  // 첫 번째 마스크 값으로 초기화
        }
    }

    void Update()
    {
    }

    // 현재 index에 해당하는 마스크를 업데이트
    public void UpdateMask(int index)
    {
        if (index >= 0 && index < maskPositions.Count)
        {
            Vector4 maskValue = maskPositions[index];
            maskMaterial.SetVector("_MaskRect", maskValue);
        }
    }
}
