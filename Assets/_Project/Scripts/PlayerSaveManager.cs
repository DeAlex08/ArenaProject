using System;
using System.IO;
using UnityEngine;

public static class PlayerSaveManager
{
    private const string SaveFileName = "arena_player_progression.json";

    [Serializable]
    public class PlayerProgressionSaveData
    {
        public int level;
        public int currentExp;
        public int maxExp;
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

        PlayerProgressionSaveData saveData = new PlayerProgressionSaveData
        {
            level = playerStats.level,
            currentExp = playerStats.currentExp,
            maxExp = playerStats.maxExp,
            availableStatPoints = playerStats.availableStatPoints,
            arenaTokens = playerStats.arenaTokens,
            allocatedStrength = playerStats.allocatedStrength,
            allocatedRage = playerStats.allocatedRage,
            allocatedReaction = playerStats.allocatedReaction,
            allocatedAgility = playerStats.allocatedAgility,
            allocatedEndurance = playerStats.allocatedEndurance,
            allocatedArmor = playerStats.allocatedArmor,
            allocatedLuck = playerStats.allocatedLuck,
            allocatedIntelligence = playerStats.allocatedIntelligence
        };

        try
        {
            string json = JsonUtility.ToJson(saveData, true);
            File.WriteAllText(SavePath, json);
            Debug.Log("PlayerSaveManager: Saved player progression to " + SavePath);
        }
        catch (Exception exception)
        {
            Debug.LogWarning("PlayerSaveManager: Failed to save player progression. " + exception.Message);
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
