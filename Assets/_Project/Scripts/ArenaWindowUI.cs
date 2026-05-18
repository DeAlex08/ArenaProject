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

    private readonly Color panelColor = new Color(0.04f, 0.035f, 0.03f, 0.96f);
    private readonly Color cardColor = new Color(0.16f, 0.13f, 0.10f, 0.94f);
    private readonly Color textColor = new Color(0.95f, 0.90f, 0.78f, 1f);
    private readonly Color mutedTextColor = new Color(0.74f, 0.68f, 0.56f, 1f);
    private readonly Color buttonColor = new Color(0.28f, 0.06f, 0.04f, 1f);

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
            background.color = panelColor;

        VerticalLayoutGroup rootLayout = gameObject.AddComponent<VerticalLayoutGroup>();
        rootLayout.padding = new RectOffset(64, 64, 42, 52);
        rootLayout.spacing = 28f;
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
        headerLayout.childForceExpandHeight = true;
        headerLayout.childAlignment = TextAnchor.MiddleCenter;

        LayoutElement headerElement = header.AddComponent<LayoutElement>();
        headerElement.minHeight = 86f;
        headerElement.preferredHeight = 86f;

        TMP_Text title = CreateText("Title", header.transform, "ARENA", 46, FontStyles.Bold, TextAlignmentOptions.Center);
        LayoutElement titleElement = title.gameObject.AddComponent<LayoutElement>();
        titleElement.flexibleWidth = 1f;

        Button closeButton = CreateButton("CloseButton", header.transform, "X", 34, new Vector2(86f, 72f));
        closeButton.onClick.AddListener(Close);
    }

    private void BuildSummary()
    {
        GameObject summary = CreateLayoutObject("Summary", transform);
        HorizontalLayoutGroup summaryLayout = summary.AddComponent<HorizontalLayoutGroup>();
        summaryLayout.spacing = 40f;
        summaryLayout.childAlignment = TextAnchor.MiddleCenter;
        summaryLayout.childControlWidth = true;
        summaryLayout.childControlHeight = true;
        summaryLayout.childForceExpandWidth = true;
        summaryLayout.childForceExpandHeight = true;

        LayoutElement summaryElement = summary.AddComponent<LayoutElement>();
        summaryElement.minHeight = 78f;
        summaryElement.preferredHeight = 78f;

        CreateText("RankText", summary.transform, "Rank: " + rank, 30, FontStyles.Bold, TextAlignmentOptions.Center);
        CreateText("ArenaTokensText", summary.transform, "Arena Tokens: " + arenaTokens, 30, FontStyles.Bold, TextAlignmentOptions.Center);
    }

    private void BuildEnemies()
    {
        GameObject list = CreateLayoutObject("EnemyCards", transform);
        HorizontalLayoutGroup listLayout = list.AddComponent<HorizontalLayoutGroup>();
        listLayout.spacing = 28f;
        listLayout.childAlignment = TextAnchor.UpperCenter;
        listLayout.childControlWidth = true;
        listLayout.childControlHeight = true;
        listLayout.childForceExpandWidth = true;
        listLayout.childForceExpandHeight = true;

        LayoutElement listElement = list.AddComponent<LayoutElement>();
        listElement.minHeight = 620f;
        listElement.preferredHeight = 620f;
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

        VerticalLayoutGroup cardLayout = card.AddComponent<VerticalLayoutGroup>();
        cardLayout.padding = new RectOffset(28, 28, 30, 30);
        cardLayout.spacing = 24f;
        cardLayout.childAlignment = TextAnchor.UpperCenter;
        cardLayout.childControlWidth = true;
        cardLayout.childControlHeight = true;
        cardLayout.childForceExpandWidth = true;
        cardLayout.childForceExpandHeight = false;

        LayoutElement cardElement = card.AddComponent<LayoutElement>();
        cardElement.minWidth = 360f;
        cardElement.preferredWidth = 400f;
        cardElement.flexibleWidth = 1f;
        cardElement.minHeight = 540f;
        cardElement.preferredHeight = 580f;

        CreateText("EnemyNameText", card.transform, enemy.enemyName, 32, FontStyles.Bold, TextAlignmentOptions.Center);
        CreateText("PowerText", card.transform, "Power: " + enemy.power, 28, FontStyles.Bold, TextAlignmentOptions.Center);
        TMP_Text rewardText = CreateText("RewardText", card.transform, "Reward: " + enemy.reward, 26, FontStyles.Normal, TextAlignmentOptions.Center);
        rewardText.color = mutedTextColor;

        GameObject spacer = CreateLayoutObject("Spacer", card.transform);
        LayoutElement spacerElement = spacer.AddComponent<LayoutElement>();
        spacerElement.flexibleHeight = 1f;

        Button fightButton = CreateButton("FightButton", card.transform, "Fight", 30, new Vector2(0f, 74f));
        LayoutElement fightElement = fightButton.gameObject.GetComponent<LayoutElement>();
        fightElement.flexibleWidth = 1f;

        int cachedEnemyIndex = enemyIndex;
        fightButton.onClick.AddListener(() => FightEnemy(cachedEnemyIndex));
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
        Vector2 size)
    {
        GameObject buttonObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        Image image = buttonObject.GetComponent<Image>();
        image.color = buttonColor;

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;

        LayoutElement layoutElement = buttonObject.AddComponent<LayoutElement>();
        layoutElement.minWidth = size.x;
        layoutElement.preferredWidth = size.x;
        layoutElement.minHeight = size.y;
        layoutElement.preferredHeight = size.y;

        TMP_Text text = CreateText("Text", buttonObject.transform, label, fontSize, FontStyles.Bold, TextAlignmentOptions.Center);
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
}
