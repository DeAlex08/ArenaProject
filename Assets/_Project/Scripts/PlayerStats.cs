using UnityEngine;

public enum PlayerStatType
{
    Strength,
    Rage,
    Reaction,
    Agility,
    Endurance,
    Armor,
    Luck,
    Intelligence
}

public class PlayerStats : MonoBehaviour
{
    [Header("Identity")]
    public string playerName = "ХГ";
    public int level = 1;

    [Header("Resources")]
    public int currentHp = 250;
    public int maxHp = 1000;

    public int currentMp = 80;
    public int maxMp = 1000;

    public int currentExp = 0;
    public int maxExp = 100;

    [Header("Currencies")]
    public int arenaTokens = 0;

    [Header("Resource Scaling")]
    public int baseHp = 100;
    public int baseMp = 100;
    public int hpPerEndurance = 10;
    public int mpPerIntelligence = 10;

    [Header("Start Stats")]
    public int startStrength = 10;
    public int startRage = 5;
    public int startReaction = 10;
    public int startAgility = 10;
    public int startEndurance = 10;
    public int startArmor = 0;
    public int startLuck = 5;
    public int startIntelligence = 10;

    [Header("Growth Per Level")]
    public int strengthPerLevel = 2;
    public int ragePerLevel = 1;
    public int reactionPerLevel = 1;
    public int agilityPerLevel = 1;
    public int endurancePerLevel = 2;
    public int armorPerLevel = 1;
    public int luckPerLevel = 1;
    public int intelligencePerLevel = 1;

    [Header("Manual Stat Points")]
    public int statPointsPerLevel = 5;
    public int availableStatPoints = 0;

    public int allocatedStrength = 0;
    public int allocatedRage = 0;
    public int allocatedReaction = 0;
    public int allocatedAgility = 0;
    public int allocatedEndurance = 0;
    public int allocatedArmor = 0;
    public int allocatedLuck = 0;
    public int allocatedIntelligence = 0;

    [Header("Native Stats")]
    public int nativeStrength;
    public int nativeRage;
    public int nativeReaction;
    public int nativeAgility;
    public int nativeEndurance;
    public int nativeArmor;
    public int nativeLuck;
    public int nativeIntelligence;

    [Header("Equipment Bonus Stats")]
    public int bonusStrength;
    public int bonusRage;
    public int bonusReaction;
    public int bonusAgility;
    public int bonusEndurance;
    public int bonusArmor;
    public int bonusLuck;
    public int bonusIntelligence;

    [Header("Final Stats")]
    public int strength;
    public int rage;
    public int reaction;
    public int agility;
    public int endurance;
    public int armor;
    public int luck;
    public int intelligence;
    public int combatPower;

    private void Start()
    {
        LoadProgression();
        EnsureValidProgressionValues();
        SyncAvailableStatPointsWithLevel();
        RecalculateStats();
        RefreshEquipmentBonusesIfAvailable();

        currentHp = maxHp;
        currentMp = maxMp;
    }

    public void RecalculateStats()
    {
        int levelBonus = Mathf.Max(level - 1, 0);

        nativeStrength = startStrength + strengthPerLevel * levelBonus + allocatedStrength;
        nativeRage = startRage + ragePerLevel * levelBonus + allocatedRage;
        nativeReaction = startReaction + reactionPerLevel * levelBonus + allocatedReaction;
        nativeAgility = startAgility + agilityPerLevel * levelBonus + allocatedAgility;
        nativeEndurance = startEndurance + endurancePerLevel * levelBonus + allocatedEndurance;
        nativeArmor = startArmor + armorPerLevel * levelBonus + allocatedArmor;
        nativeLuck = startLuck + luckPerLevel * levelBonus + allocatedLuck;
        nativeIntelligence = startIntelligence + intelligencePerLevel * levelBonus + allocatedIntelligence;

        bonusStrength = 0;
        bonusRage = 0;
        bonusReaction = 0;
        bonusAgility = 0;
        bonusEndurance = 0;
        bonusArmor = 0;
        bonusLuck = 0;
        bonusIntelligence = 0;

        UpdateFinalStats();
    }

    public void ApplyItemStats(ItemData item)
    {
        if (item == null)
            return;

        bonusStrength += item.strength;
        bonusRage += item.rage;
        bonusReaction += item.reaction;
        bonusAgility += item.agility;
        bonusEndurance += item.endurance;
        bonusArmor += item.armor;
        bonusLuck += item.luck;
        bonusIntelligence += item.intelligence;

        UpdateFinalStats();
    }

    private void UpdateFinalStats()
    {
        float hpPercent = maxHp > 0 ? (float)currentHp / maxHp : 1f;
        float mpPercent = maxMp > 0 ? (float)currentMp / maxMp : 1f;

        strength = nativeStrength + bonusStrength;
        rage = nativeRage + bonusRage;
        reaction = nativeReaction + bonusReaction;
        agility = nativeAgility + bonusAgility;
        endurance = nativeEndurance + bonusEndurance;
        armor = nativeArmor + bonusArmor;
        luck = nativeLuck + bonusLuck;
        intelligence = nativeIntelligence + bonusIntelligence;

        maxHp = baseHp + endurance * hpPerEndurance;
        maxMp = baseMp + intelligence * mpPerIntelligence;

        currentHp = Mathf.RoundToInt(maxHp * hpPercent);
        currentMp = Mathf.RoundToInt(maxMp * mpPercent);

        CalculateCombatPower();
    }

    public void LevelUp()
    {
        level++;
        availableStatPoints += statPointsPerLevel;

        RecalculateStats();
        SaveProgression();
    }

