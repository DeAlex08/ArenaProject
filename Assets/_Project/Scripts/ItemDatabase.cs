using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Arena RPG/Item Database")]
public class ItemDatabase : ScriptableObject
{
    public List<ItemData> allItems = new List<ItemData>();

    public List<ItemData> GetItemsByType(ItemType type)
    {
        List<ItemData> result = new List<ItemData>();

        foreach (ItemData item in allItems)
        {
            if (item != null && item.itemType == type)
            {
                result.Add(item);
            }
        }

        return result;
    }
}
