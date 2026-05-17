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
        playerNameText.text = playerStats.playerName;
        levelText.text = "LVL " + playerStats.level;
        combatPowerText.text = playerStats.combatPower.ToString();
    }
}