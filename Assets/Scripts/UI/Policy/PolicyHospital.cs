using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PolicyHospital : MonoBehaviour
{
    public Toggle closingIn1toggle;
    public Toggle closingIn2toggle;
    public Toggle closingOut1toggle;
    public Toggle closingOut2toggle;
    public Toggle closingW1toggle;
    public Toggle closingW2toggle;
    public Toggle closingW3toggle;
    public Toggle closingW4toggle;
    public Toggle disinfectionIn1toggle;
    public Toggle disinfectionIn2toggle;
    public Toggle disinfectionOut1toggle;
    public Toggle disinfectionOut2toggle;
    public Toggle disinfectionWard1toggle;
    public Toggle disinfectionWard2toggle;
    public Toggle disinfectionWard3toggle;
    public Toggle disinfectionWard4toggle;

    public TextMeshProUGUI disinfectionIn1Text;
    public TextMeshProUGUI disinfectionIn2Text;
    public TextMeshProUGUI disinfectionOut1Text;
    public TextMeshProUGUI disinfectionOut2Text;
    public TextMeshProUGUI disinfectionWard1Text;
    public TextMeshProUGUI disinfectionWard2Text;
    public TextMeshProUGUI disinfectionWard3Text;
    public TextMeshProUGUI disinfectionWard4Text;

    public TextMeshProUGUI closingIn1Text;
    public TextMeshProUGUI closingIn2Text;
    public TextMeshProUGUI closingOut1Text;
    public TextMeshProUGUI closingOut2Text;
    public TextMeshProUGUI closingWard1Text;
    public TextMeshProUGUI closingWard2Text;
    public TextMeshProUGUI closingWard3Text;
    public TextMeshProUGUI closingWard4Text;

    // Start is called before the first frame update
    void Start()
    {
        closingIn1toggle = GameObject.Find("ClosingIn1toggle").GetComponent<Toggle>();
        closingIn2toggle = GameObject.Find("ClosingIn2toggle").GetComponent<Toggle>();
        closingOut1toggle = GameObject.Find("ClosingOut1toggle").GetComponent<Toggle>();
        closingOut2toggle = GameObject.Find("ClosingOut2toggle").GetComponent<Toggle>();
        closingW1toggle = GameObject.Find("ClosingWard1toggle").GetComponent<Toggle>();
        closingW2toggle = GameObject.Find("ClosingWard2toggle").GetComponent<Toggle>();
        closingW3toggle = GameObject.Find("ClosingWard3toggle").GetComponent<Toggle>();
        closingW4toggle = GameObject.Find("ClosingWard4toggle").GetComponent<Toggle>();
        disinfectionIn1toggle = GameObject.Find("DisinfectionIn1toggle").GetComponent<Toggle>();
        disinfectionIn2toggle = GameObject.Find("DisinfectionIn2toggle").GetComponent<Toggle>();
        disinfectionOut1toggle = GameObject.Find("DisinfectionOut1toggle").GetComponent<Toggle>();
        disinfectionOut2toggle = GameObject.Find("DisinfectionOut2toggle").GetComponent<Toggle>();
        disinfectionWard1toggle = GameObject.Find("DisinfectionWard1toggle").GetComponent<Toggle>();
        disinfectionWard2toggle = GameObject.Find("DisinfectionWard2toggle").GetComponent<Toggle>();
        disinfectionWard3toggle = GameObject.Find("DisinfectionWard3toggle").GetComponent<Toggle>();
        disinfectionWard4toggle = GameObject.Find("DisinfectionWard4toggle").GetComponent<Toggle>();

        disinfectionIn1Text = GameObject.Find("DisinfectionIn1Text").GetComponent<TextMeshProUGUI>();
        disinfectionIn2Text = GameObject.Find("DisinfectionIn2Text").GetComponent<TextMeshProUGUI>();
        disinfectionOut1Text = GameObject.Find("DisinfectionOut1Text").GetComponent<TextMeshProUGUI>();
        disinfectionOut2Text = GameObject.Find("DisinfectionOut2Text").GetComponent<TextMeshProUGUI>();
        disinfectionWard1Text = GameObject.Find("DisinfectionWard1Text").GetComponent<TextMeshProUGUI>();
        disinfectionWard2Text = GameObject.Find("DisinfectionWard2Text").GetComponent<TextMeshProUGUI>();
        disinfectionWard3Text = GameObject.Find("DisinfectionWard3Text").GetComponent<TextMeshProUGUI>();
        disinfectionWard4Text = GameObject.Find("DisinfectionWard4Text").GetComponent<TextMeshProUGUI>();

        closingIn1Text = GameObject.Find("ClosingIn1Text").GetComponent<TextMeshProUGUI>();
        closingIn2Text = GameObject.Find("ClosingIn2Text").GetComponent<TextMeshProUGUI>();
        closingOut1Text = GameObject.Find("ClosingOut1Text").GetComponent<TextMeshProUGUI>();
        closingOut2Text = GameObject.Find("ClosingOut2Text").GetComponent<TextMeshProUGUI>();
        closingWard1Text = GameObject.Find("ClosingWard1Text").GetComponent<TextMeshProUGUI>();
        closingWard2Text = GameObject.Find("ClosingWard2Text").GetComponent<TextMeshProUGUI>();
        closingWard3Text = GameObject.Find("ClosingWard3Text").GetComponent<TextMeshProUGUI>();
        closingWard4Text = GameObject.Find("ClosingWard4Text").GetComponent<TextMeshProUGUI>();

        // Toggle 초기화
        closingIn1toggle.isOn = false;
        closingIn2toggle.isOn = false;
        closingOut1toggle.isOn = false;
        closingOut2toggle.isOn = false;
        closingW1toggle.isOn = false;
        closingW2toggle.isOn = false;
        closingW3toggle.isOn = false;
        closingW4toggle.isOn = false;

        // 각 병동별 소독 및 폐쇄 토글 설정
        TogglePairControl(closingIn1toggle, disinfectionIn1toggle, disinfectionIn1Text);
        TogglePairControl(closingIn2toggle, disinfectionIn2toggle, disinfectionIn2Text);
        TogglePairControl(closingOut1toggle, disinfectionOut1toggle, disinfectionOut1Text);
        TogglePairControl(closingOut2toggle, disinfectionOut2toggle, disinfectionOut2Text);
        TogglePairControl(closingW1toggle, disinfectionWard1toggle, disinfectionWard1Text);
        TogglePairControl(closingW2toggle, disinfectionWard2toggle, disinfectionWard2Text);
        TogglePairControl(closingW3toggle, disinfectionWard3toggle, disinfectionWard3Text);
        TogglePairControl(closingW4toggle, disinfectionWard4toggle, disinfectionWard4Text);

        // Toggle 변경 시 UpdateWardCounts 호출
        closingIn1toggle.onValueChanged.AddListener(delegate { TogglePairControl(closingIn1toggle, disinfectionIn1toggle, disinfectionIn1Text); UpdateWardCounts(); });
        closingIn2toggle.onValueChanged.AddListener(delegate { TogglePairControl(closingIn2toggle, disinfectionIn2toggle, disinfectionIn2Text); UpdateWardCounts(); });
        closingOut1toggle.onValueChanged.AddListener(delegate { TogglePairControl(closingOut1toggle, disinfectionOut1toggle, disinfectionOut1Text); UpdateWardCounts(); });
        closingOut2toggle.onValueChanged.AddListener(delegate { TogglePairControl(closingOut2toggle, disinfectionOut2toggle, disinfectionOut2Text); UpdateWardCounts(); });
        closingW1toggle.onValueChanged.AddListener(delegate { TogglePairControl(closingW1toggle, disinfectionWard1toggle, disinfectionWard1Text); UpdateWardCounts(); });
        closingW2toggle.onValueChanged.AddListener(delegate { TogglePairControl(closingW2toggle, disinfectionWard2toggle, disinfectionWard2Text); UpdateWardCounts(); });
        closingW3toggle.onValueChanged.AddListener(delegate { TogglePairControl(closingW3toggle, disinfectionWard3toggle, disinfectionWard3Text); UpdateWardCounts(); });
        closingW4toggle.onValueChanged.AddListener(delegate { TogglePairControl(closingW4toggle, disinfectionWard4toggle, disinfectionWard4Text); UpdateWardCounts(); });

        // 일정 주기로 병동 데이터 업데이트
        StartCoroutine(UpdateWardCountsPeriodically());
    }

    // 병동별 의사, 간호사, 외래환자 수 업데이트
    IEnumerator UpdateWardCountsPeriodically()
    {
        while (true)
        {
            UpdateWardCounts();
            yield return new WaitForSeconds(1f); // 1초마다 업데이트
        }
    }

    void TogglePairControl(Toggle topToggle, Toggle bottomToggle, TextMeshProUGUI bottomText)
    {
        bottomToggle.interactable = topToggle.isOn;
        bottomText.text = topToggle.isOn ? "소독 가능" : "";
        if (!topToggle.isOn)
        {
            bottomToggle.isOn = false;
        }
    }

    void UpdateWardCounts()
    {
        var wardCounts = GetStaffAndOutpatientCounts();

        if (closingIn1toggle.isOn)
            closingIn1Text.text = "의사 x0\n간호사 x0\n외래환자 x0";
        else if (wardCounts.ContainsKey("내과 1"))
            closingIn1Text.text = $"의사 x{wardCounts["내과 1"].doctorCount}\n간호사 x{wardCounts["내과 1"].nurseCount}\n외래환자 x{wardCounts["내과 1"].outpatientCount}";

        if (closingIn2toggle.isOn)
            closingIn2Text.text = "의사 x0\n간호사 x0\n외래환자 x0";
        else if (wardCounts.ContainsKey("내과 2"))
            closingIn2Text.text = $"의사 x{wardCounts["내과 2"].doctorCount}\n간호사 x{wardCounts["내과 2"].nurseCount}\n외래환자 x{wardCounts["내과 2"].outpatientCount}";

        if (closingOut1toggle.isOn)
            closingOut1Text.text = "의사 x0\n간호사 x0\n외래환자 x0";
        else if (wardCounts.ContainsKey("외과 1"))
            closingOut1Text.text = $"의사 x{wardCounts["외과 1"].doctorCount}\n간호사 x{wardCounts["외과 1"].nurseCount}\n외래환자 x{wardCounts["외과 1"].outpatientCount}";

        if (closingOut2toggle.isOn)
            closingOut2Text.text = "의사 x0\n간호사 x0\n외래환자 x0";
        else if (wardCounts.ContainsKey("외과 2"))
            closingOut2Text.text = $"의사 x{wardCounts["외과 2"].doctorCount}\n간호사 x{wardCounts["외과 2"].nurseCount}\n외래환자 x{wardCounts["외과 2"].outpatientCount}";

        if (closingW1toggle.isOn)
            closingWard1Text.text = "의사 x0\n간호사 x0\n외래환자 x0";
        else if (wardCounts.ContainsKey("입원병동1"))
            closingWard1Text.text = $"의사 x{wardCounts["입원병동1"].doctorCount}\n간호사 x{wardCounts["입원병동1"].nurseCount}\n외래환자 x{wardCounts["입원병동1"].outpatientCount}";

        if (closingW2toggle.isOn)
            closingWard2Text.text = "의사 x0\n간호사 x0\n외래환자 x0";
        else if (wardCounts.ContainsKey("입원병동2"))
            closingWard2Text.text = $"의사 x{wardCounts["입원병동2"].doctorCount}\n간호사 x{wardCounts["입원병동2"].nurseCount}\n외래환자 x{wardCounts["입원병동2"].outpatientCount}";

        if (closingW3toggle.isOn)
            closingWard3Text.text = "의사 x0\n간호사 x0\n외래환자 x0";
        else if (wardCounts.ContainsKey("입원병동3"))
            closingWard3Text.text = $"의사 x{wardCounts["입원병동3"].doctorCount}\n간호사 x{wardCounts["입원병동3"].nurseCount}\n외래환자 x{wardCounts["입원병동3"].outpatientCount}";

        if (closingW4toggle.isOn)
            closingWard4Text.text = "의사 x0\n간호사 x0\n외래환자 x0";
        else if (wardCounts.ContainsKey("입원병동4"))
            closingWard4Text.text = $"의사 x{wardCounts["입원병동4"].doctorCount}\n간호사 x{wardCounts["입원병동4"].nurseCount}\n외래환자 x{wardCounts["입원병동4"].outpatientCount}";
    }

    public Dictionary<string, (int doctorCount, int nurseCount, int outpatientCount)> GetStaffAndOutpatientCounts()
    {
        // 병동별 의사, 간호사, 외래환자 수를 저장
        Dictionary<string, (int doctorCount, int nurseCount, int outpatientCount)> wardCounts = new Dictionary<string, (int, int, int)>();

        foreach (Ward ward in Ward.wards)
        {
            // 내과1, 내과2, 외과1, 외과2 및 입원병동1~4
            if (ward.num >= 0 && ward.num <= 7)
            {
                (int doctorCount, int nurseCount, int outpatientCount) = ward.GetCounts();
                wardCounts.Add(ward.WardName, (doctorCount, nurseCount, outpatientCount));
            }
        }

        return wardCounts;
    }
}
