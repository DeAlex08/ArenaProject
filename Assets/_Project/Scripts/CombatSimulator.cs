using System.Collections.Generic;
using System.Text;
using UnityEngine;

public enum CombatBodyZone
{
    Head,
    Body,
    LeftArm,
    RightArm,
    Legs
}

public enum CombatStance
{
    Aggressive,
    Standard,
    Defensive
}

public enum CombatBlockType
{
    None,
    Weapon,
    Shield
}

public enum CombatOutcome
{
    Victory,
    Defeat,
    Draw
}

public static class CombatSimulator
{
    public const int MaxRounds = 20;

    public enum CombatPlaybackEventType
    {
        RoundStart,
        Hit,
        Dodge,
        RoundEnd
    }

    public class FighterData
    {
        public string fighterName;
        public int level;
        public int maxHp;
        public int attack;
        public int defense;
        public int strength;
        public int rage;
        public int reaction;
        public int agility;
        public int armor;
        public int luck;
        public int combatPower;
        public float critChance;
        public CombatStance stance;
        public CombatBlockType blockType;
    }

    public class CombatResult
    {
        public CombatOutcome outcome;
        public int rounds;
        public int playerStartHp;
        public int enemyStartHp;
        public int playerRemainingHp;
        public int enemyRemainingHp;
        public int playerFinalPower;
        public int enemyFinalPower;
        public int playerDamageDealt;
        public int enemyDamageDealt;
        public int playerCrits;
        public int enemyCrits;
        public int playerDodges;
        public int enemyDodges;
        public int playerBlocks;
        public int enemyBlocks;
        public CombatStance playerStance;
        public CombatStance enemyStance;
        public string combatLog;
        public List<CombatPlaybackEvent> playbackEvents = new List<CombatPlaybackEvent>();
    }

    public class CombatPlaybackEvent
    {
        public CombatPlaybackEventType eventType;
        public int round;
        public bool sourceIsPlayer;
        public bool targetIsPlayer;
        public string sourceName;
        public string targetName;
        public string targetZone;
        public int damage;
        public bool wasBlocked;
        public bool wasDodged;
        public bool wasCrit;
        public bool wasCounter;
        public int playerHp;
        public int enemyHp;
        public string message;
    }

    private class FighterState
    {
        public FighterData data;
        public int currentHp;
        public bool isPlayer;
        public int damageDealt;
        public int crits;
        public int dodges;
        public int blocks;

        public float HpPercent => data.maxHp > 0 ? (float)currentHp / data.maxHp : 0f;
        public bool IsDead => currentHp <= 0;
    }

    private class RoundPlan
    {
        public int attacks;
        public CombatBlockType blockType;
        public List<CombatBodyZone> blockedZones = new List<CombatBodyZone>();
    }

    private static readonly CombatBodyZone[] BodyZones =
    {
        CombatBodyZone.Head,
        CombatBodyZone.Body,
        CombatBodyZone.LeftArm,
        CombatBodyZone.RightArm,
        CombatBodyZone.Legs
    };

    public static CombatResult Simulate(FighterData playerData, FighterData enemyData)
    {
        FighterState player = CreateState(playerData, "Player", true);
        FighterState enemy = CreateState(enemyData, "Enemy", false);

        StringBuilder log = new StringBuilder();
        List<CombatPlaybackEvent> playbackEvents = new List<CombatPlaybackEvent>();
        log.AppendLine("Arena battle begins.");
        log.AppendLine("[STANCE] " + player.data.fighterName + ": " + player.data.stance);
        log.AppendLine("[STANCE] " + enemy.data.fighterName + ": " + enemy.data.stance);
        log.AppendLine("");

        int roundsCompleted = 0;

        for (int round = 1; round <= MaxRounds; round++)
        {
            roundsCompleted = round;

            RoundPlan playerPlan = BuildRoundPlan(player.data);
            RoundPlan enemyPlan = BuildRoundPlan(enemy.data);

            log.AppendLine("---- Round " + round + " ----");
            LogPlan(log, player.data.fighterName, playerPlan);
            LogPlan(log, enemy.data.fighterName, enemyPlan);
            log.AppendLine("");

            playbackEvents.Add(CreateRoundEvent(CombatPlaybackEventType.RoundStart, round, player, enemy, "Round " + round));

            ResolvePrimaryAttacks(player, enemy, playerPlan, enemyPlan, round, log, playbackEvents);
            ResolvePrimaryAttacks(enemy, player, enemyPlan, playerPlan, round, log, playbackEvents);

            log.AppendLine(
                "End of round: " +
                player.data.fighterName +
                " HP " +
                Mathf.Max(player.currentHp, 0) +
                "/" +
                player.data.maxHp +
                ", " +
                enemy.data.fighterName +
                " HP " +
                Mathf.Max(enemy.currentHp, 0) +
                "/" +
                enemy.data.maxHp);
            log.AppendLine("");

            playbackEvents.Add(CreateRoundEvent(
                CombatPlaybackEventType.RoundEnd,
                round,
                player,
                enemy,
                "Round " + round + " ends"));

            if (player.IsDead || enemy.IsDead)
                break;
        }

        CombatOutcome outcome = DetermineOutcome(player, enemy);

        log.AppendLine("Result: " + outcome);

        return new CombatResult
        {
            outcome = outcome,
            rounds = roundsCompleted,
            playerStartHp = player.data.maxHp,
            enemyStartHp = enemy.data.maxHp,
            playerRemainingHp = Mathf.Max(player.currentHp, 0),
            enemyRemainingHp = Mathf.Max(enemy.currentHp, 0),
            playerFinalPower = CalculateFinalPower(player),
            enemyFinalPower = CalculateFinalPower(enemy),
            playerDamageDealt = player.damageDealt,
            enemyDamageDealt = enemy.damageDealt,
            playerCrits = player.crits,
            enemyCrits = enemy.crits,
            playerDodges = player.dodges,
            enemyDodges = enemy.dodges,
            playerBlocks = player.blocks,
            enemyBlocks = enemy.blocks,
            playerStance = player.data.stance,
            enemyStance = enemy.data.stance,
            combatLog = log.ToString(),
            playbackEvents = playbackEvents
        };
    }

