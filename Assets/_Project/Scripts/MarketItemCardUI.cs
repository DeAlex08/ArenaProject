using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MarketItemCardUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private Image background;

    [Header("Item")]
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text metaText;
    [SerializeField] private TMP_Text statsText;
    [SerializeField] private TMP_Text priceText;

    [Header("Action")]
    [SerializeField] private Button actionButton;
    [SerializeField] private TMP_Text actionButtonText;

    public Image Background => background;

    public void Bind(
        ItemData item,
        string meta,
        string stats,
        int price,
        string actionText,
        bool canClick,
        UnityAction action,
        Color nameColor)
    {
        if (icon != null)
            icon.sprite = item != null ? item.icon : null;

        if (itemNameText != null)
        {
            itemNameText.text = item != null ? item.itemName : string.Empty;
            itemNameText.color = nameColor;
        }

        if (metaText != null)
            metaText.text = meta;

        if (statsText != null)
            statsText.text = stats;

        if (priceText != null)
            priceText.text = price + "\nTOKENS";

        if (actionButtonText != null)
            actionButtonText.text = actionText;

        if (actionButton != null)
        {
            actionButton.onClick.RemoveAllListeners();
            actionButton.interactable = canClick;

            if (canClick && action != null)
                actionButton.onClick.AddListener(action);
        }
    }
}
