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

    public ItemInstance(ItemData itemData, string instanceId)
    {
        this.itemData = itemData;
        this.instanceId = string.IsNullOrEmpty(instanceId)
            ? Guid.NewGuid().ToString()
            : instanceId;
    }

    public void EnsureInstanceId()
    {
        if (string.IsNullOrEmpty(instanceId))
            instanceId = Guid.NewGuid().ToString();
    }
}
