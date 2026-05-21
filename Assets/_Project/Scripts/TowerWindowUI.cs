using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TowerWindowUI : MonoBehaviour
{
    private const int TotalFloors = 10;

    [Header("Systems")]
    [SerializeField] private LocationNavigationController navigationController;
    [SerializeField] private PlayerStats playerStats;

    [Header("Fallbacks")]
    [SerializeField] private int fallbackPlayerPower = 1200;

    private class TowerFloorData
    {
        public int floorNumber;
        public string enemyName;
        public int recommendedLevel;
        public int combatPower;
        public int hp;
        public int attack;
        public int defense;
        public int armor;
        public int agility;
        public int reaction;
        public float critChance;
        public int expReward;
        public int tokenReward;
        public string description;
    }

    private class TowerFightResult
    {
        public CombatOutcome outcome;
        public int expGained;
        public int tokensGained;
        public int rounds;
        public int playerRemainingHp;
        public int playerStartHp;
        public int enemyRemainingHp;
        public int enemyStartHp;
        public int playerDamageDealt;
        public int enemyDamageDealt;
        public int playerCrits;
        public int enemyCrits;
        public int playerDodges;
        public int enemyDodges;
        public int playerBlocks;
        public int enemyBlocks;
        public CombatStance playerStance;
        public CombatStance enemyStance;
        public string combatLog;
        public List<CombatSimulator.CombatPlaybackEvent> playbackEvents;
        public bool firstClear;
    }

    private readonly Color panelColor = new Color(0.018f, 0.014f, 0.011f, 0.94f);
    private readonly Color cardColor = new Color(0.075f, 0.055f, 0.038f, 0.96f);
    private readonly Color cardLockedColor = new Color(0.04f, 0.04f, 0.04f, 0.78f);
    private readonly Color cardClearedColor = new Color(0.035f, 0.075f, 0.048f, 0.92f);
    private readonly Color currentFloorColor = new Color(0.12f, 0.075f, 0.035f, 0.98f);
    private readonly Color titleColor = new Color(0.95f, 0.73f, 0.36f, 1f);
    private readonly Color textColor = new Color(0.94f, 0.88f, 0.72f, 1f);
    private readonly Color mutedTextColor = new Color(0.62f, 0.57f, 0.48f, 1f);
    private readonly Color buttonColor = new Color(0.26f, 0.15f, 0.065f, 1f);
    private readonly Color closeButtonColor = new Color(0.12f, 0.07f, 0.035f, 1f);
    private readonly Color borderColor = new Color(0.55f, 0.36f, 0.17f, 1f);
    private readonly Color darkBorderColor = new Color(0.13f, 0.075f, 0.035f, 1f);
    private readonly Color victoryColor = new Color(0.95f, 0.73f, 0.36f, 1f);
    private readonly Color defeatColor = new Color(0.74f, 0.18f, 0.13f, 1f);
    private readonly Color drawColor = new Color(0.72f, 0.68f, 0.52f, 1f);

    private bool isBuilt;
    private int unlockedFloor = 1;
    private readonly HashSet<int> clearedFloors = new HashSet<int>();
    private readonly List<TowerFloorData> floors = new List<TowerFloorData>();
    private string lastBattleLog = "";

    private TMP_Text tokensText;
    private TMP_Text messageText;
    private Transform floorContent;
    private GameObject resultPanel;
    private GameObject battleLogPanel;
    private TMP_Text resultTitleText;
    private TMP_Text resultEnemyText;
    private TMP_Text resultHpText;
    private TMP_Text resultDamageText;
    private TMP_Text resultCombatStatsText;
    private TMP_Text resultExpText;
    private TMP_Text resultTokensText;
    private TMP_Text resultMessageText;
    private TMP_Text battleLogText;
    private CombatPlaybackUI combatPlaybackUI;

    private void Awake()
    {
        ResolveReferences();
        LoadProgression();
        BuildIfNeeded();
    }

    private void OnEnable()
    {
        ResolveReferences();
        LoadProgression();
        BuildIfNeeded();
        GenerateFloors();
        RefreshFloorList();
        RefreshTokens();
        HideResult();
        HideBattleLog();
        HideCombatPlayback();
    }

    public void Close()
    {
        if (navigationController != null)
            navigationController.CloseCurrentLocation();
        else
            gameObject.SetActive(false);
    }

    private void ResolveReferences()
    {
        if (navigationController == null)
            navigationController = FindFirstObjectByType<LocationNavigationController>();

        if (playerStats == null)
            playerStats = FindFirstObjectByType<PlayerStats>();
    }

    private void LoadProgression()
    {
        unlockedFloor = Mathf.Clamp(PlayerSaveManager.LoadTowerUnlockedFloor(1), 1, TotalFloors);
        clearedFloors.Clear();

        List<int> savedClearedFloors = PlayerSaveManager.LoadTowerClearedFloors();
        foreach (int floor in savedClearedFloors)
        {
            if (floor >= 1 && floor <= TotalFloors)
                clearedFloors.Add(floor);
        }
    }

    private void SaveProgression()
    {
        PlayerSaveManager.SaveTowerProgress(unlockedFloor, new List<int>(clearedFloors));
    }

    private void BuildIfNeeded()
    {
        if (isBuilt)
            return;

        isBuilt = true;

        RectTransform rootRect = GetComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0.235f, 0f);
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;
        rootRect.pivot = new Vector2(0.5f, 0.5f);

        Image background = GetComponent<Image>();
        if (background == null)
            background = gameObject.AddComponent<Image>();
        background.color = panelColor;

        VerticalLayoutGroup rootLayout = gameObject.AddComponent<VerticalLayoutGroup>();
        rootLayout.padding = new RectOffset(34, 34, 22, 30);
        rootLayout.spacing = 16f;
        rootLayout.childAlignment = TextAnchor.UpperCenter;
        rootLayout.childControlWidth = true;
        rootLayout.childControlHeight = true;
        rootLayout.childForceExpandWidth = true;
        rootLayout.childForceExpandHeight = false;

        BuildHeader();
        BuildSummary();
        BuildFloorScroll();
        BuildCombatPlaybackPanel();
        BuildResultPanel();
        BuildBattleLogPanel();
    }

    private void BuildHeader()
    {
        GameObject header = CreateLayoutObject("Header", transform);
        LayoutElement headerElement = header.AddComponent<LayoutElement>();
        headerElement.minHeight = 76f;
        headerElement.preferredHeight = 76f;

        TMP_Text title = CreateText("Title", header.transform, "TOWER OF TRIALS", 42, FontStyles.Bold, TextAlignmentOptions.Center);
        title.color = titleColor;
        Stretch(title.GetComponent<RectTransform>());

        Button closeButton = CreateButton("CloseButton", header.transform, "X", 26, new Vector2(58f, 54f), closeButtonColor);
        RectTransform closeRect = closeButton.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(1f, 0.5f);
        closeRect.anchorMax = new Vector2(1f, 0.5f);
        closeRect.pivot = new Vector2(1f, 0.5f);
        closeRect.anchoredPosition = new Vector2(-8f, 0f);
        closeButton.onClick.AddListener(Close);
    }

    private void BuildSummary()
    {
        GameObject summary = CreateLayoutObject("Summary", transform);
        HorizontalLayoutGroup layout = summary.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(12, 12, 0, 0);
        layout.spacing = 16f;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        LayoutElement summaryElement = summary.AddComponent<LayoutElement>();
        summaryElement.minHeight = 56f;
        summaryElement.preferredHeight = 56f;

        tokensText = CreateText("ArenaTokensText", summary.transform, "", 25, FontStyles.Bold, TextAlignmentOptions.Left);
        LayoutElement tokenElement = tokensText.GetComponent<LayoutElement>();
        tokenElement.minWidth = 330f;
        tokenElement.preferredWidth = 380f;

        messageText = CreateText("MessageText", summary.transform, "", 23, FontStyles.Bold, TextAlignmentOptions.Left);
        messageText.color = mutedTextColor;
        LayoutElement messageElement = messageText.GetComponent<LayoutElement>();
        messageElement.flexibleWidth = 1f;
    }

    private void BuildFloorScroll()
    {
        GameObject scrollObject = CreateLayoutObject("FloorsScrollView", transform);
        Image scrollImage = scrollObject.AddComponent<Image>();
        scrollImage.color = new Color(0.025f, 0.021f, 0.016f, 0.88f);
        AddOutline(scrollObject, darkBorderColor, new Vector2(2f, -2f));

        ScrollRect scrollRect = scrollObject.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.scrollSensitivity = 35f;
        LayoutElement scrollElement = scrollObject.AddComponent<LayoutElement>();
        scrollElement.flexibleHeight = 1f;
        scrollElement.minHeight = 520f;

        GameObject viewport = CreateLayoutObject("Viewport", scrollObject.transform);
        RectTransform viewportRect = viewport.GetComponent<RectTransform>();
        Stretch(viewportRect);
        viewport.AddComponent<RectMask2D>();

        GameObject content = CreateLayoutObject("Content", viewport.transform);
        RectTransform contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.offsetMin = new Vector2(18f, 0f);
        contentRect.offsetMax = new Vector2(-18f, 0f);

        VerticalLayoutGroup contentLayout = content.AddComponent<VerticalLayoutGroup>();
        contentLayout.padding = new RectOffset(12, 12, 18, 18);
        contentLayout.spacing = 14f;
        contentLayout.childAlignment = TextAnchor.UpperCenter;
        contentLayout.childControlWidth = true;
        contentLayout.childControlHeight = true;
        contentLayout.childForceExpandWidth = true;
        contentLayout.childForceExpandHeight = false;

        ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.viewport = viewportRect;
        scrollRect.content = contentRect;
        floorContent = content.transform;
    }

    private void BuildCombatPlaybackPanel()
    {
        GameObject playbackPanel = CreateLayoutObject("CombatPlaybackPanel", transform);
        playbackPanel.transform.SetAsLastSibling();
        LayoutElement playbackLayoutElement = playbackPanel.AddComponent<LayoutElement>();
        playbackLayoutElement.ignoreLayout = true;
        RectTransform playbackRect = playbackPanel.GetComponent<RectTransform>();
        Stretch(playbackRect);
        combatPlaybackUI = playbackPanel.AddComponent<CombatPlaybackUI>();
        playbackPanel.SetActive(false);
    }

    private void BuildResultPanel()
    {
        resultPanel = CreateLayoutObject("ResultPanel", transform);
        resultPanel.transform.SetAsLastSibling();
        LayoutElement resultLayoutElement = resultPanel.AddComponent<LayoutElement>();
        resultLayoutElement.ignoreLayout = true;
        Image image = resultPanel.AddComponent<Image>();
        image.color = new Color(0.018f, 0.014f, 0.011f, 0.97f);
        AddOutline(resultPanel, borderColor, new Vector2(3f, -3f));

        RectTransform rect = resultPanel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.18f, 0.12f);
        rect.anchorMax = new Vector2(0.82f, 0.88f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        VerticalLayoutGroup layout = resultPanel.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(34, 34, 30, 30);
        layout.spacing = 10f;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        resultTitleText = CreateText("ResultTitleText", resultPanel.transform, "", 42, FontStyles.Bold, TextAlignmentOptions.Center);
        resultEnemyText = CreateResultLine("EnemyText");
        resultHpText = CreateResultLine("HpText");
        resultDamageText = CreateResultLine("DamageText");
        resultCombatStatsText = CreateResultLine("CombatStatsText");
        resultExpText = CreateResultLine("ExpText");
        resultTokensText = CreateResultLine("TokensText");
        resultMessageText = CreateText("MessageText", resultPanel.transform, "", 24, FontStyles.Normal, TextAlignmentOptions.Center);
        resultMessageText.color = mutedTextColor;

        GameObject actions = CreateLayoutObject("Actions", resultPanel.transform);
        HorizontalLayoutGroup actionsLayout = actions.AddComponent<HorizontalLayoutGroup>();
        actionsLayout.spacing = 18f;
        actionsLayout.childAlignment = TextAnchor.MiddleCenter;
        actionsLayout.childControlWidth = true;
        actionsLayout.childControlHeight = true;
        actionsLayout.childForceExpandWidth = false;
        actionsLayout.childForceExpandHeight = false;

        LayoutElement actionsElement = actions.AddComponent<LayoutElement>();
        actionsElement.minHeight = 82f;
        actionsElement.preferredHeight = 82f;

        Button battleLogButton = CreateButton("BattleLogButton", actions.transform, "Battle Log", 25, new Vector2(220f, 70f), closeButtonColor);
        battleLogButton.onClick.AddListener(ShowBattleLog);

        Button continueButton = CreateButton("ContinueButton", actions.transform, "Continue", 28, new Vector2(220f, 70f), buttonColor);
        continueButton.onClick.AddListener(() =>
        {
            HideResult();
            HideBattleLog();
            RefreshFloorList();
        });

        HideResult();
    }

    private TMP_Text CreateResultLine(string objectName)
    {
        TMP_Text text = CreateText(objectName, resultPanel.transform, "", 24, FontStyles.Bold, TextAlignmentOptions.Center);
        text.color = textColor;
        return text;
    }

    private void BuildBattleLogPanel()
    {
        battleLogPanel = CreateLayoutObject("BattleLogPanel", transform);
        battleLogPanel.transform.SetAsLastSibling();
        LayoutElement battleLogLayoutElement = battleLogPanel.AddComponent<LayoutElement>();
        battleLogLayoutElement.ignoreLayout = true;
        Image image = battleLogPanel.AddComponent<Image>();
        image.color = new Color(0.014f, 0.011f, 0.009f, 0.98f);
        AddOutline(battleLogPanel, borderColor, new Vector2(3f, -3f));

        RectTransform rect = battleLogPanel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.08f, 0.08f);
        rect.anchorMax = new Vector2(0.92f, 0.92f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        VerticalLayoutGroup layout = battleLogPanel.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(28, 28, 24, 24);
        layout.spacing = 14f;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        GameObject header = CreateLayoutObject("Header", battleLogPanel.transform);
        LayoutElement headerElement = header.AddComponent<LayoutElement>();
        headerElement.minHeight = 58f;
        headerElement.preferredHeight = 58f;

        TMP_Text title = CreateText("Title", header.transform, "TOWER BATTLE LOG", 34, FontStyles.Bold, TextAlignmentOptions.Center);
        title.color = titleColor;
        Stretch(title.GetComponent<RectTransform>());

        Button closeButton = CreateButton("CloseButton", header.transform, "X", 24, new Vector2(54f, 50f), closeButtonColor);
        RectTransform closeRect = closeButton.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(1f, 0.5f);
        closeRect.anchorMax = new Vector2(1f, 0.5f);
        closeRect.pivot = new Vector2(1f, 0.5f);
        closeRect.anchoredPosition = new Vector2(-6f, 0f);
        closeButton.onClick.AddListener(HideBattleLog);

        GameObject scrollObject = CreateLayoutObject("LogScrollView", battleLogPanel.transform);
        Image scrollImage = scrollObject.AddComponent<Image>();
        scrollImage.color = new Color(0.03f, 0.025f, 0.02f, 0.9f);
        AddOutline(scrollObject, darkBorderColor, new Vector2(2f, -2f));

        ScrollRect scrollRect = scrollObject.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.scrollSensitivity = 35f;
        LayoutElement scrollElement = scrollObject.AddComponent<LayoutElement>();
        scrollElement.flexibleHeight = 1f;
        scrollElement.minHeight = 360f;

        GameObject viewport = CreateLayoutObject("Viewport", scrollObject.transform);
        RectTransform viewportRect = viewport.GetComponent<RectTransform>();
        Stretch(viewportRect);
        viewport.AddComponent<RectMask2D>();

        GameObject content = CreateLayoutObject("Content", viewport.transform);
        RectTransform contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.offsetMin = new Vector2(18f, 0f);
        contentRect.offsetMax = new Vector2(-18f, 0f);

        battleLogText = CreateText("LogText", content.transform, "", 23, FontStyles.Normal, TextAlignmentOptions.TopLeft);
        battleLogText.color = textColor;
        RectTransform logRect = battleLogText.GetComponent<RectTransform>();
        logRect.anchorMin = new Vector2(0f, 1f);
        logRect.anchorMax = new Vector2(1f, 1f);
        logRect.pivot = new Vector2(0.5f, 1f);
        logRect.offsetMin = Vector2.zero;
        logRect.offsetMax = Vector2.zero;

        ContentSizeFitter fitter = battleLogText.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.viewport = viewportRect;
        scrollRect.content = logRect;

        Button closeLogButton = CreateButton("CloseLogButton", battleLogPanel.transform, "Close", 28, new Vector2(220f, 70f), buttonColor);
        closeLogButton.onClick.AddListener(HideBattleLog);

        HideBattleLog();
    }

    private void GenerateFloors()
    {
        floors.Clear();

        string[] names =
        {
            "Crypt Initiate",
            "Bone Watcher",
            "Ashen Guard",
            "Grave Sentinel",
            "Cursed Jailor",
            "Oathless Knight",
            "Tower Reaver",
            "Warden of Chains",
            "Dread Prior",
            "Trial Sovereign"
        };

        for (int i = 1; i <= TotalFloors; i++)
        {
            int recommendedLevel = Mathf.Max(i, 1);
            int combatPower = Mathf.RoundToInt(420f + i * 260f + i * i * 55f);
            int hp = Mathf.RoundToInt(260f + i * 105f + i * i * 22f);
            int attack = Mathf.RoundToInt(24f + i * 12f + i * i * 1.8f);
            int defense = Mathf.RoundToInt(8f + i * 5f + i * i * 0.8f);
            int armor = Mathf.RoundToInt(defense * 1.1f + i * 2f);
            int agility = Mathf.RoundToInt(8f + i * 2.2f);
            int reaction = Mathf.RoundToInt(7f + i * 2.4f);
            float critChance = Mathf.Clamp(4f + i * 0.85f, 4f, 18f);
            int expReward = Mathf.RoundToInt(22f + i * 18f + i * i * 2.4f);
            int tokenReward = Mathf.RoundToInt(6f + i * 3.2f);

            floors.Add(new TowerFloorData
            {
                floorNumber = i,
                enemyName = names[i - 1],
                recommendedLevel = recommendedLevel,
                combatPower = combatPower,
                hp = hp,
                attack = attack,
                defense = defense,
                armor = armor,
                agility = agility,
                reaction = reaction,
                critChance = critChance,
                expReward = expReward,
                tokenReward = tokenReward,
                description = "A PvE trial guardian bound to floor " + i + ". Its strength rises with the tower."
            });
        }
    }

    private void RefreshFloorList()
    {
        if (floorContent == null)
            return;

        for (int i = floorContent.childCount - 1; i >= 0; i--)
            Destroy(floorContent.GetChild(i).gameObject);

        int currentFloor = GetCurrentFloor();

        foreach (TowerFloorData floor in floors)
            CreateFloorCard(floor, currentFloor);
    }

    private int GetCurrentFloor()
    {
        for (int floor = 1; floor <= unlockedFloor; floor++)
        {
            if (!clearedFloors.Contains(floor))
                return floor;
        }

        return unlockedFloor;
    }

    private void CreateFloorCard(TowerFloorData floor, int currentFloor)
    {
        bool isLocked = floor.floorNumber > unlockedFloor;
        bool isCleared = clearedFloors.Contains(floor.floorNumber);
        bool isCurrent = floor.floorNumber == currentFloor && !isLocked && !isCleared;

        GameObject card = CreateLayoutObject("Floor" + floor.floorNumber + "Card", floorContent);
        Image image = card.AddComponent<Image>();
        image.color = isLocked ? cardLockedColor : isCleared ? cardClearedColor : isCurrent ? currentFloorColor : cardColor;
        AddOutline(card, isCurrent ? titleColor : darkBorderColor, new Vector2(2f, -2f));

        HorizontalLayoutGroup layout = card.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(18, 18, 14, 14);
        layout.spacing = 18f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        LayoutElement cardElement = card.AddComponent<LayoutElement>();
        cardElement.minHeight = 150f;
        cardElement.preferredHeight = 160f;

        CreateFloorPortrait(card.transform, floor, isLocked);

        GameObject details = CreateLayoutObject("Details", card.transform);
        VerticalLayoutGroup detailsLayout = details.AddComponent<VerticalLayoutGroup>();
        detailsLayout.spacing = 2f;
        detailsLayout.childAlignment = TextAnchor.MiddleLeft;
        detailsLayout.childControlWidth = true;
        detailsLayout.childControlHeight = true;
        detailsLayout.childForceExpandWidth = true;
        detailsLayout.childForceExpandHeight = false;
        LayoutElement detailsElement = details.AddComponent<LayoutElement>();
        detailsElement.flexibleWidth = 1f;

        TMP_Text title = CreateText("FloorTitle", details.transform, "Floor " + floor.floorNumber + " - " + floor.enemyName, 27, FontStyles.Bold, TextAlignmentOptions.Left);
        title.color = isLocked ? mutedTextColor : titleColor;
        CreateCardLine(details.transform, "Recommended Level: " + floor.recommendedLevel + "    Power: " + floor.combatPower, isLocked);
        CreateCardLine(details.transform, "Rewards: " + floor.expReward + " EXP / " + floor.tokenReward + " Tokens", isLocked);
        CreateCardLine(details.transform, "Status: " + GetStatusText(floor.floorNumber), isLocked);

        Button enterButton = CreateButton("EnterButton", card.transform, isLocked ? "Locked" : "Enter", 24, new Vector2(155f, 82f), isLocked ? closeButtonColor : buttonColor);
        enterButton.interactable = !isLocked;
        enterButton.onClick.AddListener(() => EnterFloor(floor.floorNumber));
    }

    private void CreateFloorPortrait(Transform parent, TowerFloorData floor, bool isLocked)
    {
        GameObject portrait = CreateLayoutObject("EnemyPortrait", parent);
        Image portraitImage = portrait.AddComponent<Image>();
        portraitImage.color = isLocked ? new Color(0.035f, 0.035f, 0.035f, 0.85f) : new Color(0.055f, 0.045f, 0.035f, 0.95f);
        AddOutline(portrait, darkBorderColor, new Vector2(2f, -2f));

        LayoutElement portraitElement = portrait.AddComponent<LayoutElement>();
        portraitElement.minWidth = 130f;
        portraitElement.preferredWidth = 130f;
        portraitElement.minHeight = 120f;
        portraitElement.preferredHeight = 120f;

        TMP_Text mark = CreateText("Mark", portrait.transform, floor.floorNumber.ToString(), 42, FontStyles.Bold, TextAlignmentOptions.Center);
        mark.color = isLocked ? mutedTextColor : titleColor;
        Stretch(mark.GetComponent<RectTransform>());
        Destroy(mark.GetComponent<LayoutElement>());
    }

    private TMP_Text CreateCardLine(Transform parent, string textValue, bool muted)
    {
        TMP_Text line = CreateText("Line", parent, textValue, 22, FontStyles.Normal, TextAlignmentOptions.Left);
        line.color = muted ? mutedTextColor : textColor;
        return line;
    }

    private string GetStatusText(int floorNumber)
    {
        if (floorNumber > unlockedFloor)
            return "Locked";

        if (clearedFloors.Contains(floorNumber))
            return "Cleared";

        return "Available";
    }

    private void EnterFloor(int floorNumber)
    {
        TowerFloorData floor = GetFloor(floorNumber);
        if (floor == null)
            return;

        if (floor.floorNumber > unlockedFloor)
        {
            SetMessage("Floor " + floor.floorNumber + " is locked.");
            return;
        }

        TowerFightResult result = RunTowerBattle(floor);
        HideResult();
        HideBattleLog();
        PlayCombatPlayback(floor, result);
    }

    private TowerFloorData GetFloor(int floorNumber)
    {
        foreach (TowerFloorData floor in floors)
        {
            if (floor.floorNumber == floorNumber)
                return floor;
        }

        Debug.LogWarning("TowerWindowUI: Unknown floor " + floorNumber);
        return null;
    }

    private TowerFightResult RunTowerBattle(TowerFloorData floor)
    {
        CombatSimulator.FighterData playerCombatant = BuildPlayerCombatant();
        CombatSimulator.FighterData enemyCombatant = BuildEnemyCombatant(floor);
        CombatSimulator.CombatResult combatResult = CombatSimulator.Simulate(playerCombatant, enemyCombatant);

        bool firstClear = !clearedFloors.Contains(floor.floorNumber);
        int expGained = 0;
        int tokensGained = 0;

        if (combatResult.outcome == CombatOutcome.Victory)
        {
            if (firstClear)
            {
                expGained = floor.expReward;
                tokensGained = floor.tokenReward;
            }
            else
            {
                expGained = Mathf.FloorToInt(floor.expReward * 0.35f);
                tokensGained = Mathf.FloorToInt(floor.tokenReward * 0.25f);
            }
        }

        return new TowerFightResult
        {
            outcome = combatResult.outcome,
            expGained = expGained,
            tokensGained = tokensGained,
            rounds = combatResult.rounds,
            playerRemainingHp = combatResult.playerRemainingHp,
            playerStartHp = combatResult.playerStartHp,
            enemyRemainingHp = combatResult.enemyRemainingHp,
            enemyStartHp = combatResult.enemyStartHp,
            playerDamageDealt = combatResult.playerDamageDealt,
            enemyDamageDealt = combatResult.enemyDamageDealt,
            playerCrits = combatResult.playerCrits,
            enemyCrits = combatResult.enemyCrits,
            playerDodges = combatResult.playerDodges,
            enemyDodges = combatResult.enemyDodges,
            playerBlocks = combatResult.playerBlocks,
            enemyBlocks = combatResult.enemyBlocks,
            playerStance = combatResult.playerStance,
            enemyStance = combatResult.enemyStance,
            combatLog = combatResult.combatLog,
            playbackEvents = combatResult.playbackEvents,
            firstClear = firstClear
        };
    }

    private void PlayCombatPlayback(TowerFloorData floor, TowerFightResult result)
    {
        if (combatPlaybackUI == null)
        {
            FinishFight(floor, result);
            return;
        }

        CombatPlaybackUI.PlaybackData playbackData = new CombatPlaybackUI.PlaybackData
        {
            playerName = playerStats != null ? playerStats.playerName : "Player",
            enemyName = floor.enemyName,
            playerStance = result.playerStance,
            enemyStance = result.enemyStance,
            enemyLevel = floor.recommendedLevel,
            enemyCombatPower = floor.combatPower,
            playerStartHp = result.playerStartHp,
            enemyStartHp = result.enemyStartHp,
            events = result.playbackEvents,
            playerStats = playerStats
        };

        combatPlaybackUI.Play(playbackData, () => FinishFight(floor, result));
    }

    private void FinishFight(TowerFloorData floor, TowerFightResult result)
    {
        ApplyFightOutcome(floor, result);
        ShowResult(floor, result);

        Debug.Log(
            "TowerWindowUI: Floor " +
            floor.floorNumber +
            " finished with " +
            result.outcome +
            ". EXP: " +
            result.expGained +
            ", Tokens: " +
            result.tokensGained);
    }

    private void ApplyFightOutcome(TowerFloorData floor, TowerFightResult result)
    {
        if (result.outcome == CombatOutcome.Victory)
        {
            clearedFloors.Add(floor.floorNumber);

            if (floor.floorNumber < TotalFloors)
                unlockedFloor = Mathf.Max(unlockedFloor, floor.floorNumber + 1);

            SaveProgression();
        }

        ApplyRewards(result);
        RefreshTokens();
        RefreshFloorList();
    }

    private void ApplyRewards(TowerFightResult result)
    {
        if (playerStats == null)
        {
            Debug.LogWarning("TowerWindowUI: Cannot apply Tower rewards because PlayerStats is not assigned.");
            return;
        }

        int levelsGained = 0;

        if (result.expGained > 0)
            levelsGained = playerStats.AddExperience(result.expGained);

        if (result.tokensGained > 0)
            playerStats.AddArenaTokens(result.tokensGained);

        if (levelsGained > 0)
            RefreshEquipmentAfterLevelUp();

        RefreshProgressionUI();
    }

    private void RefreshEquipmentAfterLevelUp()
    {
        EquipmentManager[] managers = Resources.FindObjectsOfTypeAll<EquipmentManager>();

        foreach (EquipmentManager manager in managers)
        {
            if (manager == null || !manager.gameObject.scene.IsValid())
                continue;

            if (playerStats != null && manager.playerStats != playerStats)
                continue;

            manager.RefreshPlayerStats();
            return;
        }

        if (playerStats != null)
            playerStats.RecalculateStats();
    }

    private void RefreshProgressionUI()
    {
        RefreshTokens();

        PlayerProfileUI profileUI = FindFirstObjectByType<PlayerProfileUI>();
        if (profileUI != null)
            profileUI.RefreshProfile();

        PlayerBarsUI barsUI = FindFirstObjectByType<PlayerBarsUI>();
        if (barsUI != null)
            barsUI.RefreshBars();

        PlayerStatsWindowUI statsWindowUI = FindFirstObjectByType<PlayerStatsWindowUI>();
        if (statsWindowUI != null)
            statsWindowUI.Refresh();

        CharacterPanelStatsViewUI characterStatsView = FindFirstObjectByType<CharacterPanelStatsViewUI>();
        if (characterStatsView != null)
            characterStatsView.Refresh();
    }

    private void ShowResult(TowerFloorData floor, TowerFightResult result)
    {
        lastBattleLog = string.IsNullOrEmpty(result.combatLog) ? "No battle log available." : result.combatLog;
        resultTitleText.text = GetResultTitle(result.outcome);
        resultTitleText.color = GetResultColor(result.outcome);
        resultEnemyText.text = "Floor " + floor.floorNumber + ": " + floor.enemyName;
        resultHpText.text = "Remaining HP: " + result.playerRemainingHp + "/" + result.playerStartHp + " vs " + result.enemyRemainingHp + "/" + result.enemyStartHp;
        resultDamageText.text = "Damage Dealt: " + result.playerDamageDealt + " vs " + result.enemyDamageDealt;
        resultCombatStatsText.text =
            "C/D/B: " +
            result.playerCrits +
            "/" +
            result.playerDodges +
            "/" +
            result.playerBlocks +
            " vs " +
            result.enemyCrits +
            "/" +
            result.enemyDodges +
            "/" +
            result.enemyBlocks;
        resultExpText.text = "EXP Gained: " + result.expGained;
        resultTokensText.text = "Arena Tokens Gained: " + result.tokensGained;
        resultMessageText.text = BuildResultMessage(floor, result);

        HideBattleLog();
        resultPanel.SetActive(true);
    }

    private string BuildResultMessage(TowerFloorData floor, TowerFightResult result)
    {
        if (result.outcome == CombatOutcome.Victory)
        {
            if (result.firstClear)
            {
                if (floor.floorNumber >= TotalFloors)
                    return "First clear complete. The Tower of Trials is fully cleared.";

                return "First clear complete. Floor " + (floor.floorNumber + 1) + " is now unlocked.";
            }

            return "Repeat clear complete. The tower grants reduced rewards.";
        }

        if (result.outcome == CombatOutcome.Draw)
            return "The trial remains unresolved. No new floor is unlocked.";

        return "Defeat. Floor " + floor.floorNumber + " remains available for another attempt.";
    }

    private CombatSimulator.FighterData BuildPlayerCombatant()
    {
        CombatStance savedStance = PlayerSaveManager.LoadArenaStance(CombatStance.Standard);
        int playerPower = GetPlayerPower();

        if (playerStats == null)
        {
            return new CombatSimulator.FighterData
            {
                fighterName = "Player",
                level = 1,
                maxHp = Mathf.Max(playerPower / 5, 100),
                attack = Mathf.Max(playerPower / 45, 20),
                defense = Mathf.Max(playerPower / 90, 5),
                armorHead = Mathf.Max(playerPower / 90, 5),
                armorBody = Mathf.Max(playerPower / 90, 5),
                armorArms = Mathf.Max(playerPower / 90, 5),
                armorLegs = Mathf.Max(playerPower / 90, 5),
                block = Mathf.Max(playerPower / 110, 5),
                combatPower = playerPower,
                critChance = 5f,
                stance = savedStance,
                blockType = CombatBlockType.Weapon
            };
        }

        return new CombatSimulator.FighterData
        {
            fighterName = playerStats.playerName,
            level = playerStats.level,
            maxHp = Mathf.Max(playerStats.maxHp, 1),
            attack = Mathf.Max(playerStats.strength * 2 + playerStats.rage + playerStats.combatPower / 70, 1),
            defense = Mathf.Max(playerStats.armor, 0),
            strength = playerStats.strength,
            rage = playerStats.rage,
            reaction = playerStats.reaction,
            agility = playerStats.agility,
            armor = playerStats.armor,
            armorHead = playerStats.armor,
            armorBody = playerStats.armor,
            armorArms = playerStats.armor,
            armorLegs = playerStats.armor,
            luck = playerStats.luck,
            block = Mathf.Max(playerStats.armor + Mathf.RoundToInt(playerStats.reaction * 0.5f), 0),
            combatPower = Mathf.Max(playerStats.combatPower, playerPower),
            critChance = Mathf.Clamp(5f + playerStats.luck * 0.15f + playerStats.rage * 0.05f, 2f, 45f),
            stance = savedStance,
            blockType = CombatBlockType.Weapon
        };
    }

    private CombatSimulator.FighterData BuildEnemyCombatant(TowerFloorData floor)
    {
        return new CombatSimulator.FighterData
        {
            fighterName = floor.enemyName,
            level = floor.recommendedLevel,
            maxHp = Mathf.Max(floor.hp, 1),
            attack = Mathf.Max(floor.attack, 1),
            defense = Mathf.Max(floor.defense, 0),
            strength = Mathf.Max(floor.attack / 4, 1),
            rage = Mathf.Max(floor.attack / 9, 1),
            reaction = Mathf.Max(floor.reaction, 1),
            agility = Mathf.Max(floor.agility, 1),
            armor = Mathf.Max(floor.armor, 0),
            armorHead = Mathf.Max(floor.armor, 0),
            armorBody = Mathf.Max(floor.armor, 0),
            armorArms = Mathf.Max(floor.armor, 0),
            armorLegs = Mathf.Max(floor.armor, 0),
            luck = Mathf.Max(Mathf.RoundToInt(floor.critChance), 1),
            block = Mathf.Max(floor.defense + Mathf.RoundToInt(floor.reaction * 0.5f), 0),
            combatPower = Mathf.Max(floor.combatPower, 1),
            critChance = Mathf.Max(floor.critChance, 2f),
            stance = GetEnemyStance(floor),
            blockType = CombatBlockType.Weapon
        };
    }

    private CombatStance GetEnemyStance(TowerFloorData floor)
    {
        if (floor.floorNumber >= 7)
            return CombatStance.Defensive;

        if (floor.floorNumber % 3 == 0)
            return CombatStance.Aggressive;

        return CombatStance.Standard;
    }

    private int GetPlayerPower()
    {
        if (playerStats != null && playerStats.combatPower > 0)
            return playerStats.combatPower;

        return Mathf.Max(fallbackPlayerPower, 1);
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

    private void ShowBattleLog()
    {
        if (battleLogText != null)
            battleLogText.text = lastBattleLog;

        if (battleLogPanel != null)
            battleLogPanel.SetActive(true);
    }

    private void HideBattleLog()
    {
        if (battleLogPanel != null)
            battleLogPanel.SetActive(false);
    }

    private void HideResult()
    {
        if (resultPanel != null)
            resultPanel.SetActive(false);
    }

    private void HideCombatPlayback()
    {
        if (combatPlaybackUI != null)
            combatPlaybackUI.StopAndHide();
    }

    private string GetResultTitle(CombatOutcome outcome)
    {
        switch (outcome)
        {
            case CombatOutcome.Victory:
                return "VICTORY";
            case CombatOutcome.Draw:
                return "DRAW";
            default:
                return "DEFEAT";
        }
    }

    private Color GetResultColor(CombatOutcome outcome)
    {
        switch (outcome)
        {
            case CombatOutcome.Victory:
                return victoryColor;
            case CombatOutcome.Draw:
                return drawColor;
            default:
                return defeatColor;
        }
    }

    private GameObject CreateLayoutObject(string objectName, Transform parent)
    {
        GameObject layoutObject = new GameObject(objectName, typeof(RectTransform));
        layoutObject.transform.SetParent(parent, false);
        return layoutObject;
    }

    private TMP_Text CreateText(string objectName, Transform parent, string textValue, int fontSize, FontStyles fontStyle, TextAlignmentOptions alignment)
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
        textElement.minHeight = fontSize + 16f;
        textElement.preferredHeight = fontSize + 20f;

        return text;
    }

    private Button CreateButton(string objectName, Transform parent, string label, int fontSize, Vector2 size, Color backgroundColor)
    {
        GameObject buttonObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        Image image = buttonObject.GetComponent<Image>();
        image.color = backgroundColor;
        AddOutline(buttonObject, borderColor, new Vector2(2f, -2f));

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        ColorBlock colors = button.colors;
        colors.normalColor = backgroundColor;
        colors.highlightedColor = Color.Lerp(backgroundColor, titleColor, 0.18f);
        colors.pressedColor = Color.Lerp(backgroundColor, Color.black, 0.2f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(backgroundColor.r, backgroundColor.g, backgroundColor.b, 0.45f);
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.08f;
        button.colors = colors;

        LayoutElement layoutElement = buttonObject.AddComponent<LayoutElement>();
        layoutElement.minWidth = size.x;
        layoutElement.preferredWidth = size.x;
        layoutElement.minHeight = size.y;
        layoutElement.preferredHeight = size.y;

        TMP_Text text = CreateText("Text", buttonObject.transform, label, fontSize, FontStyles.Bold, TextAlignmentOptions.Center);
        text.color = titleColor;
        Stretch(text.GetComponent<RectTransform>());
        Destroy(text.GetComponent<LayoutElement>());

        return button;
    }

    private void AddOutline(GameObject target, Color color, Vector2 distance)
    {
        Outline outline = target.AddComponent<Outline>();
        outline.effectColor = color;
        outline.effectDistance = distance;
        outline.useGraphicAlpha = true;
    }

    private void Stretch(RectTransform rectTransform)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }
}
