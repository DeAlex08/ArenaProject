#if UNITY_EDITOR
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class MarketUiPrefabBuilder
{
    private const string ScenePath = "Assets/_Project/Scenes/MainMenu.unity";
    private const string PrefabFolder = "Assets/_Project/Prefabs/Market";
    private const string ButtonPrefabPath = PrefabFolder + "/MarketCategoryButtonPrefab.prefab";
    private const string ModeButtonPrefabPath = PrefabFolder + "/MarketModeButtonPrefab.prefab";
    private const string CardPrefabPath = PrefabFolder + "/MarketItemCardPrefab.prefab";
    private const string MarketFramePath = "Assets/_Project/Resources/Market/MarketFrame.png";
    private const string CardFramePath = "Assets/_Project/Resources/Market/MarketItemCardFrame.png";

    private static readonly Color Gold = new Color(0.9f, 0.69f, 0.36f, 1f);
    private static readonly Color Text = new Color(0.88f, 0.82f, 0.68f, 1f);
    private static readonly Color Muted = new Color(0.62f, 0.57f, 0.48f, 1f);
    private static readonly Color Dark = new Color(0.035f, 0.03f, 0.025f, 0.96f);
    private static readonly Color Warn = new Color(0.95f, 0.38f, 0.28f, 1f);

    [MenuItem("ArenaProject/Build Editable Market UI")]
    public static void BuildEditableMarketUi()
    {
        Debug.Log("MarketUiPrefabBuilder: Step 01 - start.");
        EnsureFolders();
        Debug.Log("MarketUiPrefabBuilder: Step 02 - folders ensured.");
        ConfigureSpriteImport(MarketFramePath, new Vector4(86f, 86f, 86f, 86f));
        ConfigureSpriteImport(CardFramePath, new Vector4(78f, 78f, 78f, 78f));
        Debug.Log("MarketUiPrefabBuilder: Step 03 - sprite import configured.");

        Sprite buttonFrame = AssetDatabase.LoadAssetAtPath<Sprite>(MarketFramePath);
        Sprite cardFrame = AssetDatabase.LoadAssetAtPath<Sprite>(CardFramePath);
        Debug.Log("MarketUiPrefabBuilder: Step 04 - sprites loaded. ButtonFrame=" + (buttonFrame != null) + ", CardFrame=" + (cardFrame != null));

        MarketCategoryButtonUI categoryButtonPrefab = CreateCategoryButtonPrefab(buttonFrame);
        MarketModeButtonUI modeButtonPrefab = CreateModeButtonPrefab(buttonFrame);
        MarketItemCardUI cardPrefab = CreateItemCardPrefab(buttonFrame, cardFrame);
        Debug.Log("MarketUiPrefabBuilder: Step 05 - prefabs created/loaded.");

        if (categoryButtonPrefab == null || modeButtonPrefab == null || cardPrefab == null)
        {
            Debug.LogError("MarketUiPrefabBuilder: Failed to create one or more Market prefabs.");
            return;
        }

        UnityEngine.SceneManagement.Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        Debug.Log("MarketUiPrefabBuilder: Step 06 - MainMenu scene opened.");
        GameObject marketWindow = FindSceneGameObject("MarketWindow");

        if (marketWindow == null)
        {
            Debug.LogError("MarketUiPrefabBuilder: MarketWindow not found in MainMenu scene.");
            return;
        }

        try
        {
            BuildSceneMarketWindow(marketWindow, buttonFrame, categoryButtonPrefab, modeButtonPrefab, cardPrefab);
        }
        catch (System.Exception exception)
        {
            Debug.LogError("MarketUiPrefabBuilder: Build failed at runtime step. Exact exception:\n" + exception);
            throw;
        }

        Debug.Log("MarketUiPrefabBuilder: Step 20 - editable hierarchy built.");
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("MarketUiPrefabBuilder: Editable Market UI prefabs and MainMenu hierarchy updated.");
    }

    private static void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder("Assets/_Project/Editor"))
            AssetDatabase.CreateFolder("Assets/_Project", "Editor");

        if (!AssetDatabase.IsValidFolder(PrefabFolder))
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Prefabs"))
                AssetDatabase.CreateFolder("Assets/_Project", "Prefabs");

            AssetDatabase.CreateFolder("Assets/_Project/Prefabs", "Market");
        }
    }

    private static void ConfigureSpriteImport(string path, Vector4 border)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
            return;

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.spriteBorder = border;
        importer.mipmapEnabled = false;
        importer.SaveAndReimport();
    }

    private static MarketCategoryButtonUI CreateCategoryButtonPrefab(Sprite frame)
    {
        GameObject root = CreateUiObject("MarketCategoryButtonPrefab", typeof(Image), typeof(Button), typeof(MarketCategoryButtonUI));
        RectTransform rect = root.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(220f, 58f);

        Image image = root.GetComponent<Image>();
        image.sprite = frame;
        image.type = Image.Type.Sliced;
        image.color = Color.white;

        TMP_Text label = CreateText("Text", root.transform, "Category", 20, Gold, TextAlignmentOptions.Center);
        Stretch(label.rectTransform, Vector2.zero, Vector2.zero);
        label.fontStyle = FontStyles.Bold;

        MarketCategoryButtonUI view = root.GetComponent<MarketCategoryButtonUI>();
        SetSerialized(view, "button", root.GetComponent<Button>());
        SetSerialized(view, "background", image);
        SetSerialized(view, "label", label);

        return SavePrefab<MarketCategoryButtonUI>(root, ButtonPrefabPath);
    }

    private static MarketModeButtonUI CreateModeButtonPrefab(Sprite frame)
    {
        GameObject root = CreateUiObject("MarketModeButtonPrefab", typeof(Image), typeof(Button), typeof(MarketModeButtonUI));
        RectTransform rect = root.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(190f, 62f);

        Image image = root.GetComponent<Image>();
        image.sprite = frame;
        image.type = Image.Type.Sliced;
        image.color = Color.white;

        TMP_Text label = CreateText("Text", root.transform, "BUY", 26, Gold, TextAlignmentOptions.Center);
        Stretch(label.rectTransform, Vector2.zero, Vector2.zero);
        label.fontStyle = FontStyles.Bold;

        MarketModeButtonUI view = root.GetComponent<MarketModeButtonUI>();
        SetSerialized(view, "button", root.GetComponent<Button>());
        SetSerialized(view, "background", image);
        SetSerialized(view, "label", label);

        return SavePrefab<MarketModeButtonUI>(root, ModeButtonPrefabPath);
    }

    private static MarketItemCardUI CreateItemCardPrefab(Sprite buttonFrame, Sprite cardFrame)
    {
        GameObject root = CreateUiObject("MarketItemCardPrefab", typeof(Image), typeof(LayoutElement), typeof(MarketItemCardUI));
        RectTransform rect = root.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(930f, 236f);

        Image background = root.GetComponent<Image>();
        background.sprite = cardFrame;
        background.type = Image.Type.Sliced;
        background.color = Color.white;

        LayoutElement layout = root.GetComponent<LayoutElement>();
        layout.preferredHeight = 236f;
        layout.minHeight = 236f;

        Image iconFrame = CreateImage("IconFrame", root.transform, new Vector2(-350f, 4f), new Vector2(150f, 150f), buttonFrame, true, Color.white);
        Image icon = CreateImage("Icon", iconFrame.transform, Vector2.zero, Vector2.zero, null, false, Color.white);
        Stretch(icon.rectTransform, new Vector2(10f, 10f), new Vector2(-10f, -10f));
        icon.preserveAspect = true;
        icon.raycastTarget = false;

        TMP_Text name = CreateText("ItemName", root.transform, "Item Name", 24, Gold, TextAlignmentOptions.Center);
        SetRect(name.rectTransform, new Vector2(-70f, 78f), new Vector2(520f, 38f));
        name.fontStyle = FontStyles.Bold;

        TMP_Text meta = CreateText("Meta", root.transform, "Rare | Weapon | LVL 1", 17, Muted, TextAlignmentOptions.Center);
        SetRect(meta.rectTransform, new Vector2(-70f, 42f), new Vector2(520f, 30f));

        TMP_Text stats = CreateText("Stats", root.transform, "Damage 10-15\nStrength +4", 18, Text, TextAlignmentOptions.Left);
        SetRect(stats.rectTransform, new Vector2(-35f, -35f), new Vector2(490f, 112f));
        stats.lineSpacing = -10f;

        TMP_Text price = CreateText("Price", root.transform, "150\nTOKENS", 20, Gold, TextAlignmentOptions.Center);
        SetRect(price.rectTransform, new Vector2(346f, 48f), new Vector2(150f, 58f));
        price.fontStyle = FontStyles.Bold;

        GameObject actionObject = CreateUiObject("ActionButton", typeof(Image), typeof(Button));
        actionObject.transform.SetParent(root.transform, false);
        RectTransform actionRect = actionObject.GetComponent<RectTransform>();
        SetRect(actionRect, new Vector2(346f, -48f), new Vector2(170f, 58f));
        Image actionImage = actionObject.GetComponent<Image>();
        actionImage.sprite = buttonFrame;
        actionImage.type = Image.Type.Sliced;
        actionImage.color = Color.white;

        TMP_Text actionLabel = CreateText("Text", actionObject.transform, "BUY", 21, Gold, TextAlignmentOptions.Center);
        Stretch(actionLabel.rectTransform, Vector2.zero, Vector2.zero);
        actionLabel.fontStyle = FontStyles.Bold;

        MarketItemCardUI view = root.GetComponent<MarketItemCardUI>();
        SetSerialized(view, "background", background);
        SetSerialized(view, "icon", icon);
        SetSerialized(view, "itemNameText", name);
        SetSerialized(view, "metaText", meta);
        SetSerialized(view, "statsText", stats);
        SetSerialized(view, "priceText", price);
        SetSerialized(view, "actionButton", actionObject.GetComponent<Button>());
        SetSerialized(view, "actionButtonText", actionLabel);
        SetSerialized(view, "actionButtonBackground", actionImage);

        return SavePrefab<MarketItemCardUI>(root, CardPrefabPath);
    }

    private static void BuildSceneMarketWindow(
        GameObject marketWindow,
        Sprite frame,
        MarketCategoryButtonUI categoryPrefab,
        MarketModeButtonUI modePrefab,
        MarketItemCardUI cardPrefab)
    {
        Debug.Log("MarketUiPrefabBuilder: Step 07 - configuring MarketWindow root.");
        RectTransform root = marketWindow.GetComponent<RectTransform>();
        root.anchorMin = new Vector2(0.5f, 0.5f);
        root.anchorMax = new Vector2(0.5f, 0.5f);
        root.pivot = new Vector2(0.5f, 0.5f);
        root.anchoredPosition = new Vector2(225f, 0f);
        root.sizeDelta = new Vector2(1470f, 1080f);

        Image background = marketWindow.GetComponent<Image>();
        if (background == null)
            background = marketWindow.AddComponent<Image>();
        background.sprite = frame;
        background.type = Image.Type.Sliced;
        background.color = Color.white;

        MarketWindowUI marketUi = marketWindow.GetComponent<MarketWindowUI>();
        Debug.Log("MarketUiPrefabBuilder: Step 08 - clearing old MarketWindowUI serialized refs.");
        ClearMarketUiBindings(marketUi);
        Debug.Log("MarketUiPrefabBuilder: Step 09 - destroying old MarketWindow children.");
        ClearChildren(marketWindow.transform);

        Debug.Log("MarketUiPrefabBuilder: Step 10 - creating header texts.");
        TMP_Text title = CreateText("Title", marketWindow.transform, "MARKET", 44, Gold, TextAlignmentOptions.Center);
        SetRect(title.rectTransform, new Vector2(0f, 470f), new Vector2(520f, 70f));

        TMP_Text tokens = CreateText("ArenaTokensText", marketWindow.transform, "Arena Tokens: 0", 26, Text, TextAlignmentOptions.Left);
        SetRect(tokens.rectTransform, new Vector2(-510f, 405f), new Vector2(420f, 48f));

        TMP_Text message = CreateText("MessageText", marketWindow.transform, string.Empty, 24, Warn, TextAlignmentOptions.Center);
        SetRect(message.rectTransform, new Vector2(70f, 405f), new Vector2(640f, 48f));

        Debug.Log("MarketUiPrefabBuilder: Step 11 - creating ModeButtonsRoot.");
        RectTransform modeButtonsRoot = CreateTransparentPanel("ModeButtonsRoot", marketWindow.transform, new Vector2(0f, 340f), new Vector2(420f, 70f));

        Debug.Log("MarketUiPrefabBuilder: Step 12 - creating fresh close/buy/sell buttons.");
        MarketModeButtonUI closeModeButton = CreateSceneModeButton("CloseButton", marketWindow.transform, "X", new Vector2(650f, 438f), new Vector2(84f, 64f), 32, frame);
        Button close = closeModeButton.GetComponent<Button>();

        MarketModeButtonUI buy = CreateSceneModeButton("BuyModeButton", modeButtonsRoot, "BUY", new Vector2(-105f, 0f), new Vector2(190f, 62f), 26, frame);
        MarketModeButtonUI sell = CreateSceneModeButton("SellModeButton", modeButtonsRoot, "SELL", new Vector2(105f, 0f), new Vector2(190f, 62f), 26, frame);

        Debug.Log("MarketUiPrefabBuilder: Step 13 - creating CategoryButtonsRoot.");
        RectTransform categoryRoot = CreatePanel("CategoryButtonsRoot", marketWindow.transform, new Vector2(-530f, -90f), new Vector2(260f, 760f));
        VerticalLayoutGroup categoryLayout = categoryRoot.gameObject.AddComponent<VerticalLayoutGroup>();
        categoryLayout.padding = new RectOffset(12, 12, 16, 16);
        categoryLayout.spacing = 10f;
        categoryLayout.childAlignment = TextAnchor.UpperCenter;
        categoryLayout.childControlWidth = true;
        categoryLayout.childControlHeight = false;
        categoryLayout.childForceExpandWidth = true;
        categoryLayout.childForceExpandHeight = false;

        List<MarketCategoryButtonUI> categories = new List<MarketCategoryButtonUI>();
        Debug.Log("MarketUiPrefabBuilder: Step 14 - creating category buttons.");
        foreach (ItemType itemType in GetCategoryOrder())
        {
            MarketCategoryButtonUI category = CreateSceneCategoryButton(itemType, categoryRoot, frame);
            LayoutElement categoryLayoutElement = category.gameObject.GetComponent<LayoutElement>();
            if (categoryLayoutElement == null)
                categoryLayoutElement = category.gameObject.AddComponent<LayoutElement>();
            categoryLayoutElement.preferredHeight = 58f;
            categoryLayoutElement.minHeight = 58f;
            categories.Add(category);
        }

        Debug.Log("MarketUiPrefabBuilder: Step 15 - creating ItemsRoot/ScrollView/Viewport/Content.");
        RectTransform itemsRoot = CreatePanel("ItemsRoot", marketWindow.transform, new Vector2(145f, -90f), new Vector2(1040f, 760f));
        RectTransform scrollView = CreateTransparentPanel("ItemsScrollView", itemsRoot, Vector2.zero, Vector2.zero);
        Stretch(scrollView, Vector2.zero, Vector2.zero);

        ScrollRect scroll = scrollView.gameObject.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;

        RectTransform viewport = CreateTransparentPanel("Viewport", scrollView, Vector2.zero, Vector2.zero);
        Stretch(viewport, new Vector2(18f, 18f), new Vector2(-18f, -18f));
        viewport.gameObject.AddComponent<RectMask2D>();
        scroll.viewport = viewport;

        RectTransform content = CreateTransparentPanel("Content", viewport, Vector2.zero, Vector2.zero);
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        content.anchoredPosition = Vector2.zero;
        content.sizeDelta = Vector2.zero;
        scroll.content = content;

        VerticalLayoutGroup contentLayout = content.gameObject.AddComponent<VerticalLayoutGroup>();
        contentLayout.padding = new RectOffset(10, 10, 10, 10);
        contentLayout.spacing = 12f;
        contentLayout.childAlignment = TextAnchor.UpperCenter;
        contentLayout.childControlWidth = true;
        contentLayout.childControlHeight = false;
        contentLayout.childForceExpandWidth = true;
        contentLayout.childForceExpandHeight = false;
        ContentSizeFitter fitter = content.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        Debug.Log("MarketUiPrefabBuilder: Step 16 - assigning MarketWindowUI serialized refs.");
        SetSerialized(marketUi, "useEditableUi", true);
        SetSerialized(marketUi, "marketBackground", background);
        SetSerialized(marketUi, "closeButton", close);
        SetSerialized(marketUi, "titleText", title);
        SetSerialized(marketUi, "tokensText", tokens);
        SetSerialized(marketUi, "messageText", message);
        SetSerialized(marketUi, "buyModeButton", buy);
        SetSerialized(marketUi, "sellModeButton", sell);
        SetSerialized(marketUi, "categoryButtonContainer", categoryRoot);
        SetSerialized(marketUi, "categoryButtonPrefab", categoryPrefab);
        SetSerializedObjectList(marketUi, "categoryButtonViews", categories);
        SetSerialized(marketUi, "itemCardContainer", content);
        SetSerialized(marketUi, "itemCardPrefab", cardPrefab);
        Debug.Log("MarketUiPrefabBuilder: Step 17 - MarketWindowUI serialized refs assigned.");
    }

    private static RectTransform CreatePanel(string name, Transform parent, Vector2 anchoredPosition, Vector2 size)
    {
        GameObject panel = CreateUiObject(name, typeof(Image));
        panel.transform.SetParent(parent, false);
        RectTransform rect = panel.GetComponent<RectTransform>();
        SetRect(rect, anchoredPosition, size);
        Image image = panel.GetComponent<Image>();
        image.color = new Color(0.075f, 0.065f, 0.052f, 0.55f);
        return rect;
    }

    private static RectTransform CreateTransparentPanel(string name, Transform parent, Vector2 anchoredPosition, Vector2 size)
    {
        GameObject panel = CreateUiObject(name);
        panel.transform.SetParent(parent, false);
        RectTransform rect = panel.GetComponent<RectTransform>();
        SetRect(rect, anchoredPosition, size);
        return rect;
    }

    private static MarketModeButtonUI CreateSceneModeButton(
        string name,
        Transform parent,
        string labelText,
        Vector2 anchoredPosition,
        Vector2 size,
        int fontSize,
        Sprite frame)
    {
        GameObject buttonObject = CreateUiObject(name, typeof(Image), typeof(Button), typeof(MarketModeButtonUI));
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        SetRect(rect, anchoredPosition, size);

        Image image = buttonObject.GetComponent<Image>();
        image.sprite = frame;
        image.type = Image.Type.Sliced;
        image.color = Color.white;

        TMP_Text label = CreateText("Text", buttonObject.transform, labelText, fontSize, Gold, TextAlignmentOptions.Center);
        Stretch(label.rectTransform, Vector2.zero, Vector2.zero);
        label.fontStyle = FontStyles.Bold;

        MarketModeButtonUI modeButton = buttonObject.GetComponent<MarketModeButtonUI>();
        SetSerialized(modeButton, "button", buttonObject.GetComponent<Button>());
        SetSerialized(modeButton, "background", image);
        SetSerialized(modeButton, "label", label);
        modeButton.Initialize(labelText);

        return modeButton;
    }

    private static MarketCategoryButtonUI CreateSceneCategoryButton(ItemType itemType, Transform parent, Sprite frame)
    {
        GameObject buttonObject = CreateUiObject("CategoryButton_" + itemType, typeof(Image), typeof(Button), typeof(MarketCategoryButtonUI));
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        SetRect(rect, Vector2.zero, new Vector2(220f, 58f));

        Image image = buttonObject.GetComponent<Image>();
        image.sprite = frame;
        image.type = Image.Type.Sliced;
        image.color = Color.white;

        TMP_Text label = CreateText("Text", buttonObject.transform, GetCategoryLabelStatic(itemType), 20, Gold, TextAlignmentOptions.Center);
        Stretch(label.rectTransform, Vector2.zero, Vector2.zero);
        label.fontStyle = FontStyles.Bold;

        MarketCategoryButtonUI categoryButton = buttonObject.GetComponent<MarketCategoryButtonUI>();
        SetSerialized(categoryButton, "button", buttonObject.GetComponent<Button>());
        SetSerialized(categoryButton, "background", image);
        SetSerialized(categoryButton, "label", label);
        categoryButton.Initialize(itemType, GetCategoryLabelStatic(itemType));

        return categoryButton;
    }

    private static Image CreateImage(string name, Transform parent, Vector2 anchoredPosition, Vector2 size, Sprite sprite, bool sliced, Color color)
    {
        GameObject imageObject = CreateUiObject(name, typeof(Image));
        imageObject.transform.SetParent(parent, false);
        RectTransform rect = imageObject.GetComponent<RectTransform>();
        SetRect(rect, anchoredPosition, size);
        Image image = imageObject.GetComponent<Image>();
        image.sprite = sprite;
        image.type = sliced ? Image.Type.Sliced : Image.Type.Simple;
        image.color = color;
        return image;
    }

    private static TMP_Text CreateText(string name, Transform parent, string text, int fontSize, Color color, TextAlignmentOptions alignment)
    {
        GameObject textObject = CreateUiObject(name, typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);
        TMP_Text label = textObject.GetComponent<TMP_Text>();
        label.text = text;
        label.fontSize = fontSize;
        label.color = color;
        label.alignment = alignment;
        label.raycastTarget = false;
        return label;
    }

    private static GameObject CreateUiObject(string name, params System.Type[] components)
    {
        List<System.Type> types = new List<System.Type> { typeof(RectTransform), typeof(CanvasRenderer) };
        types.AddRange(components);
        GameObject gameObject = new GameObject(name, types.ToArray());
        gameObject.layer = 5;
        return gameObject;
    }

    private static void SetRect(RectTransform rect, Vector2 anchoredPosition, Vector2 size)
    {
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
    }

    private static void Stretch(RectTransform rect, Vector2 offsetMin, Vector2 offsetMax)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
    }

    private static T SavePrefab<T>(GameObject root, string path) where T : Component
    {
        PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        return prefab != null ? prefab.GetComponent<T>() : null;
    }

    private static void ClearMarketUiBindings(MarketWindowUI marketUi)
    {
        if (marketUi == null)
            return;

        SetSerialized(marketUi, "marketBackground", null);
        SetSerialized(marketUi, "closeButton", null);
        SetSerialized(marketUi, "titleText", null);
        SetSerialized(marketUi, "tokensText", null);
        SetSerialized(marketUi, "messageText", null);
        SetSerialized(marketUi, "buyModeButton", null);
        SetSerialized(marketUi, "sellModeButton", null);
        SetSerialized(marketUi, "categoryButtonContainer", null);
        SetSerialized(marketUi, "categoryButtonPrefab", null);
        SetSerializedObjectList(marketUi, "categoryButtonViews", new List<MarketCategoryButtonUI>());
        SetSerialized(marketUi, "itemCardContainer", null);
        SetSerialized(marketUi, "itemCardPrefab", null);
    }

    private static void SetSerialized(Object target, string propertyName, Object value)
    {
        if (target == null)
            return;

        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(propertyName);

        if (property == null)
        {
            Debug.LogWarning("MarketUiPrefabBuilder: Missing serialized property " + propertyName + " on " + target.name);
            return;
        }

        property.objectReferenceValue = value;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetSerialized(Object target, string propertyName, bool value)
    {
        if (target == null)
            return;

        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(propertyName);

        if (property == null)
        {
            Debug.LogWarning("MarketUiPrefabBuilder: Missing serialized property " + propertyName + " on " + target.name);
            return;
        }

        property.boolValue = value;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetSerializedObjectList<T>(Object target, string propertyName, List<T> values) where T : Object
    {
        if (target == null)
            return;

        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty list = serializedObject.FindProperty(propertyName);

        if (list == null)
        {
            Debug.LogWarning("MarketUiPrefabBuilder: Missing serialized list property " + propertyName + " on " + target.name);
            return;
        }

        list.ClearArray();

        for (int i = 0; i < values.Count; i++)
        {
            list.InsertArrayElementAtIndex(i);
            list.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }

        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
            Object.DestroyImmediate(parent.GetChild(i).gameObject);
    }

    private static GameObject FindSceneGameObject(string objectName)
    {
        GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>();

        foreach (GameObject foundObject in objects)
        {
            if (foundObject.name == objectName && foundObject.scene.IsValid())
                return foundObject;
        }

        return null;
    }

    private static ItemType[] GetCategoryOrder()
    {
        return new[]
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
    }

    private static string GetCategoryLabelStatic(ItemType itemType)
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
}
#endif
