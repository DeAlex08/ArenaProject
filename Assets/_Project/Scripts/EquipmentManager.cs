using UnityEngine;
using UnityEngine.UI;

public class EquipmentManager : MonoBehaviour
{
    [Header("Player")]
    public PlayerStats playerStats;
    public PlayerStatsDisplayUI statsDisplayUI;

    [Header("Equipment Slots")]
    public Image helmetSlot;

    public Image weapon1Slot;
    public Image weapon2Slot;
    public Image weaponVisualLeft;
public Image weaponVisualRight;

    public Image armorSlot;
    public Image glovesSlot;
    public Image beltSlot;
    public Image legsSlot;
    public Image bootsSlot;

    public Image ring1Slot;
    public Image ring2Slot;
    public Image ring3Slot;
    public Image ring4Slot;

    public Image amuletSlot;
    public Image artifactSlot;

    private ItemInstance equippedHelmet;

    private ItemInstance equippedWeapon1;
    private ItemInstance equippedWeapon2;

    private ItemInstance equippedArmor;
    private ItemInstance equippedGloves;
    private ItemInstance equippedBelt;
    private ItemInstance equippedLegs;
    private ItemInstance equippedBoots;

    private ItemInstance equippedRing1;
    private ItemInstance equippedRing2;
    private ItemInstance equippedRing3;
    private ItemInstance equippedRing4;

    private ItemInstance equippedAmulet;
    private ItemInstance equippedArtifact;

    public void ToggleEquipItem(ItemInstance itemInstance)
    {
        if (itemInstance == null || itemInstance.itemData == null)
            return;

        if (IsItemEquipped(itemInstance))
            UnequipItem(itemInstance);
        else
            EquipItem(itemInstance);
    }

    public bool IsItemEquipped(ItemInstance itemInstance)
    {
        return itemInstance == equippedHelmet
            || itemInstance == equippedWeapon1
            || itemInstance == equippedWeapon2
            || itemInstance == equippedArmor
            || itemInstance == equippedGloves
            || itemInstance == equippedBelt
            || itemInstance == equippedLegs
            || itemInstance == equippedBoots
            || itemInstance == equippedRing1
            || itemInstance == equippedRing2
            || itemInstance == equippedRing3
            || itemInstance == equippedRing4
            || itemInstance == equippedAmulet
            || itemInstance == equippedArtifact;
    }

    public void EquipItem(ItemInstance itemInstance)
    {
        if (itemInstance == null || itemInstance.itemData == null || itemInstance.itemData.icon == null)
            return;

        ItemData item = itemInstance.itemData;

        switch (item.itemType)
        {
            case ItemType.Helmet:
                equippedHelmet = itemInstance;
                EquipToSlot(helmetSlot, item.icon);
                break;

            case ItemType.Weapon:

    if (equippedWeapon1 == null)
    {
        equippedWeapon1 = itemInstance;

        EquipToSlot(weapon1Slot, item.icon);
        EquipWeaponVisual(weaponVisualRight, item.icon);
    }
    else if (equippedWeapon2 == null)
    {
        equippedWeapon2 = itemInstance;

        EquipToSlot(weapon2Slot, item.icon);
        EquipWeaponVisual(weaponVisualLeft, item.icon);
    }
    else
    {
        equippedWeapon1 = itemInstance;

        EquipToSlot(weapon1Slot, item.icon);
        EquipWeaponVisual(weaponVisualRight, item.icon);
    }

    break;

            case ItemType.Armor:
                equippedArmor = itemInstance;
                EquipToSlot(armorSlot, item.icon);
                break;

            case ItemType.Gloves:
                equippedGloves = itemInstance;
                EquipToSlot(glovesSlot, item.icon);
                break;

            case ItemType.Belt:
                equippedBelt = itemInstance;
                EquipToSlot(beltSlot, item.icon);
                break;

            case ItemType.Legs:
                equippedLegs = itemInstance;
                EquipToSlot(legsSlot, item.icon);
                break;

            case ItemType.Boots:
                equippedBoots = itemInstance;
                EquipToSlot(bootsSlot, item.icon);
                break;

            case ItemType.Ring:
                if (equippedRing1 == null)
                {
                    equippedRing1 = itemInstance;
                    EquipToSlot(ring1Slot, item.icon);
                }
                else if (equippedRing2 == null)
                {
                    equippedRing2 = itemInstance;
                    EquipToSlot(ring2Slot, item.icon);
                }
                else if (equippedRing3 == null)
                {
                    equippedRing3 = itemInstance;
                    EquipToSlot(ring3Slot, item.icon);
                }
                else if (equippedRing4 == null)
                {
                    equippedRing4 = itemInstance;
                    EquipToSlot(ring4Slot, item.icon);
                }
                else
                {
                    equippedRing1 = itemInstance;
                    EquipToSlot(ring1Slot, item.icon);
                }
                break;

            case ItemType.Amulet:
                equippedAmulet = itemInstance;
                EquipToSlot(amuletSlot, item.icon);
                break;

            case ItemType.Artifact:
                equippedArtifact = itemInstance;
                EquipToSlot(artifactSlot, item.icon);
                break;
        }

        RecalculatePlayerStats();
    }

