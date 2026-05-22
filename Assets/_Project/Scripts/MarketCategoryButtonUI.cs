using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MarketCategoryButtonUI : MonoBehaviour
{
    [SerializeField] private ItemType itemType;
    [SerializeField] private Button button;
    [SerializeField] private Image background;
    [SerializeField] private TMP_Text label;

    public ItemType ItemType => itemType;
    public Button Button => button;
    public Image Background => background;
    public TMP_Text Label => label;

    public void Initialize(ItemType type, string displayName)
    {
        itemType = type;

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