    private static FighterState CreateState(FighterData data, string fallbackName, bool isPlayer)
    {
        FighterData safeData = data ?? new FighterData();
        safeData.fighterName = string.IsNullOrEmpty(safeData.fighterName) ? fallbackName : safeData.fighterName;
        safeData.maxHp = Mathf.Max(safeData.maxHp, 1);
        safeData.attack = Mathf.Max(safeData.attack, 1);
        safeData.defense = Mathf.Max(safeData.defense, 0);
        safeData.combatPower = Mathf.Max(safeData.combatPower, 1);

        return new FighterState
        {
            data = safeData,
            currentHp = safeData.maxHp,
            isPlayer = isPlayer
        };
    }

    private static RoundPlan BuildRoundPlan(FighterData fighter)
    {
        RoundPlan plan = new RoundPlan();

        switch (fighter.stance)
        {
            case CombatStance.Aggressive:
                plan.attacks = 2;
                plan.blockType = CombatBlockType.None;
                break;

            case CombatStance.Defensive:
                if (Random.value <= 0.75f)
                {
                    plan.attacks = 0;
                    plan.blockType = GetUsableBlockType(fighter);
                    plan.blockedZones = PickRandomZones(4);
                }
                else
                {
                    plan.attacks = 1;
                    plan.blockType = CombatBlockType.None;
                }
                break;

            default:
                plan.attacks = 1;
                plan.blockType = GetUsableBlockType(fighter);
                plan.blockedZones = PickRandomZones(2);
                break;
        }

        return plan;
    }

    private static CombatBlockType GetUsableBlockType(FighterData fighter)
    {
        if (fighter.blockType == CombatBlockType.Shield)
            return CombatBlockType.Shield;

        return CombatBlockType.Weapon;
    }

    private static List<CombatBodyZone> PickRandomZones(int count)
    {
        List<CombatBodyZone> zones = new List<CombatBodyZone>(BodyZones);
        List<CombatBodyZone> selected = new List<CombatBodyZone>();

        count = Mathf.Clamp(count, 0, BodyZones.Length);

        for (int i = 0; i < count; i++)
        {
            int index = Random.Range(0, zones.Count);
            selected.Add(zones[index]);
            zones.RemoveAt(index);
        }

        return selected;
    }

    private static void ResolvePrimaryAttacks(
        FighterState attacker,
        FighterState defender,
        RoundPlan attackerPlan,
        RoundPlan defenderPlan,
        int round,
        StringBuilder log,
        List<CombatPlaybackEvent> playbackEvents)
    {
        for (int i = 0; i < attackerPlan.attacks; i++)
        {
            ResolveAttack(attacker, defender, defenderPlan, round, false, log, playbackEvents);
        }
    }

