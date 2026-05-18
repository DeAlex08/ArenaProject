using UnityEngine;

public enum ItemType
{
    Helmet,
    Weapon,
    Armor,
    Gloves,
    Belt,
    Legs,
    Boots,
    Ring,
    Amulet,
    Artifact
}

public enum ItemRarity
{
    Common,
    Rare,
    Epic,
    Legendary,
    Mythic,
    Named
}

[CreateAssetMenu(fileName = "New Item", menuName = "Arena RPG/Item")]
public class ItemData : ScriptableObject
{
    [Header("Main")]
    public string itemId;
    public string itemName;
    public ItemType itemType;
    public ItemRarity rarity;
    public Sprite icon;

    [Header("Requirements")]
    public int requiredLevel;

    [Header("Combat")]
    public int minDamage;
    public int maxDamage;
    public int armor;

    [Header("Stats")]
    public int strength;
    public int rage;
    public int reaction;
    public int agility;
    public int endurance;
    public int luck;
    public int intelligence;

    [Header("Economy")]
    public int price;
}
