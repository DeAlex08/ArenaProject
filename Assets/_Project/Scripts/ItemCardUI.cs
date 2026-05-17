using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemCardUI : MonoBehaviour
{
    [Header("UI")]
    public Image itemIcon;
    public TMP_Text itemNameText;
    public TMP_Text itemStatsText;

    public Button equipButton;
    public TMP_Text equipButtonText;

    private ItemInstance currentItemInstance;
    private EquipmentManager equipmentManager;
    private BarracksInventoryUI inventoryUI;

    public void Setup(
        ItemInstance itemInstance,
        EquipmentManager manager,
        BarracksInventoryUI inventory)
    {
        currentItemInstance = itemInstance;
        equipmentManager = manager;
        inventoryUI = inventory;

        ItemData item = itemInstance.itemData;

        if (itemNameText != null)
        {
            itemNameText.text = item.itemName;
            itemNameText.color = GetRarityColor(item.rarity);
        }

        if (itemIcon != null && item.icon != null)
            itemIcon.sprite = item.icon;

        if (itemStatsText != null)
            itemStatsText.text = BuildStatsText(item);

        if (equipButton != null)
        {
            if (equipButtonText == null)
                equipButtonText = equipButton.GetComponentInChildren<TMP_Text>();

            equipButton.onClick.RemoveAllListeners();
            equipButton.onClick.AddListener(OnEquipButtonClicked);
        }

        RefreshButtonState();
    }

    private void OnEquipButtonClicked()
    {
        if (equipmentManager == null || currentItemInstance == null)
            return;

        equipmentManager.ToggleEquipItem(currentItemInstance);

        if (inventoryUI != null)
            inventoryUI.RefreshCurrentCategory();
    }

    private void RefreshButtonState()
    {
        if (equipButtonText == null ||
            equipmentManager == null ||
            currentItemInstance == null)
            return;

        bool equipped = equipmentManager.IsItemEquipped(currentItemInstance);

        equipButtonText.text = equipped ? "Снять" : "Надеть";
    }

    private string BuildStatsText(ItemData item)
    {
        StringBuilder stats = new StringBuilder();

        stats.AppendLine("Редкость: " + GetRarityText(item.rarity));
        stats.AppendLine("Уровень: " + item.requiredLevel);

        if (item.itemType == ItemType.Weapon &&
            (item.minDamage > 0 || item.maxDamage > 0))
        {
            stats.AppendLine("Урон: " + item.minDamage + " - " + item.maxDamage);
        }

        if (item.armor > 0) stats.AppendLine("Защита: " + item.armor);
        if (item.strength > 0) stats.AppendLine("Сила: " + item.strength);
        if (item.rage > 0) stats.AppendLine("Ярость: " + item.rage);
        if (item.reaction > 0) stats.AppendLine("Реакция: " + item.reaction);
        if (item.agility > 0) stats.AppendLine("Ловкость: " + item.agility);
        if (item.endurance > 0) stats.AppendLine("Выносливость: " + item.endurance);
        if (item.luck > 0) stats.AppendLine("Удача: " + item.luck);
        if (item.intelligence > 0) stats.AppendLine("Интеллект: " + item.intelligence);
        if (item.price > 0) stats.AppendLine("Цена: " + item.price);

        return stats.ToString();
    }

    private string GetRarityText(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Rare: return "Редкий";
            case ItemRarity.Epic: return "Эпический";
            case ItemRarity.Legendary: return "Легендарный";
            case ItemRarity.Mythic: return "Мифический";
            case ItemRarity.Named: return "Именной";
            default: return "Обычный";
        }
    }

    private Color32 GetRarityColor(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Rare: return new Color32(63, 167, 255, 255);
            case ItemRarity.Epic: return new Color32(163, 53, 238, 255);
            case ItemRarity.Legendary: return new Color32(214, 168, 74, 255);
            case ItemRarity.Mythic: return new Color32(255, 70, 70, 255);
            case ItemRarity.Named: return new Color32(255, 140, 40, 255);
            default: return new Color32(180, 180, 180, 255);
        }
    }
}