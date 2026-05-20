using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MarketWindowUI : MonoBehaviour
{
    [Serializable]
    private class ShopEntry
    {
        public ItemData itemData;
        public int price;
    }

    [Header("Navigation")]
    [SerializeField] private LocationNavigationController navigationController;

    [Header("Systems")]
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private ItemDatabase itemDatabase;
    [SerializeField] private BarracksInventoryUI barracksInventoryUI;

    [Header("Shop")]
    [SerializeField] private List<ShopEntry> shopEntries = new List<ShopEntry>();

    private TMP_Text tokensText;
    private TMP_Text messageText;
    private RectTransform content;
    private bool isPurchasing;
    private bool isBuilt;

    private static readonly Color BackgroundColor = new Color(0.04f, 0.035f, 0.03f, 0.96f);
    private static readonly Color PanelColor = new Color(0.12f, 0.105f, 0.08f, 0.92f);
    private static readonly Color CardColor = new Color(0.19f, 0.17f, 0.13f, 0.95f);
    private static readonly Color GoldColor = new Color(0.9f, 0.69f, 0.36f, 1f);
    private static readonly Color TextColor = new Color(0.88f, 0.82f, 0.68f, 1f);
    private static readonly Color WarningColor = new Color(0.95f, 0.38f, 0.28f, 1f);

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

        RefreshTokens();
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
        ClearChildren();

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
        tokensText = CreateText("ArenaTokensText", root, string.Empty, new Vector2(-510f, 395f), new Vector2(420f, 48f), 26, TextColor, TextAlignmentOptions.Left);
        messageText = CreateText("MessageText", root, string.Empty, new Vector2(0f, 395f), new Vector2(600f, 48f), 24, WarningColor, TextAlignmentOptions.Center);

        Button closeButton = CreateButton("CloseButton", root, "X", new Vector2(650f, 438f), new Vector2(84f, 64f), 32);
        closeButton.onClick.AddListener(Close);

        RectTransform listPanel = CreatePanel("ShopListPanel", root, new Vector2(0f, -55f), new Vector2(1260f, 790f), PanelColor);
        ScrollRect scrollRect = listPanel.gameObject.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;

        RectTransform viewport = CreatePanel("Viewport", listPanel, Vector2.zero, Vector2.zero, new Color(0f, 0f, 0f, 0f));
        viewport.anchorMin = Vector2.zero;
        viewport.anchorMax = Vector2.one;
        viewport.offsetMin = new Vector2(18f, 18f);
        viewport.offsetMax = new Vector2(-18f, -18f);
        Mask mask = viewport.gameObject.AddComponent<Mask>();
        mask.showMaskGraphic = false;
        scrollRect.viewport = viewport;

        content = CreatePanel("Content", viewport, Vector2.zero, Vector2.zero, new Color(0f, 0f, 0f, 0f));
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        content.anchoredPosition = Vector2.zero;
        content.sizeDelta = new Vector2(0f, 0f);
        scrollRect.content = content;

        VerticalLayoutGroup layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(12, 12, 12, 12);
        layout.spacing = 14f;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = content.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        foreach (ShopEntry entry in shopEntries)
        {
            if (entry != null && entry.itemData != null)
                CreateShopCard(entry);
        }

        isBuilt = true;
    }

    private void CreateShopCard(ShopEntry entry)
    {
        RectTransform card = CreatePanel("ShopCard_" + ItemDatabase.GetStableItemId(entry.itemData), content, Vector2.zero, new Vector2(0f, 148f), CardColor);
        LayoutElement layoutElement = card.gameObject.AddComponent<LayoutElement>();
        layoutElement.minHeight = 148f;
        layoutElement.preferredHeight = 148f;

        CreateText("ItemName", card, entry.itemData.itemName, new Vector2(-420f, 38f), new Vector2(330f, 44f), 26, GoldColor, TextAlignmentOptions.Left);
        CreateText("ItemType", card, entry.itemData.itemType.ToString(), new Vector2(-420f, -8f), new Vector2(330f, 36f), 20, TextColor, TextAlignmentOptions.Left);
        CreateText("Stats", card, BuildStatsText(entry.itemData), new Vector2(-40f, 10f), new Vector2(520f, 92f), 20, TextColor, TextAlignmentOptions.Left);
        CreateText("Price", card, entry.price + " Tokens", new Vector2(420f, 32f), new Vector2(190f, 40f), 22, GoldColor, TextAlignmentOptions.Center);

        Button buyButton = CreateButton("BuyButton", card, "BUY", new Vector2(420f, -36f), new Vector2(190f, 58f), 24);
        buyButton.onClick.AddListener(() => BuyItem(entry));
    }

    private void BuyItem(ShopEntry entry)
    {
        if (isPurchasing)
            return;

        if (entry == null || entry.itemData == null)
            return;

        if (playerStats == null || playerInventory == null)
        {
            SetMessage("Market is not ready");
            Debug.LogWarning("MarketWindowUI: Missing PlayerStats or PlayerInventory.");
            return;
        }

        if (playerStats.arenaTokens < entry.price)
        {
            SetMessage("Not enough Arena Tokens");
            Debug.Log("MarketWindowUI: Not enough Arena Tokens to buy " + entry.itemData.itemName);
            return;
        }

        isPurchasing = true;

        playerStats.arenaTokens -= entry.price;
        playerStats.SaveProgression();
        playerInventory.AddItem(entry.itemData);

        RefreshTokens();

        if (barracksInventoryUI != null)
            barracksInventoryUI.RefreshCurrentCategory();

        SetMessage("Purchased: " + entry.itemData.itemName);
        Debug.Log("MarketWindowUI: Purchased " + entry.itemData.itemName + " for " + entry.price + " Arena Tokens.");

        isPurchasing = false;
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

        AddStat(parts, "Armor", item.armor);
        AddStat(parts, "Strength", item.strength);
        AddStat(parts, "Rage", item.rage);
        AddStat(parts, "Reaction", item.reaction);
        AddStat(parts, "Agility", item.agility);
        AddStat(parts, "Endurance", item.endurance);
        AddStat(parts, "Luck", item.luck);
        AddStat(parts, "Intelligence", item.intelligence);

        if (item.minDamage > 0 || item.maxDamage > 0)
            parts.Insert(0, "Damage " + item.minDamage + "-" + item.maxDamage);

        return parts.Count > 0 ? string.Join("  ", parts) : "No stat bonuses";
    }

    private void AddStat(List<string> parts, string label, int value)
    {
        if (value != 0)
            parts.Add(label + " +" + value);
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
        RectTransform buttonRect = CreatePanel(objectName, parent, anchoredPosition, size, new Color(0.08f, 0.055f, 0.035f, 0.96f));
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

    private void ClearChildren()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);
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
    }

    private void EnsureDefaultShopEntries()
    {
        if (shopEntries != null && shopEntries.Count > 0)
            return;

        shopEntries = new List<ShopEntry>
        {
            CreateShopEntry("Armor_Test", 30),
            CreateShopEntry("Gloves_Test", 20),
            CreateShopEntry("Belt_Test", 20),
            CreateShopEntry("Legs_Test", 25),
            CreateShopEntry("Boots_Test", 20),
            CreateShopEntry("Ring_Test", 35),
            CreateShopEntry("Amulet_Test", 40),
            CreateShopEntry("Artifact_Test", 50)
        };

        shopEntries.RemoveAll(entry => entry == null || entry.itemData == null);
    }

    private ShopEntry CreateShopEntry(string itemId, int price)
    {
        ItemData itemData = itemDatabase != null ? itemDatabase.GetItemById(itemId) : null;

        if (itemData == null)
        {
            Debug.LogWarning("MarketWindowUI: Could not find shop item: " + itemId);
            return null;
        }

        return new ShopEntry
        {
            itemData = itemData,
            price = price
        };
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
