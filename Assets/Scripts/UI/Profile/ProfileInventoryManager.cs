using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ProfileInventoryManager : MonoBehaviour
{
    public Transform inventoryContainer;    // 인벤토리 아이템을 추가할 스크롤 뷰의 콘텐츠 영역
    public GameObject itemPrefab;           // 아이템 프리팹
    public TextMeshProUGUI moneyText;       // 게임 재화 Text
    public GameObject decreaseTextPrefab;   // 감소 텍스트 프리팹
    public MonthlyReportUI monthlyReportUI;

    void Start()
    {
        inventoryContainer = Assign(inventoryContainer, "InventoryContent");
        itemPrefab = Assign(itemPrefab, "ItemPrefab");
        moneyText = Assign(moneyText, "MoneyInfo");
        decreaseTextPrefab = Assign(decreaseTextPrefab, "DecreasePrefab");
        monthlyReportUI = FindObjectOfType<MonthlyReportUI>();

        // GridLayoutGroup 설정
        GridLayoutGroup gridLayoutGroup = inventoryContainer.GetComponent<GridLayoutGroup>();
        if (gridLayoutGroup != null)
        {
            gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayoutGroup.constraintCount = 1; // 한 줄에 들어가는 프로필 개수
            gridLayoutGroup.cellSize = new Vector2(200, 125); // 프로필의 크기 (필요에 따라 조정)
            gridLayoutGroup.spacing = new Vector2(10, 10); // 프로필 간의 간격
            gridLayoutGroup.padding = new RectOffset(10, 10, 10, 10); // 스크롤 뷰와의 간격
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

    // 인벤토리 활성화
    public void ShowInventory(int personID)
    {
        ClearInventory();

        Person person = PersonManager.Instance.GetPerson(personID);
        if (person == null)
        {
            Debug.LogError($"Person with ID {personID} not found.");
            return;
        }

        if (person.IsResting)
        {
            Debug.Log($"Person {person.Name} is resting. Inventory will not be shown.");
            return;
        }

        foreach (var item in person.Inventory)
        {
            GameObject itemObj = Instantiate(itemPrefab, inventoryContainer);
            TextMeshProUGUI itemNameText = itemObj.transform.Find("ItemNameText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI itemStatusText = itemObj.transform.Find("ItemStatusText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI purchaseStatusText = itemObj.transform.Find("PurchaseStatusText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI itemPriceText = itemObj.transform.Find("ItemPriceText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI protectionRateText = itemObj.transform.Find("ProtectionRateText").GetComponent<TextMeshProUGUI>();
            Image itemImage = itemObj.transform.Find("ItemImage").GetComponent<Image>(); // ItemImage 오브젝트에 접근

            if (itemNameText == null || itemStatusText == null || purchaseStatusText == null || itemPriceText == null)
            {
                Debug.LogError("Item prefab is missing required TextMeshProUGUI components.");
                continue;
            }

            itemNameText.text = item.Key;
            itemStatusText.text = item.Value.isEquipped ? "착용 중" : "착용 안함";
            purchaseStatusText.text = item.Value.isPurchased ? "구매 완료" : "구매 안됨";
            protectionRateText.text = $"+ {item.Value.protectionRate}";
            if (item.Value.isPurchased)
            {
                itemPriceText.gameObject.SetActive(false);
            }
            else
            {
                itemPriceText.text = $"가격: {item.Value.price}원";
                itemPriceText.gameObject.SetActive(true);
            }

            Sprite itemSprite = Resources.Load<Sprite>($"Sprites/ItemSprites/{item.Key}");
            if (itemSprite != null)
            {
                itemImage.sprite = itemSprite;
            }
            else
            {
                Debug.LogWarning($"Sprite for item '{item.Key}' not found in Resources/Sprites/ItemSprites.");
            }

            // 테두리 효과 추가
            Outline outline = itemObj.AddComponent<Outline>();
            outline.effectColor = Color.yellow;
            outline.effectDistance = new Vector2(5, 5);
            outline.enabled = false;

            // 마우스 포인터 이벤트 추가
            EventTrigger trigger = itemObj.AddComponent<EventTrigger>();

            EventTrigger.Entry pointerEnter = new EventTrigger.Entry();
            pointerEnter.eventID = EventTriggerType.PointerEnter;
            pointerEnter.callback.AddListener((eventData) => OnPointerEnter(outline));
            trigger.triggers.Add(pointerEnter);

            EventTrigger.Entry pointerExit = new EventTrigger.Entry();
            pointerExit.eventID = EventTriggerType.PointerExit;
            pointerExit.callback.AddListener((eventData) => OnPointerExit(outline));
            trigger.triggers.Add(pointerExit);

            EventTrigger.Entry equipEntry = new EventTrigger.Entry();
            equipEntry.eventID = EventTriggerType.PointerClick;
            equipEntry.callback.AddListener((eventData) => OnEquipItemClick(personID, item.Key, itemStatusText));
            trigger.triggers.Add(equipEntry);

            EventTrigger.Entry purchaseEntry = new EventTrigger.Entry();
            purchaseEntry.eventID = EventTriggerType.PointerClick;
            purchaseEntry.callback.AddListener((eventData) => OnPurchaseItemClick(personID, item.Key, purchaseStatusText, itemStatusText, itemPriceText));
            trigger.triggers.Add(purchaseEntry);

            Debug.Log($"Added item {item.Key} to inventory.");
        }
    }

    void OnPointerEnter(Outline outline)
    {
        outline.enabled = true;
    }

    void OnPointerExit(Outline outline)
    {
        outline.enabled = false;
    }

    public void ClearInventory()
    {
        foreach (Transform child in inventoryContainer)
        {
            Destroy(child.gameObject);
        }
        Debug.Log("Inventory cleared.");
    }

    void OnEquipItemClick(int personID, string itemName, TextMeshProUGUI itemStatusText)
    {
        Person person = PersonManager.Instance.GetPerson(personID);
        if (person != null)
        {
            if (person.Inventory[itemName].isPurchased)
            {
                var itemInfo = person.Inventory[itemName];

                // 착용 불가 상태인지 확인, 착용 불가 문구가 있으면 작동 안하도록 설정
                if (itemStatusText.text == "착용 불가")
                {
                    Debug.Log($"Cannot equip {itemName} because it is in a '착용 불가' state.");
                    return;
                }

                // 마스크 끼리의 상태 확인
                if ((itemName == "Dental 마스크" && person.Inventory.ContainsKey("N95 마스크") && person.Inventory["N95 마스크"].isEquipped) ||
                    (itemName == "N95 마스크" && person.Inventory.ContainsKey("Dental 마스크") && person.Inventory["Dental 마스크"].isEquipped))
                {
                    Debug.Log($"Cannot equip {itemName} because the other mask is already equipped.");
                    return;
                }
                // Level D 또는 Level C가 이미 착용 => 다른 아이템 착용 불가
                if ((itemName == "Level D" && person.Inventory.ContainsKey("Level C") && person.Inventory["Level C"].isEquipped) ||
                    (itemName == "Level C" && person.Inventory.ContainsKey("Level D") && person.Inventory["Level D"].isEquipped))
                {
                    Debug.Log($"Cannot equip {itemName} because the other Level protective gear is already equipped.");
                    return;
                }

                bool isEquipped = !itemInfo.isEquipped;
                person.Inventory[itemName] = new ItemInfo(itemInfo.isPurchased, isEquipped, itemInfo.price, itemInfo.protectionRate, itemInfo.stressIncreaseValue);
                itemStatusText.text = isEquipped ? "착용 중" : "착용 안함";
                PersonManager.Instance.UpdatePerson(person);
                Debug.Log($"Item {itemName} for person {person.Name} is now {(isEquipped ? "equipped" : "unequipped")}.");

                // Level D or Level C 착용 및 해제 시 다른 아이템 모두 해제 & 상태 갱신
                if (itemName == "Level D" || itemName == "Level C")
                {
                    HandleLevelDAndC(person, itemName, isEquipped);
                }

                // 다른 마스크 상태 갱신
                if (itemName == "Dental 마스크" || itemName == "N95 마스크")
                {
                    UpdateMaskStatus(person, itemName, isEquipped);
                }
            }
            else
            {
                Debug.Log($"Item {itemName} for person {person.Name} cannot be equipped because it is not purchased.");
            }
        }
    }

    void HandleLevelDAndC(Person person, string equippedLevel, bool isEquipped)
    {
        foreach (var item in person.Inventory.Keys.ToList())
        {
            if (item != equippedLevel && item != "PAPR")
            {
                // 모든 아이템 착용 해제 및 착용 불가 상태로 설정
                person.Inventory[item] = new ItemInfo(person.Inventory[item].isPurchased, false, person.Inventory[item].price, person.Inventory[item].protectionRate, person.Inventory[item].stressIncreaseValue);
                GameObject itemObj = inventoryContainer.Cast<Transform>().FirstOrDefault(t => t.Find("ItemNameText").GetComponent<TextMeshProUGUI>().text == item)?.gameObject;
                if (itemObj != null)
                {
                    TextMeshProUGUI itemStatusText = itemObj.transform.Find("ItemStatusText").GetComponent<TextMeshProUGUI>();
                    if (isEquipped)
                    {
                        itemStatusText.text = "착용 불가";
                    }
                    else
                    {
                        itemStatusText.text = "착용 안함";
                    }
                }
                Debug.Log($"Item {item} for person {person.Name} is now unequipped and status updated because {equippedLevel} is {(isEquipped ? "equipped" : "unequipped")}.");
            }
        }
    }

    void UpdateMaskStatus(Person person, string equippedMask, bool isEquipped)
    {
        string otherMask = equippedMask == "Dental 마스크" ? "N95 마스크" : "Dental 마스크";
        if (person.Inventory.ContainsKey(otherMask))
        {
            GameObject otherMaskItem = inventoryContainer.Cast<Transform>().FirstOrDefault(t => t.Find("ItemNameText").GetComponent<TextMeshProUGUI>().text == otherMask)?.gameObject;
            if (otherMaskItem != null)
            {
                TextMeshProUGUI otherMaskStatusText = otherMaskItem.transform.Find("ItemStatusText").GetComponent<TextMeshProUGUI>();
                if (isEquipped)
                {
                    // 현재 마스크 착용 => 다른 마스크 착용 불가 상태로 설정
                    otherMaskStatusText.text = "착용 불가";
                    person.Inventory[otherMask] = new ItemInfo(person.Inventory[otherMask].isPurchased, false, person.Inventory[otherMask].price, person.Inventory[otherMask].protectionRate, person.Inventory[otherMask].stressIncreaseValue);
                    Debug.Log($"Updated status of {otherMask} for person {person.Name} to unequipped because {equippedMask} is equipped.");
                }
                else
                {
                    // 현재 마스크 착용 해제 => 다른 마스크 착용 가능 상태로 설정
                    otherMaskStatusText.text = "착용 안함";
                    Debug.Log($"Updated status of {otherMask} for person {person.Name} to available because {equippedMask} is unequipped.");
                }
            }
        }
    }




    void OnPurchaseItemClick(int personID, string itemName, TextMeshProUGUI purchaseStatusText, TextMeshProUGUI itemStatusText, TextMeshProUGUI itemPriceText)
    {
        Person person = PersonManager.Instance.GetPerson(personID);
        if (person != null)
        {
            var itemInfo = person.Inventory[itemName];
            if (!itemInfo.isPurchased)
            {
                if (TrySpendMoney(itemInfo.price))
                {
                    person.Inventory[itemName] = new ItemInfo(true, itemInfo.isEquipped, itemInfo.price, itemInfo.protectionRate, itemInfo.stressIncreaseValue);
                    purchaseStatusText.text = "구매 완료";
                    itemPriceText.text = "";
                    PersonManager.Instance.UpdatePerson(person);
                    Debug.Log($"Item {itemName} for person {person.Name} has been purchased.");

                    ShowDecreaseText(itemInfo.price);
                }
                else
                {
                    Debug.Log("Not enough money to purchase the item.");
                }
            }
            else
            {
                Debug.Log($"Item {itemName} for person {person.Name} is already purchased.");
            }
        }
    }

    bool TrySpendMoney(int amount)
    {
        string moneyTextValue = moneyText.text;

        if (int.TryParse(moneyTextValue.Trim().Replace(",", ""), out int currentMoney))
        {
            if (currentMoney >= amount)
            {
                currentMoney -= amount;
                moneyText.text = $"{currentMoney:N0}";
                monthlyReportUI.AddExpense(amount);
                return true;
            }
            else
            {
                Debug.Log("Not enough money.");
                return false;
            }
        }
        else
        {
            Debug.LogError("Failed to parse money amount.");
            return false;
        }
    }


    void ShowDecreaseText(int amount)
    {
        GameObject decreaseTextObj = Instantiate(decreaseTextPrefab, moneyText.transform.parent);
        TextMeshProUGUI decreaseText = decreaseTextObj.GetComponent<TextMeshProUGUI>();
        decreaseText.text = $"-{amount}";

        StartCoroutine(AnimateDecreaseText(decreaseText));
    }

    IEnumerator AnimateDecreaseText(TextMeshProUGUI decreaseText)
    {
        float duration = 1.0f;
        float elapsed = 0.0f;
        Vector3 originalPosition = decreaseText.transform.localPosition;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            decreaseText.transform.localPosition = originalPosition + Vector3.up * (50 * progress);

            Color color = decreaseText.color;
            color.a = Mathf.Lerp(1, 0, progress);
            decreaseText.color = color;

            yield return null;
        }

        Destroy(decreaseText.gameObject);
    }
}

