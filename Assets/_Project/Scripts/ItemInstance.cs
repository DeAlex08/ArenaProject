using System;
using UnityEngine;

[Serializable]
public class ItemInstance
{
    public string instanceId;
    public ItemData itemData;

    public ItemInstance(ItemData itemData)
    {
        this.itemData = itemData;
        instanceId = Guid.NewGuid().ToString();
    }
}