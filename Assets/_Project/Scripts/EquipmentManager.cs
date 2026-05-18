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

        RecalculatePlayerStatsAndSave();
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
        RecalculatePlayerStatsAndSave();
    }

   public void UnequipWeapon1()
{
    equippedWeapon1 = null;

    ClearSlot(weapon1Slot);
    ClearWeaponVisual(weaponVisualRight);

    RecalculatePlayerStatsAndSave();
}

public void UnequipWeapon2()
{
    equippedWeapon2 = null;

    ClearSlot(weapon2Slot);
    ClearWeaponVisual(weaponVisualLeft);

    RecalculatePlayerStatsAndSave();
}

    public void UnequipArmor()
    {
        equippedArmor = null;
        ClearSlot(armorSlot);
        RecalculatePlayerStatsAndSave();
    }

    public void UnequipGloves()
    {
        equippedGloves = null;
        ClearSlot(glovesSlot);
        RecalculatePlayerStatsAndSave();
    }

    public void UnequipBelt()
    {
        equippedBelt = null;
        ClearSlot(beltSlot);
        RecalculatePlayerStatsAndSave();
    }

    public void UnequipLegs()
    {
        equippedLegs = null;
        ClearSlot(legsSlot);
        RecalculatePlayerStatsAndSave();
    }

    public void UnequipBoots()
    {
        equippedBoots = null;
        ClearSlot(bootsSlot);
        RecalculatePlayerStatsAndSave();
    }

    public void UnequipRing1()
    {
        equippedRing1 = null;
        ClearSlot(ring1Slot);
        RecalculatePlayerStatsAndSave();
    }

    public void UnequipRing2()
    {
        equippedRing2 = null;
        ClearSlot(ring2Slot);
        RecalculatePlayerStatsAndSave();
    }

    public void UnequipRing3()
    {
        equippedRing3 = null;
        ClearSlot(ring3Slot);
        RecalculatePlayerStatsAndSave();
    }

    public void UnequipRing4()
    {
        equippedRing4 = null;
        ClearSlot(ring4Slot);
        RecalculatePlayerStatsAndSave();
    }

    public void UnequipAmulet()
    {
        equippedAmulet = null;
        ClearSlot(amuletSlot);
        RecalculatePlayerStatsAndSave();
    }

    public void UnequipArtifact()
    {
        equippedArtifact = null;
        ClearSlot(artifactSlot);
        RecalculatePlayerStatsAndSave();
    }

    public PlayerSaveManager.SavedEquipment BuildEquipmentSaveData()
    {
        return new PlayerSaveManager.SavedEquipment
        {
            helmet = GetInstanceId(equippedHelmet),
            weapon1 = GetInstanceId(equippedWeapon1),
            weapon2 = GetInstanceId(equippedWeapon2),
            armor = GetInstanceId(equippedArmor),
            gloves = GetInstanceId(equippedGloves),
            belt = GetInstanceId(equippedBelt),
            legs = GetInstanceId(equippedLegs),
            boots = GetInstanceId(equippedBoots),
            ring1 = GetInstanceId(equippedRing1),
            ring2 = GetInstanceId(equippedRing2),
            ring3 = GetInstanceId(equippedRing3),
            ring4 = GetInstanceId(equippedRing4),
            amulet = GetInstanceId(equippedAmulet),
            artifact = GetInstanceId(equippedArtifact)
        };
    }

    public void RestoreEquipmentFromSave(PlayerSaveManager.SavedEquipment savedEquipment, PlayerInventory inventory)
    {
        ClearEquippedState();

        if (savedEquipment == null || inventory == null)
        {
            RefreshPlayerStats();
            return;
        }

        equippedHelmet = GetSavedEquippedItem(savedEquipment.helmet, inventory, ItemType.Helmet, "Helmet");
        equippedWeapon1 = GetSavedEquippedItem(savedEquipment.weapon1, inventory, ItemType.Weapon, "Weapon 1");
        equippedWeapon2 = GetSavedEquippedItem(savedEquipment.weapon2, inventory, ItemType.Weapon, "Weapon 2");
        equippedArmor = GetSavedEquippedItem(savedEquipment.armor, inventory, ItemType.Armor, "Armor");
        equippedGloves = GetSavedEquippedItem(savedEquipment.gloves, inventory, ItemType.Gloves, "Gloves");
        equippedBelt = GetSavedEquippedItem(savedEquipment.belt, inventory, ItemType.Belt, "Belt");
        equippedLegs = GetSavedEquippedItem(savedEquipment.legs, inventory, ItemType.Legs, "Legs");
        equippedBoots = GetSavedEquippedItem(savedEquipment.boots, inventory, ItemType.Boots, "Boots");
        equippedRing1 = GetSavedEquippedItem(savedEquipment.ring1, inventory, ItemType.Ring, "Ring 1");
        equippedRing2 = GetSavedEquippedItem(savedEquipment.ring2, inventory, ItemType.Ring, "Ring 2");
        equippedRing3 = GetSavedEquippedItem(savedEquipment.ring3, inventory, ItemType.Ring, "Ring 3");
        equippedRing4 = GetSavedEquippedItem(savedEquipment.ring4, inventory, ItemType.Ring, "Ring 4");
        equippedAmulet = GetSavedEquippedItem(savedEquipment.amulet, inventory, ItemType.Amulet, "Amulet");
        equippedArtifact = GetSavedEquippedItem(savedEquipment.artifact, inventory, ItemType.Artifact, "Artifact");

        RestoreSlotVisuals();
        RefreshPlayerStats();

        Debug.Log("EquipmentManager: Restored equipment from save.");
    }

    private void RecalculatePlayerStatsAndSave()
    {
        RecalculatePlayerStats();
        SaveInventoryAndEquipment();
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

    private void SaveInventoryAndEquipment()
    {
        PlayerInventory inventory = FindPlayerInventory();

        if (inventory != null)
            PlayerSaveManager.SaveInventoryAndEquipment(inventory, this);
    }

    private void ApplyIfEquipped(ItemInstance itemInstance)
    {
        if (itemInstance != null && itemInstance.itemData != null)
            playerStats.ApplyItemStats(itemInstance.itemData);
    }

    private PlayerInventory FindPlayerInventory()
    {
        PlayerInventory[] inventories = Resources.FindObjectsOfTypeAll<PlayerInventory>();

        foreach (PlayerInventory inventory in inventories)
        {
            if (inventory != null && inventory.gameObject.scene.IsValid())
                return inventory;
        }

        return null;
    }

    private string GetInstanceId(ItemInstance itemInstance)
    {
        if (itemInstance == null)
            return string.Empty;

        itemInstance.EnsureInstanceId();
        return itemInstance.instanceId;
    }

    private ItemInstance GetSavedEquippedItem(string instanceId, PlayerInventory inventory, ItemType expectedType, string slotName)
    {
        if (string.IsNullOrEmpty(instanceId))
            return null;

        ItemInstance itemInstance = inventory.GetItemByInstanceId(instanceId);

        if (itemInstance == null || itemInstance.itemData == null)
        {
            Debug.LogWarning("EquipmentManager: Saved " + slotName + " item was not found in loaded inventory.");
            return null;
        }

        if (itemInstance.itemData.itemType != expectedType)
        {
            Debug.LogWarning("EquipmentManager: Saved " + slotName + " item has wrong type and will be ignored.");
            return null;
        }

        return itemInstance;
    }

    private void ClearEquippedState()
    {
        equippedHelmet = null;
        equippedWeapon1 = null;
        equippedWeapon2 = null;
        equippedArmor = null;
        equippedGloves = null;
        equippedBelt = null;
        equippedLegs = null;
        equippedBoots = null;
        equippedRing1 = null;
        equippedRing2 = null;
        equippedRing3 = null;
        equippedRing4 = null;
        equippedAmulet = null;
        equippedArtifact = null;

        ClearSlot(helmetSlot);
        ClearSlot(weapon1Slot);
        ClearSlot(weapon2Slot);
        ClearSlot(armorSlot);
        ClearSlot(glovesSlot);
        ClearSlot(beltSlot);
        ClearSlot(legsSlot);
        ClearSlot(bootsSlot);
        ClearSlot(ring1Slot);
        ClearSlot(ring2Slot);
        ClearSlot(ring3Slot);
        ClearSlot(ring4Slot);
        ClearSlot(amuletSlot);
        ClearSlot(artifactSlot);
        ClearWeaponVisual(weaponVisualLeft);
        ClearWeaponVisual(weaponVisualRight);
    }

    private void RestoreSlotVisuals()
    {
        RestoreSlotVisual(helmetSlot, equippedHelmet);
        RestoreSlotVisual(weapon1Slot, equippedWeapon1);
        RestoreSlotVisual(weapon2Slot, equippedWeapon2);
        RestoreSlotVisual(armorSlot, equippedArmor);
        RestoreSlotVisual(glovesSlot, equippedGloves);
        RestoreSlotVisual(beltSlot, equippedBelt);
        RestoreSlotVisual(legsSlot, equippedLegs);
        RestoreSlotVisual(bootsSlot, equippedBoots);
        RestoreSlotVisual(ring1Slot, equippedRing1);
        RestoreSlotVisual(ring2Slot, equippedRing2);
        RestoreSlotVisual(ring3Slot, equippedRing3);
        RestoreSlotVisual(ring4Slot, equippedRing4);
        RestoreSlotVisual(amuletSlot, equippedAmulet);
        RestoreSlotVisual(artifactSlot, equippedArtifact);

        RestoreWeaponVisual(weaponVisualRight, equippedWeapon1);
        RestoreWeaponVisual(weaponVisualLeft, equippedWeapon2);
    }

    private void RestoreSlotVisual(Image slot, ItemInstance itemInstance)
    {
        if (itemInstance != null && itemInstance.itemData != null && itemInstance.itemData.icon != null)
            EquipToSlot(slot, itemInstance.itemData.icon);
        else
            ClearSlot(slot);
    }

    private void RestoreWeaponVisual(Image visual, ItemInstance itemInstance)
    {
        if (itemInstance != null && itemInstance.itemData != null && itemInstance.itemData.icon != null)
            EquipWeaponVisual(visual, itemInstance.itemData.icon);
        else
            ClearWeaponVisual(visual);
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
