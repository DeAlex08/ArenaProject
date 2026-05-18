using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ArenaWindowUI : MonoBehaviour
{
    private struct ArenaFightResult
    {
        public bool isVictory;
        public int finalPlayerPower;
        public int finalEnemyPower;
        public int expGained;
        public int tokensGained;
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
        public float critChance = 5f;
        public int baseExpReward = 25;
        public int tokenReward = 10;
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

    [Header("Enemy Previews")]
    [SerializeField] private ArenaEnemyData[] enemies =
    {
        new ArenaEnemyData
        {
            enemyName = "Grave Duelist",
            level = 9,
            combatPower = 1800,
            hp = 740,
            attack = 110,
            defense = 38,
            critChance = 7.5f,
            baseExpReward = 30,
            tokenReward = 10,
            description = "A silent blade from the old burial pits. Fast, disciplined, and difficult to read."
        },
        new ArenaEnemyData
        {
            enemyName = "Ash Knight",
            level = 12,
            combatPower = 3200,
            hp = 1180,
            attack = 165,
            defense = 72,
            critChance = 9f,
            baseExpReward = 45,
            tokenReward = 16,
            description = "A burned champion in cracked plate armor. Slow to start, brutal once close."
        },
        new ArenaEnemyData
        {
            enemyName = "Blood Champion",
            level = 15,
            combatPower = 6200,
            hp = 1680,
            attack = 240,
            defense = 104,
            critChance = 13.5f,
            baseExpReward = 70,
            tokenReward = 25,
            description = "A feared crowd favorite who turns every duel into a public execution."
        }
    };

    private bool isBuilt;
    private GameObject enemyInfoPanel;
    private TMP_Text infoTitleText;
    private TMP_Text infoPortraitMarkText;
    private TMP_Text infoLevelText;
    private TMP_Text infoPowerText;
    private TMP_Text infoHpText;
    private TMP_Text infoAttackText;
    private TMP_Text infoDefenseText;
    private TMP_Text infoCritText;
    private TMP_Text infoExpText;
    private TMP_Text infoTokenText;
    private TMP_Text infoDescriptionText;
    private Button infoFightButton;
    private GameObject resultPanel;
    private TMP_Text resultTitleText;
    private TMP_Text resultEnemyText;
    private TMP_Text resultPlayerPowerText;
    private TMP_Text resultEnemyPowerText;
    private TMP_Text resultExpText;
    private TMP_Text resultTokensText;
    private TMP_Text resultMessageText;
    private int selectedEnemyIndex = -1;

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
        HideEnemyInfo();
        HideResult();
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
        ShowResult(enemy, result);

        Debug.Log(
            "ArenaWindowUI: Fight result: " +
            (result.isVictory ? "Victory" : "Defeat") +
            " | Enemy: " +
            enemy.enemyName +
            " | Level: " + enemy.level +
            " | Power: " + enemy.combatPower +
            " | Player Final Power: " + result.finalPlayerPower +
            " | Enemy Final Power: " + result.finalEnemyPower +
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
        rootLayout.spacing = 22f;
        rootLayout.childAlignment = TextAnchor.UpperCenter;
        rootLayout.childControlWidth = true;
        rootLayout.childControlHeight = true;
        rootLayout.childForceExpandWidth = true;
        rootLayout.childForceExpandHeight = false;

        BuildHeader();
        BuildSummary();
        BuildEnemies();
        BuildEnemyInfoPanel();
        BuildResultPanel();
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
        cardElement.minHeight = 620f;
        cardElement.preferredHeight = 650f;

        CreatePortraitPlaceholder(card.transform, enemyIndex, 190f, 204f);

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
        rect.sizeDelta = new Vector2(780f, 720f);
        rect.pivot = new Vector2(0.5f, 0.5f);

        VerticalLayoutGroup layout = resultPanel.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(48, 48, 42, 46);
        layout.spacing = 18f;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        resultTitleText = CreateText("ResultTitleText", resultPanel.transform, "", 44, FontStyles.Bold, TextAlignmentOptions.Center);
        resultTitleText.characterSpacing = 6f;
        resultTitleText.gameObject.GetComponent<LayoutElement>().preferredHeight = 72f;

        resultEnemyText = CreateResultLine("EnemyText");
        resultPlayerPowerText = CreateResultLine("PlayerPowerText");
        resultEnemyPowerText = CreateResultLine("EnemyPowerText");
        resultExpText = CreateResultLine("ExpText");
        resultTokensText = CreateResultLine("TokensText");

        resultMessageText = CreateText("MessageText", resultPanel.transform, "", 25, FontStyles.Normal, TextAlignmentOptions.Center);
        resultMessageText.color = mutedTextColor;
        resultMessageText.gameObject.GetComponent<LayoutElement>().preferredHeight = 100f;

        GameObject spacer = CreateLayoutObject("Spacer", resultPanel.transform);
        spacer.AddComponent<LayoutElement>().flexibleHeight = 1f;

        Button continueButton = CreateButton("ContinueButton", resultPanel.transform, "Continue", 30, new Vector2(0f, 78f), buttonColor);
        continueButton.gameObject.GetComponent<LayoutElement>().flexibleWidth = 1f;
        continueButton.onClick.AddListener(HideResult);

        HideResult();
    }

    private TMP_Text CreateResultLine(string objectName)
    {
        TMP_Text text = CreateText(objectName, resultPanel.transform, "", 27, FontStyles.Bold, TextAlignmentOptions.Center);
        text.color = textColor;
        text.gameObject.GetComponent<LayoutElement>().preferredHeight = 44f;
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
        infoAttackText.text = "Attack: " + enemy.attack;
        infoDefenseText.text = "Defense: " + enemy.defense;
        infoCritText.text = "Crit Chance: " + enemy.critChance.ToString("0.#") + "%";
        infoExpText.text = "EXP Reward: " + expReward;
        infoTokenText.text = "Arena Tokens: " + enemy.tokenReward;
        infoDescriptionText.text = enemy.description;

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
        int playerPower = GetPlayerPower();
        int finalPlayerPower = Mathf.RoundToInt(playerPower * Random.Range(0.9f, 1.1f));
        int finalEnemyPower = Mathf.RoundToInt(enemy.combatPower * Random.Range(0.9f, 1.1f));
        bool isVictory = finalPlayerPower >= finalEnemyPower;
        int calculatedExpReward = CalculateExpReward(enemy);

        ArenaFightResult result = new ArenaFightResult
        {
            isVictory = isVictory,
            finalPlayerPower = finalPlayerPower,
            finalEnemyPower = finalEnemyPower,
            expGained = isVictory ? calculatedExpReward : Mathf.RoundToInt(calculatedExpReward * 0.25f),
            tokensGained = isVictory ? enemy.tokenReward : 0
        };

        return result;
    }

    private void ShowResult(ArenaEnemyData enemy, ArenaFightResult result)
    {
        resultTitleText.text = result.isVictory ? "VICTORY" : "DEFEAT";
        resultTitleText.color = result.isVictory ? titleColor : new Color(0.74f, 0.18f, 0.13f, 1f);
        resultEnemyText.text = "Enemy: " + enemy.enemyName;
        resultPlayerPowerText.text = "Player Final Power: " + result.finalPlayerPower;
        resultEnemyPowerText.text = "Enemy Final Power: " + result.finalEnemyPower;
        resultExpText.text = "EXP Gained: " + result.expGained;
        resultTokensText.text = "Arena Tokens Gained: " + result.tokensGained;
        resultMessageText.text = result.isVictory
            ? "The crowd roars as your challenger falls. Rewards are ready for the next Arena progression pass."
            : "You survive the duel, but the Arena grants only a small lesson for this defeat.";

        resultPanel.SetActive(true);
    }

    private void HideResult()
    {
        if (resultPanel != null)
            resultPanel.SetActive(false);
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
