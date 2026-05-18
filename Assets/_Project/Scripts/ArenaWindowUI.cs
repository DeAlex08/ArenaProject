using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ArenaWindowUI : MonoBehaviour
{
    [System.Serializable]
    private class ArenaEnemyPreview
    {
        public string enemyName = "Arena Fighter";
        public int power = 1000;
        public string reward = "10 tokens";
    }

    [Header("Navigation")]
    [SerializeField] private LocationNavigationController navigationController;

    [Header("Arena State")]
    [SerializeField] private string rank = "Bronze III";
    [SerializeField] private int arenaTokens = 0;

    [Header("Enemy Previews")]
    [SerializeField] private ArenaEnemyPreview[] enemies =
    {
        new ArenaEnemyPreview { enemyName = "Grave Duelist", power = 1800, reward = "10 tokens" },
        new ArenaEnemyPreview { enemyName = "Ash Knight", power = 2400, reward = "15 tokens" },
        new ArenaEnemyPreview { enemyName = "Blood Champion", power = 3200, reward = "25 tokens" }
    };

    private bool isBuilt;

    private readonly Color panelColor = new Color(0.015f, 0.013f, 0.012f, 0.985f);
    private readonly Color cardColor = new Color(0.09f, 0.075f, 0.06f, 0.98f);
    private readonly Color portraitColor = new Color(0.04f, 0.038f, 0.035f, 0.98f);
    private readonly Color textColor = new Color(0.96f, 0.89f, 0.70f, 1f);
    private readonly Color titleColor = new Color(0.95f, 0.73f, 0.36f, 1f);
    private readonly Color mutedTextColor = new Color(0.72f, 0.65f, 0.52f, 1f);
    private readonly Color borderColor = new Color(0.50f, 0.34f, 0.18f, 1f);
    private readonly Color darkBorderColor = new Color(0.13f, 0.075f, 0.035f, 1f);
    private readonly Color buttonColor = new Color(0.22f, 0.14f, 0.07f, 1f);
    private readonly Color buttonPressedColor = new Color(0.34f, 0.22f, 0.11f, 1f);
    private readonly Color closeButtonColor = new Color(0.12f, 0.08f, 0.05f, 1f);

    private void Awake()
    {
        BuildIfNeeded();
    }

    private void OnEnable()
    {
        BuildIfNeeded();
    }

    public void Close()
    {
        if (navigationController != null)
        {
            navigationController.CloseCurrentLocation();
            return;
        }

        gameObject.SetActive(false);
    }

    public void FightEnemy(int enemyIndex)
    {
        if (enemies == null || enemyIndex < 0 || enemyIndex >= enemies.Length)
        {
            Debug.LogWarning("ArenaWindowUI: Enemy index is not configured: " + enemyIndex);
            return;
        }

        Debug.Log("ArenaWindowUI: Selected arena enemy: " + enemies[enemyIndex].enemyName);
    }

    private void BuildIfNeeded()
    {
        if (isBuilt)
            return;

        isBuilt = true;

        RectTransform rootRect = GetComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0.5f, 0.5f);
        rootRect.anchorMax = new Vector2(0.5f, 0.5f);
        rootRect.anchoredPosition = new Vector2(225f, 0f);
        rootRect.sizeDelta = new Vector2(1470f, 1080f);
        rootRect.pivot = new Vector2(0.5f, 0.5f);

        Image background = GetComponent<Image>();
        if (background != null)
        {
            background.color = panelColor;
            AddOutline(background.gameObject, borderColor, new Vector2(2f, -2f));
        }

        VerticalLayoutGroup rootLayout = gameObject.AddComponent<VerticalLayoutGroup>();
        rootLayout.padding = new RectOffset(72, 72, 36, 56);
        rootLayout.spacing = 22f;
        rootLayout.childAlignment = TextAnchor.UpperCenter;
        rootLayout.childControlWidth = true;
        rootLayout.childControlHeight = true;
        rootLayout.childForceExpandWidth = true;
        rootLayout.childForceExpandHeight = false;

        BuildHeader();
        BuildSummary();
        BuildEnemies();
    }

    private void BuildHeader()
    {
        GameObject header = CreateLayoutObject("Header", transform);
        HorizontalLayoutGroup headerLayout = header.AddComponent<HorizontalLayoutGroup>();
        headerLayout.childControlWidth = true;
        headerLayout.childControlHeight = true;
        headerLayout.childForceExpandWidth = false;
        headerLayout.childForceExpandHeight = false;
        headerLayout.childAlignment = TextAnchor.MiddleCenter;

        LayoutElement headerElement = header.AddComponent<LayoutElement>();
        headerElement.minHeight = 74f;
        headerElement.preferredHeight = 74f;

        TMP_Text title = CreateText("Title", header.transform, "ARENA", 46, FontStyles.Bold, TextAlignmentOptions.Center);
        title.color = titleColor;
        title.characterSpacing = 8f;
        LayoutElement titleElement = title.gameObject.AddComponent<LayoutElement>();
        titleElement.flexibleWidth = 1f;

        Button closeButton = CreateButton("CloseButton", header.transform, "X", 26, new Vector2(58f, 54f), closeButtonColor);
        closeButton.onClick.AddListener(Close);
    }

    private void BuildSummary()
    {
        GameObject summary = CreateLayoutObject("Summary", transform);
        HorizontalLayoutGroup summaryLayout = summary.AddComponent<HorizontalLayoutGroup>();
        summaryLayout.spacing = 28f;
        summaryLayout.childAlignment = TextAnchor.MiddleCenter;
        summaryLayout.childControlWidth = true;
        summaryLayout.childControlHeight = true;
        summaryLayout.childForceExpandWidth = false;
        summaryLayout.childForceExpandHeight = false;

        LayoutElement summaryElement = summary.AddComponent<LayoutElement>();
        summaryElement.minHeight = 62f;
        summaryElement.preferredHeight = 62f;

        CreateStatusLabel("RankText", summary.transform, "Rank: " + rank);
        CreateStatusLabel("ArenaTokensText", summary.transform, "Arena Tokens: " + arenaTokens);
    }

    private void BuildEnemies()
    {
        GameObject list = CreateLayoutObject("EnemyCards", transform);
        HorizontalLayoutGroup listLayout = list.AddComponent<HorizontalLayoutGroup>();
        listLayout.spacing = 30f;
        listLayout.childAlignment = TextAnchor.UpperCenter;
        listLayout.childControlWidth = true;
        listLayout.childControlHeight = true;
        listLayout.childForceExpandWidth = true;
        listLayout.childForceExpandHeight = true;

        LayoutElement listElement = list.AddComponent<LayoutElement>();
        listElement.minHeight = 690f;
        listElement.preferredHeight = 690f;
        listElement.flexibleHeight = 1f;

        int enemyCount = enemies != null ? enemies.Length : 0;
        for (int i = 0; i < enemyCount; i++)
        {
            BuildEnemyCard(list.transform, enemies[i], i);
        }
    }

    private void BuildEnemyCard(Transform parent, ArenaEnemyPreview enemy, int enemyIndex)
    {
        GameObject card = CreateLayoutObject("EnemyCard_" + enemyIndex, parent);
        Image cardImage = card.AddComponent<Image>();
        cardImage.color = cardColor;
        AddOutline(card, borderColor, new Vector2(2f, -2f));

        VerticalLayoutGroup cardLayout = card.AddComponent<VerticalLayoutGroup>();
        cardLayout.padding = new RectOffset(28, 28, 26, 28);
        cardLayout.spacing = 18f;
        cardLayout.childAlignment = TextAnchor.UpperCenter;
        cardLayout.childControlWidth = true;
        cardLayout.childControlHeight = true;
        cardLayout.childForceExpandWidth = true;
        cardLayout.childForceExpandHeight = false;

        LayoutElement cardElement = card.AddComponent<LayoutElement>();
        cardElement.minWidth = 360f;
        cardElement.preferredWidth = 400f;
        cardElement.flexibleWidth = 1f;
        cardElement.minHeight = 620f;
        cardElement.preferredHeight = 650f;

        CreatePortraitPlaceholder(card.transform, enemyIndex);

        TMP_Text enemyNameText = CreateText("EnemyNameText", card.transform, enemy.enemyName, 34, FontStyles.Bold, TextAlignmentOptions.Center);
        enemyNameText.color = titleColor;

        TMP_Text powerText = CreateText("PowerText", card.transform, "Power: " + enemy.power, 27, FontStyles.Bold, TextAlignmentOptions.Center);
        powerText.color = textColor;

        TMP_Text rewardText = CreateText("RewardText", card.transform, "Reward: " + enemy.reward, 25, FontStyles.Normal, TextAlignmentOptions.Center);
        rewardText.color = mutedTextColor;

        GameObject spacer = CreateLayoutObject("Spacer", card.transform);
        LayoutElement spacerElement = spacer.AddComponent<LayoutElement>();
        spacerElement.flexibleHeight = 1f;

        Button fightButton = CreateButton("FightButton", card.transform, "Fight", 30, new Vector2(0f, 78f), buttonColor);
        LayoutElement fightElement = fightButton.gameObject.GetComponent<LayoutElement>();
        fightElement.flexibleWidth = 1f;

        int cachedEnemyIndex = enemyIndex;
        fightButton.onClick.AddListener(() => FightEnemy(cachedEnemyIndex));
    }

    private void CreatePortraitPlaceholder(Transform parent, int enemyIndex)
    {
        GameObject portrait = CreateLayoutObject("PortraitPlaceholder", parent);
        Image portraitImage = portrait.AddComponent<Image>();
        portraitImage.color = portraitColor;
        AddOutline(portrait, darkBorderColor, new Vector2(3f, -3f));

        LayoutElement portraitElement = portrait.AddComponent<LayoutElement>();
        portraitElement.minHeight = 220f;
        portraitElement.preferredHeight = 240f;
        portraitElement.flexibleWidth = 1f;

        TMP_Text mark = CreateText("Mark", portrait.transform, GetPortraitMark(enemyIndex), 72, FontStyles.Bold, TextAlignmentOptions.Center);
        mark.color = new Color(0.42f, 0.31f, 0.20f, 0.9f);

        RectTransform markRect = mark.GetComponent<RectTransform>();
        markRect.anchorMin = Vector2.zero;
        markRect.anchorMax = Vector2.one;
        markRect.offsetMin = Vector2.zero;
        markRect.offsetMax = Vector2.zero;

        LayoutElement markElement = mark.GetComponent<LayoutElement>();
        if (markElement != null)
            Destroy(markElement);
    }

    private string GetPortraitMark(int enemyIndex)
    {
        switch (enemyIndex)
        {
            case 0:
                return "I";
            case 1:
                return "II";
            case 2:
                return "III";
            default:
                return "?";
        }
    }

    private TMP_Text CreateStatusLabel(string objectName, Transform parent, string textValue)
    {
        GameObject label = CreateLayoutObject(objectName, parent);
        Image labelImage = label.AddComponent<Image>();
        labelImage.color = new Color(0.07f, 0.052f, 0.035f, 0.92f);
        AddOutline(label, darkBorderColor, new Vector2(2f, -2f));

        HorizontalLayoutGroup labelLayout = label.AddComponent<HorizontalLayoutGroup>();
        labelLayout.padding = new RectOffset(26, 26, 8, 8);
        labelLayout.childAlignment = TextAnchor.MiddleCenter;
        labelLayout.childControlWidth = true;
        labelLayout.childControlHeight = true;
        labelLayout.childForceExpandWidth = false;
        labelLayout.childForceExpandHeight = false;

        LayoutElement labelElement = label.AddComponent<LayoutElement>();
        labelElement.minWidth = 330f;
        labelElement.preferredWidth = 360f;
        labelElement.minHeight = 54f;
        labelElement.preferredHeight = 54f;

        TMP_Text text = CreateText("Text", label.transform, textValue, 26, FontStyles.Bold, TextAlignmentOptions.Center);
        text.color = textColor;
        return text;
    }

    private GameObject CreateLayoutObject(string objectName, Transform parent)
    {
        GameObject layoutObject = new GameObject(objectName, typeof(RectTransform));
        layoutObject.transform.SetParent(parent, false);
        return layoutObject;
    }

    private TMP_Text CreateText(
        string objectName,
        Transform parent,
        string textValue,
        int fontSize,
        FontStyles fontStyle,
        TextAlignmentOptions alignment)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        TMP_Text text = textObject.GetComponent<TMP_Text>();
        text.text = textValue;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.color = textColor;
        text.alignment = alignment;
        text.textWrappingMode = TextWrappingModes.Normal;

        LayoutElement textElement = textObject.AddComponent<LayoutElement>();
        textElement.minHeight = fontSize + 20f;
        textElement.preferredHeight = fontSize + 24f;

        return text;
    }

    private Button CreateButton(
        string objectName,
        Transform parent,
        string label,
        int fontSize,
        Vector2 size,
        Color backgroundColor)
    {
        GameObject buttonObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        Image image = buttonObject.GetComponent<Image>();
        image.color = backgroundColor;
        AddOutline(buttonObject, borderColor, new Vector2(2f, -2f));

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        ConfigureButtonColors(button, backgroundColor);

        LayoutElement layoutElement = buttonObject.AddComponent<LayoutElement>();
        layoutElement.minWidth = size.x;
        layoutElement.preferredWidth = size.x;
        layoutElement.minHeight = size.y;
        layoutElement.preferredHeight = size.y;

        TMP_Text text = CreateText("Text", buttonObject.transform, label, fontSize, FontStyles.Bold, TextAlignmentOptions.Center);
        text.color = titleColor;
        RectTransform textRect = text.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        LayoutElement textElement = text.GetComponent<LayoutElement>();
        if (textElement != null)
            Destroy(textElement);

        return button;
    }

    private void ConfigureButtonColors(Button button, Color normalColor)
    {
        ColorBlock colors = button.colors;
        colors.normalColor = normalColor;
        colors.highlightedColor = Color.Lerp(normalColor, titleColor, 0.18f);
        colors.pressedColor = buttonPressedColor;
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(normalColor.r, normalColor.g, normalColor.b, 0.45f);
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.08f;
        button.colors = colors;
    }

    private void AddOutline(GameObject target, Color color, Vector2 distance)
    {
        Outline outline = target.AddComponent<Outline>();
        outline.effectColor = color;
        outline.effectDistance = distance;
        outline.useGraphicAlpha = true;
    }
}
