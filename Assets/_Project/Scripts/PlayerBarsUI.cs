using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerBarsUI : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private PlayerStats playerStats;

    [Header("Fill Images")]
    [SerializeField] private Image hpFill;
    [SerializeField] private Image mpFill;
    [SerializeField] private Image expFill;

    [Header("Texts")]
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private TMP_Text mpText;
    [SerializeField] private TMP_Text expText;

    private void Update()
    {
        RefreshBars();
    }

    public void RefreshBars()
    {
        if (playerStats == null)
            return;

        if (hpFill == null || mpFill == null || expFill == null)
            return;

        hpFill.fillAmount =
            playerStats.maxHp > 0 ? (float)playerStats.currentHp / playerStats.maxHp : 0f;

        mpFill.fillAmount =
            playerStats.maxMp > 0 ? (float)playerStats.currentMp / playerStats.maxMp : 0f;

        expFill.fillAmount =
            playerStats.maxExp > 0 ? (float)playerStats.currentExp / playerStats.maxExp : 0f;

        if (hpText != null)
            hpText.text = playerStats.currentHp.ToString();

        if (mpText != null)
            mpText.text = playerStats.currentMp.ToString();

        if (expText != null)
            expText.text = playerStats.currentExp + "/" + playerStats.maxExp;
    }
}
