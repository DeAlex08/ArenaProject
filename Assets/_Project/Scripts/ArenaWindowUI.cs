using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ArenaWindowUI : MonoBehaviour
{
    private enum ArenaEnemyDifficulty
    {
        Easy,
        Balanced,
        Hard
    }

    private enum ArenaEnemyArchetype
    {
        Berserker,
        Gambler,
        Duelist
    }

    private struct EnemyStatBlock
    {
        public int strength;
        public int rage;
        public int reaction;
        public int agility;
        public int endurance;
        public int armor;
        public int luck;
        public int intelligence;
        public int combatPower;
        public int hp;
        public int attack;
    }

    private struct ArenaFightResult
    {
        public CombatOutcome outcome;
        public int finalPlayerPower;
        public int finalEnemyPower;
        public int playerRemainingHp;
        public int playerStartHp;
        public int enemyRemainingHp;
        public int enemyStartHp;
        public int expGained;
        public int tokensGained;
        public int rounds;
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
    }

    [System.Serializable]
    private class ArenaEnemyData
    {
        public string enemyName = "Arena Fighter";
        public int level = 10;
        public int combatPower = 1000;
        public int hp = 500;
        public int attack = 80;
        public int defense = 30;
        public int strength = 10;
        public int rage = 5;
        public int luck = 5;
        public int endurance = 10;
        public int armor = 30;
        public int agility = 10;
        public int reaction = 10;
        public float critChance = 5f;
        public int baseExpReward = 25;
        public int tokenReward = 10;
        public string gearSummary = "Tier 1 common gear style";
        [TextArea(2, 4)] public string description = "A hungry arena challenger looking for glory.";
    }

    [Header("Navigation")]
    [SerializeField] private LocationNavigationController navigationController;

    [Header("Player")]
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private int fallbackPlayerPower = 5567;

    [Header("Arena State")]
    [SerializeField] private string rank = "Bronze III";
    [SerializeField] private int arenaTokens = 0;
    [SerializeField] private CombatStance playerStance = CombatStance.Standard;

    [Header("Generated Enemies")]
    [SerializeField] private ArenaEnemyData[] enemies;

    private bool isBuilt;
    private Transform enemyCardsRoot;
    private GameObject enemyInfoPanel;
    private TMP_Text infoTitleText;
    private TMP_Text infoPortraitMarkText;
    private TMP_Text infoLevelText;
    private TMP_Text infoPowerText;
    private TMP_Text infoHpText;
    private TMP_Text infoAttackText;
    private TMP_Text infoDefenseText;
    private TMP_Text infoAgilityText;
    private TMP_Text infoReactionText;
    private TMP_Text infoCritText;
    private TMP_Text infoExpText;
    private TMP_Text infoTokenText;
    private TMP_Text infoDescriptionText;
    private Button infoFightButton;
    private GameObject resultPanel;
    private TMP_Text resultTitleText;
    private TMP_Text resultEnemyText;
    private TMP_Text resultPlayerStanceText;
    private TMP_Text resultEnemyStanceText;
    private TMP_Text resultHpText;
    private TMP_Text resultDamageText;
    private TMP_Text resultCombatStatsText;
    private TMP_Text resultExpText;
    private TMP_Text resultTokensText;
    private TMP_Text resultMessageText;
    private CombatPlaybackUI combatPlaybackUI;
    private GameObject battleLogPanel;
    private TMP_Text battleLogText;
    private TMP_Text arenaTokensText;
    private Button aggressiveStanceButton;
    private Button standardStanceButton;
    private Button defensiveStanceButton;
    private int selectedEnemyIndex = -1;
    private string lastBattleLog = "";

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
        LoadSavedPlayerStance();
        GenerateEnemies();
        BuildIfNeeded();
    }

    private void OnEnable()
    {
        LoadSavedPlayerStance();
        GenerateEnemies();
        BuildIfNeeded();
        RefreshEnemyCards();
        RefreshStanceButtons();
        RefreshArenaTokensText();
        HideEnemyInfo();
        HideResult();
        HideBattleLog();
        HideCombatPlayback();
        selectedEnemyIndex = -1;
    }

    private void OnDisable()
    {
        HideCombatPlayback();
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
        if (!TryGetEnemy(enemyIndex, out ArenaEnemyData enemy))
            return;

        ArenaFightResult result = RunPseudoBattle(enemy);
        HideEnemyInfo();
        HideResult();
        HideBattleLog();
        PlayCombatPlayback(enemy, result);

        Debug.Log(
            "ArenaWindowUI: Fight calculated: " +
            result.outcome +
            " | Enemy: " +
            enemy.enemyName +
            " | Level: " + enemy.level +
            " | Power: " + enemy.combatPower +
            " | Player Stance: " + result.playerStance +
            " | Enemy Stance: " + result.enemyStance +
            " | EXP Gained: " + result.expGained +
            " | Arena Tokens Gained: " + result.tokensGained);
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
        rootLayout.spacing = 16f;
        rootLayout.childAlignment = TextAnchor.UpperCenter;
        rootLayout.childControlWidth = true;
        rootLayout.childControlHeight = true;
        rootLayout.childForceExpandWidth = true;
        rootLayout.childForceExpandHeight = false;

        BuildHeader();
        BuildSummary();
        BuildStanceSelector();
        BuildEnemies();
        BuildEnemyInfoPanel();
        BuildCombatPlaybackPanel();
        BuildResultPanel();
        BuildBattleLogPanel();
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
        arenaTokensText = CreateStatusLabel("ArenaTokensText", summary.transform, BuildArenaTokensText());

        Button refreshButton = CreateButton("RefreshOpponentsButton", summary.transform, "Refresh Opponents", 22, new Vector2(300f, 54f), closeButtonColor);
        refreshButton.onClick.AddListener(RefreshOpponents);
    }

    private void BuildStanceSelector()
    {
        GameObject selector = CreateLayoutObject("StanceSelector", transform);
        Image selectorImage = selector.AddComponent<Image>();
        selectorImage.color = new Color(0.045f, 0.034f, 0.024f, 0.92f);
        AddOutline(selector, darkBorderColor, new Vector2(2f, -2f));

        HorizontalLayoutGroup layout = selector.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(18, 18, 8, 8);
        layout.spacing = 12f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        LayoutElement selectorElement = selector.AddComponent<LayoutElement>();
        selectorElement.minHeight = 64f;
        selectorElement.preferredHeight = 64f;

        TMP_Text label = CreateText("Label", selector.transform, "Stance", 23, FontStyles.Bold, TextAlignmentOptions.Center);
        label.color = mutedTextColor;
        LayoutElement labelElement = label.gameObject.GetComponent<LayoutElement>();
        labelElement.minWidth = 120f;
        labelElement.preferredWidth = 130f;

        aggressiveStanceButton = CreateStanceButton(selector.transform, "Aggressive", CombatStance.Aggressive);
        standardStanceButton = CreateStanceButton(selector.transform, "Standard", CombatStance.Standard);
        defensiveStanceButton = CreateStanceButton(selector.transform, "Defensive", CombatStance.Defensive);

        RefreshStanceButtons();
    }

    private Button CreateStanceButton(Transform parent, string label, CombatStance stance)
    {
        Button button = CreateButton("Stance" + stance + "Button", parent, label, 22, new Vector2(220f, 48f), closeButtonColor);
        button.onClick.AddListener(() => SetPlayerStance(stance, true));
        return button;
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
        listElement.minHeight = 610f;
        listElement.preferredHeight = 610f;
        listElement.flexibleHeight = 1f;

        enemyCardsRoot = list.transform;
        RefreshEnemyCards();
    }

    private void RefreshEnemyCards()
    {
        if (enemyCardsRoot == null)
            return;

        for (int i = enemyCardsRoot.childCount - 1; i >= 0; i--)
        {
            GameObject child = enemyCardsRoot.GetChild(i).gameObject;
            child.SetActive(false);
            Destroy(child);
        }

        int enemyCount = enemies != null ? enemies.Length : 0;
        for (int i = 0; i < enemyCount; i++)
        {
            BuildEnemyCard(enemyCardsRoot, enemies[i], i);
        }
    }

    public void RefreshOpponents()
    {
        GenerateEnemies();
        RefreshEnemyCards();
        HideEnemyInfo();
        HideResult();
        HideBattleLog();
        HideCombatPlayback();
        selectedEnemyIndex = -1;

        Debug.Log("ArenaWindowUI: Refreshed generated Arena opponents.");
    }

    private void BuildEnemyCard(Transform parent, ArenaEnemyData enemy, int enemyIndex)
    {
        GameObject card = CreateLayoutObject("EnemyCard_" + enemyIndex, parent);
        Image cardImage = card.AddComponent<Image>();
        cardImage.color = cardColor;
        AddOutline(card, borderColor, new Vector2(2f, -2f));

        VerticalLayoutGroup cardLayout = card.AddComponent<VerticalLayoutGroup>();
        cardLayout.padding = new RectOffset(24, 24, 22, 24);
        cardLayout.spacing = 12f;
        cardLayout.childAlignment = TextAnchor.UpperCenter;
        cardLayout.childControlWidth = true;
        cardLayout.childControlHeight = true;
        cardLayout.childForceExpandWidth = true;
        cardLayout.childForceExpandHeight = false;

        LayoutElement cardElement = card.AddComponent<LayoutElement>();
        cardElement.minWidth = 360f;
        cardElement.preferredWidth = 400f;
        cardElement.flexibleWidth = 1f;
        cardElement.minHeight = 560f;
        cardElement.preferredHeight = 590f;

        CreatePortraitPlaceholder(card.transform, enemyIndex, 150f, 160f);

        TMP_Text enemyNameText = CreateText("EnemyNameText", card.transform, enemy.enemyName, 31, FontStyles.Bold, TextAlignmentOptions.Center);
        enemyNameText.color = titleColor;

        TMP_Text levelText = CreateText("LevelText", card.transform, "Level: " + enemy.level, 23, FontStyles.Bold, TextAlignmentOptions.Center);
        levelText.color = textColor;

        TMP_Text powerText = CreateText("PowerText", card.transform, "Power: " + enemy.combatPower, 24, FontStyles.Bold, TextAlignmentOptions.Center);
        powerText.color = textColor;

        TMP_Text expText = CreateText("ExpRewardText", card.transform, "EXP: " + CalculateExpReward(enemy), 22, FontStyles.Normal, TextAlignmentOptions.Center);
        expText.color = mutedTextColor;

        TMP_Text tokenText = CreateText("TokenRewardText", card.transform, "Tokens: " + enemy.tokenReward, 22, FontStyles.Normal, TextAlignmentOptions.Center);
        tokenText.color = mutedTextColor;

        GameObject spacer = CreateLayoutObject("Spacer", card.transform);
        LayoutElement spacerElement = spacer.AddComponent<LayoutElement>();
        spacerElement.flexibleHeight = 1f;

        GameObject buttons = CreateLayoutObject("Actions", card.transform);
        HorizontalLayoutGroup buttonsLayout = buttons.AddComponent<HorizontalLayoutGroup>();
        buttonsLayout.spacing = 16f;
        buttonsLayout.childAlignment = TextAnchor.MiddleCenter;
        buttonsLayout.childControlWidth = true;
        buttonsLayout.childControlHeight = true;
        buttonsLayout.childForceExpandWidth = true;
        buttonsLayout.childForceExpandHeight = false;

        LayoutElement buttonsElement = buttons.AddComponent<LayoutElement>();
        buttonsElement.minHeight = 76f;
        buttonsElement.preferredHeight = 76f;

        Button infoButton = CreateButton("InfoButton", buttons.transform, "Info", 24, new Vector2(0f, 70f), closeButtonColor);
        Button fightButton = CreateButton("FightButton", buttons.transform, "Fight", 26, new Vector2(0f, 70f), buttonColor);

        infoButton.gameObject.GetComponent<LayoutElement>().flexibleWidth = 1f;
        fightButton.gameObject.GetComponent<LayoutElement>().flexibleWidth = 1f;

        int cachedEnemyIndex = enemyIndex;
        infoButton.onClick.AddListener(() => ShowEnemyInfo(cachedEnemyIndex));
        fightButton.onClick.AddListener(() => FightEnemy(cachedEnemyIndex));
    }

    private void BuildEnemyInfoPanel()
    {
        enemyInfoPanel = CreateLayoutObject("EnemyInfoPanel", transform);
        LayoutElement ignoreLayout = enemyInfoPanel.AddComponent<LayoutElement>();
        ignoreLayout.ignoreLayout = true;

        Image panelImage = enemyInfoPanel.AddComponent<Image>();
        panelImage.color = new Color(0.025f, 0.02f, 0.016f, 0.985f);
        AddOutline(enemyInfoPanel, borderColor, new Vector2(2f, -2f));

        RectTransform rect = enemyInfoPanel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(920f, 880f);
        rect.pivot = new Vector2(0.5f, 0.5f);

        VerticalLayoutGroup layout = enemyInfoPanel.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(42, 42, 34, 38);
        layout.spacing = 16f;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        GameObject header = CreateLayoutObject("Header", enemyInfoPanel.transform);
        HorizontalLayoutGroup headerLayout = header.AddComponent<HorizontalLayoutGroup>();
        headerLayout.childAlignment = TextAnchor.MiddleCenter;
        headerLayout.childControlWidth = true;
        headerLayout.childControlHeight = true;
        headerLayout.childForceExpandWidth = false;
        headerLayout.childForceExpandHeight = false;

        header.AddComponent<LayoutElement>().preferredHeight = 58f;

        infoTitleText = CreateText("EnemyNameText", header.transform, "", 34, FontStyles.Bold, TextAlignmentOptions.Center);
        infoTitleText.color = titleColor;
        infoTitleText.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;

        Button closeButton = CreateButton("CloseInfoButton", header.transform, "X", 24, new Vector2(54f, 50f), closeButtonColor);
        closeButton.onClick.AddListener(HideEnemyInfo);

        GameObject body = CreateLayoutObject("Body", enemyInfoPanel.transform);
        HorizontalLayoutGroup bodyLayout = body.AddComponent<HorizontalLayoutGroup>();
        bodyLayout.spacing = 28f;
        bodyLayout.childAlignment = TextAnchor.UpperCenter;
        bodyLayout.childControlWidth = true;
        bodyLayout.childControlHeight = true;
        bodyLayout.childForceExpandWidth = true;
        bodyLayout.childForceExpandHeight = true;
        body.AddComponent<LayoutElement>().preferredHeight = 560f;

        GameObject portrait = CreatePortraitPlaceholder(body.transform, 0, 500f, 520f);
        portrait.GetComponent<LayoutElement>().preferredWidth = 360f;
        infoPortraitMarkText = portrait.transform.Find("Mark").GetComponent<TMP_Text>();

        GameObject stats = CreateLayoutObject("Stats", body.transform);
        VerticalLayoutGroup statsLayout = stats.AddComponent<VerticalLayoutGroup>();
        statsLayout.spacing = 9f;
        statsLayout.childAlignment = TextAnchor.UpperLeft;
        statsLayout.childControlWidth = true;
        statsLayout.childControlHeight = true;
        statsLayout.childForceExpandWidth = true;
        statsLayout.childForceExpandHeight = false;
        stats.AddComponent<LayoutElement>().flexibleWidth = 1f;

        infoLevelText = CreateInfoLine("LevelText", stats.transform);
        infoPowerText = CreateInfoLine("PowerText", stats.transform);
        infoHpText = CreateInfoLine("HpText", stats.transform);
        infoAttackText = CreateInfoLine("AttackText", stats.transform);
        infoDefenseText = CreateInfoLine("DefenseText", stats.transform);
        infoAgilityText = CreateInfoLine("AgilityText", stats.transform);
        infoReactionText = CreateInfoLine("ReactionText", stats.transform);
        infoCritText = CreateInfoLine("CritText", stats.transform);
        infoExpText = CreateInfoLine("ExpText", stats.transform);
        infoTokenText = CreateInfoLine("TokenText", stats.transform);

        infoDescriptionText = CreateText("DescriptionText", enemyInfoPanel.transform, "", 24, FontStyles.Normal, TextAlignmentOptions.Center);
        infoDescriptionText.color = mutedTextColor;
        infoDescriptionText.gameObject.GetComponent<LayoutElement>().preferredHeight = 106f;

        GameObject actions = CreateLayoutObject("Actions", enemyInfoPanel.transform);
        HorizontalLayoutGroup actionsLayout = actions.AddComponent<HorizontalLayoutGroup>();
        actionsLayout.spacing = 24f;
        actionsLayout.childAlignment = TextAnchor.MiddleCenter;
        actionsLayout.childControlWidth = true;
        actionsLayout.childControlHeight = true;
        actionsLayout.childForceExpandWidth = true;
        actionsLayout.childForceExpandHeight = false;
        actions.AddComponent<LayoutElement>().preferredHeight = 78f;

        Button closeInfoButton = CreateButton("CloseButton", actions.transform, "Close", 26, new Vector2(0f, 74f), closeButtonColor);
        closeInfoButton.onClick.AddListener(HideEnemyInfo);
        closeInfoButton.gameObject.GetComponent<LayoutElement>().flexibleWidth = 1f;

        infoFightButton = CreateButton("FightButton", actions.transform, "Fight", 28, new Vector2(0f, 74f), buttonColor);
        infoFightButton.gameObject.GetComponent<LayoutElement>().flexibleWidth = 1f;

        HideEnemyInfo();
    }

    private void BuildCombatPlaybackPanel()
    {
        GameObject playbackPanel = CreateLayoutObject("CombatPlaybackPanel", transform);
        LayoutElement ignoreLayout = playbackPanel.AddComponent<LayoutElement>();
        ignoreLayout.ignoreLayout = true;

        combatPlaybackUI = playbackPanel.AddComponent<CombatPlaybackUI>();
        combatPlaybackUI.StopAndHide();
    }

    private void BuildResultPanel()
    {
        resultPanel = CreateLayoutObject("ResultPanel", transform);
        LayoutElement ignoreLayout = resultPanel.AddComponent<LayoutElement>();
        ignoreLayout.ignoreLayout = true;

        Image panelImage = resultPanel.AddComponent<Image>();
        panelImage.color = new Color(0.025f, 0.02f, 0.016f, 0.99f);
        AddOutline(resultPanel, borderColor, new Vector2(2f, -2f));

        RectTransform rect = resultPanel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(840f, 820f);
        rect.pivot = new Vector2(0.5f, 0.5f);

        VerticalLayoutGroup layout = resultPanel.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(48, 48, 42, 46);
        layout.spacing = 12f;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        resultTitleText = CreateText("ResultTitleText", resultPanel.transform, "", 44, FontStyles.Bold, TextAlignmentOptions.Center);
        resultTitleText.characterSpacing = 6f;
        resultTitleText.gameObject.GetComponent<LayoutElement>().preferredHeight = 72f;

        resultEnemyText = CreateResultLine("EnemyText");
        resultPlayerStanceText = CreateResultLine("PlayerStanceText");
        resultEnemyStanceText = CreateResultLine("EnemyStanceText");
        resultHpText = CreateResultLine("RemainingHpText");
        resultDamageText = CreateResultLine("DamageText");
        resultCombatStatsText = CreateResultLine("CombatStatsText");
        resultExpText = CreateResultLine("ExpText");
        resultTokensText = CreateResultLine("TokensText");

        resultMessageText = CreateText("MessageText", resultPanel.transform, "", 25, FontStyles.Normal, TextAlignmentOptions.Center);
        resultMessageText.color = mutedTextColor;
        resultMessageText.gameObject.GetComponent<LayoutElement>().preferredHeight = 86f;

        GameObject spacer = CreateLayoutObject("Spacer", resultPanel.transform);
        spacer.AddComponent<LayoutElement>().flexibleHeight = 1f;

        GameObject actions = CreateLayoutObject("Actions", resultPanel.transform);
        HorizontalLayoutGroup actionsLayout = actions.AddComponent<HorizontalLayoutGroup>();
        actionsLayout.spacing = 24f;
        actionsLayout.childAlignment = TextAnchor.MiddleCenter;
        actionsLayout.childControlWidth = true;
        actionsLayout.childControlHeight = true;
        actionsLayout.childForceExpandWidth = true;
        actionsLayout.childForceExpandHeight = false;
        actions.AddComponent<LayoutElement>().preferredHeight = 78f;

        Button battleLogButton = CreateButton("BattleLogButton", actions.transform, "Battle Log", 26, new Vector2(0f, 78f), closeButtonColor);
        battleLogButton.gameObject.GetComponent<LayoutElement>().flexibleWidth = 1f;
        battleLogButton.onClick.AddListener(ShowBattleLog);

        Button continueButton = CreateButton("ContinueButton", actions.transform, "Continue", 30, new Vector2(0f, 78f), buttonColor);
        continueButton.gameObject.GetComponent<LayoutElement>().flexibleWidth = 1f;
        continueButton.onClick.AddListener(HideResult);

        HideResult();
    }

    private void BuildBattleLogPanel()
    {
        battleLogPanel = CreateLayoutObject("BattleLogPanel", transform);
        LayoutElement ignoreLayout = battleLogPanel.AddComponent<LayoutElement>();
        ignoreLayout.ignoreLayout = true;

        Image panelImage = battleLogPanel.AddComponent<Image>();
        panelImage.color = new Color(0.018f, 0.015f, 0.012f, 0.995f);
        AddOutline(battleLogPanel, borderColor, new Vector2(2f, -2f));

        RectTransform rect = battleLogPanel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(1040f, 900f);
        rect.pivot = new Vector2(0.5f, 0.5f);

        VerticalLayoutGroup layout = battleLogPanel.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(42, 42, 34, 38);
        layout.spacing = 18f;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        GameObject header = CreateLayoutObject("Header", battleLogPanel.transform);
        HorizontalLayoutGroup headerLayout = header.AddComponent<HorizontalLayoutGroup>();
        headerLayout.childAlignment = TextAnchor.MiddleCenter;
        headerLayout.childControlWidth = true;
        headerLayout.childControlHeight = true;
        headerLayout.childForceExpandWidth = false;
        headerLayout.childForceExpandHeight = false;
        header.AddComponent<LayoutElement>().preferredHeight = 62f;

        TMP_Text title = CreateText("Title", header.transform, "BATTLE LOG", 36, FontStyles.Bold, TextAlignmentOptions.Center);
        title.color = titleColor;
        title.gameObject.GetComponent<LayoutElement>().flexibleWidth = 1f;

        Button closeButton = CreateButton("CloseButton", header.transform, "X", 24, new Vector2(54f, 50f), closeButtonColor);
        closeButton.onClick.AddListener(HideBattleLog);

        GameObject scrollObject = CreateLayoutObject("ScrollView", battleLogPanel.transform);
        Image scrollImage = scrollObject.AddComponent<Image>();
        scrollImage.color = new Color(0.035f, 0.028f, 0.022f, 0.96f);
        AddOutline(scrollObject, darkBorderColor, new Vector2(2f, -2f));
        ScrollRect scrollRect = scrollObject.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollObject.AddComponent<LayoutElement>().preferredHeight = 690f;

        GameObject viewport = CreateLayoutObject("Viewport", scrollObject.transform);
        RectTransform viewportRect = viewport.GetComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = new Vector2(18f, 18f);
        viewportRect.offsetMax = new Vector2(-18f, -18f);
        Image viewportImage = viewport.AddComponent<Image>();
        viewportImage.color = new Color(0f, 0f, 0f, 0.01f);
        Mask mask = viewport.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        GameObject content = CreateLayoutObject("Content", viewport.transform);
        RectTransform contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.offsetMin = Vector2.zero;
        contentRect.offsetMax = Vector2.zero;
        VerticalLayoutGroup contentLayout = content.AddComponent<VerticalLayoutGroup>();
        contentLayout.childControlWidth = true;
        contentLayout.childControlHeight = true;
        contentLayout.childForceExpandWidth = true;
        contentLayout.childForceExpandHeight = false;
        ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        battleLogText = CreateText("LogText", content.transform, "", 24, FontStyles.Normal, TextAlignmentOptions.TopLeft);
        battleLogText.color = textColor;
        battleLogText.textWrappingMode = TextWrappingModes.Normal;
        battleLogText.gameObject.GetComponent<LayoutElement>().preferredHeight = 5000f;

        scrollRect.viewport = viewportRect;
        scrollRect.content = contentRect;

        Button closeLogButton = CreateButton("CloseLogButton", battleLogPanel.transform, "Close", 28, new Vector2(0f, 76f), buttonColor);
        closeLogButton.gameObject.GetComponent<LayoutElement>().flexibleWidth = 1f;
        closeLogButton.onClick.AddListener(HideBattleLog);

        HideBattleLog();
    }

    private TMP_Text CreateResultLine(string objectName)
    {
        TMP_Text text = CreateText(objectName, resultPanel.transform, "", 23, FontStyles.Bold, TextAlignmentOptions.Center);
        text.color = textColor;
        text.gameObject.GetComponent<LayoutElement>().preferredHeight = 36f;
        return text;
    }

    private TMP_Text CreateInfoLine(string objectName, Transform parent)
    {
        TMP_Text text = CreateText(objectName, parent, "", 25, FontStyles.Bold, TextAlignmentOptions.Left);
        text.color = textColor;
        text.gameObject.GetComponent<LayoutElement>().preferredHeight = 39f;
        return text;
    }

    private void ShowEnemyInfo(int enemyIndex)
    {
        if (!TryGetEnemy(enemyIndex, out ArenaEnemyData enemy))
            return;

        selectedEnemyIndex = enemyIndex;
        int expReward = CalculateExpReward(enemy);

        infoTitleText.text = enemy.enemyName;
        infoPortraitMarkText.text = GetPortraitMark(enemyIndex);
        infoLevelText.text = "Level: " + enemy.level;
        infoPowerText.text = "Combat Power: " + enemy.combatPower;
        infoHpText.text = "HP: " + enemy.hp;
        infoAttackText.text = "Attack / Strength: " + enemy.attack + " / " + enemy.strength;
        infoDefenseText.text = "Armor / Endurance: " + enemy.armor + " / " + enemy.endurance;
        infoAgilityText.text = "Agility: " + enemy.agility;
        infoReactionText.text = "Reaction: " + enemy.reaction;
        infoCritText.text = "Rage / Luck / Crit: " + enemy.rage + " / " + enemy.luck + " / " + enemy.critChance.ToString("0.#") + "%";
        infoExpText.text = "EXP Reward: " + expReward;
        infoTokenText.text = "Arena Tokens: " + enemy.tokenReward;
        infoDescriptionText.text = enemy.gearSummary + ". " + enemy.description;

        infoFightButton.onClick.RemoveAllListeners();
        infoFightButton.onClick.AddListener(() => FightEnemy(selectedEnemyIndex));

        enemyInfoPanel.SetActive(true);
    }

    private void HideEnemyInfo()
    {
        if (enemyInfoPanel != null)
            enemyInfoPanel.SetActive(false);
    }

    private ArenaFightResult RunPseudoBattle(ArenaEnemyData enemy)
    {
        CombatSimulator.FighterData playerCombatant = BuildPlayerCombatant();
        CombatSimulator.FighterData enemyCombatant = BuildEnemyCombatant(enemy);
        CombatSimulator.CombatResult combatResult = CombatSimulator.Simulate(playerCombatant, enemyCombatant);

        int calculatedExpReward = CalculateExpReward(enemy);
        int expGained = 0;
        int tokensGained = 0;

        switch (combatResult.outcome)
        {
            case CombatOutcome.Victory:
                expGained = calculatedExpReward;
                tokensGained = enemy.tokenReward;
                break;

            case CombatOutcome.Draw:
                expGained = Mathf.RoundToInt(calculatedExpReward * 0.5f);
                tokensGained = Mathf.RoundToInt(enemy.tokenReward * 0.25f);
                break;

            default:
                expGained = Mathf.RoundToInt(calculatedExpReward * 0.25f);
                tokensGained = 0;
                break;
        }

        ArenaFightResult result = new ArenaFightResult
        {
            outcome = combatResult.outcome,
            finalPlayerPower = combatResult.playerFinalPower,
            finalEnemyPower = combatResult.enemyFinalPower,
            playerRemainingHp = combatResult.playerRemainingHp,
            playerStartHp = combatResult.playerStartHp,
            enemyRemainingHp = combatResult.enemyRemainingHp,
            enemyStartHp = combatResult.enemyStartHp,
            expGained = expGained,
            tokensGained = tokensGained,
            rounds = combatResult.rounds,
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
            playbackEvents = combatResult.playbackEvents
        };

        return result;
    }

    private void PlayCombatPlayback(ArenaEnemyData enemy, ArenaFightResult result)
    {
        if (combatPlaybackUI == null)
        {
            FinishFight(enemy, result);
            return;
        }

        CombatPlaybackUI.PlaybackData playbackData = new CombatPlaybackUI.PlaybackData
        {
            playerName = playerStats != null ? playerStats.playerName : "Player",
            enemyName = enemy.enemyName,
            playerStance = result.playerStance,
            enemyStance = result.enemyStance,
            enemyLevel = enemy.level,
            enemyCombatPower = enemy.combatPower,
            playerStartHp = result.playerStartHp,
            enemyStartHp = result.enemyStartHp,
            events = result.playbackEvents,
            playerStats = playerStats
        };

        combatPlaybackUI.Play(playbackData, () => FinishFight(enemy, result));
    }

    private void FinishFight(ArenaEnemyData enemy, ArenaFightResult result)
    {
        ApplyFightRewards(result);
        ShowResult(enemy, result);

        Debug.Log(
            "ArenaWindowUI: Fight result shown: " +
            result.outcome +
            " | Enemy: " +
            enemy.enemyName +
            " | Level: " + enemy.level +
            " | Power: " + enemy.combatPower +
            " | Player Stance: " + result.playerStance +
            " | Enemy Stance: " + result.enemyStance +
            " | EXP Gained: " + result.expGained +
            " | Arena Tokens Gained: " + result.tokensGained);
    }

    private void ShowResult(ArenaEnemyData enemy, ArenaFightResult result)
    {
        lastBattleLog = string.IsNullOrEmpty(result.combatLog) ? "No battle log available." : result.combatLog;
        resultTitleText.text = GetResultTitle(result.outcome);
        resultTitleText.color = GetResultColor(result.outcome);
        resultEnemyText.text = "Enemy: " + enemy.enemyName;
        resultPlayerStanceText.text = "Player Stance: " + result.playerStance;
        resultEnemyStanceText.text = "Enemy Stance: " + result.enemyStance;
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
        resultMessageText.text = BuildResultMessage(result, enemy);

        HideBattleLog();
        resultPanel.SetActive(true);
    }

    private void ApplyFightRewards(ArenaFightResult result)
    {
        if (playerStats == null)
        {
            Debug.LogWarning("ArenaWindowUI: Cannot apply Arena rewards because PlayerStats is not assigned.");
            return;
        }

        int levelsGained = 0;

        if (result.expGained > 0)
            levelsGained = playerStats.AddExperience(result.expGained);

        if (result.tokensGained > 0)
            playerStats.AddArenaTokens(result.tokensGained);

        if (levelsGained > 0)
        {
            EquipmentManager equipmentManager = FindEquipmentManager();
            if (equipmentManager != null)
            {
                equipmentManager.RefreshPlayerStats();
            }
            else
            {
                Debug.LogWarning(
                    "ArenaWindowUI: EquipmentManager was not found after level up. " +
                    "Refreshing PlayerStats without equipment bonuses.");
                playerStats.RecalculateStats();
            }
        }

        RefreshProgressionUI();

        Debug.Log(
            "ArenaWindowUI: Applied Arena rewards. EXP: " +
            result.expGained +
            ", Arena Tokens: " +
            result.tokensGained +
            ", Levels gained: " +
            levelsGained);
    }

    private void RefreshProgressionUI()
    {
        RefreshArenaTokensText();

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

    private void LoadSavedPlayerStance()
    {
        playerStance = PlayerSaveManager.LoadArenaStance(CombatStance.Standard);
    }

    private void SetPlayerStance(CombatStance stance, bool save)
    {
        playerStance = stance;
        RefreshStanceButtons();

        if (save)
            PlayerSaveManager.SaveArenaStance(playerStance);
    }

    private void RefreshStanceButtons()
    {
        RefreshStanceButton(aggressiveStanceButton, CombatStance.Aggressive);
        RefreshStanceButton(standardStanceButton, CombatStance.Standard);
        RefreshStanceButton(defensiveStanceButton, CombatStance.Defensive);
    }

    private void RefreshStanceButton(Button button, CombatStance stance)
    {
        if (button == null)
            return;

        bool isSelected = playerStance == stance;
        Color backgroundColor = isSelected ? buttonColor : closeButtonColor;
        Image image = button.GetComponent<Image>();

        if (image != null)
            image.color = backgroundColor;

        TMP_Text text = button.GetComponentInChildren<TMP_Text>();
        if (text != null)
            text.color = isSelected ? titleColor : mutedTextColor;

        ConfigureButtonColors(button, backgroundColor);
    }

    private void RefreshArenaTokensText()
    {
        if (arenaTokensText != null)
            arenaTokensText.text = BuildArenaTokensText();
    }

    private string BuildArenaTokensText()
    {
        return "Arena Tokens: " + GetArenaTokens();
    }

    private int GetArenaTokens()
    {
        if (playerStats != null)
            return playerStats.arenaTokens;

        return arenaTokens;
    }

    private EquipmentManager FindEquipmentManager()
    {
        EquipmentManager[] managers = Resources.FindObjectsOfTypeAll<EquipmentManager>();
        EquipmentManager fallbackManager = null;

        foreach (EquipmentManager manager in managers)
        {
            if (manager == null || !manager.gameObject.scene.IsValid())
                continue;

            if (fallbackManager == null)
                fallbackManager = manager;

            if (playerStats != null && manager.playerStats == playerStats)
                return manager;
        }

        return fallbackManager;
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
                return titleColor;
            case CombatOutcome.Draw:
                return new Color(0.82f, 0.72f, 0.46f, 1f);
            default:
                return new Color(0.74f, 0.18f, 0.13f, 1f);
        }
    }

    private string BuildResultMessage(ArenaFightResult result, ArenaEnemyData enemy)
    {
        string hpLine =
            "Rounds: " +
            result.rounds +
            ". ";

        switch (result.outcome)
        {
            case CombatOutcome.Victory:
                return hpLine + "The crowd roars as " + enemy.enemyName + " falls.";
            case CombatOutcome.Draw:
                return hpLine + "Both fighters survive the clash. The Arena grants partial rewards.";
            default:
                return hpLine + "You survive the duel, but the Arena grants only a small lesson for this defeat.";
        }
    }

    private CombatSimulator.FighterData BuildPlayerCombatant()
    {
        int playerPower = GetPlayerPower();

        if (playerStats == null)
        {
            return new CombatSimulator.FighterData
            {
                fighterName = "Player",
                level = 1,
                maxHp = Mathf.Max(playerPower / 5, 100),
                attack = Mathf.Max(playerPower / 45, 20),
                defense = 0,
                strength = 10,
                rage = 5,
                reaction = 10,
                agility = 10,
                armor = Mathf.Max(playerPower / 90, 5),
                armorHead = Mathf.Max(playerPower / 90, 5),
                armorBody = Mathf.Max(playerPower / 90, 5),
                armorArms = Mathf.Max(playerPower / 90, 5),
                armorLegs = Mathf.Max(playerPower / 90, 5),
                luck = 5,
                block = Mathf.Max(playerPower / 110, 5),
                combatPower = playerPower,
                critChance = 5f,
                stance = playerStance,
                blockType = CombatBlockType.Weapon
            };
        }

        return new CombatSimulator.FighterData
        {
            fighterName = playerStats.playerName,
            level = playerStats.level,
            maxHp = Mathf.Max(playerStats.maxHp, 1),
            attack = Mathf.Max(playerStats.strength * 2 + playerStats.rage + playerStats.combatPower / 70, 1),
            defense = 0,
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
            stance = playerStance,
            blockType = CombatBlockType.Weapon
        };
    }

    private CombatSimulator.FighterData BuildEnemyCombatant(ArenaEnemyData enemy)
    {
        return new CombatSimulator.FighterData
        {
            fighterName = enemy.enemyName,
            level = enemy.level,
            maxHp = Mathf.Max(enemy.hp, 1),
            attack = Mathf.Max(enemy.attack, 1),
            defense = 0,
            strength = Mathf.Max(enemy.strength, 1),
            rage = Mathf.Max(enemy.rage, 0),
            reaction = Mathf.Max(enemy.reaction, 1),
            agility = Mathf.Max(enemy.agility, 1),
            armor = Mathf.Max(enemy.armor, 0),
            armorHead = Mathf.Max(enemy.armor, 0),
            armorBody = Mathf.Max(enemy.armor, 0),
            armorArms = Mathf.Max(enemy.armor, 0),
            armorLegs = Mathf.Max(enemy.armor, 0),
            luck = Mathf.Max(enemy.luck, 0),
            block = Mathf.Max(enemy.armor + Mathf.RoundToInt(enemy.reaction * 0.5f), 0),
            combatPower = Mathf.Max(enemy.combatPower, 1),
            critChance = Mathf.Max(enemy.critChance, 2f),
            stance = GetEnemyStance(enemy),
            blockType = CombatBlockType.Weapon
        };
    }

    private CombatStance GetEnemyStance(ArenaEnemyData enemy)
    {
        if (enemy.armor + enemy.endurance > enemy.strength + enemy.rage + enemy.luck)
            return CombatStance.Defensive;

        if (enemy.strength + enemy.rage >= enemy.agility + enemy.reaction)
            return CombatStance.Aggressive;

        return CombatStance.Standard;
    }

    private int CalculateExpReward(ArenaEnemyData enemy)
    {
        int playerPower = GetPlayerPower();
        float multiplier = GetExpMultiplier(playerPower, enemy.combatPower);

        return Mathf.RoundToInt(enemy.baseExpReward * multiplier);
    }

    private float GetExpMultiplier(int playerPower, int enemyPower)
    {
        if (enemyPower <= playerPower * 0.7f)
            return 0.5f;

        if (enemyPower <= playerPower * 0.9f)
            return 0.75f;

        if (enemyPower <= playerPower * 1.1f)
            return 1f;

        if (enemyPower <= playerPower * 1.3f)
            return 1.25f;

        return 1.5f;
    }

    private int GetPlayerPower()
    {
        if (playerStats != null && playerStats.combatPower > 0)
            return playerStats.combatPower;

        return Mathf.Max(fallbackPlayerPower, 1);
    }

    private int GetPlayerLevel()
    {
        if (playerStats != null)
            return Mathf.Max(playerStats.level, 1);

        return 1;
    }

    private void GenerateEnemies()
    {
        int playerLevel = GetPlayerLevel();

        enemies = new[]
        {
            GenerateEnemy(ArenaEnemyDifficulty.Easy, playerLevel),
            GenerateEnemy(ArenaEnemyDifficulty.Balanced, playerLevel),
            GenerateEnemy(ArenaEnemyDifficulty.Hard, playerLevel)
        };
    }

    private ArenaEnemyData GenerateEnemy(ArenaEnemyDifficulty difficulty, int playerLevel)
    {
        int minLevelOffset;
        int maxLevelOffset;
        float expScale;
        float statScaleMin;
        float statScaleMax;
        string[] names;

        switch (difficulty)
        {
            case ArenaEnemyDifficulty.Easy:
                minLevelOffset = -1;
                maxLevelOffset = 0;
                expScale = 0.72f;
                statScaleMin = 0.62f;
                statScaleMax = 0.72f;
                names = new[] { "Grave Cutthroat", "Ash Squire", "Pit Wanderer", "Bone Initiate" };
                break;

            case ArenaEnemyDifficulty.Hard:
                minLevelOffset = 1;
                maxLevelOffset = 3;
                expScale = 1.42f;
                statScaleMin = 1.08f;
                statScaleMax = 1.18f;
                names = new[] { "Blood Champion", "Blackguard Warden", "Dread Executioner", "Crownless Butcher" };
                break;

            default:
                minLevelOffset = 0;
                maxLevelOffset = 1;
                expScale = 1f;
                statScaleMin = 0.94f;
                statScaleMax = 1.04f;
                names = new[] { "Arena Duelist", "Iron Reaver", "Oathbound Knight", "Veil Hunter" };
                break;
        }

        int level = Mathf.Max(playerLevel + Random.Range(minLevelOffset, maxLevelOffset + 1), 1);
        ArenaEnemyArchetype archetype = PickEnemyArchetype();
        ItemRarity gearRarity = PickEnemyGearRarity(difficulty);
        float statScale = Random.Range(statScaleMin, statScaleMax);
        EnemyStatBlock stats = BuildEnemyStats(level, archetype, gearRarity, statScale);
        float critChance = CalculateCritChance(stats.rage, stats.luck);
        int baseExp = Mathf.Max(Mathf.RoundToInt((level * 6f + stats.combatPower * 0.010f) * expScale), 10);
        int tokens = CalculateTokenReward(difficulty, level, stats.combatPower);

        return new ArenaEnemyData
        {
            enemyName = names[Random.Range(0, names.Length)],
            level = level,
            combatPower = stats.combatPower,
            hp = stats.hp,
            attack = stats.attack,
            defense = 0,
            strength = stats.strength,
            rage = stats.rage,
            luck = stats.luck,
            endurance = stats.endurance,
            armor = stats.armor,
            agility = stats.agility,
            reaction = stats.reaction,
            critChance = critChance,
            baseExpReward = baseExp,
            tokenReward = tokens,
            gearSummary = BuildEnemyGearSummary(archetype, gearRarity),
            description = BuildEnemyDescription(difficulty, archetype)
        };
    }

    private ArenaEnemyArchetype PickEnemyArchetype()
    {
        return (ArenaEnemyArchetype)Random.Range(0, 3);
    }

    private ItemRarity PickEnemyGearRarity(ArenaEnemyDifficulty difficulty)
    {
        switch (difficulty)
        {
            case ArenaEnemyDifficulty.Easy:
                return ItemRarity.Common;
            case ArenaEnemyDifficulty.Hard:
                return Random.value < 0.35f ? ItemRarity.Epic : ItemRarity.Rare;
            default:
                return Random.value < 0.45f ? ItemRarity.Rare : ItemRarity.Common;
        }
    }

    private EnemyStatBlock BuildEnemyStats(int level, ArenaEnemyArchetype archetype, ItemRarity gearRarity, float statScale)
    {
        int levelBonus = Mathf.Max(level - 1, 0);

        EnemyStatBlock stats = new EnemyStatBlock
        {
            strength = 10 + levelBonus * 2,
            rage = 5 + levelBonus,
            reaction = 10 + levelBonus,
            agility = 10 + levelBonus,
            endurance = 10 + levelBonus * 2,
            armor = levelBonus,
            luck = 5 + levelBonus,
            intelligence = 10 + levelBonus
        };

        ApplyTierOneEnemyGear(ref stats, archetype, gearRarity);
        ScaleEnemyStats(ref stats, statScale);

        stats.hp = CalculateHpFromEndurance(stats.endurance);
        stats.combatPower = CalculateCombatPower(
            level,
            stats.strength,
            stats.rage,
            stats.reaction,
            stats.agility,
            stats.endurance,
            stats.armor,
            stats.luck,
            stats.intelligence);
        stats.attack = CalculateAttack(stats.strength, stats.rage, stats.combatPower);

        return stats;
    }

    private void ApplyTierOneEnemyGear(ref EnemyStatBlock stats, ArenaEnemyArchetype archetype, ItemRarity gearRarity)
    {
        int gearTier = GetGearTier(gearRarity);

        switch (archetype)
        {
            case ArenaEnemyArchetype.Gambler:
                stats.strength += 4 + gearTier * 6;
                stats.luck += 4 + gearTier * 6;
                stats.rage += gearTier * 2;
                stats.agility += gearTier * 2;
                stats.endurance += gearTier * 4;
                stats.armor += 2 + gearTier * 5;
                break;

            case ArenaEnemyArchetype.Duelist:
                stats.strength += 4 + gearTier * 5;
                stats.agility += 5 + gearTier * 7;
                stats.reaction += 4 + gearTier * 6;
                stats.endurance += gearTier * 3;
                stats.armor += 2 + gearTier * 4;
                break;

            default:
                stats.strength += 6 + gearTier * 6;
                stats.rage += 4 + gearTier * 6;
                stats.reaction += gearTier;
                stats.endurance += 1 + gearTier * 5;
                stats.armor += 3 + gearTier * 5;
                break;
        }
    }

    private int GetGearTier(ItemRarity gearRarity)
    {
        switch (gearRarity)
        {
            case ItemRarity.Epic:
                return 2;
            case ItemRarity.Rare:
                return 1;
            default:
                return 0;
        }
    }

    private void ScaleEnemyStats(ref EnemyStatBlock stats, float statScale)
    {
        stats.strength = Mathf.Max(Mathf.RoundToInt(stats.strength * statScale), 1);
        stats.rage = Mathf.Max(Mathf.RoundToInt(stats.rage * statScale), 0);
        stats.reaction = Mathf.Max(Mathf.RoundToInt(stats.reaction * statScale), 1);
        stats.agility = Mathf.Max(Mathf.RoundToInt(stats.agility * statScale), 1);
        stats.endurance = Mathf.Max(Mathf.RoundToInt(stats.endurance * statScale), 1);
        stats.armor = Mathf.Max(Mathf.RoundToInt(stats.armor * statScale), 0);
        stats.luck = Mathf.Max(Mathf.RoundToInt(stats.luck * statScale), 0);
        stats.intelligence = Mathf.Max(Mathf.RoundToInt(stats.intelligence * statScale), 1);
    }

    private int CalculateHpFromEndurance(int enduranceValue)
    {
        return 100 + Mathf.Max(enduranceValue, 1) * 10;
    }

    private int CalculateAttack(int strengthValue, int rageValue, int combatPowerValue)
    {
        return Mathf.Max(strengthValue * 2 + rageValue + combatPowerValue / 70, 1);
    }

    private int CalculateCombatPower(
        int levelValue,
        int strengthValue,
        int rageValue,
        int reactionValue,
        int agilityValue,
        int enduranceValue,
        int armorValue,
        int luckValue,
        int intelligenceValue)
    {
        return Mathf.Max(
            levelValue * 100 +
            strengthValue * 10 +
            rageValue * 8 +
            reactionValue * 8 +
            agilityValue * 8 +
            enduranceValue * 10 +
            armorValue * 5 +
            luckValue * 6 +
            intelligenceValue * 8,
            1);
    }

    private float CalculateCritChance(int rageValue, int luckValue)
    {
        return Mathf.Clamp(5f + luckValue * 0.15f + rageValue * 0.05f, 2f, 45f);
    }

    private int CalculateTokenReward(ArenaEnemyDifficulty difficulty, int enemyLevel, int enemyPower)
    {
        int baseReward;

        switch (difficulty)
        {
            case ArenaEnemyDifficulty.Easy:
                baseReward = 10;
                break;
            case ArenaEnemyDifficulty.Hard:
                baseReward = 30;
                break;
            default:
                baseReward = 18;
                break;
        }

        int levelReward = enemyLevel * (difficulty == ArenaEnemyDifficulty.Hard ? 4 : 3);
        int powerReward = Mathf.RoundToInt(enemyPower / 260f);

        return Mathf.Max(baseReward + levelReward + powerReward, 8);
    }

    private string BuildEnemyGearSummary(ArenaEnemyArchetype archetype, ItemRarity gearRarity)
    {
        return "Tier 1 " + gearRarity + " " + GetArchetypeLabel(archetype) + " gear style";
    }

    private string GetArchetypeLabel(ArenaEnemyArchetype archetype)
    {
        switch (archetype)
        {
            case ArenaEnemyArchetype.Gambler:
                return "Strength + Luck";
            case ArenaEnemyArchetype.Duelist:
                return "Agility + Reaction";
            default:
                return "Strength + Rage";
        }
    }

    private string BuildEnemyDescription(ArenaEnemyDifficulty difficulty, ArenaEnemyArchetype archetype)
    {
        string archetypeText = GetArchetypeLabel(archetype);

        switch (difficulty)
        {
            case ArenaEnemyDifficulty.Easy:
                return "A lower-ranked " + archetypeText + " challenger looking for a mistake to punish. Safer fight, lower reward.";
            case ArenaEnemyDifficulty.Hard:
                return "A dangerous " + archetypeText + " Arena favorite with enough force to punish weak stance choices. High risk, high reward.";
            default:
                return "A close " + archetypeText + " match for your current strength. A fair duel with solid Arena rewards.";
        }
    }

    private bool TryGetEnemy(int enemyIndex, out ArenaEnemyData enemy)
    {
        enemy = null;

        if (enemies == null || enemyIndex < 0 || enemyIndex >= enemies.Length)
        {
            Debug.LogWarning("ArenaWindowUI: Enemy index is not configured: " + enemyIndex);
            return false;
        }

        enemy = enemies[enemyIndex];
        return enemy != null;
    }

    private GameObject CreatePortraitPlaceholder(Transform parent, int enemyIndex, float minHeight, float preferredHeight)
    {
        GameObject portrait = CreateLayoutObject("PortraitPlaceholder", parent);
        Image portraitImage = portrait.AddComponent<Image>();
        portraitImage.color = portraitColor;
        AddOutline(portrait, darkBorderColor, new Vector2(3f, -3f));

        LayoutElement portraitElement = portrait.AddComponent<LayoutElement>();
        portraitElement.minHeight = minHeight;
        portraitElement.preferredHeight = preferredHeight;
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

        return portrait;
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
