using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CombatPlaybackUI : MonoBehaviour
{
    private const string PlayerPoseResourceRoot = "Combat/FighterPoses/";
    private const string EnemyOrcPoseResourceRoot = "Combat/EnemyOrcPoses/";

    public class PlaybackData
    {
        public string playerName;
        public string enemyName;
        public CombatStance playerStance;
        public CombatStance enemyStance;
        public int playerStartHp;
        public int enemyStartHp;
        public List<CombatSimulator.CombatPlaybackEvent> events;
    }

    private bool isBuilt;
    private bool canSkip;
    private bool isFinishing;
    private Coroutine playbackRoutine;
    private Coroutine skipDelayRoutine;
    private Action onPlaybackComplete;

    private TMP_Text stageTitleText;
    private TMP_Text playerStageText;
    private TMP_Text enemyStageText;
    private TMP_Text playerStageHpText;
    private TMP_Text enemyNameText;
    private TMP_Text enemyHpText;
    private TMP_Text enemyMpText;
    private TMP_Text floatingText;
    private Button skipButton;
    private Image enemyPortraitImage;
    private Image enemyHpFill;
    private Image enemyMpFill;
    private CombatFighterPuppetUI playerPuppet;
    private CombatFighterPuppetUI enemyPuppet;
    private RectTransform playerFighterRect;
    private RectTransform enemyFighterRect;
    private RectTransform floatingTextRect;

    private int playerStartHp = 1;
    private int enemyStartHp = 1;
    private int currentPlayerHp = 1;
    private int currentEnemyHp = 1;

    private readonly Color panelColor = new Color(0.012f, 0.010f, 0.008f, 0.99f);
    private readonly Color stageColor = new Color(0.028f, 0.022f, 0.017f, 0.96f);
    private readonly Color cardColor = new Color(0.07f, 0.052f, 0.035f, 0.96f);
    private readonly Color portraitColor = new Color(0.035f, 0.032f, 0.030f, 1f);
    private readonly Color textColor = new Color(0.96f, 0.89f, 0.70f, 1f);
    private readonly Color titleColor = new Color(0.95f, 0.73f, 0.36f, 1f);
    private readonly Color mutedTextColor = new Color(0.70f, 0.63f, 0.50f, 1f);
    private readonly Color borderColor = new Color(0.50f, 0.34f, 0.18f, 1f);
    private readonly Color darkBorderColor = new Color(0.13f, 0.075f, 0.035f, 1f);
    private readonly Color buttonColor = new Color(0.22f, 0.14f, 0.07f, 1f);
    private readonly Color hitFlashColor = new Color(0.65f, 0.12f, 0.08f, 1f);
    private readonly Color dodgeColor = new Color(0.55f, 0.70f, 0.95f, 1f);
    private readonly Color blockColor = new Color(0.62f, 0.72f, 0.76f, 1f);

    private void Awake()
    {
        BuildIfNeeded();
        StopAndHide();
    }

    public void Play(PlaybackData playbackData, Action completed)
    {
        BuildIfNeeded();

        if (playbackData == null)
        {
            completed?.Invoke();
            return;
        }

        StopRunningCoroutines();

        onPlaybackComplete = completed;
        isFinishing = false;
        canSkip = false;
        gameObject.SetActive(true);

        ResetPlayback(playbackData);
        playbackRoutine = StartCoroutine(PlayRoutine(playbackData));
        skipDelayRoutine = StartCoroutine(EnableSkipAfterDelay());
    }

    public void StopAndHide()
    {
        StopRunningCoroutines();
        canSkip = false;
        isFinishing = false;
        onPlaybackComplete = null;

        if (skipButton != null)
            skipButton.gameObject.SetActive(false);

        gameObject.SetActive(false);
    }

    private void Skip()
    {
        if (!canSkip)
            return;

        FinishPlayback();
    }

    private IEnumerator PlayRoutine(PlaybackData playbackData)
    {
        List<CombatSimulator.CombatPlaybackEvent> events = playbackData.events ?? new List<CombatSimulator.CombatPlaybackEvent>();

        if (events.Count == 0)
        {
            yield return new WaitForSeconds(0.4f);
            FinishPlayback();
            yield break;
        }

        foreach (CombatSimulator.CombatPlaybackEvent playbackEvent in events)
        {
            if (isFinishing)
                yield break;

            yield return PlayEvent(playbackEvent);

            if (playbackEvent.eventType == CombatSimulator.CombatPlaybackEventType.Hit ||
                playbackEvent.eventType == CombatSimulator.CombatPlaybackEventType.Dodge)
            {
                SetSkipAvailable();
            }
        }

        yield return new WaitForSeconds(0.45f);
        FinishPlayback();
    }

    private IEnumerator PlayEvent(CombatSimulator.CombatPlaybackEvent playbackEvent)
    {
        if (playbackEvent == null)
            yield break;

        switch (playbackEvent.eventType)
        {
            case CombatSimulator.CombatPlaybackEventType.RoundStart:
                ShowFloatingText(playbackEvent.message, titleColor, null);
                yield return new WaitForSeconds(0.32f);
                break;

            case CombatSimulator.CombatPlaybackEventType.RoundEnd:
                yield return AnimateHpBars(playbackEvent.playerHp, playbackEvent.enemyHp, 0.16f);
                yield return AnimateDeathsIfNeeded(playbackEvent.playerHp, playbackEvent.enemyHp);
                yield return new WaitForSeconds(0.12f);
                break;

            case CombatSimulator.CombatPlaybackEventType.Dodge:
                yield return AnimateAttack(playbackEvent.sourceIsPlayer);
                yield return AnimateDodge(playbackEvent.targetIsPlayer);
                ShowFloatingText(BuildFloatingText(playbackEvent), dodgeColor, GetFighterRect(playbackEvent.targetIsPlayer));
                yield return new WaitForSeconds(0.22f);
                break;

            default:
                yield return AnimateAttack(playbackEvent.sourceIsPlayer);
                if (playbackEvent.wasBlocked)
                    yield return AnimateBlock(playbackEvent.targetIsPlayer);

                ShowFloatingText(BuildFloatingText(playbackEvent), GetFloatingTextColor(playbackEvent), GetFighterRect(playbackEvent.targetIsPlayer));
                yield return AnimateHpBars(playbackEvent.playerHp, playbackEvent.enemyHp, 0.22f);
                yield return AnimateHit(playbackEvent.targetIsPlayer, playbackEvent.wasCrit);
                yield return new WaitForSeconds(0.24f);
                break;
        }
    }

    private void FinishPlayback()
    {
        if (isFinishing)
            return;

        isFinishing = true;
        StopRunningCoroutines();
        gameObject.SetActive(false);

        Action completed = onPlaybackComplete;
        onPlaybackComplete = null;
        completed?.Invoke();
    }

    private IEnumerator EnableSkipAfterDelay()
    {
        yield return new WaitForSeconds(2f);
        SetSkipAvailable();
    }

    private void SetSkipAvailable()
    {
        if (canSkip)
            return;

        canSkip = true;

        if (skipButton != null)
            skipButton.gameObject.SetActive(true);
    }

    private void ResetPlayback(PlaybackData playbackData)
    {
        playerStartHp = Mathf.Max(playbackData.playerStartHp, 1);
        enemyStartHp = Mathf.Max(playbackData.enemyStartHp, 1);
        currentPlayerHp = playerStartHp;
        currentEnemyHp = enemyStartHp;

        stageTitleText.text = "ARENA DUEL";
        playerStageText.text = BuildFighterName(playbackData.playerName, playbackData.playerStance);
        enemyStageText.text = BuildFighterName(playbackData.enemyName, playbackData.enemyStance);
        enemyNameText.text = playbackData.enemyName;
        floatingText.text = "FIGHT";
        floatingText.color = titleColor;

        Canvas.ForceUpdateCanvases();
        playerPuppet.ResetPose();
        enemyPuppet.ResetPose();
        playerPuppet.CaptureCurrentPoseAsRest();
        enemyPuppet.CaptureCurrentPoseAsRest();

        SetHpBars(currentPlayerHp, currentEnemyHp);
        SetEnemyMpBar(1f);

        playerPuppet.ResetPose();
        enemyPuppet.ResetPose();
        enemyPortraitImage.color = portraitColor;

        if (skipButton != null)
            skipButton.gameObject.SetActive(false);
    }

    private string BuildFighterName(string fighterName, CombatStance stance)
    {
        string safeName = string.IsNullOrEmpty(fighterName) ? "Fighter" : fighterName;
        return safeName + "\n" + stance;
    }

    private string BuildFloatingText(CombatSimulator.CombatPlaybackEvent playbackEvent)
    {
        if (playbackEvent.wasDodged)
            return playbackEvent.wasCounter ? "COUNTER DODGE" : "DODGE";

        string text = "-" + playbackEvent.damage;

        if (playbackEvent.wasCrit)
            text += " CRIT";

        if (playbackEvent.wasBlocked)
            text += " BLOCK";

        if (playbackEvent.wasCounter)
            text = "COUNTER " + text;

        return text;
    }

    private Color GetFloatingTextColor(CombatSimulator.CombatPlaybackEvent playbackEvent)
    {
        if (playbackEvent.wasCrit)
            return titleColor;

        if (playbackEvent.wasBlocked)
            return blockColor;

        return textColor;
    }

    private RectTransform GetFighterRect(bool isPlayer)
    {
        return isPlayer ? playerFighterRect : enemyFighterRect;
    }

    private void ShowFloatingText(string textValue, Color color, RectTransform target)
    {
        if (floatingText == null)
            return;

        floatingText.text = textValue;
        floatingText.color = color;

        if (floatingTextRect == null)
            return;

        floatingTextRect.anchoredPosition = target == null
            ? new Vector2(0f, 150f)
            : new Vector2(target.anchoredPosition.x, target.anchoredPosition.y + 185f);
    }

    private IEnumerator AnimateAttack(bool attackerIsPlayer)
    {
        CombatFighterPuppetUI attacker = GetPuppet(attackerIsPlayer);

        if (attacker == null)
            yield break;

        yield return attacker.PlayAttack();
    }

    private IEnumerator AnimateBlock(bool targetIsPlayer)
    {
        CombatFighterPuppetUI target = GetPuppet(targetIsPlayer);

        if (target == null)
            yield break;

        yield return target.PlayBlock();
    }

    private IEnumerator AnimateDodge(bool targetIsPlayer)
    {
        CombatFighterPuppetUI target = GetPuppet(targetIsPlayer);

        if (target == null)
            yield break;

        yield return target.PlayDodge();
    }

    private IEnumerator AnimateHit(bool targetIsPlayer, bool isCrit)
    {
        CombatFighterPuppetUI target = GetPuppet(targetIsPlayer);

        if (target == null)
            yield break;

        if (!targetIsPlayer && enemyPortraitImage != null)
            enemyPortraitImage.color = isCrit ? titleColor : hitFlashColor;

        yield return target.PlayHit(isCrit);

        if (!targetIsPlayer && enemyPortraitImage != null)
            enemyPortraitImage.color = portraitColor;
    }

    private IEnumerator AnimateDeathsIfNeeded(int playerHp, int enemyHp)
    {
        bool playedDeath = false;

        if (playerHp <= 0 && playerPuppet != null)
        {
            yield return playerPuppet.PlayDeath();
            playedDeath = true;
        }

        if (enemyHp <= 0 && enemyPuppet != null)
        {
            yield return enemyPuppet.PlayDeath();
            playedDeath = true;
        }

        if (playedDeath)
            yield return new WaitForSeconds(0.15f);
    }

    private CombatFighterPuppetUI GetPuppet(bool isPlayer)
    {
        return isPlayer ? playerPuppet : enemyPuppet;
    }

    private IEnumerator AnimateHpBars(int targetPlayerHp, int targetEnemyHp, float duration)
    {
        int startPlayerHp = currentPlayerHp;
        int startEnemyHp = currentEnemyHp;
        targetPlayerHp = Mathf.Clamp(targetPlayerHp, 0, playerStartHp);
        targetEnemyHp = Mathf.Clamp(targetEnemyHp, 0, enemyStartHp);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = duration > 0f ? Mathf.Clamp01(elapsed / duration) : 1f;
            int nextPlayerHp = Mathf.RoundToInt(Mathf.Lerp(startPlayerHp, targetPlayerHp, t));
            int nextEnemyHp = Mathf.RoundToInt(Mathf.Lerp(startEnemyHp, targetEnemyHp, t));
            SetHpBars(nextPlayerHp, nextEnemyHp);
            yield return null;
        }

        currentPlayerHp = targetPlayerHp;
        currentEnemyHp = targetEnemyHp;
        SetHpBars(currentPlayerHp, currentEnemyHp);
    }

    private void SetHpBars(int playerHp, int enemyHp)
    {
        currentPlayerHp = Mathf.Clamp(playerHp, 0, playerStartHp);
        currentEnemyHp = Mathf.Clamp(enemyHp, 0, enemyStartHp);

        if (playerStageHpText != null)
            playerStageHpText.text = "HP " + currentPlayerHp + " / " + playerStartHp;

        float enemyFill = enemyStartHp > 0 ? (float)currentEnemyHp / enemyStartHp : 0f;

        if (enemyHpFill != null)
            enemyHpFill.fillAmount = Mathf.Clamp01(enemyFill);

        if (enemyHpText != null)
            enemyHpText.text = currentEnemyHp + " / " + enemyStartHp;
    }

    private void SetEnemyMpBar(float fillAmount)
    {
        if (enemyMpFill != null)
            enemyMpFill.fillAmount = Mathf.Clamp01(fillAmount);

        if (enemyMpText != null)
            enemyMpText.text = "100 / 100";
    }

    private void StopRunningCoroutines()
    {
        if (playbackRoutine != null)
        {
            StopCoroutine(playbackRoutine);
            playbackRoutine = null;
        }

        if (skipDelayRoutine != null)
        {
            StopCoroutine(skipDelayRoutine);
            skipDelayRoutine = null;
        }
    }

    private void BuildIfNeeded()
    {
        if (isBuilt)
            return;

        isBuilt = true;

        RectTransform rootRect = GetComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0.5f, 0.5f);
        rootRect.anchorMax = new Vector2(0.5f, 0.5f);
        rootRect.anchoredPosition = Vector2.zero;
        rootRect.sizeDelta = new Vector2(1470f, 1080f);
        rootRect.pivot = new Vector2(0.5f, 0.5f);

        Image background = gameObject.GetComponent<Image>();
        if (background == null)
            background = gameObject.AddComponent<Image>();

        background.color = panelColor;
        AddOutline(gameObject, borderColor, new Vector2(2f, -2f));

        HorizontalLayoutGroup layout = gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(34, 34, 34, 34);
        layout.spacing = 30f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;

        BuildBattlefield();
        BuildEnemyPanel();
    }

    private void BuildBattlefield()
    {
        GameObject stage = CreateLayoutObject("BattlefieldStage", transform);
        Image stageImage = stage.AddComponent<Image>();
        stageImage.color = stageColor;
        AddOutline(stage, darkBorderColor, new Vector2(2f, -2f));

        LayoutElement stageElement = stage.AddComponent<LayoutElement>();
        stageElement.minWidth = 900f;
        stageElement.flexibleWidth = 1f;

        stageTitleText = CreateText("StageTitle", stage.transform, "ARENA DUEL", 40, FontStyles.Bold, TextAlignmentOptions.Center);
        stageTitleText.color = titleColor;
        AnchorTo(stageTitleText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -36f), new Vector2(540f, 70f));

        skipButton = CreateButton("SkipButton", stage.transform, "Skip", 24, new Vector2(150f, 56f), buttonColor);
        skipButton.onClick.AddListener(Skip);
        RectTransform skipRect = skipButton.GetComponent<RectTransform>();
        AnchorTo(skipRect, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-92f, -42f), new Vector2(150f, 56f));
        skipButton.gameObject.SetActive(false);

        playerFighterRect = BuildStageFighter(stage.transform, "PlayerFighterRoot", true);
        enemyFighterRect = BuildStageFighter(stage.transform, "EnemyFighterRoot", false);

        playerStageText = CreateText("PlayerStageName", stage.transform, "", 25, FontStyles.Bold, TextAlignmentOptions.Center);
        playerStageText.color = textColor;
        AnchorTo(playerStageText.rectTransform, new Vector2(0.28f, 0.18f), new Vector2(0.28f, 0.18f), Vector2.zero, new Vector2(260f, 80f));

        enemyStageText = CreateText("EnemyStageName", stage.transform, "", 25, FontStyles.Bold, TextAlignmentOptions.Center);
        enemyStageText.color = textColor;
        AnchorTo(enemyStageText.rectTransform, new Vector2(0.72f, 0.18f), new Vector2(0.72f, 0.18f), Vector2.zero, new Vector2(260f, 80f));

        playerStageHpText = CreateText("PlayerStageHp", stage.transform, "", 23, FontStyles.Bold, TextAlignmentOptions.Center);
        playerStageHpText.color = mutedTextColor;
        AnchorTo(playerStageHpText.rectTransform, new Vector2(0.28f, 0.10f), new Vector2(0.28f, 0.10f), Vector2.zero, new Vector2(250f, 54f));

        floatingText = CreateText("FloatingText", stage.transform, "", 38, FontStyles.Bold, TextAlignmentOptions.Center);
        floatingTextRect = floatingText.rectTransform;
        AnchorTo(floatingTextRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 150f), new Vector2(430f, 94f));

        CreateStageFloor(stage.transform);
    }

    private RectTransform BuildStageFighter(Transform parent, string objectName, bool isPlayer)
    {
        GameObject fighter = CreateLayoutObject(objectName, parent);

        RectTransform rect = fighter.GetComponent<RectTransform>();
        AnchorTo(rect, new Vector2(isPlayer ? 0.28f : 0.72f, 0.48f), new Vector2(isPlayer ? 0.28f : 0.72f, 0.48f), Vector2.zero, new Vector2(220f, 360f));

        CombatFighterPuppetUI puppet = fighter.AddComponent<CombatFighterPuppetUI>();
        Color bodyColor = isPlayer
            ? new Color(0.13f, 0.12f, 0.11f, 1f)
            : new Color(0.16f, 0.09f, 0.075f, 1f);
        Color accentColor = isPlayer
            ? new Color(0.19f, 0.17f, 0.14f, 1f)
            : new Color(0.24f, 0.12f, 0.10f, 1f);
        Color bladeColor = isPlayer
            ? new Color(0.48f, 0.40f, 0.30f, 1f)
            : new Color(0.56f, 0.28f, 0.17f, 1f);

        string poseResourceRoot = isPlayer ? PlayerPoseResourceRoot : EnemyOrcPoseResourceRoot;
        puppet.Initialize(isPlayer, bodyColor, accentColor, bladeColor, poseResourceRoot);

        if (isPlayer)
            playerPuppet = puppet;
        else
            enemyPuppet = puppet;

        return rect;
    }

    private void CreateStageFloor(Transform parent)
    {
        GameObject floor = CreateLayoutObject("StageFloor", parent);
        Image floorImage = floor.AddComponent<Image>();
        floorImage.color = new Color(0.10f, 0.075f, 0.045f, 0.9f);

        RectTransform rect = floor.GetComponent<RectTransform>();
        AnchorTo(rect, new Vector2(0.5f, 0.28f), new Vector2(0.5f, 0.28f), Vector2.zero, new Vector2(720f, 18f));
        floor.transform.SetAsFirstSibling();
    }

    private void BuildEnemyPanel()
    {
        GameObject panel = CreateLayoutObject("EnemyPanel", transform);
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = cardColor;
        AddOutline(panel, borderColor, new Vector2(2f, -2f));

        VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(20, 20, 20, 22);
        layout.spacing = 12f;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        LayoutElement element = panel.AddComponent<LayoutElement>();
        element.minWidth = 360f;
        element.preferredWidth = 380f;
        element.flexibleWidth = 0f;

        enemyNameText = CreateText("EnemyName", panel.transform, "", 30, FontStyles.Bold, TextAlignmentOptions.Center);
        enemyNameText.color = titleColor;
        enemyNameText.gameObject.GetComponent<LayoutElement>().preferredHeight = 70f;

        GameObject portrait = CreatePanelBox("EnemyPortrait", panel.transform, 410f);
        enemyPortraitImage = portrait.GetComponent<Image>();
        TMP_Text portraitMark = CreateText("Mark", portrait.transform, "ENEMY", 42, FontStyles.Bold, TextAlignmentOptions.Center);
        portraitMark.color = new Color(0.42f, 0.31f, 0.20f, 0.9f);
        AnchorTo(portraitMark.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        BuildWeaponSlots(panel.transform);
        BuildResourceBar(panel.transform, "HP", true);
        BuildResourceBar(panel.transform, "MP", false);
    }

    private void BuildWeaponSlots(Transform parent)
    {
        GameObject slots = CreateLayoutObject("WeaponSlots", parent);
        HorizontalLayoutGroup layout = slots.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 12f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        slots.AddComponent<LayoutElement>().preferredHeight = 118f;

        CreateWeaponSlot(slots.transform, "I");
        CreateWeaponSlot(slots.transform, "II");
    }

    private void CreateWeaponSlot(Transform parent, string label)
    {
        GameObject slot = CreatePanelBox("WeaponSlot" + label, parent, 108f);
        slot.GetComponent<LayoutElement>().flexibleWidth = 1f;

        TMP_Text mark = CreateText("Mark", slot.transform, label, 34, FontStyles.Bold, TextAlignmentOptions.Center);
        mark.color = mutedTextColor;
        AnchorTo(mark.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
    }

    private void BuildResourceBar(Transform parent, string label, bool isHp)
    {
        GameObject row = CreateLayoutObject(label + "Row", parent);
        HorizontalLayoutGroup rowLayout = row.AddComponent<HorizontalLayoutGroup>();
        rowLayout.spacing = 10f;
        rowLayout.childAlignment = TextAnchor.MiddleCenter;
        rowLayout.childControlWidth = true;
        rowLayout.childControlHeight = true;
        rowLayout.childForceExpandWidth = false;
        rowLayout.childForceExpandHeight = false;
        row.AddComponent<LayoutElement>().preferredHeight = 48f;

        TMP_Text labelText = CreateText(label + "Label", row.transform, label, 24, FontStyles.Bold, TextAlignmentOptions.Center);
        labelText.color = titleColor;
        LayoutElement labelElement = labelText.gameObject.GetComponent<LayoutElement>();
        labelElement.minWidth = 48f;
        labelElement.preferredWidth = 48f;

        GameObject bar = CreateLayoutObject(label + "Bar", row.transform);
        Image barBackground = bar.AddComponent<Image>();
        barBackground.color = new Color(0.018f, 0.012f, 0.01f, 1f);
        AddOutline(bar, darkBorderColor, new Vector2(2f, -2f));
        LayoutElement barElement = bar.AddComponent<LayoutElement>();
        barElement.preferredWidth = 220f;
        barElement.flexibleWidth = 1f;
        barElement.preferredHeight = 42f;

        GameObject fill = CreateLayoutObject("Fill", bar.transform);
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = isHp
            ? new Color(0.68f, 0.04f, 0.03f, 1f)
            : new Color(0.02f, 0.18f, 0.72f, 1f);
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        fillImage.fillAmount = 1f;
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = new Vector2(5f, 5f);
        fillRect.offsetMax = new Vector2(-5f, -5f);

        TMP_Text valueText = CreateText("Value", bar.transform, "", 21, FontStyles.Bold, TextAlignmentOptions.Center);
        valueText.color = textColor;
        AnchorTo(valueText.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        Destroy(valueText.GetComponent<LayoutElement>());

        if (isHp)
        {
            enemyHpFill = fillImage;
            enemyHpText = valueText;
        }
        else
        {
            enemyMpFill = fillImage;
            enemyMpText = valueText;
        }
    }

    private GameObject CreatePanelBox(string objectName, Transform parent, float preferredHeight)
    {
        GameObject box = CreateLayoutObject(objectName, parent);
        Image image = box.AddComponent<Image>();
        image.color = portraitColor;
        AddOutline(box, darkBorderColor, new Vector2(2f, -2f));

        LayoutElement element = box.AddComponent<LayoutElement>();
        element.minHeight = preferredHeight;
        element.preferredHeight = preferredHeight;

        return box;
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
        textElement.minHeight = fontSize + 18f;
        textElement.preferredHeight = fontSize + 22f;

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
        ColorBlock colors = button.colors;
        colors.normalColor = backgroundColor;
        colors.highlightedColor = Color.Lerp(backgroundColor, titleColor, 0.18f);
        colors.pressedColor = new Color(0.34f, 0.22f, 0.11f, 1f);
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
        AnchorTo(text.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        Destroy(text.GetComponent<LayoutElement>());

        return button;
    }

    private void AnchorTo(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;

        if (anchorMin == Vector2.zero && anchorMax == Vector2.one)
        {
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = sizeDelta;
        }
    }

    private void AddOutline(GameObject target, Color color, Vector2 distance)
    {
        Outline outline = target.AddComponent<Outline>();
        outline.effectColor = color;
        outline.effectDistance = distance;
        outline.useGraphicAlpha = true;
    }
}
