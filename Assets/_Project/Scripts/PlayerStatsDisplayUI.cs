using TMPro;
using UnityEngine;

public class PlayerStatsDisplayUI : MonoBehaviour
{
    public PlayerStats playerStats;
    public TMP_Text statsText;

    public void Refresh()
    {
        if (playerStats == null || statsText == null)
            return;

        statsText.text =
            "<b>Боевая мощь:</b> " + playerStats.combatPower + "\n\n" +

            BuildStatLine(
                "Сила",
                playerStats.nativeStrength,
                playerStats.bonusStrength) +

            BuildStatLine(
                "Ярость",
                playerStats.nativeRage,
                playerStats.bonusRage) +

            BuildStatLine(
                "Реакция",
                playerStats.nativeReaction,
                playerStats.bonusReaction) +

            BuildStatLine(
                "Ловкость",
                playerStats.nativeAgility,
                playerStats.bonusAgility) +

            BuildStatLine(
                "Выносливость",
                playerStats.nativeEndurance,
                playerStats.bonusEndurance) +

            BuildStatLine(
                "Защита",
                playerStats.nativeArmor,
                playerStats.bonusArmor) +

            BuildStatLine(
                "Удача",
                playerStats.nativeLuck,
                playerStats.bonusLuck) +

            BuildStatLine(
                "Интеллект",
                playerStats.nativeIntelligence,
                playerStats.bonusIntelligence);
    }

    private string BuildStatLine(
        string statName,
        int nativeValue,
        int bonusValue)
    {
        if (bonusValue > 0)
        {
            return statName + ": " +
                   nativeValue +
                   " <color=#55FF55>+" +
                   bonusValue +
                   "</color>\n";
        }

        return statName + ": " +
               nativeValue +
               "\n";
    }

    private void Start()
    {
        Refresh();
    }
}