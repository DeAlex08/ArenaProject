using TMPro;
using UnityEngine;

public class PlayerStatsWindowUI : MonoBehaviour
{
    [Header("Player")]
    public PlayerStats playerStats;

    [Header("Texts")]
    public TMP_Text combatPowerText;

    public TMP_Text strengthText;
    public TMP_Text rageText;
    public TMP_Text reactionText;
    public TMP_Text agilityText;
    public TMP_Text enduranceText;
    public TMP_Text armorText;
    public TMP_Text luckText;
    public TMP_Text intelligenceText;

    private void Update()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (playerStats == null)
            return;

        if (combatPowerText != null)
            combatPowerText.text =
                playerStats.combatPower.ToString();

        SetStatText(
            strengthText,
            playerStats.nativeStrength,
            playerStats.bonusStrength);

        SetStatText(
            rageText,
            playerStats.nativeRage,
            playerStats.bonusRage);

        SetStatText(
            reactionText,
            playerStats.nativeReaction,
            playerStats.bonusReaction);

        SetStatText(
            agilityText,
            playerStats.nativeAgility,
            playerStats.bonusAgility);

        SetStatText(
            enduranceText,
            playerStats.nativeEndurance,
            playerStats.bonusEndurance);

        SetStatText(
            armorText,
            playerStats.nativeArmor,
            playerStats.bonusArmor);

        SetStatText(
            luckText,
            playerStats.nativeLuck,
            playerStats.bonusLuck);

        SetStatText(
            intelligenceText,
            playerStats.nativeIntelligence,
            playerStats.bonusIntelligence);
    }

    private void SetStatText(
        TMP_Text text,
        int nativeValue,
        int bonusValue)
    {
        if (text == null)
            return;

        if (bonusValue > 0)
        {
            text.text =
                nativeValue +
                " <color=#55FF55>+" +
                bonusValue +
                "</color>";
        }
        else
        {
            text.text = nativeValue.ToString();
        }
    }
}