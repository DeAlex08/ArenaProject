using TMPro;
using UnityEngine;

public class BarracksViewToggle : MonoBehaviour
{
    public GameObject equipmentView;
    public GameObject statsView;
    public TMP_Text toggleButtonText;

    private bool showingStats = false;

    public void ToggleView()
    {
        showingStats = !showingStats;

        equipmentView.SetActive(!showingStats);
        statsView.SetActive(showingStats);

        toggleButtonText.text = showingStats
            ? "Показать экипировку"
            : "Показать статы";
    }
}