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

    [Header("Editable UI")]
    [SerializeField] private bool useEditableUi = true;
    [SerializeField] private Image marketBackground;
    [SerializeField] private Button closeButton;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text tokensText;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private MarketModeButtonUI buyModeButton;
    [SerializeField] private MarketModeButtonUI sellModeButton;
    [SerializeField] private RectTransform categoryButtonContainer;
    [SerializeField] private MarketCategoryButtonUI categoryButtonPrefab;
    [SerializeField] private List<MarketCategoryButtonUI> categoryButtonViews = new List<MarketCategoryButtonUI>();
    [SerializeField] private RectTransform itemCardContainer;
    [SerializeField] private MarketItemCardUI itemCardPrefab;

    private TMP_Text buyModeText;
    private TMP_Text sellModeText;
    private Image buyModeBackground;
    private Image sellModeBackground;
    private RectTransform content;
    private Sprite marketFrameSprite;
    private Sprite itemCardFrameSprite;
    private readonly List<CategoryButtonBinding> categoryButtons = new List<CategoryButtonBinding>();

    private MarketMode currentMode = MarketMode.Buy;
    private ItemType currentCategory = ItemType.Armor;
    private bool isProcessingTransaction;
    private bool isBuilt;

    private static readonly Color BackgroundColor = new Color(0.035f, 0.03f, 0.025f, 0.96f);
    private static readonly Color PanelColor = new Color(0.075f, 0.065f, 0.052f, 0.94f);
    private static readonly Color CardColor = new Color(0.18f, 0.165f, 0.13f, 0.94f);
    private static readonly Color ButtonColor = new Color(0.075f, 0.058f, 0.04f, 0.96f);
    private static readonly Color SelectedColor = new Color(0.32f, 0.215f, 0.085f, 0.98f);
    private static readonly Color DisabledColor = new Color(0.12f, 0.12f, 0.12f, 0.85f);
    private static readonly Color GoldColor = new Color(0.9f, 0.69f, 0.36f, 1f);
    private static readonly Color TextColor = new Color(0.88f, 0.82f, 0.68f, 1f);
    private static readonly Color MutedTextColor = new Color(0.62f, 0.57f, 0.48f, 1f);
    private static readonly Color WarningColor = new Color(0.95f, 0.38f, 0.28f, 1f);
    private static readonly Color BorderColor = new Color(0.58f, 0.43f, 0.22f, 0.72f);
    private static readonly Color DarkBorderColor = new Color(0.02f, 0.017f, 0.013f, 1f);
    private static readonly Color PriceColor = new Color(1f, 0.78f, 0.33f, 1f);

    private static readonly Dictionary<string, int> TierOneShopPrices = new Dictionary<string, int>
    {
        { "T1_Berserker_RustySword", 50 },
        { "T1_Berserker_Blade", 150 },
        { "T1_Berserker_BloodrageGreatsword", 400 },
        { "T1_Berserker_CrackedHelm", 50 },
        { "T1_Berserker_Helm", 150 },
        { "T1_Berserker_BloodrageHelm", 400 },
        { "T1_Berserker_WornArmor", 50 },
        { "T1_Berserker_Armor", 150 },
        { "T1_Berserker_BloodrageArmor", 400 },
        { "T1_Berserker_Gloves", 50 },
        { "T1_Berserker_Gauntlets", 150 },
        { "T1_Berserker_BloodrageGauntlets", 400 },
        { "T1_Berserker_Boots", 50 },
        { "T1_Berserker_RareBoots", 150 },
        { "T1_Berserker_BloodrageBoots", 400 },
        { "T1_Berserker_Ring", 180 },
        { "T1_Berserker_BloodrageRing", 450 },
        { "T1_Berserker_Amulet", 180 },
        { "T1_Berserker_BloodrageAmulet", 450 },
        { "T1_Berserker_Belt", 50 },
        { "T1_Berserker_WarBelt", 180 },
        { "T1_Berserker_BloodrageBelt", 450 },

        { "T1_Gambler_RustyDagger", 50 },
        { "T1_Gambler_Blade", 150 },
        { "T1_Gambler_Fatepiercer", 400 },
        { "T1_Gambler_Hood", 50 },
        { "T1_Gambler_RareHood", 150 },
        { "T1_Gambler_FateHood", 400 },
        { "T1_Gambler_Vest", 50 },
        { "T1_Gambler_RareVest", 150 },
        { "T1_Gambler_FateVest", 400 },
        { "T1_Gambler_Gloves", 50 },
        { "T1_Gambler_RareGloves", 150 },
        { "T1_Gambler_FateGloves", 400 },
        { "T1_Gambler_Boots", 50 },
        { "T1_Gambler_RareBoots", 150 },
        { "T1_Gambler_FateBoots", 400 },
        { "T1_Gambler_Ring", 180 },
        { "T1_Gambler_FateRing", 450 },
        { "T1_Gambler_Amulet", 180 },
        { "T1_Gambler_FateAmulet", 450 },
        { "T1_Gambler_Belt", 50 },
        { "T1_Gambler_LuckyBelt", 180 },
        { "T1_Gambler_FateBelt", 450 },

        { "T1_Duelist_TrainingRapier", 50 },
        { "T1_Duelist_Rapier", 150 },
        { "T1_Duelist_SwiftRapier", 400 },
        { "T1_Duelist_Mask", 50 },
        { "T1_Duelist_RareMask", 150 },
        { "T1_Duelist_SwiftMask", 400 },
        { "T1_Duelist_Jacket", 50 },
        { "T1_Duelist_RareJacket", 150 },
        { "T1_Duelist_SwiftJacket", 400 },
        { "T1_Duelist_Gloves", 50 },
        { "T1_Duelist_RareGloves", 150 },
        { "T1_Duelist_SwiftGloves", 400 },
        { "T1_Duelist_Boots", 50 },
        { "T1_Duelist_RareBoots", 150 },
        { "T1_Duelist_SwiftBoots", 400 },
        { "T1_Duelist_Ring", 180 },
        { "T1_Duelist_SwiftRing", 450 },
        { "T1_Duelist_Amulet", 180 },
        { "T1_Duelist_SwiftAmulet", 450 },
        { "T1_Duelist_Belt", 50 },
        { "T1_Duelist_FineBelt", 180 },
        { "T1_Duelist_SwiftBelt", 450 }
    };

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
        LoadVisualAssets();
        EnsureDefaultShopEntries();
    }

    private void OnEnable()
    {
        ResolveReferences();
        LoadVisualAssets();
        EnsureDefaultShopEntries();

        if (HasEditableUi())
            WireEditableWindow();
        else if (!isBuilt)
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

        ApplySpriteImage(background, marketFrameSprite, Color.white, BackgroundColor, true);
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

    private bool HasEditableUi()
    {
        return useEditableUi &&
            itemCardContainer &&
            itemCardPrefab &&
            buyModeButton &&
            sellModeButton;
    }

    private void WireEditableWindow()
    {
        content = itemCardContainer;
        categoryButtons.Clear();

        if (!marketBackground)
            marketBackground = GetComponent<Image>();

        if (marketBackground && marketBackground.sprite == null)
            ApplySpriteImage(marketBackground, marketFrameSprite, Color.white, BackgroundColor, true);

        if (titleText)
            titleText.text = "MARKET";

        if (closeButton)
        {
            closeButton.onClick.RemoveListener(Close);
            closeButton.onClick.AddListener(Close);
        }

        if (!buyModeButton || !sellModeButton)
            return;

        buyModeButton.Initialize("BUY");
        sellModeButton.Initialize("SELL");
        buyModeBackground = buyModeButton.Background;
        buyModeText = buyModeButton.Label;
        sellModeBackground = sellModeButton.Background;
        sellModeText = sellModeButton.Label;

        if (buyModeButton.Button)
        {
            buyModeButton.Button.onClick.RemoveAllListeners();
            buyModeButton.Button.onClick.AddListener(() => SelectMode(MarketMode.Buy));
        }

        if (sellModeButton.Button)
        {
            sellModeButton.Button.onClick.RemoveAllListeners();
            sellModeButton.Button.onClick.AddListener(() => SelectMode(MarketMode.Sell));
        }

        EnsureEditableCategoryButtons();

        foreach (MarketCategoryButtonUI categoryButton in categoryButtonViews)
        {
            if (!categoryButton)
                continue;

            categoryButton.Initialize(categoryButton.ItemType, GetCategoryLabel(categoryButton.ItemType));
            categoryButtons.Add(new CategoryButtonBinding
            {
                itemType = categoryButton.ItemType,
                background = categoryButton.Background,
                label = categoryButton.Label
            });

            if (categoryButton.Button)
            {
                ItemType capturedType = categoryButton.ItemType;
                categoryButton.Button.onClick.RemoveAllListeners();
                categoryButton.Button.onClick.AddListener(() => SelectCategory(capturedType));
            }
        }

        isBuilt = true;
    }

    private void EnsureEditableCategoryButtons()
    {
        if (!categoryButtonContainer)
            return;

        categoryButtonViews.RemoveAll(button => button == null);

        if (categoryButtonViews.Count == 0)
            categoryButtonViews.AddRange(categoryButtonContainer.GetComponentsInChildren<MarketCategoryButtonUI>(true));

        if (categoryButtonViews.Count > 0 || !categoryButtonPrefab)
            return;

        foreach (ItemType itemType in categoryOrder)
        {
            MarketCategoryButtonUI button = Instantiate(categoryButtonPrefab, categoryButtonContainer);
            button.gameObject.name = "CategoryButton_" + itemType;
            button.Initialize(itemType, GetCategoryLabel(itemType));
            categoryButtonViews.Add(button);
        }
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
        AddFrame(panel.gameObject, BorderColor, new Vector2(2f, -2f));

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
        AddFrame(listPanel.gameObject, BorderColor, new Vector2(2f, -2f));

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
        if (!content)
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
        if (itemCardPrefab && content)
        {
            MarketItemCardUI cardView = Instantiate(itemCardPrefab, content);
            cardView.gameObject.name = "MarketCard_" + ItemDatabase.GetStableItemId(item);

            cardView.Bind(
                item,
                GetRarityLabel(item.rarity) + "  |  " + GetSlotLabel(item.itemType) + "  |  LVL " + Mathf.Max(item.requiredLevel, 1),
                BuildStatsText(item),
                price,
                actionText,
                canClick,
                action,
                GetRarityColor(item.rarity));
            return;
        }

        RectTransform card = CreatePanel("MarketCard_" + ItemDatabase.GetStableItemId(item), content, Vector2.zero, new Vector2(930f, 236f), CardColor);
        Image cardImage = card.GetComponent<Image>();
        ApplySpriteImage(cardImage, itemCardFrameSprite, Color.white, CardColor, true);

        if (itemCardFrameSprite == null)
            AddFrame(card.gameObject, GetRarityColor(item.rarity), new Vector2(2f, -2f));

        LayoutElement layoutElement = card.gameObject.AddComponent<LayoutElement>();
        layoutElement.minWidth = 900f;
        layoutElement.preferredWidth = 930f;
        layoutElement.minHeight = 236f;
        layoutElement.preferredHeight = 236f;

        CreateIcon(card, item.icon, new Vector2(-350f, 4f), new Vector2(150f, 150f));

        TMP_Text itemName = CreateText("ItemName", card, item.itemName, new Vector2(-70f, 78f), new Vector2(520f, 38f), 24, GetRarityColor(item.rarity), TextAlignmentOptions.Center);
        itemName.fontStyle = FontStyles.Bold;

        CreateText("Meta", card, GetRarityLabel(item.rarity) + "  |  " + GetSlotLabel(item.itemType) + "  |  LVL " + Mathf.Max(item.requiredLevel, 1), new Vector2(-70f, 42f), new Vector2(520f, 30f), 17, MutedTextColor, TextAlignmentOptions.Center);

        TMP_Text stats = CreateText("Stats", card, BuildStatsText(item), new Vector2(-35f, -35f), new Vector2(490f, 112f), 18, TextColor, TextAlignmentOptions.Left);
        stats.textWrappingMode = TextWrappingModes.Normal;
        stats.lineSpacing = -10f;

        TMP_Text priceText = CreateText("Price", card, price + "\nTOKENS", new Vector2(346f, 48f), new Vector2(150f, 58f), 20, PriceColor, TextAlignmentOptions.Center);
        priceText.fontStyle = FontStyles.Bold;

        Button actionButton = CreateButton("ActionButton", card, actionText, new Vector2(346f, -48f), new Vector2(170f, 58f), 21);
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
        AddFrame(frame.gameObject, BorderColor, new Vector2(2f, -2f));

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
        if (tokensText)
            tokensText.text = "Arena Tokens: " + (playerStats != null ? playerStats.arenaTokens : 0);
    }

    private void SetMessage(string message)
    {
        if (messageText)
            messageText.text = message;
    }

    private string BuildStatsText(ItemData item)
    {
        List<string> parts = new List<string>();

        if (item.minDamage > 0 || item.maxDamage > 0)
            parts.Add("Damage " + item.minDamage + "-" + item.maxDamage);

        AddStat(parts, GetArmorStatLabel(item.itemType), item.armor);

        AddStat(parts, "Strength", item.strength);

        AddStat(parts, "Rage", item.rage);
        AddStat(parts, "Reaction", item.reaction);
        AddStat(parts, "Agility", item.agility);
        AddStat(parts, "Endurance", item.endurance);
        AddStat(parts, "Luck", item.luck);
        AddStat(parts, "Intelligence", item.intelligence);

        return parts.Count > 0 ? string.Join("\n", parts) : "No stat bonuses";
    }

    private void AddStat(List<string> parts, string label, int value)
    {
        if (value != 0)
            parts.Add(label + " +" + value);
    }

    private string GetArmorStatLabel(ItemType itemType)
    {
        switch (itemType)
        {
            case ItemType.Helmet:
                return "ArmorHead";
            case ItemType.Armor:
                return "ArmorBody";
            case ItemType.Gloves:
                return "ArmorArms";
            case ItemType.Boots:
                return "ArmorLegs";
            default:
                return "Armor";
        }
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

    private Color32 GetRarityColor(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Rare:
                return new Color32(63, 167, 255, 255);
            case ItemRarity.Epic:
                return new Color32(163, 53, 238, 255);
            case ItemRarity.Legendary:
                return new Color32(214, 168, 74, 255);
            case ItemRarity.Mythic:
                return new Color32(255, 70, 70, 255);
            case ItemRarity.Named:
                return new Color32(255, 140, 40, 255);
            default:
                return new Color32(190, 185, 170, 255);
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

    private string GetSlotLabel(ItemType itemType)
    {
        switch (itemType)
        {
            case ItemType.Helmet:
                return "Helmet";
            case ItemType.Weapon:
                return "Weapon";
            case ItemType.Armor:
                return "Chest";
            case ItemType.Gloves:
                return "Gloves";
            case ItemType.Belt:
                return "Belt";
            case ItemType.Legs:
                return "Legs";
            case ItemType.Boots:
                return "Boots";
            case ItemType.Ring:
                return "Ring";
            case ItemType.Amulet:
                return "Amulet";
            case ItemType.Artifact:
                return "Artifact";
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
        Image buttonImage = buttonRect.GetComponent<Image>();
        ApplySpriteImage(buttonImage, marketFrameSprite, Color.white, ButtonColor, true);

        if (marketFrameSprite == null)
            AddFrame(buttonRect.gameObject, BorderColor, new Vector2(2f, -2f));

        Button button = buttonRect.gameObject.AddComponent<Button>();
        button.targetGraphic = buttonImage;

        TMP_Text label = CreateText("Text", buttonRect, text, Vector2.zero, size, fontSize, GoldColor, TextAlignmentOptions.Center);
        label.fontStyle = FontStyles.Bold;

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

    private void AddFrame(GameObject target, Color color, Vector2 distance)
    {
        if (target == null)
            return;

        Outline darkOutline = target.AddComponent<Outline>();
        darkOutline.effectColor = DarkBorderColor;
        darkOutline.effectDistance = distance * 2f;
        darkOutline.useGraphicAlpha = true;

        Outline goldOutline = target.AddComponent<Outline>();
        goldOutline.effectColor = color;
        goldOutline.effectDistance = distance;
        goldOutline.useGraphicAlpha = true;
    }

    private void ApplySpriteImage(Image image, Sprite sprite, Color spriteColor, Color fallbackColor, bool sliced)
    {
        if (image == null)
            return;

        if (sprite != null)
        {
            image.sprite = sprite;
            image.type = sliced ? Image.Type.Sliced : Image.Type.Simple;
            image.preserveAspect = false;
            image.color = spriteColor;
        }
        else
        {
            image.sprite = null;
            image.type = Image.Type.Simple;
            image.color = fallbackColor;
        }
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

    private void LoadVisualAssets()
    {
        if (marketFrameSprite == null)
            marketFrameSprite = LoadSpriteResource("Market/MarketFrame", new Vector4(86f, 86f, 86f, 86f));

        if (itemCardFrameSprite == null)
            itemCardFrameSprite = LoadSpriteResource("Market/MarketItemCardFrame", new Vector4(78f, 78f, 78f, 78f));
    }

    private Sprite LoadSpriteResource(string resourcePath, Vector4 border)
    {
        Texture2D texture = Resources.Load<Texture2D>(resourcePath);

        if (texture == null)
            return Resources.Load<Sprite>(resourcePath);

        return Sprite.Create(
            texture,
            new Rect(0f, 0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            100f,
            0,
            SpriteMeshType.FullRect,
            border);
    }

    private void EnsureDefaultShopEntries()
    {
        shopEntries.RemoveAll(entry =>
            entry == null ||
            entry.itemData == null ||
            !TierOneShopPrices.ContainsKey(ItemDatabase.GetStableItemId(entry.itemData)));

        foreach (KeyValuePair<string, int> shopItem in TierOneShopPrices)
            AddMissingShopEntry(shopItem.Key, shopItem.Value);
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
