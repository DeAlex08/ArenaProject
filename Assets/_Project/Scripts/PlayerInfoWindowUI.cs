using TMPro;
using UnityEngine;

public class PlayerInfoWindowUI : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private PlayerStats playerStats;

    [Header("Text")]
    [SerializeField] private TMP_Text infoText;

    private void OnEnable()
    {
        RefreshInfoWindow();
    }

    public void RefreshInfoWindow()
    {
        infoText.text =
            "Имя: " + playerStats.playerName + "\n\n" +
            "Уровень: " + playerStats.level + "\n\n" +
            "Боевая мощь: " + playerStats.combatPower + "\n\n" +
            "HP: " + playerStats.currentHp + " / " + playerStats.maxHp + "\n" +
            "MP: " + playerStats.currentMp + " / " + playerStats.maxMp;
    }
}