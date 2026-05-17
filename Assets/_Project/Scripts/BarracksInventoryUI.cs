using System.Collections.Generic;
using UnityEngine;

public class BarracksInventoryUI : MonoBehaviour
{
    [Header("UI")]
    public Transform content;
    public GameObject itemCardPrefab;

    [Header("Systems")]
    public EquipmentManager equipmentManager;
    public PlayerInventory playerInventory;

    private ItemType currentCategory;

    private void Start()
    {
        ShowHelmets();
    }

    public void ShowHelmets() => ShowCategory(ItemType.Helmet);
    public void ShowWeapons() => ShowCategory(ItemType.Weapon);
    public void ShowArmor() => ShowCategory(ItemType.Armor);
    public void ShowGloves() => ShowCategory(ItemType.Gloves);
    public void ShowBelt() => ShowCategory(ItemType.Belt);
    public void ShowLegs() => ShowCategory(ItemType.Legs);
    public void ShowBoots() => ShowCategory(ItemType.Boots);
    public void ShowRings() => ShowCategory(ItemType.Ring);
    public void ShowAmulets() => ShowCategory(ItemType.Amulet);
    public void ShowArtifacts() => ShowCategory(ItemType.Artifact);

    public void ShowCategory(ItemType type)
    {
        currentCategory = type;

        ClearItems();

        if (playerInventory == null)
            return;

        List<ItemInstance> items = playerInventory.GetItemsByType(type);

        items.Sort((a, b) =>
        {
            bool aEquipped = equipmentManager != null && equipmentManager.IsItemEquipped(a);
            bool bEquipped = equipmentManager != null && equipmentManager.IsItemEquipped(b);

            if (aEquipped != bEquipped)
                return bEquipped.CompareTo(aEquipped);

            int rarityCompare =
                GetRarityRank(b.itemData.rarity).CompareTo(GetRarityRank(a.itemData.rarity));

            if (rarityCompare != 0)
                return rarityCompare;

            return string.Compare(a.itemData.itemName, b.itemData.itemName);
        });

        foreach (ItemInstance itemInstance in items)
        {
            GameObject card = Instantiate(itemCardPrefab, content);

            ItemCardUI cardUI = card.GetComponent<ItemCardUI>();

            if (cardUI != null)
            {
                cardUI.Setup(itemInstance, equipmentManager, this);
            }
        }
    }

    public void RefreshCurrentCategory()
    {
        ShowCategory(currentCategory);
    }

    private void ClearItems()
    {
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }
    }

    private int GetRarityRank(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Named:
                return 6;

            case ItemRarity.Mythic:
                return 5;

            case ItemRarity.Legendary:
                return 4;

            case ItemRarity.Epic:
                return 3;

            case ItemRarity.Rare:
                return 2;

            case ItemRarity.Common:
                return 1;

            default:
                return 0;
        }
    }
}