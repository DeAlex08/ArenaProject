using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class PlayerSaveManager
{
    private const string SaveFileName = "arena_player_progression.json";
    public const int FreshNewPlayerArenaTokens = 0;

    [Serializable]
    public class PlayerProgressionSaveData
    {
        public int level = 1;
        public int currentExp;
        public int maxExp = 100;
        public int availableStatPoints;
        public int arenaTokens;
        public int allocatedStrength;
        public int allocatedRage;
        public int allocatedReaction;
        public int allocatedAgility;
        public int allocatedEndurance;
        public int allocatedArmor;
        public int allocatedLuck;
        public int allocatedIntelligence;
        public string selectedArenaStance = "Standard";
        public int towerUnlockedFloor = 1;
        public List<int> towerClearedFloors = new List<int>();
        public List<SavedInventoryItem> inventoryItems = new List<SavedInventoryItem>();
        public SavedEquipment equipment = new SavedEquipment();
    }

    [Serializable]
    public class SavedInventoryItem
    {
        public string instanceId;
        public string itemId;
    }

    [Serializable]
    public class SavedEquipment
    {
        public string helmet;
        public string weapon1;
        public string weapon2;
        public string armor;
        public string gloves;
        public string belt;
        public string legs;
        public string boots;
        public string ring1;
        public string ring2;
        public string ring3;
        public string ring4;
        public string amulet;
        public string artifact;
    }

    public static string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

    public static bool TryLoad(out PlayerProgressionSaveData saveData)
    {
        saveData = null;

        if (!File.Exists(SavePath))
            return false;

        try
        {
            string json = File.ReadAllText(SavePath);
            saveData = JsonUtility.FromJson<PlayerProgressionSaveData>(json);
            EnsureSaveDefaults(saveData);
            return saveData != null;
        }
        catch (Exception exception)
        {
            Debug.LogWarning("PlayerSaveManager: Failed to load player save. " + exception.Message);
            return false;
        }
    }

    public static void Save(PlayerStats playerStats)
    {
        if (playerStats == null)
            return;

        PlayerProgressionSaveData saveData = LoadOrCreateSaveData();

        saveData.level = playerStats.level;
        saveData.currentExp = playerStats.currentExp;
        saveData.maxExp = playerStats.maxExp;
        saveData.availableStatPoints = playerStats.availableStatPoints;
        saveData.arenaTokens = playerStats.arenaTokens;
        saveData.allocatedStrength = playerStats.allocatedStrength;
        saveData.allocatedRage = playerStats.allocatedRage;
        saveData.allocatedReaction = playerStats.allocatedReaction;
        saveData.allocatedAgility = playerStats.allocatedAgility;
        saveData.allocatedEndurance = playerStats.allocatedEndurance;
        saveData.allocatedArmor = playerStats.allocatedArmor;
        saveData.allocatedLuck = playerStats.allocatedLuck;
        saveData.allocatedIntelligence = playerStats.allocatedIntelligence;

        WriteSaveData(saveData);
    }

    public static void SaveInventoryAndEquipment(PlayerInventory inventory, EquipmentManager equipmentManager)
    {
        if (inventory == null)
            return;

        PlayerProgressionSaveData saveData = LoadOrCreateSaveData();
        saveData.inventoryItems = BuildInventorySave(inventory);

        if (equipmentManager != null)
            saveData.equipment = equipmentManager.BuildEquipmentSaveData();

        WriteSaveData(saveData);
    }

    public static CombatStance LoadArenaStance(CombatStance fallbackStance)
    {
        if (!TryLoad(out PlayerProgressionSaveData saveData))
            return fallbackStance;

        if (string.IsNullOrEmpty(saveData.selectedArenaStance))
            return fallbackStance;

        try
        {
            return (CombatStance)Enum.Parse(typeof(CombatStance), saveData.selectedArenaStance);
        }
        catch (Exception)
        {
            Debug.LogWarning("PlayerSaveManager: Unknown saved Arena stance: " + saveData.selectedArenaStance);
            return fallbackStance;
        }
    }

    public static void SaveArenaStance(CombatStance stance)
    {
        PlayerProgressionSaveData saveData = LoadOrCreateSaveData();
        saveData.selectedArenaStance = stance.ToString();
        WriteSaveData(saveData);
    }

    public static int LoadTowerUnlockedFloor(int fallbackFloor)
    {
        if (!TryLoad(out PlayerProgressionSaveData saveData))
            return Mathf.Max(fallbackFloor, 1);

        return Mathf.Max(saveData.towerUnlockedFloor, 1);
    }

    public static List<int> LoadTowerClearedFloors()
    {
        if (!TryLoad(out PlayerProgressionSaveData saveData))
            return new List<int>();

        if (saveData.towerClearedFloors == null)
            return new List<int>();

        return new List<int>(saveData.towerClearedFloors);
    }

    public static void SaveTowerProgress(int unlockedFloor, List<int> clearedFloors)
    {
        PlayerProgressionSaveData saveData = LoadOrCreateSaveData();
        saveData.towerUnlockedFloor = Mathf.Max(unlockedFloor, 1);
        saveData.towerClearedFloors = SanitizeTowerClearedFloors(clearedFloors);
        WriteSaveData(saveData);

        Debug.Log(
            "PlayerSaveManager: Saved Tower progress. Unlocked floor: " +
            saveData.towerUnlockedFloor +
            ", cleared floors: " +
            saveData.towerClearedFloors.Count);
    }

    private static List<SavedInventoryItem> BuildInventorySave(PlayerInventory inventory)
    {
        List<SavedInventoryItem> savedItems = new List<SavedInventoryItem>();

        foreach (ItemInstance itemInstance in inventory.ownedItems)
        {
            if (itemInstance == null || itemInstance.itemData == null)
                continue;

            itemInstance.EnsureInstanceId();

            savedItems.Add(new SavedInventoryItem
            {
                instanceId = itemInstance.instanceId,
                itemId = ItemDatabase.GetStableItemId(itemInstance.itemData)
            });
        }

        return savedItems;
    }

    private static PlayerProgressionSaveData LoadOrCreateSaveData()
    {
        if (TryLoad(out PlayerProgressionSaveData saveData))
            return saveData;

        return new PlayerProgressionSaveData();
    }

    public static void ResetToFreshNewPlayerSave()
    {
        PlayerProgressionSaveData saveData = CreateFreshNewPlayerSaveData(FreshNewPlayerArenaTokens);
        WriteSaveData(saveData);

        Debug.Log("PlayerSaveManager: Reset player save to fresh new player state.");
    }

    private static PlayerProgressionSaveData CreateFreshNewPlayerSaveData(int starterArenaTokens)
    {
        return new PlayerProgressionSaveData
        {
            level = 1,
            currentExp = 0,
            maxExp = 100,
            availableStatPoints = 0,
            arenaTokens = Mathf.Max(starterArenaTokens, 0),
            allocatedStrength = 0,
            allocatedRage = 0,
            allocatedReaction = 0,
            allocatedAgility = 0,
            allocatedEndurance = 0,
            allocatedArmor = 0,
            allocatedLuck = 0,
            allocatedIntelligence = 0,
            selectedArenaStance = "Standard",
            towerUnlockedFloor = 1,
            towerClearedFloors = new List<int>(),
            inventoryItems = new List<SavedInventoryItem>(),
            equipment = new SavedEquipment()
        };
    }

    private static List<int> SanitizeTowerClearedFloors(List<int> clearedFloors)
    {
        List<int> sanitizedFloors = new List<int>();

        if (clearedFloors == null)
            return sanitizedFloors;

        foreach (int floor in clearedFloors)
        {
            if (floor < 1 || sanitizedFloors.Contains(floor))
                continue;

            sanitizedFloors.Add(floor);
        }

        sanitizedFloors.Sort();
        return sanitizedFloors;
    }

    private static void EnsureSaveDefaults(PlayerProgressionSaveData saveData)
    {
        if (saveData == null)
            return;

        if (saveData.inventoryItems == null)
            saveData.inventoryItems = new List<SavedInventoryItem>();

        if (saveData.equipment == null)
            saveData.equipment = new SavedEquipment();

        if (string.IsNullOrEmpty(saveData.selectedArenaStance))
            saveData.selectedArenaStance = "Standard";

        saveData.towerUnlockedFloor = Mathf.Max(saveData.towerUnlockedFloor, 1);

        if (saveData.towerClearedFloors == null)
            saveData.towerClearedFloors = new List<int>();
    }

    private static void WriteSaveData(PlayerProgressionSaveData saveData)
    {
        try
        {
            string json = JsonUtility.ToJson(saveData, true);
            File.WriteAllText(SavePath, json);
            Debug.Log("PlayerSaveManager: Saved player data to " + SavePath);
        }
        catch (Exception exception)
        {
            Debug.LogWarning("PlayerSaveManager: Failed to save player data. " + exception.Message);
        }
    }

    public static void ClearSave()
    {
        if (!File.Exists(SavePath))
            return;

        try
        {
            File.Delete(SavePath);
            Debug.Log("PlayerSaveManager: Cleared player save at " + SavePath);
        }
        catch (Exception exception)
        {
            Debug.LogWarning("PlayerSaveManager: Failed to clear player save. " + exception.Message);
        }
    }
}