    private static void ResolveAttack(
        FighterState attacker,
        FighterState defender,
        RoundPlan defenderPlan,
        int round,
        bool isCounter,
        StringBuilder log,
        List<CombatPlaybackEvent> playbackEvents)
    {
        CombatBodyZone targetZone = GetRandomBodyZone();
        string attackLabel = isCounter ? "counterattacks" : "attacks";

        if (RollDodge(defender.data))
        {
            defender.dodges++;

            log.AppendLine("[DODGE] " + attacker.data.fighterName + " " + attackLabel + " " + FormatZone(targetZone) + ", but " + defender.data.fighterName + " dodges.");
            playbackEvents.Add(CreateDodgeEvent(attacker, defender, targetZone, round, isCounter));

            if (!isCounter)
                TryCounter(defender, attacker, round, log, playbackEvents);

            return;
        }

        bool isBlocked = IsZoneBlocked(defenderPlan, targetZone);
        if (isBlocked)
            defender.blocks++;

        int damage = CalculateDamage(attacker.data, defender.data, defenderPlan.blockType, targetZone, isBlocked);
        bool isCrit = RollCrit(attacker.data);

        if (isCrit)
        {
            damage = Mathf.Max(Mathf.RoundToInt(damage * 1.5f), damage + 1);
            attacker.crits++;
        }

        defender.currentHp -= damage;
        attacker.damageDealt += damage;

        string blockText = isBlocked
            ? " [BLOCK] Blocked with " + defenderPlan.blockType + "."
            : "";
        string critText = isCrit ? " [CRIT] Critical hit." : "";

        log.AppendLine(
            (isCounter ? "[COUNTER] " : "") +
            attacker.data.fighterName +
            " " +
            attackLabel +
            " " +
            FormatZone(targetZone) +
            " for " +
            damage +
            " damage." +
            blockText +
            critText);
        playbackEvents.Add(CreateHitEvent(attacker, defender, targetZone, round, damage, isBlocked, isCrit, isCounter));

        if (isBlocked && !isCounter)
            TryCounter(defender, attacker, round, log, playbackEvents);
    }

    private static bool RollDodge(FighterData defender)
    {
        float chance = Mathf.Clamp(4f + defender.agility * 0.18f, 4f, 35f);
        return Random.Range(0f, 100f) < chance;
    }

    private static bool RollCrit(FighterData attacker)
    {
        float chance = attacker.critChance > 0f
            ? attacker.critChance
            : 5f + attacker.luck * 0.15f;

        chance = Mathf.Clamp(chance, 2f, 45f);
        return Random.Range(0f, 100f) < chance;
    }

    private static void TryCounter(
        FighterState counterAttacker,
        FighterState originalAttacker,
        int round,
        StringBuilder log,
        List<CombatPlaybackEvent> playbackEvents)
    {
        if (counterAttacker.IsDead || originalAttacker.IsDead)
            return;

        float chance = Mathf.Clamp(6f + counterAttacker.data.reaction * 0.22f, 6f, 40f);

        if (Random.Range(0f, 100f) >= chance)
            return;

        log.AppendLine("[COUNTER] " + counterAttacker.data.fighterName + " finds an opening for a counterattack.");
        ResolveAttack(counterAttacker, originalAttacker, new RoundPlan(), round, true, log, playbackEvents);
    }

    private static int CalculateDamage(
        FighterData attacker,
        FighterData defender,
        CombatBlockType activeBlockType,
        CombatBodyZone targetZone,
        bool isBlocked)
    {
        float baseDamage =
            attacker.attack +
            attacker.strength * 1.35f +
            attacker.rage * 0.9f +
            attacker.combatPower * 0.018f;

        float zoneMultiplier = GetZoneDamageMultiplier(targetZone);
        float mitigation = Mathf.Clamp01((defender.defense + defender.armor * 0.75f) / 260f);
        float blockMultiplier = isBlocked ? GetBlockDamageMultiplier(activeBlockType) : 1f;

        float finalDamage = baseDamage * zoneMultiplier * (1f - mitigation) * blockMultiplier;
        return Mathf.Max(Mathf.RoundToInt(finalDamage), 1);
    }

    private static float GetBlockDamageMultiplier(CombatBlockType blockType)
    {
        switch (blockType)
        {
            case CombatBlockType.Shield:
                return 0.2f;
            case CombatBlockType.Weapon:
                return 0.5f;
            default:
                return 1f;
        }
    }

    private static float GetZoneDamageMultiplier(CombatBodyZone zone)
    {
        switch (zone)
        {
            case CombatBodyZone.Head:
                return 1.22f;
            case CombatBodyZone.LeftArm:
            case CombatBodyZone.RightArm:
                return 0.9f;
            case CombatBodyZone.Legs:
                return 0.96f;
            default:
                return 1f;
        }
    }

    private static bool IsZoneBlocked(RoundPlan plan, CombatBodyZone zone)
    {
        return plan != null &&
               plan.blockType != CombatBlockType.None &&
               plan.blockedZones.Contains(zone);
    }

    private static CombatBodyZone GetRandomBodyZone()
    {
        return BodyZones[Random.Range(0, BodyZones.Length)];
    }

