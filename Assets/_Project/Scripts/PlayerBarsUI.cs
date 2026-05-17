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
        hpFill.fillAmount =
            (float)playerStats.currentHp / playerStats.maxHp;

        mpFill.fillAmount =
            (float)playerStats.currentMp / playerStats.maxMp;

        expFill.fillAmount =
            (float)playerStats.currentExp / playerStats.maxExp;

        hpText.text =
            playerStats.currentHp.ToString();

        mpText.text =
            playerStats.currentMp.ToString();

        expText.text =
            playerStats.currentExp.ToString() + "%";
    }
}