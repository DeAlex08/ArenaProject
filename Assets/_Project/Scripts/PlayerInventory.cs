using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public List<ItemInstance> ownedItems = new List<ItemInstance>();

    public List<ItemInstance> GetItemsByType(ItemType type)
    {
        List<ItemInstance> result = new List<ItemInstance>();

        foreach (ItemInstance itemInstance in ownedItems)
        {
            if (itemInstance != null &&
                itemInstance.itemData != null &&
                itemInstance.itemData.itemType == type)
            {
                result.Add(itemInstance);
            }
        }

        return result;
    }

    public void AddItem(ItemData itemData)
    {
        if (itemData == null)
            return;

        ownedItems.Add(new ItemInstance(itemData));
    }

    public void RemoveItem(ItemInstance itemInstance)
    {
        if (itemInstance != null && ownedItems.Contains(itemInstance))
        {
            ownedItems.Remove(itemInstance);
        }
    }
}