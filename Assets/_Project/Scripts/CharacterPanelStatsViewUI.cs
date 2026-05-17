using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterPanelStatsViewUI : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private EquipmentManager equipmentManager;

    [Header("Views")]
    [SerializeField] private GameObject portraitView;
    [SerializeField] private GameObject statsView;

    [Header("Texts")]
    [SerializeField] private TMP_Text combatPowerText;
    [SerializeField] private TMP_Text availablePointsText;

    private RectTransform statsContent;
    private readonly List<StatRow> statRows = new List<StatRow>();
    private bool showingStats;
    private readonly Color primaryTextColor = new Color(0.86f, 0.79f, 0.64f, 1f);
    private readonly Color secondaryTextColor = new Color(0.95f, 0.90f, 0.78f, 1f);
    private readonly Color addButtonActiveColor = new Color(0.86f, 0.12f, 0.08f, 1f);
    private readonly Color addButtonDisabledColor = new Color(0.35f, 0.25f, 0.22f, 1f);

    private class StatRow
    {
        public PlayerStatType statType;
        public TMP_Text valueText;
        public Button addButton;
        public TMP_Text addButtonText;
    }

    private void Awake()
    {
        CachePortraitView();
    }

    private void Start()
    {
        EnsureBuilt();
        Refresh();
        ShowPortrait();
    }

    public void Initialize(PlayerStats stats, EquipmentManager equipment)
    {
        playerStats = stats;
        equipmentManager = equipment;

        EnsureBuilt();
        Refresh();
        ShowPortrait();
    }

    public void ToggleView()
    {
        if (showingStats)
            ShowPortrait();
        else
            ShowStats();
    }

    public void ShowStats()
    {
        EnsureBuilt();
        showingStats = true;

        SetPortraitVisible(false);

        if (statsView != null)
            statsView.SetActive(true);

        Refresh();
    }

    public void ShowPortrait()
    {
        showingStats = false;

        SetPortraitVisible(true);

        if (statsView != null)
            statsView.SetActive(false);
    }

    public bool IsShowingStats()
    {
        return showingStats;
    }

    public void Refresh()
    {
        if (playerStats == null)
            return;

        if (combatPowerText != null)
            combatPowerText.text = "Боевая мощь: " + playerStats.combatPower;

        if (availablePointsText != null)
            availablePointsText.text = "Свободные очки: " + playerStats.availableStatPoints;

        bool canAllocate = playerStats.availableStatPoints > 0;

        foreach (StatRow row in statRows)
        {
            if (row.valueText != null)
                row.valueText.text = BuildStatValue(row.statType);

            if (row.addButton != null)
                row.addButton.interactable = canAllocate;

            if (row.addButtonText != null)
                row.addButtonText.color = canAllocate
                    ? addButtonActiveColor
                    : addButtonDisabledColor;
        }
    }

    private void AllocateStat(PlayerStatType statType)
    {
        if (playerStats == null || !playerStats.TryAllocateStat(statType))
            return;

        if (equipmentManager != null)
            equipmentManager.RefreshPlayerStats();

        Refresh();
    }

    private void EnsureBuilt()
    {
        if (statsView != null)
            return;

        CachePortraitView();
        BuildStatsView();
    }

    private void CachePortraitView()
    {
        if (portraitView != null)
            return;

        Transform portrait = transform.Find("CharacterPortrait");
        if (portrait != null)
            portraitView = portrait.gameObject;
    }

    private void BuildStatsView()
    {
        statsView = new GameObject("CharacterStatsView", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        statsView.transform.SetParent(transform, false);
        statsView.SetActive(false);

        RectTransform statsRect = statsView.GetComponent<RectTransform>();
        statsRect.anchorMin = new Vector2(0f, 0f);
        statsRect.anchorMax = new Vector2(1f, 1f);
        statsRect.offsetMin = new Vector2(22f, 605f);
        statsRect.offsetMax = new Vector2(-22f, -84f);

        Image background = statsView.GetComponent<Image>();
        Image panelImage = GetComponent<Image>();
        if (panelImage != null)
        {
            background.sprite = panelImage.sprite;
            background.type = panelImage.type;
            background.preserveAspect = panelImage.preserveAspect;
            background.pixelsPerUnitMultiplier = panelImage.pixelsPerUnitMultiplier;
        }

        background.color = new Color(0.18f, 0.14f, 0.10f, 0.94f);
        background.raycastTarget = true;

        GameObject viewportObject = new GameObject("Viewport", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(RectMask2D));
        viewportObject.transform.SetParent(statsView.transform, false);

        RectTransform viewportRect = viewportObject.GetComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = new Vector2(12f, 10f);
        viewportRect.offsetMax = new Vector2(-12f, -10f);

        Image viewportImage = viewportObject.GetComponent<Image>();
        viewportImage.color = new Color(1f, 1f, 1f, 0f);
        viewportImage.raycastTarget = true;

        GameObject contentObject = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        contentObject.transform.SetParent(viewportObject.transform, false);
        statsContent = contentObject.GetComponent<RectTransform>();
        statsContent.anchorMin = new Vector2(0f, 1f);
        statsContent.anchorMax = new Vector2(1f, 1f);
        statsContent.pivot = new Vector2(0.5f, 1f);
        statsContent.anchoredPosition = Vector2.zero;
        statsContent.sizeDelta = new Vector2(0f, 0f);

        VerticalLayoutGroup layout = contentObject.GetComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(14, 14, 12, 12);
        layout.spacing = 4f;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = contentObject.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        ScrollRect scrollRect = statsView.AddComponent<ScrollRect>();
        scrollRect.viewport = viewportRect;
        scrollRect.content = statsContent;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 34f;

        combatPowerText = CreateText("CombatPowerText", statsContent, 24, FontStyles.Bold);
        availablePointsText = CreateText("AvailablePointsText", statsContent, 22, FontStyles.Bold);
        combatPowerText.color = secondaryTextColor;
        availablePointsText.color = secondaryTextColor;

        AddStatRow("Сила", PlayerStatType.Strength);
        AddStatRow("Ярость", PlayerStatType.Rage);
        AddStatRow("Реакция", PlayerStatType.Reaction);
        AddStatRow("Ловкость", PlayerStatType.Agility);
        AddStatRow("Выносливость", PlayerStatType.Endurance);
        AddStatRow("Защита", PlayerStatType.Armor);
        AddStatRow("Удача", PlayerStatType.Luck);
        AddStatRow("Интеллект", PlayerStatType.Intelligence);
    }

    private void AddStatRow(string label, PlayerStatType statType)
    {
        GameObject rowObject = new GameObject(label + "Row", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        rowObject.transform.SetParent(statsContent, false);

        RectTransform rowRect = rowObject.GetComponent<RectTransform>();
        rowRect.sizeDelta = new Vector2(0f, 58f);

        LayoutElement rowElement = rowObject.AddComponent<LayoutElement>();
        rowElement.minHeight = 58f;
        rowElement.preferredHeight = 58f;

        HorizontalLayoutGroup rowLayout = rowObject.GetComponent<HorizontalLayoutGroup>();
        rowLayout.spacing = 10f;
        rowLayout.childControlWidth = true;
        rowLayout.childControlHeight = true;
        rowLayout.childForceExpandWidth = false;
        rowLayout.childForceExpandHeight = true;

        TMP_Text labelText = CreateText(label + "Label", rowObject.transform, 22, FontStyles.Normal);
        labelText.text = label;

        LayoutElement labelElement = labelText.gameObject.AddComponent<LayoutElement>();
        labelElement.minWidth = 155f;
        labelElement.preferredWidth = 155f;

        TMP_Text valueText = CreateText(label + "Value", rowObject.transform, 22, FontStyles.Bold);
        valueText.color = secondaryTextColor;
        LayoutElement valueElement = valueText.gameObject.AddComponent<LayoutElement>();
        valueElement.minWidth = 95f;
        valueElement.flexibleWidth = 1f;

        Button addButton = CreateAddButton(rowObject.transform);
        TMP_Text addButtonText = addButton.GetComponentInChildren<TMP_Text>();
        PlayerStatType cachedType = statType;
        addButton.onClick.AddListener(() => AllocateStat(cachedType));

        statRows.Add(new StatRow
        {
            statType = statType,
            valueText = valueText,
            addButton = addButton,
            addButtonText = addButtonText
        });
    }

    private TMP_Text CreateText(string objectName, Transform parent, int fontSize, FontStyles fontStyle)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        TMP_Text text = textObject.GetComponent<TMP_Text>();
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.color = primaryTextColor;
        text.alignment = TextAlignmentOptions.MidlineLeft;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.richText = true;

        return text;
    }

    private Button CreateAddButton(Transform parent)
    {
        GameObject buttonObject = new GameObject("AddButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(72f, 56f);

        LayoutElement buttonElement = buttonObject.AddComponent<LayoutElement>();
        buttonElement.minWidth = 72f;
        buttonElement.preferredWidth = 72f;
        buttonElement.minHeight = 56f;
        buttonElement.preferredHeight = 56f;

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0f);
        image.raycastTarget = true;

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;

        TMP_Text text = CreateText("Text", buttonObject.transform, 38, FontStyles.Bold);
        text.text = "+";
        text.alignment = TextAlignmentOptions.Center;
        text.color = addButtonActiveColor;
        text.raycastTarget = false;

        RectTransform textRect = text.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        return button;
    }

    private void SetPortraitVisible(bool visible)
    {
        if (portraitView != null)
            portraitView.SetActive(visible);

        SetChildVisible("PowerText", visible);
    }

    private void SetChildVisible(string childName, bool visible)
    {
        Transform child = transform.Find(childName);
        if (child != null)
            child.gameObject.SetActive(visible);
    }

    private string BuildStatValue(PlayerStatType statType)
    {
        int nativeValue = GetNativeValue(statType);
        int bonusValue = GetBonusValue(statType);

        if (bonusValue > 0)
            return nativeValue + " <color=#55FF55>+" + bonusValue + "</color>";

        return nativeValue.ToString();
    }

    private int GetNativeValue(PlayerStatType statType)
    {
        switch (statType)
        {
            case PlayerStatType.Strength:
                return playerStats.nativeStrength;
            case PlayerStatType.Rage:
                return playerStats.nativeRage;
            case PlayerStatType.Reaction:
                return playerStats.nativeReaction;
            case PlayerStatType.Agility:
                return playerStats.nativeAgility;
            case PlayerStatType.Endurance:
                return playerStats.nativeEndurance;
            case PlayerStatType.Armor:
                return playerStats.nativeArmor;
            case PlayerStatType.Luck:
                return playerStats.nativeLuck;
            case PlayerStatType.Intelligence:
                return playerStats.nativeIntelligence;
        }

        return 0;
    }

    private int GetBonusValue(PlayerStatType statType)
    {
        switch (statType)
        {
            case PlayerStatType.Strength:
                return playerStats.bonusStrength;
            case PlayerStatType.Rage:
                return playerStats.bonusRage;
            case PlayerStatType.Reaction:
                return playerStats.bonusReaction;
            case PlayerStatType.Agility:
                return playerStats.bonusAgility;
            case PlayerStatType.Endurance:
                return playerStats.bonusEndurance;
            case PlayerStatType.Armor:
                return playerStats.bonusArmor;
            case PlayerStatType.Luck:
                return playerStats.bonusLuck;
            case PlayerStatType.Intelligence:
                return playerStats.bonusIntelligence;
        }

        return 0;
    }
}
