using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MarketModeButtonUI : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image background;
    [SerializeField] private TMP_Text label;

    public Button Button => button;
    public Image Background => background;
    public TMP_Text Label => label;

    public void Initialize(string displayName)
    {
        if (button == null)
            button = GetComponent<Button>();

        if (background == null)
            background = GetComponent<Image>();

        if (label == null)
            label = GetComponentInChildren<TMP_Text>(true);

        if (label != null)
            label.text = displayName;
    }
}
