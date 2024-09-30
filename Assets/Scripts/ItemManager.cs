using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item
{
    public string itemName;
    public bool isEquipped;
    public float protectionRate;

    public Item(string itemName, bool isEquipped, float protectionRate)
    {
        this.itemName = itemName;
        this.isEquipped = isEquipped;
        this.protectionRate = protectionRate;
    }
}

public class ItemManager
{ 
    public List<Item> items = InitItems();
    
    public static List<Item> InitItems()
    {
        return new List<Item>
        {
            new Item("Dental 마스크",false, 0.1f),
            new Item("N95 마스크",false, 0.2f),
            new Item("일회용 장갑",false, 0.1f),
            new Item("라텍스 장갑",false, 0.2f),
            new Item("의료용 헤어캡",false, 0.05f),
            new Item("의료용 고글",false, 0.3f),
            new Item("AP 가운",false, 0.4f),
            new Item("Level C",false, 0.7f)
        };
    }
}