    public int AddExperience(int amount)
    {
        if (amount <= 0)
            return 0;

        currentExp += amount;
        int levelsGained = 0;

        while (maxExp > 0 && currentExp >= maxExp)
        {
            currentExp -= maxExp;
            maxExp = CalculateNextMaxExp(maxExp);
            LevelUp();
            levelsGained++;

            Debug.Log("PlayerStats: Level up! New level: " + level);
        }

        Debug.Log(
            "PlayerStats: Added EXP: " +
            amount +
            ". Current EXP: " +
            currentExp +
            "/" +
            maxExp);

        SaveProgression();

        return levelsGained;
    }

    public void AddArenaTokens(int amount)
    {
        if (amount <= 0)
            return;

        arenaTokens += amount;

        Debug.Log(
            "PlayerStats: Added Arena Tokens: " +
            amount +
            ". Total Arena Tokens: " +
            arenaTokens);

        SaveProgression();
    }

    public void SyncAvailableStatPointsWithLevel()
    {
        int earnedPoints = Mathf.Max(level - 1, 0) * statPointsPerLevel;
        int unspentPoints = earnedPoints - GetAllocatedStatPoints();

        availableStatPoints = Mathf.Max(availableStatPoints, unspentPoints);
    }

    private int CalculateNextMaxExp(int currentRequiredExp)
    {
        return Mathf.Max(Mathf.RoundToInt(currentRequiredExp * 1.2f), currentRequiredExp + 25);
    }

    public int GetAllocatedStatPoints()
    {
        return allocatedStrength +
               allocatedRage +
               allocatedReaction +
               allocatedAgility +
               allocatedEndurance +
               allocatedArmor +
               allocatedLuck +
               allocatedIntelligence;
    }

    public bool TryAllocateStat(PlayerStatType statType)
    {
        if (availableStatPoints <= 0)
            return false;

        switch (statType)
        {
            case PlayerStatType.Strength:
                allocatedStrength++;
                break;

            case PlayerStatType.Rage:
                allocatedRage++;
                break;

            case PlayerStatType.Reaction:
                allocatedReaction++;
                break;

            case PlayerStatType.Agility:
                allocatedAgility++;
                break;

            case PlayerStatType.Endurance:
                allocatedEndurance++;
                break;

            case PlayerStatType.Armor:
                allocatedArmor++;
                break;

            case PlayerStatType.Luck:
                allocatedLuck++;
                break;

            case PlayerStatType.Intelligence:
                allocatedIntelligence++;
                break;
        }

        availableStatPoints--;
        RecalculateStats();
        SaveProgression();

        return true;
    }

    public void SaveProgression()
    {
        PlayerSaveManager.Save(this);
    }

    [ContextMenu("Clear Saved Progression")]
    public void ClearSavedProgression()
    {
        PlayerSaveManager.ClearSave();
    }

    private void LoadProgression()
    {
        if (!PlayerSaveManager.TryLoad(out PlayerSaveManager.PlayerProgressionSaveData saveData))
            return;

        level = saveData.level;
        currentExp = saveData.currentExp;
        maxExp = saveData.maxExp;
        availableStatPoints = saveData.availableStatPoints;
        arenaTokens = saveData.arenaTokens;
        allocatedStrength = saveData.allocatedStrength;
        allocatedRage = saveData.allocatedRage;
        allocatedReaction = saveData.allocatedReaction;
        allocatedAgility = saveData.allocatedAgility;
        allocatedEndurance = saveData.allocatedEndurance;
        allocatedArmor = saveData.allocatedArmor;
        allocatedLuck = saveData.allocatedLuck;
        allocatedIntelligence = saveData.allocatedIntelligence;

        Debug.Log("PlayerStats: Loaded saved progression.");
    }

    private void EnsureValidProgressionValues()
    {
        level = Mathf.Max(level, 1);
        currentExp = Mathf.Max(currentExp, 0);
        maxExp = Mathf.Max(maxExp, 1);
        availableStatPoints = Mathf.Max(availableStatPoints, 0);
        arenaTokens = Mathf.Max(arenaTokens, 0);
    }

    private void RefreshEquipmentBonusesIfAvailable()
    {
        EquipmentManager[] managers = Resources.FindObjectsOfTypeAll<EquipmentManager>();

        foreach (EquipmentManager manager in managers)
        {
            if (manager == null || !manager.gameObject.scene.IsValid())
                continue;

            if (manager.playerStats != this)
                continue;

            manager.RefreshPlayerStats();
            return;
        }
    }

    public void AllocateStrength()
    {
        TryAllocateStat(PlayerStatType.Strength);
    }

    public void AllocateRage()
    {
        TryAllocateStat(PlayerStatType.Rage);
    }

    public void AllocateReaction()
    {
        TryAllocateStat(PlayerStatType.Reaction);
    }

    public void AllocateAgility()
    {
        TryAllocateStat(PlayerStatType.Agility);
    }

    public void AllocateEndurance()
    {
        TryAllocateStat(PlayerStatType.Endurance);
    }

    public void AllocateArmor()
    {
        TryAllocateStat(PlayerStatType.Armor);
    }

    public void AllocateLuck()
    {
        TryAllocateStat(PlayerStatType.Luck);
    }

    public void AllocateIntelligence()
    {
        TryAllocateStat(PlayerStatType.Intelligence);
    }

    private void CalculateCombatPower()
    {
        combatPower =
            level * 100 +
            strength * 10 +
            rage * 8 +
            reaction * 8 +
            agility * 8 +
            endurance * 10 +
            armor * 5 +
            luck * 6 +
            intelligence * 8;
    }
}
