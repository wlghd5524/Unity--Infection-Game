using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item
{
    public string itemName;
    public bool isEquipped;
    public int protectionRate;

    public Item(string itemName, bool isEquipped, int protectionRate)
    {
        this.itemName = itemName;
        this.isEquipped = isEquipped;
        this.protectionRate = protectionRate;
    }

    // Clone 메서드로 개별 인스턴스를 생성
    public Item Clone()
    {
        return new Item(itemName, isEquipped, protectionRate);
    }
}

public class ItemManager
{
    public List<Item> items = InitItems();

    public static List<Item> InitItems()
    {
        return new List<Item>
        {
            new Item("Dental 마스크",false, 66),
            new Item("N95 마스크",false, 83),
            new Item("일회용 장갑",false, 25),
            new Item("라텍스 장갑",false, 50),
            new Item("의료용 헤어캡",false, 30),
            new Item("의료용 고글",false, 15),
            new Item("AP 가운",false, 30),
            new Item("Level C",false, 80)
        };
    }


}