    public void UnequipItem(ItemInstance itemInstance)
    {
        if (itemInstance == null)
            return;

        if (itemInstance == equippedHelmet)
            UnequipHelmet();
        else if (itemInstance == equippedWeapon1)
            UnequipWeapon1();
        else if (itemInstance == equippedWeapon2)
            UnequipWeapon2();
        else if (itemInstance == equippedArmor)
            UnequipArmor();
        else if (itemInstance == equippedGloves)
            UnequipGloves();
        else if (itemInstance == equippedBelt)
            UnequipBelt();
        else if (itemInstance == equippedLegs)
            UnequipLegs();
        else if (itemInstance == equippedBoots)
            UnequipBoots();
        else if (itemInstance == equippedRing1)
            UnequipRing1();
        else if (itemInstance == equippedRing2)
            UnequipRing2();
        else if (itemInstance == equippedRing3)
            UnequipRing3();
        else if (itemInstance == equippedRing4)
            UnequipRing4();
        else if (itemInstance == equippedAmulet)
            UnequipAmulet();
        else if (itemInstance == equippedArtifact)
            UnequipArtifact();
    }

    public void UnequipHelmet()
    {
        equippedHelmet = null;
        ClearSlot(helmetSlot);
        RecalculatePlayerStats();
    }

   public void UnequipWeapon1()
{
    equippedWeapon1 = null;

    ClearSlot(weapon1Slot);
    ClearWeaponVisual(weaponVisualRight);

    RecalculatePlayerStats();
}

public void UnequipWeapon2()
{
    equippedWeapon2 = null;

    ClearSlot(weapon2Slot);
    ClearWeaponVisual(weaponVisualLeft);

    RecalculatePlayerStats();
}

    public void UnequipArmor()
    {
        equippedArmor = null;
        ClearSlot(armorSlot);
        RecalculatePlayerStats();
    }

    public void UnequipGloves()
    {
        equippedGloves = null;
        ClearSlot(glovesSlot);
        RecalculatePlayerStats();
    }

    public void UnequipBelt()
    {
        equippedBelt = null;
        ClearSlot(beltSlot);
        RecalculatePlayerStats();
    }

    public void UnequipLegs()
    {
        equippedLegs = null;
        ClearSlot(legsSlot);
        RecalculatePlayerStats();
    }

    public void UnequipBoots()
    {
        equippedBoots = null;
        ClearSlot(bootsSlot);
        RecalculatePlayerStats();
    }

    public void UnequipRing1()
    {
        equippedRing1 = null;
        ClearSlot(ring1Slot);
        RecalculatePlayerStats();
    }

    public void UnequipRing2()
    {
        equippedRing2 = null;
        ClearSlot(ring2Slot);
        RecalculatePlayerStats();
    }

    public void UnequipRing3()
    {
        equippedRing3 = null;
        ClearSlot(ring3Slot);
        RecalculatePlayerStats();
    }

    public void UnequipRing4()
    {
        equippedRing4 = null;
        ClearSlot(ring4Slot);
        RecalculatePlayerStats();
    }

    public void UnequipAmulet()
    {
        equippedAmulet = null;
        ClearSlot(amuletSlot);
        RecalculatePlayerStats();
    }

    public void UnequipArtifact()
    {
        equippedArtifact = null;
        ClearSlot(artifactSlot);
        RecalculatePlayerStats();
    }

    private void RecalculatePlayerStats()
    {
        if (playerStats == null)
            return;

        playerStats.RecalculateStats();

        ApplyIfEquipped(equippedHelmet);

        ApplyIfEquipped(equippedWeapon1);
        ApplyIfEquipped(equippedWeapon2);

        ApplyIfEquipped(equippedArmor);
        ApplyIfEquipped(equippedGloves);
        ApplyIfEquipped(equippedBelt);
        ApplyIfEquipped(equippedLegs);
        ApplyIfEquipped(equippedBoots);

        ApplyIfEquipped(equippedRing1);
        ApplyIfEquipped(equippedRing2);
        ApplyIfEquipped(equippedRing3);
        ApplyIfEquipped(equippedRing4);

        ApplyIfEquipped(equippedAmulet);
        ApplyIfEquipped(equippedArtifact);

        if (statsDisplayUI != null)
            statsDisplayUI.Refresh();

        FindFirstObjectByType<CharacterPanelStatsViewUI>()?.Refresh();
    
            FindFirstObjectByType<BarracksInventoryUI>()?.RefreshCurrentCategory();
    }

    public void RefreshPlayerStats()
    {
        RecalculatePlayerStats();
    }

    private void ApplyIfEquipped(ItemInstance itemInstance)
    {
        if (itemInstance != null && itemInstance.itemData != null)
            playerStats.ApplyItemStats(itemInstance.itemData);
    }

    private void EquipToSlot(Image slot, Sprite icon)
    {
        if (slot == null)
            return;

        slot.sprite = icon;
        slot.color = Color.white;
    }
private void EquipWeaponVisual(Image visual, Sprite icon)
{
    if (visual == null)
        return;

    visual.sprite = icon;
    visual.color = Color.white;
}
    private void ClearSlot(Image slot)
    {
        if (slot == null)
            return;

        slot.sprite = null;
        slot.color = new Color(1f, 1f, 1f, 0f);
    }
    private void ClearWeaponVisual(Image visual)
{
    if (visual == null)
        return;

    visual.sprite = null;
    visual.color = new Color(1f, 1f, 1f, 0f);
}
}
