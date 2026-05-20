using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MarketWindowUI : MonoBehaviour
{
    private enum MarketMode
    {
        Buy,
        Sell
    }

    [Serializable]
    private class ShopEntry
    {
        public ItemData itemData;
        public int price;
    }

    private class CategoryButtonBinding
    {
        public ItemType itemType;
        public Image background;
        public TMP_Text label;
    }

    [Header("Navigation")]
    [SerializeField] private LocationNavigationController navigationController;

    [Header("Systems")]
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private ItemDatabase itemDatabase;
    [SerializeField] private BarracksInventoryUI barracksInventoryUI;
    [SerializeField] private EquipmentManager equipmentManager;

    [Header("Shop")]
    [SerializeField] private List<ShopEntry> shopEntries = new List<ShopEntry>();

    private TMP_Text tokensText;
    private TMP_Text messageText;
    private TMP_Text buyModeText;
    private TMP_Text sellModeText;
    private Image buyModeBackground;
    private Image sellModeBackground;
    private RectTransform content;
    private readonly List<CategoryButtonBinding> categoryButtons = new List<CategoryButtonBinding>();

    private MarketMode currentMode = MarketMode.Buy;
    private ItemType currentCategory = ItemType.Armor;
    private bool isProcessingTransaction;
    private bool isBuilt;

    private static readonly Color BackgroundColor = new Color(0.04f, 0.035f, 0.03f, 0.96f);
    private static readonly Color PanelColor = new Color(0.11f, 0.095f, 0.075f, 0.92f);
    private static readonly Color CardColor = new Color(0.18f, 0.155f, 0.115f, 0.95f);
    private static readonly Color ButtonColor = new Color(0.09f, 0.065f, 0.04f, 0.96f);
    private static readonly Color SelectedColor = new Color(0.35f, 0.235f, 0.095f, 0.98f);
    private static readonly Color DisabledColor = new Color(0.12f, 0.12f, 0.12f, 0.85f);
    private static readonly Color GoldColor = new Color(0.9f, 0.69f, 0.36f, 1f);
    private static readonly Color TextColor = new Color(0.88f, 0.82f, 0.68f, 1f);
    private static readonly Color MutedTextColor = new Color(0.62f, 0.57f, 0.48f, 1f);
    private static readonly Color WarningColor = new Color(0.95f, 0.38f, 0.28f, 1f);

    private readonly ItemType[] categoryOrder =
    {
        ItemType.Helmet,
        ItemType.Weapon,
        ItemType.Armor,
        ItemType.Gloves,
        ItemType.Belt,
        ItemType.Legs,
        ItemType.Boots,
        ItemType.Ring,
        ItemType.Amulet,
        ItemType.Artifact
    };

    private void Awake()
    {
        ResolveReferences();
        EnsureDefaultShopEntries();
    }

    private void OnEnable()
    {
        ResolveReferences();
        EnsureDefaultShopEntries();

        if (!isBuilt)
            BuildWindow();

        RefreshAll();
    }

    public void Close()
    {
        if (navigationController != null)
            navigationController.CloseCurrentLocation();
        else
            gameObject.SetActive(false);
    }

    private void BuildWindow()
    {
        ClearChildren(transform);
        categoryButtons.Clear();

        Image background = GetComponent<Image>();
        if (background == null)
            background = gameObject.AddComponent<Image>();

        background.color = BackgroundColor;
        background.raycastTarget = true;

        RectTransform root = GetComponent<RectTransform>();
        root.anchorMin = new Vector2(0.5f, 0.5f);
        root.anchorMax = new Vector2(0.5f, 0.5f);
        root.pivot = new Vector2(0.5f, 0.5f);
        root.anchoredPosition = new Vector2(225f, 0f);
        root.sizeDelta = new Vector2(1470f, 1080f);

        CreateText("Title", root, "MARKET", new Vector2(0f, 470f), new Vector2(520f, 70f), 44, GoldColor, TextAlignmentOptions.Center);
        tokensText = CreateText("ArenaTokensText", root, string.Empty, new Vector2(-510f, 405f), new Vector2(420f, 48f), 26, TextColor, TextAlignmentOptions.Left);
        messageText = CreateText("MessageText", root, string.Empty, new Vector2(70f, 405f), new Vector2(640f, 48f), 24, WarningColor, TextAlignmentOptions.Center);

        Button closeButton = CreateButton("CloseButton", root, "X", new Vector2(650f, 438f), new Vector2(84f, 64f), 32);
        closeButton.onClick.AddListener(Close);

        CreateModeToggle(root);
        CreateCategoryPanel(root);
        CreateItemsPanel(root);

        isBuilt = true;
    }

    private void CreateModeToggle(RectTransform root)
    {
        Button buyButton = CreateButton("BuyModeButton", root, "BUY", new Vector2(-100f, 340f), new Vector2(190f, 62f), 26);
        buyModeBackground = buyButton.targetGraphic as Image;
        buyModeText = buyButton.GetComponentInChildren<TMP_Text>();
        buyButton.onClick.AddListener(() => SelectMode(MarketMode.Buy));

        Button sellButton = CreateButton("SellModeButton", root, "SELL", new Vector2(110f, 340f), new Vector2(190f, 62f), 26);
        sellModeBackground = sellButton.targetGraphic as Image;
        sellModeText = sellButton.GetComponentInChildren<TMP_Text>();
        sellButton.onClick.AddListener(() => SelectMode(MarketMode.Sell));
    }

    private void CreateCategoryPanel(RectTransform root)
    {
        RectTransform panel = CreatePanel("CategoryPanel", root, new Vector2(-530f, -90f), new Vector2(260f, 760f), PanelColor);

        VerticalLayoutGroup layout = panel.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(12, 12, 16, 16);
        layout.spacing = 10f;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        foreach (ItemType itemType in categoryOrder)
            CreateCategoryButton(panel, itemType);
    }

    private void CreateCategoryButton(RectTransform parent, ItemType itemType)
    {
        Button button = CreateButton("CategoryButton_" + itemType, parent, GetCategoryLabel(itemType), Vector2.zero, new Vector2(220f, 58f), 20);
        LayoutElement layoutElement = button.gameObject.AddComponent<LayoutElement>();
        layoutElement.minHeight = 58f;
        layoutElement.preferredHeight = 58f;

        CategoryButtonBinding binding = new CategoryButtonBinding
        {
            itemType = itemType,
            background = button.targetGraphic as Image,
            label = button.GetComponentInChildren<TMP_Text>()
        };

        categoryButtons.Add(binding);
        button.onClick.AddListener(() => SelectCategory(itemType));
    }

    private void CreateItemsPanel(RectTransform root)
    {
        RectTransform listPanel = CreatePanel("ItemsPanel", root, new Vector2(145f, -90f), new Vector2(1040f, 760f), PanelColor);

        ScrollRect scrollRect = listPanel.gameObject.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;

        RectTransform viewport = CreatePanel("Viewport", listPanel, Vector2.zero, Vector2.zero, new Color(0f, 0f, 0f, 0f));
        viewport.anchorMin = Vector2.zero;
        viewport.anchorMax = Vector2.one;
        viewport.offsetMin = new Vector2(18f, 18f);
        viewport.offsetMax = new Vector2(-18f, -18f);
        viewport.gameObject.AddComponent<RectMask2D>();
        scrollRect.viewport = viewport;

        content = CreatePanel("Content", viewport, Vector2.zero, Vector2.zero, new Color(0f, 0f, 0f, 0f));
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        content.anchoredPosition = Vector2.zero;
        content.sizeDelta = new Vector2(0f, 0f);
        scrollRect.content = content;

        VerticalLayoutGroup layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(10, 10, 10, 10);
        layout.spacing = 12f;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = content.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    private void SelectMode(MarketMode mode)
    {
        if (currentMode == mode)
            return;

        currentMode = mode;
        SetMessage(string.Empty);
        RefreshAll();
    }

    private void SelectCategory(ItemType itemType)
    {
        if (currentCategory == itemType)
            return;

        currentCategory = itemType;
        SetMessage(string.Empty);
        RefreshAll();
    }

    private void RefreshAll()
    {
        RefreshTokens();
        RefreshModeVisuals();
        RefreshCategoryVisuals();
        RefreshItemCards();
    }

    private void RefreshModeVisuals()
    {
        SetToggleVisual(buyModeBackground, buyModeText, currentMode == MarketMode.Buy);
        SetToggleVisual(sellModeBackground, sellModeText, currentMode == MarketMode.Sell);
    }

    private void RefreshCategoryVisuals()
    {
        foreach (CategoryButtonBinding binding in categoryButtons)
            SetToggleVisual(binding.background, binding.label, binding.itemType == currentCategory);
    }

    private void SetToggleVisual(Image background, TMP_Text label, bool selected)
    {
        if (background != null)
            background.color = selected ? SelectedColor : ButtonColor;

        if (label != null)
            label.color = selected ? GoldColor : TextColor;
    }

    private void RefreshItemCards()
    {
        if (content == null)
            return;

        ClearChildren(content);

        int createdCards = currentMode == MarketMode.Buy
            ? CreateBuyCards()
            : CreateSellCards();

        if (createdCards == 0)
            CreateEmptyMessage("No items available");

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
    }

    private int CreateBuyCards()
    {
        int count = 0;

        foreach (ShopEntry entry in shopEntries)
        {
            if (entry == null || entry.itemData == null || entry.itemData.itemType != currentCategory)
                continue;

            CreateMarketCard(entry.itemData, GetShopPrice(entry.itemData), "BUY", true, () => BuyItem(entry));
            count++;
        }

        return count;
    }

    private int CreateSellCards()
    {
        if (playerInventory == null)
            return 0;

        List<ItemInstance> items = playerInventory.GetItemsByType(currentCategory);
        int count = 0;

        foreach (ItemInstance itemInstance in items)
        {
            if (itemInstance == null || itemInstance.itemData == null)
                continue;

            bool isEquipped = IsEquipped(itemInstance);
            string actionText = isEquipped ? "EQUIPPED" : "SELL";
            int sellPrice = GetSellPrice(itemInstance.itemData);

            CreateMarketCard(
                itemInstance.itemData,
                sellPrice,
                actionText,
                !isEquipped,
                () => SellItem(itemInstance));

            count++;
        }

        return count;
    }

    private void CreateEmptyMessage(string message)
    {
        TMP_Text label = CreateText("EmptyMessage", content, message, Vector2.zero, new Vector2(700f, 80f), 28, MutedTextColor, TextAlignmentOptions.Center);
        LayoutElement layoutElement = label.gameObject.AddComponent<LayoutElement>();
        layoutElement.minHeight = 90f;
        layoutElement.preferredHeight = 90f;
    }

    private void CreateMarketCard(ItemData item, int price, string actionText, bool canClick, UnityEngine.Events.UnityAction action)
    {
        RectTransform card = CreatePanel("MarketCard_" + ItemDatabase.GetStableItemId(item), content, Vector2.zero, new Vector2(930f, 170f), CardColor);
        LayoutElement layoutElement = card.gameObject.AddComponent<LayoutElement>();
        layoutElement.minWidth = 900f;
        layoutElement.preferredWidth = 930f;
        layoutElement.minHeight = 170f;
        layoutElement.preferredHeight = 170f;

        CreateIcon(card, item.icon, new Vector2(-405f, 0f), new Vector2(112f, 112f));
        CreateText("ItemName", card, item.itemName, new Vector2(-250f, 48f), new Vector2(330f, 38f), 24, GoldColor, TextAlignmentOptions.Left);
        CreateText("Meta", card, GetRarityLabel(item.rarity) + "  LVL " + Mathf.Max(item.requiredLevel, 1), new Vector2(-250f, 10f), new Vector2(330f, 32f), 18, TextColor, TextAlignmentOptions.Left);
        CreateText("Stats", card, BuildStatsText(item), new Vector2(80f, 8f), new Vector2(430f, 110f), 18, TextColor, TextAlignmentOptions.Left);
        CreateText("Price", card, price + " Tokens", new Vector2(360f, 44f), new Vector2(160f, 34f), 20, GoldColor, TextAlignmentOptions.Center);

        Button actionButton = CreateButton("ActionButton", card, actionText, new Vector2(360f, -34f), new Vector2(170f, 56f), 21);
        actionButton.interactable = canClick;

        Image buttonImage = actionButton.targetGraphic as Image;
        if (!canClick && buttonImage != null)
            buttonImage.color = DisabledColor;

        if (canClick && action != null)
            actionButton.onClick.AddListener(action);
    }

    private void CreateIcon(Transform parent, Sprite sprite, Vector2 anchoredPosition, Vector2 size)
    {
        RectTransform frame = CreatePanel("IconFrame", parent, anchoredPosition, size, new Color(0.055f, 0.05f, 0.045f, 0.96f));

        GameObject iconObject = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        iconObject.layer = gameObject.layer;
        iconObject.transform.SetParent(frame, false);

        RectTransform iconRect = iconObject.GetComponent<RectTransform>();
        iconRect.anchorMin = Vector2.zero;
        iconRect.anchorMax = Vector2.one;
        iconRect.offsetMin = new Vector2(8f, 8f);
        iconRect.offsetMax = new Vector2(-8f, -8f);

        Image icon = iconObject.GetComponent<Image>();
        icon.sprite = sprite;
        icon.preserveAspect = true;
        icon.color = sprite != null ? Color.white : MutedTextColor;
        icon.raycastTarget = false;
    }

    private void BuyItem(ShopEntry entry)
    {
        if (isProcessingTransaction)
            return;

        if (entry == null || entry.itemData == null)
            return;

        if (playerStats == null || playerInventory == null)
        {
            SetMessage("Market is not ready");
            Debug.LogWarning("MarketWindowUI: Missing PlayerStats or PlayerInventory.");
            return;
        }

        int price = GetShopPrice(entry.itemData);

        if (playerStats.arenaTokens < price)
        {
            SetMessage("Not enough Arena Tokens");
            Debug.Log("MarketWindowUI: Not enough Arena Tokens to buy " + entry.itemData.itemName);
            return;
        }

        isProcessingTransaction = true;

        playerStats.arenaTokens -= price;
        playerStats.SaveProgression();
        playerInventory.AddItem(entry.itemData);

        if (barracksInventoryUI != null)
            barracksInventoryUI.RefreshCurrentCategory();

        SetMessage("Purchased: " + entry.itemData.itemName);
        Debug.Log("MarketWindowUI: Purchased " + entry.itemData.itemName + " for " + price + " Arena Tokens.");

        isProcessingTransaction = false;
        RefreshAll();
    }

    private void SellItem(ItemInstance itemInstance)
    {
        if (isProcessingTransaction)
            return;

        if (itemInstance == null || itemInstance.itemData == null || playerStats == null || playerInventory == null)
            return;

        if (IsEquipped(itemInstance))
        {
            SetMessage("Unequip item before selling");
            return;
        }

        int sellPrice = GetSellPrice(itemInstance.itemData);
        isProcessingTransaction = true;

        playerInventory.RemoveItem(itemInstance);
        playerStats.arenaTokens += sellPrice;
        playerStats.SaveProgression();
        playerInventory.SaveInventoryAndEquipment();

        if (barracksInventoryUI != null)
            barracksInventoryUI.RefreshCurrentCategory();

        SetMessage("Sold: " + itemInstance.itemData.itemName);
        Debug.Log("MarketWindowUI: Sold " + itemInstance.itemData.itemName + " for " + sellPrice + " Arena Tokens.");

        isProcessingTransaction = false;
        RefreshAll();
    }

    private bool IsEquipped(ItemInstance itemInstance)
    {
        return equipmentManager != null && equipmentManager.IsItemEquipped(itemInstance);
    }

    private int GetShopPrice(ItemData item)
    {
        if (item == null)
            return 0;

        string itemId = ItemDatabase.GetStableItemId(item);

        foreach (ShopEntry entry in shopEntries)
        {
            if (entry != null &&
                entry.itemData != null &&
                ItemDatabase.GetStableItemId(entry.itemData) == itemId &&
                entry.price > 0)
            {
                return entry.price;
            }
        }

        return item.price > 0 ? item.price : 10;
    }

    private int GetSellPrice(ItemData item)
    {
        return Mathf.FloorToInt(GetShopPrice(item) * 0.5f);
    }

    private void RefreshTokens()
    {
        if (tokensText != null)
            tokensText.text = "Arena Tokens: " + (playerStats != null ? playerStats.arenaTokens : 0);
    }

    private void SetMessage(string message)
    {
        if (messageText != null)
            messageText.text = message;
    }

    private string BuildStatsText(ItemData item)
    {
        List<string> parts = new List<string>();

        if (item.minDamage > 0 || item.maxDamage > 0)
            parts.Add("Damage " + item.minDamage + "-" + item.maxDamage);

        AddStat(parts, "Armor", item.armor);
        AddStat(parts, "Strength", item.strength);
        AddStat(parts, "Rage", item.rage);
        AddStat(parts, "Reaction", item.reaction);
        AddStat(parts, "Agility", item.agility);
        AddStat(parts, "Endurance", item.endurance);
        AddStat(parts, "Luck", item.luck);
        AddStat(parts, "Intelligence", item.intelligence);

        return parts.Count > 0 ? string.Join("  ", parts) : "No stat bonuses";
    }

    private void AddStat(List<string> parts, string label, int value)
    {
        if (value != 0)
            parts.Add(label + " +" + value);
    }

    private string GetRarityLabel(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Common:
                return "Common";
            case ItemRarity.Rare:
                return "Rare";
            case ItemRarity.Epic:
                return "Epic";
            case ItemRarity.Legendary:
                return "Legendary";
            case ItemRarity.Mythic:
                return "Mythic";
            case ItemRarity.Named:
                return "Named";
            default:
                return rarity.ToString();
        }
    }

    private string GetCategoryLabel(ItemType itemType)
    {
        switch (itemType)
        {
            case ItemType.Helmet:
                return "Helmets";
            case ItemType.Weapon:
                return "Weapons";
            case ItemType.Armor:
                return "Armor";
            case ItemType.Gloves:
                return "Gloves";
            case ItemType.Belt:
                return "Belts";
            case ItemType.Legs:
                return "Legs";
            case ItemType.Boots:
                return "Boots";
            case ItemType.Ring:
                return "Rings";
            case ItemType.Amulet:
                return "Amulets";
            case ItemType.Artifact:
                return "Artifacts";
            default:
                return itemType.ToString();
        }
    }

    private TMP_Text CreateText(string objectName, Transform parent, string text, Vector2 anchoredPosition, Vector2 size, int fontSize, Color color, TextAlignmentOptions alignment)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.layer = gameObject.layer;
        textObject.transform.SetParent(parent, false);

        RectTransform rectTransform = textObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = size;

        TMP_Text label = textObject.GetComponent<TMP_Text>();
        label.text = text;
        label.fontSize = fontSize;
        label.color = color;
        label.alignment = alignment;
        label.textWrappingMode = TextWrappingModes.Normal;
        label.raycastTarget = false;

        return label;
    }

    private Button CreateButton(string objectName, Transform parent, string text, Vector2 anchoredPosition, Vector2 size, int fontSize)
    {
        RectTransform buttonRect = CreatePanel(objectName, parent, anchoredPosition, size, ButtonColor);
        Button button = buttonRect.gameObject.AddComponent<Button>();
        button.targetGraphic = buttonRect.GetComponent<Image>();

        CreateText("Text", buttonRect, text, Vector2.zero, size, fontSize, GoldColor, TextAlignmentOptions.Center);

        return button;
    }

    private RectTransform CreatePanel(string objectName, Transform parent, Vector2 anchoredPosition, Vector2 size, Color color)
    {
        GameObject panel = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panel.layer = gameObject.layer;
        panel.transform.SetParent(parent, false);

        RectTransform rectTransform = panel.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = size;

        Image image = panel.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = true;

        return rectTransform;
    }

    private void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
            Destroy(parent.GetChild(i).gameObject);
    }

    private void ResolveReferences()
    {
        if (navigationController == null)
            navigationController = FindSceneObject<LocationNavigationController>();

        if (playerStats == null)
            playerStats = FindSceneObject<PlayerStats>();

        if (playerInventory == null)
            playerInventory = FindSceneObject<PlayerInventory>();

        if (itemDatabase == null && playerInventory != null)
            itemDatabase = playerInventory.itemDatabase;

        if (itemDatabase == null)
            itemDatabase = FindResource<ItemDatabase>();

        if (barracksInventoryUI == null)
            barracksInventoryUI = FindSceneObject<BarracksInventoryUI>();

        if (equipmentManager == null)
            equipmentManager = FindSceneObject<EquipmentManager>();
    }

    private void EnsureDefaultShopEntries()
    {
        AddMissingShopEntry("Helmet_Long", 1250);
        AddMissingShopEntry("Helmet_Lord", 1650);
        AddMissingShopEntry("Sword_Pain", 1650);
        AddMissingShopEntry("Sword_Rage", 230);
        AddMissingShopEntry("Armor_Test", 30);
        AddMissingShopEntry("Gloves_Test", 20);
        AddMissingShopEntry("Belt_Test", 20);
        AddMissingShopEntry("Legs_Test", 25);
        AddMissingShopEntry("Boots_Test", 20);
        AddMissingShopEntry("Ring_Test", 35);
        AddMissingShopEntry("Amulet_Test", 40);
        AddMissingShopEntry("Artifact_Test", 50);
        shopEntries.RemoveAll(entry => entry == null || entry.itemData == null);
    }

    private void AddMissingShopEntry(string itemId, int fallbackPrice)
    {
        if (ContainsShopItem(itemId))
            return;

        ItemData itemData = itemDatabase != null ? itemDatabase.GetItemById(itemId) : null;

        if (itemData == null)
        {
            Debug.LogWarning("MarketWindowUI: Could not find shop item: " + itemId);
            return;
        }

        shopEntries.Add(new ShopEntry
        {
            itemData = itemData,
            price = fallbackPrice > 0 ? fallbackPrice : itemData.price
        });
    }

    private bool ContainsShopItem(string itemId)
    {
        foreach (ShopEntry entry in shopEntries)
        {
            if (entry != null &&
                entry.itemData != null &&
                ItemDatabase.GetStableItemId(entry.itemData) == itemId)
            {
                return true;
            }
        }

        return false;
    }

    private T FindSceneObject<T>() where T : UnityEngine.Object
    {
        T[] objects = Resources.FindObjectsOfTypeAll<T>();

        foreach (T foundObject in objects)
        {
            Component component = foundObject as Component;
            if (component != null && component.gameObject.scene.IsValid())
                return foundObject;
        }

        return null;
    }

    private T FindResource<T>() where T : UnityEngine.Object
    {
        T[] objects = Resources.FindObjectsOfTypeAll<T>();
        return objects.Length > 0 ? objects[0] : null;
    }
}
