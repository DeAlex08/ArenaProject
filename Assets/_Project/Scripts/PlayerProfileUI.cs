using TMPro;
using UnityEngine;

public class PlayerProfileUI : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private PlayerStats playerStats;

    [Header("Texts")]
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text combatPowerText;

    private void Update()
    {
        RefreshProfile();
    }

    public void RefreshProfile()
    {
        if (playerStats == null)
            return;

        if (playerNameText != null)
            playerNameText.text = playerStats.playerName;

        if (levelText != null)
            levelText.text = "LVL " + playerStats.level;

        if (combatPowerText != null)
            combatPowerText.text = playerStats.combatPower.ToString();
    }
}
