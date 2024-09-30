using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ProfileClickHandler : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public int personID;
    private ProfileInventoryManager inventoryManager;
    private Outline outline;

    public void Initialize(int id, ProfileInventoryManager manager)
    {
        personID = id;
        inventoryManager = manager;
        outline = gameObject.AddComponent<Outline>(); // Outline 컴포넌트 추가
        outline.effectColor = Color.yellow; // 테두리 색상 설정
        outline.effectDistance = new Vector2(5, 5); // 테두리 두께 설정
        outline.enabled = false; // 초기 상태에서는 테두리 비활성화
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (inventoryManager != null)
        {
            Person person = PersonManager.Instance.GetPerson(personID);

            //ProfileUI.Instance.ShowProfileUI(personID);
            if (person != null && !person.IsResting)
            {
                // 인벤토리 표시
                inventoryManager.ShowInventory(personID);
            }
            else if (person != null && person.IsResting)
            {
                Debug.Log($"{person.Name} is resting. Cannot open inventory.");
            }
        }
        else
        {
            Debug.LogError("InventoryManager is not assigned.");
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (outline != null)
        {
            outline.enabled = true; // 마우스 포인터가 들어올 때 테두리 활성화
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (outline != null)
        {
            outline.enabled = false; // 마우스 포인터가 나갈 때 테두리 비활성화
        }
    }
}