    private static CombatPlaybackEvent CreateRoundEvent(
        CombatPlaybackEventType eventType,
        int round,
        FighterState player,
        FighterState enemy,
        string message)
    {
        return new CombatPlaybackEvent
        {
            eventType = eventType,
            round = round,
            playerHp = Mathf.Max(player.currentHp, 0),
            enemyHp = Mathf.Max(enemy.currentHp, 0),
            message = message
        };
    }

    private static CombatPlaybackEvent CreateDodgeEvent(
        FighterState attacker,
        FighterState defender,
        CombatBodyZone targetZone,
        int round,
        bool isCounter)
    {
        return new CombatPlaybackEvent
        {
            eventType = CombatPlaybackEventType.Dodge,
            round = round,
            sourceIsPlayer = attacker.isPlayer,
            targetIsPlayer = defender.isPlayer,
            sourceName = attacker.data.fighterName,
            targetName = defender.data.fighterName,
            targetZone = FormatZone(targetZone),
            wasDodged = true,
            wasCounter = isCounter,
            playerHp = GetPlayerHp(attacker, defender),
            enemyHp = GetEnemyHp(attacker, defender),
            message = defender.data.fighterName + " dodges"
        };
    }

    private static CombatPlaybackEvent CreateHitEvent(
        FighterState attacker,
        FighterState defender,
        CombatBodyZone targetZone,
        int round,
        int damage,
        bool isBlocked,
        bool isCrit,
        bool isCounter)
    {
        return new CombatPlaybackEvent
        {
            eventType = CombatPlaybackEventType.Hit,
            round = round,
            sourceIsPlayer = attacker.isPlayer,
            targetIsPlayer = defender.isPlayer,
            sourceName = attacker.data.fighterName,
            targetName = defender.data.fighterName,
            targetZone = FormatZone(targetZone),
            damage = damage,
            wasBlocked = isBlocked,
            wasCrit = isCrit,
            wasCounter = isCounter,
            playerHp = GetPlayerHp(attacker, defender),
            enemyHp = GetEnemyHp(attacker, defender),
            message = attacker.data.fighterName + " hits " + defender.data.fighterName
        };
    }

    private static int GetPlayerHp(FighterState first, FighterState second)
    {
        FighterState player = first.isPlayer ? first : second;
        return Mathf.Max(player.currentHp, 0);
    }

    private static int GetEnemyHp(FighterState first, FighterState second)
    {
        FighterState enemy = first.isPlayer ? second : first;
        return Mathf.Max(enemy.currentHp, 0);
    }

    private static CombatOutcome DetermineOutcome(FighterState player, FighterState enemy)
    {
        if (player.IsDead && enemy.IsDead)
            return CombatOutcome.Draw;

        if (enemy.IsDead)
            return CombatOutcome.Victory;

        if (player.IsDead)
            return CombatOutcome.Defeat;

        float difference = Mathf.Abs(player.HpPercent - enemy.HpPercent);

        if (difference <= 0.05f)
            return CombatOutcome.Draw;

        return player.HpPercent > enemy.HpPercent
            ? CombatOutcome.Victory
            : CombatOutcome.Defeat;
    }

    private static int CalculateFinalPower(FighterState fighter)
    {
        return Mathf.Max(Mathf.RoundToInt(fighter.data.combatPower * Mathf.Clamp01(fighter.HpPercent)), 1);
    }

    private static void LogPlan(StringBuilder log, string fighterName, RoundPlan plan)
    {
        if (plan.blockType == CombatBlockType.None)
        {
            log.AppendLine(fighterName + ": " + plan.attacks + " attack(s), no block.");
            return;
        }

        log.AppendLine(
            fighterName +
            ": " +
            plan.attacks +
            " attack(s), blocks " +
            FormatZones(plan.blockedZones) +
            " with " +
            plan.blockType +
            ".");
    }

    private static string FormatZones(List<CombatBodyZone> zones)
    {
        if (zones == null || zones.Count == 0)
            return "no zones";

        StringBuilder builder = new StringBuilder();

        for (int i = 0; i < zones.Count; i++)
        {
            if (i > 0)
                builder.Append(", ");

            builder.Append(FormatZone(zones[i]));
        }

        return builder.ToString();
    }

    private static string FormatZone(CombatBodyZone zone)
    {
        switch (zone)
        {
            case CombatBodyZone.Head:
                return "Head";
            case CombatBodyZone.LeftArm:
                return "Left Arm";
            case CombatBodyZone.RightArm:
                return "Right Arm";
            case CombatBodyZone.Legs:
                return "Legs";
            default:
                return "Body";
        }
    }
}
