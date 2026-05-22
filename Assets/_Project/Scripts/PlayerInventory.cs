using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("Database")]
    public ItemDatabase itemDatabase;

    [Header("Systems")]
    public EquipmentManager equipmentManager;

    public List<ItemInstance> ownedItems = new List<ItemInstance>();

    private bool isLoadingFromSave;

    private void Start()
    {
        EnsureOwnedItemInstanceIds();
        LoadInventoryAndEquipmentIfAvailable();
    }

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
        SaveInventoryAndEquipment();
    }

    public void RemoveItem(ItemInstance itemInstance)
    {
        if (itemInstance != null && ownedItems.Contains(itemInstance))
        {
            ownedItems.Remove(itemInstance);
            SaveInventoryAndEquipment();
        }
    }

    public ItemInstance GetItemByInstanceId(string instanceId)
    {
        if (string.IsNullOrEmpty(instanceId))
            return null;

        foreach (ItemInstance itemInstance in ownedItems)
        {
            if (itemInstance != null && itemInstance.instanceId == instanceId)
                return itemInstance;
        }

        return null;
    }

    public void EnsureOwnedItemInstanceIds()
    {
        foreach (ItemInstance itemInstance in ownedItems)
        {
            itemInstance?.EnsureInstanceId();
        }
    }

    public void SaveInventoryAndEquipment()
    {
        if (isLoadingFromSave)
            return;

        EnsureOwnedItemInstanceIds();
        PlayerSaveManager.SaveInventoryAndEquipment(this, GetEquipmentManager());
    }

    private void LoadInventoryAndEquipmentIfAvailable()
    {
        if (!PlayerSaveManager.TryLoad(out PlayerSaveManager.PlayerProgressionSaveData saveData))
        {
            ClearSceneDefaultInventory();
            EquipmentManager fallbackManager = GetEquipmentManager();

            if (fallbackManager != null)
                fallbackManager.RestoreEquipmentFromSave(new PlayerSaveManager.SavedEquipment(), this);

            return;
        }

        isLoadingFromSave = true;
        LoadInventory(saveData.inventoryItems);
        isLoadingFromSave = false;

        EquipmentManager manager = GetEquipmentManager();

        if (manager != null)
            manager.RestoreEquipmentFromSave(saveData.equipment, this);

        Debug.Log("PlayerInventory: Loaded " + ownedItems.Count + " inventory items from save.");
    }

    private void ClearSceneDefaultInventory()
    {
        if (ownedItems.Count <= 0)
            return;

        ownedItems.Clear();
        Debug.Log("PlayerInventory: Cleared scene default inventory because no player save exists.");
    }

    private void LoadInventory(List<PlayerSaveManager.SavedInventoryItem> savedItems)
    {
        ownedItems.Clear();

        if (savedItems == null)
            return;

        HashSet<string> loadedInstanceIds = new HashSet<string>();

        foreach (PlayerSaveManager.SavedInventoryItem savedItem in savedItems)
        {
            if (savedItem == null)
                continue;

            if (string.IsNullOrEmpty(savedItem.instanceId) || loadedInstanceIds.Contains(savedItem.instanceId))
                continue;

            ItemData itemData = ResolveItemData(savedItem.itemId);

            if (itemData == null)
            {
                Debug.LogWarning("PlayerInventory: Could not resolve saved item id '" + savedItem.itemId + "'.");
                continue;
            }

            ownedItems.Add(new ItemInstance(itemData, savedItem.instanceId));
            loadedInstanceIds.Add(savedItem.instanceId);
        }
    }

    private ItemData ResolveItemData(string itemId)
    {
        if (itemDatabase == null)
        {
            ItemDatabase[] databases = Resources.FindObjectsOfTypeAll<ItemDatabase>();
            itemDatabase = databases.Length > 0 ? databases[0] : null;
        }

        return itemDatabase != null ? itemDatabase.GetItemById(itemId) : null;
    }

    private EquipmentManager GetEquipmentManager()
    {
        if (equipmentManager != null)
            return equipmentManager;

        EquipmentManager[] managers = Resources.FindObjectsOfTypeAll<EquipmentManager>();

        foreach (EquipmentManager manager in managers)
        {
            if (manager != null && manager.gameObject.scene.IsValid())
            {
                equipmentManager = manager;
                return equipmentManager;
            }
        }

        return null;
    }
}
