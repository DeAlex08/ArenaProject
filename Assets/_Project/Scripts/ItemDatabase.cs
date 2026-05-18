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

    public ItemData GetItemById(string itemId)
    {
        if (string.IsNullOrEmpty(itemId))
            return null;

        foreach (ItemData item in allItems)
        {
            if (item == null)
                continue;

            if (GetStableItemId(item) == itemId)
                return item;
        }

        return null;
    }

    public static string GetStableItemId(ItemData item)
    {
        if (item == null)
            return string.Empty;

        if (!string.IsNullOrEmpty(item.itemId))
            return item.itemId;

        return item.name;
    }
}
