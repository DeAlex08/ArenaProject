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
    private const string EnemyPanelFrameResourcePath = "Combat/UI/EnemyPanelFrame";
    private const string EnemyOrcPortraitResourcePath = "Combat/UI/EnemyOrcPortrait";
    private const string ArenaBattlefieldBackgroundResourcePath = "Combat/UI/ArenaBattlefieldBackground";
    private const float EnemyPanelWidth = 420f;
    private const float EnemyPanelFrameHeight = 960f;
    private const float EnemyPortraitSize = 382f;
    private const float EnemyResourceBarWidth = 274f;
    private const float EnemyResourceBarHeight = 30f;
    private const float EnemyResourceBarPaddingX = 7f;
    private const float EnemyResourceBarPaddingY = 6f;

    [Header("Combat Audio Hooks")]
    [SerializeField] private AudioClip slashClip;
    [SerializeField] private AudioClip hitClip;
    [SerializeField] private AudioClip critClip;
    [SerializeField] private AudioClip blockClip;
    [SerializeField] private AudioClip dodgeClip;
    [SerializeField, Range(0f, 1f)] private float slashVolume = 0.42f;
    [SerializeField, Range(0f, 1f)] private float hitVolume = 0.48f;
    [SerializeField, Range(0f, 1f)] private float critVolume = 0.55f;
    [SerializeField, Range(0f, 1f)] private float blockVolume = 0.44f;
    [SerializeField, Range(0f, 1f)] private float dodgeVolume = 0.34f;

    public class PlaybackData
    {
        public string playerName;
        public string enemyName;
        public CombatStance playerStance;
        public CombatStance enemyStance;
        public int enemyLevel;
        public int enemyCombatPower;
        public int playerStartHp;
        public int enemyStartHp;
        public List<CombatSimulator.CombatPlaybackEvent> events;
        public PlayerStats playerStats;
    }

    private class RoundPlaybackGroup
    {
        public CombatSimulator.CombatPlaybackEvent roundStart;
        public readonly List<CombatSimulator.CombatPlaybackEvent> exchangeEvents = new List<CombatSimulator.CombatPlaybackEvent>();
        public CombatSimulator.CombatPlaybackEvent roundEnd;
    }

    private bool isBuilt;
    private bool canSkip;
    private bool isFinishing;
    private bool enemyPanelUsesFrame;
    private Coroutine playbackRoutine;
    private Coroutine skipDelayRoutine;
    private Coroutine shakeRoutine;
    private Coroutine hitStopRoutine;
    private Coroutine floatingTextRoutine;
    private Coroutine floatingTextSecondaryRoutine;
    private Action onPlaybackComplete;
    private readonly List<Coroutine> parallelRoutines = new List<Coroutine>();
    private Color enemyPortraitDefaultColor = Color.clear;

    private TMP_Text stageTitleText;
    private TMP_Text playerStageText;
    private TMP_Text enemyStageText;
    private TMP_Text playerStageHpText;
    private TMP_Text enemyNameText;
    private TMP_Text enemyLevelText;
    private TMP_Text enemyPowerText;
    private TMP_Text enemyHpText;
    private TMP_Text enemyMpText;
    private TMP_Text floatingText;
    private TMP_Text floatingTextSecondary;
    private Button skipButton;
    private Image critScreenFlashImage;
    private Image enemyPortraitImage;
    private Image enemyHpFill;
    private Image enemyMpFill;
    private Image slashVfxImage;
    private Image impactVfxImage;
    private CombatFighterPuppetUI playerPuppet;
    private CombatFighterPuppetUI enemyPuppet;
    private RectTransform playerFighterRect;
    private RectTransform enemyFighterRect;
    private RectTransform playbackRootRect;
    private RectTransform critScreenFlashRect;
    private RectTransform enemyHpFillRect;
    private RectTransform enemyMpFillRect;
    private RectTransform slashVfxRect;
    private RectTransform impactVfxRect;
    private RectTransform floatingTextRect;
    private RectTransform floatingTextSecondaryRect;
    private PlayerStats playbackPlayerStats;
    private AudioSource slashAudioSource;
    private AudioSource hitAudioSource;
    private AudioSource critAudioSource;
    private AudioSource blockAudioSource;
    private AudioSource dodgeAudioSource;
    private float slashAudioVolume;
    private float hitAudioVolume;
    private float critAudioVolume;
    private float blockAudioVolume;
    private float dodgeAudioVolume;

    private int playerStartHp = 1;
    private int enemyStartHp = 1;
    private int currentPlayerHp = 1;
    private int currentEnemyHp = 1;
    private int originalPlayerStatsHp;
    private float savedTimeScale = 1f;
    private bool hasOriginalPlayerStatsHp;
    private bool hasActiveHitStop;

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
    private readonly Color critColor = new Color(0.95f, 0.18f, 0.12f, 1f);
    private readonly Color luckyColor = new Color(0.35f, 0.62f, 1f, 1f);
    private readonly Color luckyCritColor = new Color(0.72f, 0.36f, 1f, 1f);
    private readonly Color hudTextColor = new Color(0.847f, 0.788f, 0.639f, 1f);

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
        RestorePlaybackPlayerHp();

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
        RestorePlaybackPlayerHp();
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
        List<RoundPlaybackGroup> rounds = BuildRoundGroups(events);

        if (events.Count == 0)
        {
            yield return new WaitForSeconds(0.4f);
            FinishPlayback();
            yield break;
        }

        if (rounds.Count == 0)
        {
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
        }
        else
        {
            foreach (RoundPlaybackGroup round in rounds)
            {
                if (isFinishing)
                    yield break;

                yield return PlayRound(round);
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
        RestorePlaybackPlayerHp();
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
        playbackPlayerStats = playbackData.playerStats;
        hasOriginalPlayerStatsHp = playbackPlayerStats != null;

        if (hasOriginalPlayerStatsHp)
            originalPlayerStatsHp = playbackPlayerStats.currentHp;

        stageTitleText.text = "ARENA DUEL";
        playerStageText.text = BuildFighterName(playbackData.playerName, playbackData.playerStance);
        enemyStageText.text = BuildFighterName(playbackData.enemyName, playbackData.enemyStance);
        enemyNameText.text = playbackData.enemyName;

        if (enemyLevelText != null)
            enemyLevelText.text = "LVL " + Mathf.Max(playbackData.enemyLevel, 1);

        if (enemyPowerText != null)
            enemyPowerText.text = Mathf.Max(playbackData.enemyCombatPower, 0).ToString("N0").Replace(",", " ");

        floatingText.text = "FIGHT";
        floatingText.color = titleColor;

        if (floatingTextSecondary != null)
            floatingTextSecondary.text = "";

        Canvas.ForceUpdateCanvases();
        playerPuppet.ResetPose();
        enemyPuppet.ResetPose();
        playerPuppet.CaptureCurrentPoseAsRest();
        enemyPuppet.CaptureCurrentPoseAsRest();

        SetHpBars(currentPlayerHp, currentEnemyHp);
        SetEnemyMpBar(1f);

        playerPuppet.ResetPose();
        enemyPuppet.ResetPose();

        if (enemyPortraitImage != null)
            enemyPortraitImage.color = enemyPortraitDefaultColor;

        if (skipButton != null)
            skipButton.gameObject.SetActive(false);

        ClearTransientVisuals();
    }

    private string BuildFighterName(string fighterName, CombatStance stance)
    {
        string safeName = string.IsNullOrEmpty(fighterName) ? "Fighter" : fighterName;
        return safeName + "\n" + stance;
    }

    private string BuildFloatingText(CombatSimulator.CombatPlaybackEvent playbackEvent)
    {
        if (playbackEvent.wasDodged)
            return WrapFloatingTextColor(playbackEvent, playbackEvent.wasCounter ? "COUNTER DODGE" : "DODGE");

        string text = playbackEvent.damage > 0 ? "-" + playbackEvent.damage : "0";

        if (playbackEvent.wasLuckyCrit)
            text += " LUCKY CRIT";
        else if (playbackEvent.wasLucky)
            text += " LUCKY";
        else if (playbackEvent.wasCrit)
            text += " CRIT";

        if (playbackEvent.wasBlocked)
            text = playbackEvent.damage > 0 ? text + " BLOCK" : "BLOCK";

        if (playbackEvent.wasCounter)
            text = "COUNTER " + text;

        return WrapFloatingTextColor(playbackEvent, text);
    }

    private string WrapFloatingTextColor(CombatSimulator.CombatPlaybackEvent playbackEvent, string text)
    {
        if (playbackEvent == null || string.IsNullOrEmpty(text))
            return text;

        return "<color=#" + GetFloatingTextHexColor(playbackEvent) + ">" + text + "</color>";
    }

    private string GetFloatingTextHexColor(CombatSimulator.CombatPlaybackEvent playbackEvent)
    {
        if (playbackEvent.wasLuckyCrit)
            return "B85CFF";

        if (playbackEvent.wasLucky)
            return "599EFF";

        if (playbackEvent.wasCrit)
            return "F22E1F";

        if (playbackEvent.wasDodged)
            return "8CB2F2";

        if (playbackEvent.wasBlocked)
            return "9EB8C2";

        return "F5E3B3";
    }

    private Color GetFloatingTextColor(CombatSimulator.CombatPlaybackEvent playbackEvent)
    {
        if (playbackEvent.wasLuckyCrit)
            return luckyCritColor;

        if (playbackEvent.wasLucky)
            return luckyColor;

        if (playbackEvent.wasCrit)
            return critColor;

        if (playbackEvent.wasBlocked)
            return blockColor;

        return textColor;
    }

    private RectTransform GetFighterRect(bool isPlayer)
    {
        return isPlayer ? playerFighterRect : enemyFighterRect;
    }

    private List<RoundPlaybackGroup> BuildRoundGroups(List<CombatSimulator.CombatPlaybackEvent> events)
    {
        List<RoundPlaybackGroup> rounds = new List<RoundPlaybackGroup>();
        RoundPlaybackGroup currentRound = null;

        foreach (CombatSimulator.CombatPlaybackEvent playbackEvent in events)
        {
            if (playbackEvent == null)
                continue;

            switch (playbackEvent.eventType)
            {
                case CombatSimulator.CombatPlaybackEventType.RoundStart:
                    if (currentRound != null)
                        rounds.Add(currentRound);

                    currentRound = new RoundPlaybackGroup { roundStart = playbackEvent };
                    break;

                case CombatSimulator.CombatPlaybackEventType.RoundEnd:
                    if (currentRound == null)
                        currentRound = new RoundPlaybackGroup();

                    currentRound.roundEnd = playbackEvent;
                    rounds.Add(currentRound);
                    currentRound = null;
                    break;

                case CombatSimulator.CombatPlaybackEventType.Hit:
                case CombatSimulator.CombatPlaybackEventType.Dodge:
                    if (currentRound == null)
                        currentRound = new RoundPlaybackGroup();

                    currentRound.exchangeEvents.Add(playbackEvent);
                    break;
            }
        }

        if (currentRound != null)
            rounds.Add(currentRound);

        return rounds;
    }

    private IEnumerator PlayRound(RoundPlaybackGroup round)
    {
        if (round.roundStart != null)
        {
            ShowFloatingText(round.roundStart.message, titleColor, null);
            yield return new WaitForSeconds(0.32f);
        }

        if (round.exchangeEvents.Count > 0)
        {
            yield return PlayRoundExchange(round.exchangeEvents);
            SetSkipAvailable();
        }

        if (round.roundEnd != null)
        {
            yield return AnimateHpBars(round.roundEnd.playerHp, round.roundEnd.enemyHp, 0.20f);
            yield return AnimateDeathsIfNeeded(round.roundEnd.playerHp, round.roundEnd.enemyHp);
        }

        yield return new WaitForSeconds(0.14f);
    }

    private IEnumerator PlayRoundExchange(List<CombatSimulator.CombatPlaybackEvent> exchangeEvents)
    {
        bool playerAttacks = HasSourceAction(exchangeEvents, true);
        bool enemyAttacks = HasSourceAction(exchangeEvents, false);
        bool playerDodges = HasTargetEvent(exchangeEvents, true, e => e.wasDodged);
        bool enemyDodges = HasTargetEvent(exchangeEvents, false, e => e.wasDodged);
        bool playerBlocks = HasTargetEvent(exchangeEvents, true, e => e.wasBlocked);
        bool enemyBlocks = HasTargetEvent(exchangeEvents, false, e => e.wasBlocked);
        bool playerHit = HasTargetEvent(exchangeEvents, true, e => e.eventType == CombatSimulator.CombatPlaybackEventType.Hit && !e.wasDodged);
        bool enemyHit = HasTargetEvent(exchangeEvents, false, e => e.eventType == CombatSimulator.CombatPlaybackEventType.Hit && !e.wasDodged);
        bool playerCrit = HasTargetEvent(exchangeEvents, true, e => e.wasCrit);
        bool enemyCrit = HasTargetEvent(exchangeEvents, false, e => e.wasCrit);

        List<IEnumerator> primaryActions = new List<IEnumerator>();
        AddPrimaryRoundAction(primaryActions, true, playerAttacks, playerDodges, playerBlocks, playerHit, playerCrit);
        AddPrimaryRoundAction(primaryActions, false, enemyAttacks, enemyDodges, enemyBlocks, enemyHit, enemyCrit);

        if (primaryActions.Count > 0)
            yield return RunParallel(primaryActions);

        ShowRoundFloatingTexts(exchangeEvents);

        List<IEnumerator> impactActions = new List<IEnumerator>();

        if (playerHit && playerAttacks && !playerDodges && !playerBlocks)
            impactActions.Add(AnimateHit(true, playerCrit));

        if (enemyHit && enemyAttacks && !enemyDodges && !enemyBlocks)
            impactActions.Add(AnimateHit(false, enemyCrit));

        if (impactActions.Count > 0)
            yield return RunParallel(impactActions);

        yield return new WaitForSeconds(0.18f);
    }

    private void AddPrimaryRoundAction(
        List<IEnumerator> actions,
        bool isPlayer,
        bool attacks,
        bool dodges,
        bool blocks,
        bool hit,
        bool crit)
    {
        if (dodges)
        {
            actions.Add(AnimateDodge(isPlayer));
            return;
        }

        if (blocks)
        {
            actions.Add(AnimateBlock(isPlayer));
            return;
        }

        if (attacks)
        {
            actions.Add(AnimateAttack(isPlayer));
            return;
        }

        if (hit)
            actions.Add(AnimateHit(isPlayer, crit));
    }

    private bool HasSourceAction(List<CombatSimulator.CombatPlaybackEvent> events, bool sourceIsPlayer)
    {
        return events.Exists(e => e != null && e.sourceIsPlayer == sourceIsPlayer);
    }

    private bool HasTargetEvent(
        List<CombatSimulator.CombatPlaybackEvent> events,
        bool targetIsPlayer,
        Predicate<CombatSimulator.CombatPlaybackEvent> predicate)
    {
        return events.Exists(e => e != null && e.targetIsPlayer == targetIsPlayer && predicate(e));
    }

    private IEnumerator RunParallel(List<IEnumerator> routines)
    {
        int remaining = routines.Count;

        foreach (IEnumerator routine in routines)
            parallelRoutines.Add(StartCoroutine(RunAndComplete(routine, () => remaining--)));

        while (remaining > 0)
            yield return null;

        parallelRoutines.Clear();
    }

    private IEnumerator RunAndComplete(IEnumerator routine, Action completed)
    {
        if (routine != null)
            yield return routine;

        completed?.Invoke();
    }

    private void ShowFloatingText(string textValue, Color color, RectTransform target)
    {
        if (floatingText == null)
            return;

        floatingText.text = textValue;
        floatingText.color = color;
        floatingText.alpha = 1f;
        floatingText.fontSize = IsHighImpactFloatingText(textValue) ? 44f : 38f;

        if (floatingTextRect == null)
            return;

        bool targetIsPlayer = target == playerFighterRect;
        floatingTextRect.anchoredPosition = target == null
            ? new Vector2(0f, 150f)
            : GetFloatingTextPosition(floatingTextRect, target, targetIsPlayer, 0);

        PlayFloatingTextMotion(floatingText, floatingTextRect, ref floatingTextRoutine);

        if (floatingTextSecondary != null)
            floatingTextSecondary.text = "";
    }

    private void ShowRoundFloatingTexts(List<CombatSimulator.CombatPlaybackEvent> exchangeEvents)
    {
        string playerText = BuildTargetFloatingText(exchangeEvents, true);
        string enemyText = BuildTargetFloatingText(exchangeEvents, false);

        SetFloatingText(floatingText, floatingTextRect, playerText, GetTargetFloatingTextColor(exchangeEvents, true), playerFighterRect, true, 0);
        SetFloatingText(floatingTextSecondary, floatingTextSecondaryRect, enemyText, GetTargetFloatingTextColor(exchangeEvents, false), enemyFighterRect, false, playerText.Length > 0 ? 1 : 0);
    }

    private void SetFloatingText(
        TMP_Text text,
        RectTransform rect,
        string value,
        Color color,
        RectTransform target,
        bool targetIsPlayer,
        int stackIndex)
    {
        if (text == null)
            return;

        text.text = value;
        text.color = color;
        text.alpha = 1f;
        text.fontSize = IsHighImpactFloatingText(value) ? 44f : 38f;

        if (rect == null || target == null)
            return;

        rect.anchoredPosition = GetFloatingTextPosition(rect, target, targetIsPlayer, stackIndex);

        if (text == floatingText)
            PlayFloatingTextMotion(text, rect, ref floatingTextRoutine);
        else if (text == floatingTextSecondary)
            PlayFloatingTextMotion(text, rect, ref floatingTextSecondaryRoutine);
    }

    private void PlayFloatingTextMotion(TMP_Text text, RectTransform rect, ref Coroutine routine)
    {
        if (text == null || rect == null || string.IsNullOrEmpty(text.text))
            return;

        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(AnimateFloatingText(text, rect));
    }

    private IEnumerator AnimateFloatingText(TMP_Text text, RectTransform rect)
    {
        Vector2 start = rect.anchoredPosition;
        Vector2 end = start + new Vector2(0f, 34f);
        float elapsed = 0f;
        const float duration = 0.48f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            rect.anchoredPosition = Vector2.Lerp(start, end, EaseOutCubic(t));
            text.alpha = Mathf.Lerp(1f, 0.35f, t);
            yield return null;
        }

        rect.anchoredPosition = end;
    }

    private Vector2 GetFloatingTextPosition(RectTransform textRect, RectTransform target, bool targetIsPlayer, int stackIndex)
    {
        if (target == null)
            return new Vector2(0f, 150f);

        RectTransform parentRect = textRect != null ? textRect.parent as RectTransform : null;
        Vector2 localPosition;

        if (parentRect != null)
        {
            Vector3 targetWorldPosition = target.TransformPoint(new Vector3(0f, target.rect.height * 0.28f, 0f));
            localPosition = parentRect.InverseTransformPoint(targetWorldPosition);
        }
        else
        {
            localPosition = target.anchoredPosition;
        }

        float sideOffset = targetIsPlayer ? -36f : 36f;
        float verticalOffset = 132f + stackIndex * 26f;
        Vector2 position = localPosition + new Vector2(sideOffset, verticalOffset);

        return ClampFloatingTextPosition(position, textRect, parentRect);
    }

    private Vector2 ClampFloatingTextPosition(Vector2 position, RectTransform textRect, RectTransform parentRect)
    {
        if (textRect == null || parentRect == null)
            return position;

        Vector2 textSize = textRect.rect.size;
        float halfTextWidth = Mathf.Max(textSize.x * 0.5f, 140f);
        float halfTextHeight = Mathf.Max(textSize.y * 0.5f, 42f);
        Rect parentBounds = parentRect.rect;
        float padding = 22f;

        position.x = Mathf.Clamp(
            position.x,
            parentBounds.xMin + halfTextWidth + padding,
            parentBounds.xMax - halfTextWidth - padding);

        position.y = Mathf.Clamp(
            position.y,
            parentBounds.yMin + halfTextHeight + padding,
            parentBounds.yMax - halfTextHeight - padding);

        return position;
    }

    private string BuildTargetFloatingText(List<CombatSimulator.CombatPlaybackEvent> events, bool targetIsPlayer)
    {
        List<string> parts = new List<string>();

        foreach (CombatSimulator.CombatPlaybackEvent playbackEvent in events)
        {
            if (playbackEvent == null || playbackEvent.targetIsPlayer != targetIsPlayer)
                continue;

            parts.Add(BuildFloatingText(playbackEvent));
        }

        return parts.Count > 0 ? string.Join("\n", parts) : "";
    }

    private Color GetTargetFloatingTextColor(List<CombatSimulator.CombatPlaybackEvent> events, bool targetIsPlayer)
    {
        if (HasTargetEvent(events, targetIsPlayer, e => e.wasLuckyCrit))
            return luckyCritColor;

        if (HasTargetEvent(events, targetIsPlayer, e => e.wasLucky))
            return luckyColor;

        if (HasTargetEvent(events, targetIsPlayer, e => e.wasCrit))
            return critColor;

        if (HasTargetEvent(events, targetIsPlayer, e => e.wasDodged))
            return dodgeColor;

        if (HasTargetEvent(events, targetIsPlayer, e => e.wasBlocked))
            return blockColor;

        return textColor;
    }

    private bool IsHighImpactFloatingText(string value)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        return value.Contains("CRIT") || value.Contains("LUCKY");
    }

    private IEnumerator AnimateAttack(bool attackerIsPlayer)
    {
        CombatFighterPuppetUI attacker = GetPuppet(attackerIsPlayer);

        if (attacker == null)
            yield break;

        PlayAudioHook(slashAudioSource, slashAudioVolume);
        StartCoroutine(PlaySlashVfx(attackerIsPlayer));
        yield return attacker.PlayAttack();
    }

    private IEnumerator AnimateBlock(bool targetIsPlayer)
    {
        CombatFighterPuppetUI target = GetPuppet(targetIsPlayer);

        if (target == null)
            yield break;

        PlayAudioHook(blockAudioSource, blockAudioVolume);
        StartCameraShake(0.045f, 3f);
        yield return target.PlayBlock();
    }

    private IEnumerator AnimateDodge(bool targetIsPlayer)
    {
        CombatFighterPuppetUI target = GetPuppet(targetIsPlayer);

        if (target == null)
            yield break;

        PlayAudioHook(dodgeAudioSource, dodgeAudioVolume);
        yield return target.PlayDodge();
    }

    private IEnumerator AnimateHit(bool targetIsPlayer, bool isCrit)
    {
        CombatFighterPuppetUI target = GetPuppet(targetIsPlayer);

        if (target == null)
            yield break;

        if (!targetIsPlayer && enemyPortraitImage != null)
            enemyPortraitImage.color = isCrit ? titleColor : hitFlashColor;

        PlayAudioHook(isCrit ? critAudioSource : hitAudioSource, isCrit ? critAudioVolume : hitAudioVolume);
        StartHitStop(isCrit ? 0.055f : 0.025f, isCrit ? 0.12f : 0.35f);
        StartCameraShake(isCrit ? 0.14f : 0.075f, isCrit ? 13f : 7f);

        if (isCrit)
            StartCoroutine(PlayCritScreenFlash());

        StartCoroutine(PlayImpactVfx(targetIsPlayer, isCrit));
        yield return target.PlayHit(isCrit);

        if (!targetIsPlayer && enemyPortraitImage != null)
            enemyPortraitImage.color = enemyPortraitDefaultColor;
    }

    private IEnumerator PlaySlashVfx(bool attackerIsPlayer)
    {
        if (slashVfxImage == null || slashVfxRect == null)
            yield break;

        RectTransform attackerRect = attackerIsPlayer ? playerFighterRect : enemyFighterRect;
        if (attackerRect == null)
            yield break;

        Vector2 startPosition = GetStageLocalPoint(slashVfxRect, attackerRect, new Vector2(attackerIsPlayer ? 0.28f : -0.28f, 0.24f));
        Vector2 endPosition = startPosition + new Vector2(attackerIsPlayer ? 95f : -95f, 20f);
        float angle = attackerIsPlayer ? -18f : 18f;

        slashVfxRect.sizeDelta = new Vector2(170f, 18f);
        slashVfxRect.localEulerAngles = new Vector3(0f, 0f, angle);
        slashVfxImage.color = new Color(1f, 0.86f, 0.34f, 0.78f);

        float elapsed = 0f;
        const float duration = 0.18f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            slashVfxRect.anchoredPosition = Vector2.Lerp(startPosition, endPosition, t);
            slashVfxRect.sizeDelta = Vector2.Lerp(new Vector2(82f, 20f), new Vector2(230f, 42f), EaseOutCubic(t));
            slashVfxImage.color = new Color(1f, 0.86f, 0.34f, Mathf.Lerp(0.82f, 0f, t));
            yield return null;
        }

        slashVfxImage.color = Color.clear;
        slashVfxRect.anchoredPosition = Vector2.zero;
    }

    private IEnumerator PlayImpactVfx(bool targetIsPlayer, bool isCrit)
    {
        if (impactVfxImage == null || impactVfxRect == null)
            yield break;

        RectTransform targetRect = targetIsPlayer ? playerFighterRect : enemyFighterRect;
        if (targetRect == null)
            yield break;

        Vector2 impactPosition = GetStageLocalPoint(impactVfxRect, targetRect, new Vector2(targetIsPlayer ? 0.22f : -0.22f, 0.14f));
        float startSize = isCrit ? 42f : 30f;
        float endSize = isCrit ? 124f : 82f;
        Color flashColor = isCrit
            ? new Color(1f, 0.72f, 0.18f, 0.96f)
            : new Color(1f, 0.22f, 0.12f, 0.82f);

        impactVfxRect.anchoredPosition = impactPosition;
        impactVfxRect.localEulerAngles = Vector3.zero;
        impactVfxImage.color = flashColor;

        float elapsed = 0f;
        float duration = isCrit ? 0.20f : 0.14f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float size = Mathf.Lerp(startSize, endSize, EaseOutCubic(t));
            impactVfxRect.sizeDelta = new Vector2(size, size);
            impactVfxImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, Mathf.Lerp(flashColor.a, 0f, t));
            yield return null;
        }

        impactVfxImage.color = Color.clear;
        impactVfxRect.sizeDelta = Vector2.one;
    }

    private IEnumerator PlayCritScreenFlash()
    {
        if (critScreenFlashImage == null)
            yield break;

        float elapsed = 0f;
        const float duration = 0.18f;
        Color flashColor = new Color(1f, 0.72f, 0.20f, 0.32f);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            critScreenFlashImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, Mathf.Lerp(flashColor.a, 0f, t));
            yield return null;
        }

        critScreenFlashImage.color = Color.clear;
    }

    private void StartCameraShake(float duration, float strength)
    {
        if (playbackRootRect == null)
            return;

        if (shakeRoutine != null)
            StopCoroutine(shakeRoutine);

        shakeRoutine = StartCoroutine(PlayCameraShake(duration, strength));
    }

    private IEnumerator PlayCameraShake(float duration, float strength)
    {
        Vector2 start = Vector2.zero;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float fade = 1f - Mathf.Clamp01(elapsed / duration);
            playbackRootRect.anchoredPosition = start + UnityEngine.Random.insideUnitCircle * strength * fade;
            yield return null;
        }

        playbackRootRect.anchoredPosition = start;
        shakeRoutine = null;
    }

    private void StartHitStop(float duration, float timeScale)
    {
        if (hitStopRoutine != null)
            StopCoroutine(hitStopRoutine);

        hitStopRoutine = StartCoroutine(PlayHitStop(duration, timeScale));
    }

    private IEnumerator PlayHitStop(float duration, float timeScale)
    {
        if (!hasActiveHitStop)
        {
            savedTimeScale = Time.timeScale;
            hasActiveHitStop = true;
        }

        Time.timeScale = Mathf.Clamp(timeScale, 0.05f, 1f);
        yield return new WaitForSecondsRealtime(duration);
        RestoreTimeScale();
        hitStopRoutine = null;
    }

    private void RestoreTimeScale()
    {
        if (!hasActiveHitStop)
            return;

        Time.timeScale = savedTimeScale;
        hasActiveHitStop = false;
    }

    private void PlayAudioHook(AudioSource source, float volume)
    {
        if (source == null || source.clip == null)
            return;

        source.PlayOneShot(source.clip, Mathf.Clamp01(volume));
    }

    private Vector2 GetStageLocalPoint(RectTransform uiRect, RectTransform targetRect, Vector2 normalizedOffset)
    {
        RectTransform parentRect = uiRect != null ? uiRect.parent as RectTransform : null;

        if (parentRect == null || targetRect == null)
            return Vector2.zero;

        Vector3 targetWorldPosition = targetRect.TransformPoint(new Vector3(
            targetRect.rect.width * normalizedOffset.x,
            targetRect.rect.height * normalizedOffset.y,
            0f));

        return parentRect.InverseTransformPoint(targetWorldPosition);
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

        UpdatePlaybackPlayerHp(currentPlayerHp);

        float enemyFill = enemyStartHp > 0 ? (float)currentEnemyHp / enemyStartHp : 0f;

        if (enemyHpFill != null)
            enemyHpFill.fillAmount = Mathf.Clamp01(enemyFill);

        SetFillWidth(enemyHpFillRect, enemyFill);

        if (enemyHpText != null)
            enemyHpText.text = currentEnemyHp + " / " + enemyStartHp;
    }

    private void UpdatePlaybackPlayerHp(int playerHp)
    {
        if (playbackPlayerStats == null)
            return;

        float hpPercent = playerStartHp > 0 ? Mathf.Clamp01((float)playerHp / playerStartHp) : 0f;
        playbackPlayerStats.currentHp = Mathf.Clamp(
            Mathf.RoundToInt(playbackPlayerStats.maxHp * hpPercent),
            0,
            playbackPlayerStats.maxHp);
    }

    private void RestorePlaybackPlayerHp()
    {
        if (!hasOriginalPlayerStatsHp || playbackPlayerStats == null)
            return;

        playbackPlayerStats.currentHp = originalPlayerStatsHp;
        hasOriginalPlayerStatsHp = false;
        playbackPlayerStats = null;
    }

    private void SetEnemyMpBar(float fillAmount)
    {
        if (enemyMpFill != null)
            enemyMpFill.fillAmount = Mathf.Clamp01(fillAmount);

        SetFillWidth(enemyMpFillRect, fillAmount);

        if (enemyMpText != null)
            enemyMpText.text = "100 / 100";
    }

    private void SetFillWidth(RectTransform fillRect, float fillAmount)
    {
        if (fillRect == null)
            return;

        fillAmount = Mathf.Clamp01(fillAmount);
        fillRect.anchorMin = new Vector2(1f, 0f);
        fillRect.anchorMax = new Vector2(1f, 1f);
        fillRect.pivot = new Vector2(1f, 0.5f);
        fillRect.anchoredPosition = new Vector2(-EnemyResourceBarPaddingX, 0f);
        fillRect.sizeDelta = new Vector2(
            Mathf.Max((EnemyResourceBarWidth - EnemyResourceBarPaddingX * 2f) * fillAmount, 0f),
            -EnemyResourceBarPaddingY * 2f);
        fillRect.localScale = Vector3.one;
    }

    private void StopRunningCoroutines()
    {
        foreach (Coroutine routine in parallelRoutines)
        {
            if (routine != null)
                StopCoroutine(routine);
        }

        parallelRoutines.Clear();

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

        if (shakeRoutine != null)
        {
            StopCoroutine(shakeRoutine);
            shakeRoutine = null;
        }

        if (hitStopRoutine != null)
        {
            StopCoroutine(hitStopRoutine);
            hitStopRoutine = null;
        }

        if (floatingTextRoutine != null)
        {
            StopCoroutine(floatingTextRoutine);
            floatingTextRoutine = null;
        }

        if (floatingTextSecondaryRoutine != null)
        {
            StopCoroutine(floatingTextSecondaryRoutine);
            floatingTextSecondaryRoutine = null;
        }

        StopAudioHooks();
        RestoreTimeScale();
        ClearTransientVisuals();
    }

    private void BuildIfNeeded()
    {
        if (isBuilt)
            return;

        isBuilt = true;

        playbackRootRect = GetComponent<RectTransform>();
        playbackRootRect.anchorMin = new Vector2(0.5f, 0.5f);
        playbackRootRect.anchorMax = new Vector2(0.5f, 0.5f);
        playbackRootRect.anchoredPosition = Vector2.zero;
        playbackRootRect.sizeDelta = new Vector2(1470f, 1080f);
        playbackRootRect.pivot = new Vector2(0.5f, 0.5f);

        Image background = gameObject.GetComponent<Image>();
        if (background == null)
            background = gameObject.AddComponent<Image>();

        background.color = panelColor;
        AddOutline(gameObject, borderColor, new Vector2(2f, -2f));

        HorizontalLayoutGroup layout = gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(34, 0, 0, 0);
        layout.spacing = 24f;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;

        BuildAudioHooks();
        BuildBattlefield();
        BuildEnemyPanel();
    }

    private void BuildBattlefield()
    {
        GameObject stage = CreateLayoutObject("BattlefieldStage", transform);
        Image stageImage = stage.AddComponent<Image>();
        Sprite arenaBackground = Resources.Load<Sprite>(ArenaBattlefieldBackgroundResourcePath);
        if (arenaBackground != null)
        {
            stageImage.sprite = arenaBackground;
            stageImage.type = Image.Type.Simple;
            stageImage.preserveAspect = false;
            stageImage.color = Color.white;
        }
        else
        {
            stageImage.color = stageColor;
            Debug.LogWarning("CombatPlaybackUI: Arena battlefield background sprite not found at Resources/" + ArenaBattlefieldBackgroundResourcePath);
        }
        AddOutline(stage, darkBorderColor, new Vector2(2f, -2f));

        LayoutElement stageElement = stage.AddComponent<LayoutElement>();
        stageElement.minWidth = 900f;
        stageElement.flexibleWidth = 1f;

        if (arenaBackground != null)
            CreateBattlefieldReadabilityOverlay(stage.transform);

        CreateStageFloor(stage.transform);
        BuildCritScreenFlash(stage.transform);

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

        BuildCombatVfx(stage.transform);

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

        floatingTextSecondary = CreateText("FloatingTextSecondary", stage.transform, "", 38, FontStyles.Bold, TextAlignmentOptions.Center);
        floatingTextSecondaryRect = floatingTextSecondary.rectTransform;
        AnchorTo(floatingTextSecondaryRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 150f), new Vector2(430f, 94f));
    }

    private RectTransform BuildStageFighter(Transform parent, string objectName, bool isPlayer)
    {
        GameObject fighter = CreateLayoutObject(objectName, parent);

        RectTransform rect = fighter.GetComponent<RectTransform>();
        AnchorTo(rect, new Vector2(isPlayer ? 0.28f : 0.72f, 0.40f), new Vector2(isPlayer ? 0.28f : 0.72f, 0.40f), Vector2.zero, new Vector2(250f, 410f));

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

    private void BuildCombatVfx(Transform parent)
    {
        slashVfxImage = CreateVfxImage("SlashVfx", parent, out slashVfxRect);
        impactVfxImage = CreateVfxImage("ImpactVfx", parent, out impactVfxRect);
        slashVfxImage.sprite = CreateSoftSlashSprite();
        impactVfxImage.sprite = CreateSoftCircleSprite();
        slashVfxImage.type = Image.Type.Simple;
        impactVfxImage.type = Image.Type.Simple;
    }

    private Image CreateVfxImage(string objectName, Transform parent, out RectTransform rect)
    {
        GameObject vfx = CreateLayoutObject(objectName, parent);
        Image image = vfx.AddComponent<Image>();
        image.color = Color.clear;
        image.raycastTarget = false;

        rect = vfx.GetComponent<RectTransform>();
        AnchorTo(rect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1f, 1f));
        return image;
    }

    private void BuildCritScreenFlash(Transform parent)
    {
        GameObject flash = CreateLayoutObject("CritScreenFlash", parent);
        critScreenFlashImage = flash.AddComponent<Image>();
        critScreenFlashImage.color = Color.clear;
        critScreenFlashImage.raycastTarget = false;
        critScreenFlashRect = flash.GetComponent<RectTransform>();
        AnchorTo(critScreenFlashRect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
    }

    private void BuildAudioHooks()
    {
        slashAudioVolume = slashVolume;
        hitAudioVolume = hitVolume;
        critAudioVolume = critVolume;
        blockAudioVolume = blockVolume;
        dodgeAudioVolume = dodgeVolume;

        slashAudioSource = CreateAudioHook("Audio_Slash", slashClip != null ? slashClip : CreateSlashClip(), slashAudioVolume);
        hitAudioSource = CreateAudioHook("Audio_Hit", hitClip != null ? hitClip : CreateHitClip(), hitAudioVolume);
        critAudioSource = CreateAudioHook("Audio_Crit", critClip != null ? critClip : CreateCritClip(), critAudioVolume);
        blockAudioSource = CreateAudioHook("Audio_Block", blockClip != null ? blockClip : CreateBlockClip(), blockAudioVolume);
        dodgeAudioSource = CreateAudioHook("Audio_Dodge", dodgeClip != null ? dodgeClip : CreateDodgeClip(), dodgeAudioVolume);
    }

    private AudioSource CreateAudioHook(string objectName, AudioClip clip, float volume)
    {
        GameObject hook = new GameObject(objectName, typeof(AudioSource));
        hook.transform.SetParent(transform, false);

        AudioSource source = hook.GetComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 0f;
        source.volume = Mathf.Clamp01(volume);
        source.clip = clip;
        return source;
    }

    private void StopAudioHooks()
    {
        StopAudioHook(slashAudioSource);
        StopAudioHook(hitAudioSource);
        StopAudioHook(critAudioSource);
        StopAudioHook(blockAudioSource);
        StopAudioHook(dodgeAudioSource);
    }

    private void StopAudioHook(AudioSource source)
    {
        if (source != null)
            source.Stop();
    }

    private AudioClip CreateSlashClip()
    {
        return CreateProceduralClip("Generated_SlashWhoosh", 0.18f, (time, duration) =>
        {
            float t = time / duration;
            float envelope = Mathf.Sin(Mathf.PI * Mathf.Clamp01(t));
            float pitchSweep = Mathf.Lerp(920f, 240f, t);
            float noise = PseudoNoise(time * 9200f);
            return (Mathf.Sin(Mathf.PI * 2f * pitchSweep * time) * 0.22f + noise * 0.78f) * envelope * 0.45f;
        });
    }

    private AudioClip CreateHitClip()
    {
        return CreateProceduralClip("Generated_HitThud", 0.16f, (time, duration) =>
        {
            float t = time / duration;
            float envelope = Mathf.Exp(-t * 8f);
            float low = Mathf.Sin(Mathf.PI * 2f * Mathf.Lerp(115f, 58f, t) * time);
            float body = Mathf.Sin(Mathf.PI * 2f * 72f * time) * 0.45f;
            return (low * 0.65f + body * 0.35f) * envelope * 0.65f;
        });
    }

    private AudioClip CreateCritClip()
    {
        return CreateProceduralClip("Generated_CritImpact", 0.24f, (time, duration) =>
        {
            float t = time / duration;
            float transient = t < 0.035f ? (1f - t / 0.035f) : 0f;
            float envelope = Mathf.Exp(-t * 6f);
            float low = Mathf.Sin(Mathf.PI * 2f * Mathf.Lerp(145f, 54f, t) * time) * envelope;
            float sharp = Mathf.Sin(Mathf.PI * 2f * 1550f * time) * transient;
            return (low * 0.72f + sharp * 0.34f + PseudoNoise(time * 13000f) * transient * 0.22f) * 0.72f;
        });
    }

    private AudioClip CreateBlockClip()
    {
        return CreateProceduralClip("Generated_BlockClang", 0.28f, (time, duration) =>
        {
            float t = time / duration;
            float envelope = Mathf.Exp(-t * 7f);
            float clangA = Mathf.Sin(Mathf.PI * 2f * 820f * time);
            float clangB = Mathf.Sin(Mathf.PI * 2f * 1280f * time + 0.8f);
            float metal = Mathf.Sin(Mathf.PI * 2f * 2340f * time) * 0.2f;
            return (clangA * 0.46f + clangB * 0.34f + metal) * envelope * 0.48f;
        });
    }

    private AudioClip CreateDodgeClip()
    {
        return CreateProceduralClip("Generated_DodgeWhoosh", 0.14f, (time, duration) =>
        {
            float t = time / duration;
            float envelope = Mathf.Sin(Mathf.PI * Mathf.Clamp01(t));
            float noise = PseudoNoise(time * 11500f);
            float airy = Mathf.Sin(Mathf.PI * 2f * Mathf.Lerp(620f, 1180f, t) * time) * 0.18f;
            return (noise * 0.72f + airy) * envelope * 0.32f;
        });
    }

    private AudioClip CreateProceduralClip(string clipName, float duration, Func<float, float, float> sampleGenerator)
    {
        const int sampleRate = 44100;
        int sampleCount = Mathf.Max(1, Mathf.CeilToInt(duration * sampleRate));
        int samplePosition = 0;

        return AudioClip.Create(clipName, sampleCount, 1, sampleRate, false, data =>
        {
            for (int i = 0; i < data.Length; i++)
            {
                int wrappedPosition = samplePosition % sampleCount;
                float time = (float)wrappedPosition / sampleRate;
                data[i] = Mathf.Clamp(sampleGenerator(time, duration), -1f, 1f);
                samplePosition++;
            }
        });
    }

    private float PseudoNoise(float value)
    {
        return Mathf.Sin(value * 12.9898f + Mathf.Sin(value * 0.071f) * 78.233f);
    }

    private Sprite CreateSoftSlashSprite()
    {
        const int width = 128;
        const int height = 32;
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        Color[] pixels = new Color[width * height];
        Vector2 center = new Vector2(width * 0.5f, height * 0.5f);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float normalizedX = Mathf.Abs((x - center.x) / (width * 0.5f));
                float normalizedY = Mathf.Abs((y - center.y) / (height * 0.5f));
                float alpha = Mathf.Clamp01(1f - normalizedX);
                alpha *= Mathf.Clamp01(1f - normalizedY * normalizedY);
                alpha = Mathf.Pow(alpha, 1.8f);
                pixels[y * width + x] = new Color(1f, 0.78f, 0.18f, alpha);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), 100f);
    }

    private Sprite CreateSoftCircleSprite()
    {
        const int size = 96;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        Color[] pixels = new Color[size * size];
        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center) / (size * 0.5f);
                float alpha = Mathf.Clamp01(1f - distance);
                alpha = Mathf.Pow(alpha, 2.2f);
                pixels[y * size + x] = new Color(1f, 0.24f, 0.12f, alpha);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
    }

    private void ClearTransientVisuals()
    {
        if (playbackRootRect != null)
            playbackRootRect.anchoredPosition = Vector2.zero;

        if (slashVfxImage != null)
            slashVfxImage.color = Color.clear;

        if (impactVfxImage != null)
            impactVfxImage.color = Color.clear;

        if (critScreenFlashImage != null)
            critScreenFlashImage.color = Color.clear;
    }

    private float EaseOutCubic(float t)
    {
        t = Mathf.Clamp01(t);
        float inverse = 1f - t;
        return 1f - inverse * inverse * inverse;
    }

    private void CreateStageFloor(Transform parent)
    {
        GameObject floor = CreateLayoutObject("StageFloor", parent);
        Image floorImage = floor.AddComponent<Image>();
        floorImage.color = new Color(0.10f, 0.075f, 0.045f, 0.45f);

        RectTransform rect = floor.GetComponent<RectTransform>();
        AnchorTo(rect, new Vector2(0.5f, 0.28f), new Vector2(0.5f, 0.28f), Vector2.zero, new Vector2(720f, 18f));
    }

    private void CreateBattlefieldReadabilityOverlay(Transform parent)
    {
        GameObject overlay = CreateLayoutObject("BattlefieldReadabilityOverlay", parent);
        Image overlayImage = overlay.AddComponent<Image>();
        overlayImage.color = new Color(0.10f, 0.015f, 0.012f, 0.50f);
        AnchorTo(overlay.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
    }

    private void BuildEnemyPanel()
    {
        GameObject panel = CreateLayoutObject("EnemyPanel", transform);
        Image panelImage = panel.AddComponent<Image>();
        Sprite enemyPanelFrame = Resources.Load<Sprite>(EnemyPanelFrameResourcePath);
        enemyPanelUsesFrame = enemyPanelFrame != null;

        if (enemyPanelUsesFrame)
        {
            panelImage.color = Color.clear;
        }
        else
        {
            panelImage.color = cardColor;
            AddOutline(panel, borderColor, new Vector2(2f, -2f));
            Debug.LogWarning("CombatPlaybackUI: EnemyPanel frame sprite not found at Resources/" + EnemyPanelFrameResourcePath);
        }

        LayoutElement element = panel.AddComponent<LayoutElement>();
        element.minWidth = EnemyPanelWidth;
        element.preferredWidth = EnemyPanelWidth;
        element.flexibleWidth = 0f;

        if (enemyPanelUsesFrame)
        {
            GameObject frameRoot = CreateEnemyPanelFrameRoot(panel.transform, enemyPanelFrame);
            BuildFramedEnemyPanelContent(frameRoot.transform);
            return;
        }

        VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(20, 20, 20, 22);
        layout.spacing = 12f;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

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

    private GameObject CreateEnemyPanelFrameRoot(Transform parent, Sprite frameSprite)
    {
        GameObject frameRoot = CreateLayoutObject("EnemyPanelFrameRoot", parent);
        Image frameImage = frameRoot.AddComponent<Image>();
        frameImage.sprite = frameSprite;
        frameImage.type = Image.Type.Simple;
        frameImage.preserveAspect = false;
        frameImage.color = Color.white;

        RectTransform frameRect = frameRoot.GetComponent<RectTransform>();
        AnchorTo(
            frameRect,
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0f, -EnemyPanelFrameHeight * 0.5f),
            new Vector2(EnemyPanelWidth, EnemyPanelFrameHeight));

        // The current enemy HUD source has a taller portrait region than the left CharacterPanel.
        // Keeping the parent panel full-height while compressing this frame upward preserves the
        // battlefield width and leaves harmless empty space below the mirrored enemy HUD.
        return frameRoot;
    }

    private void BuildFramedEnemyPanelContent(Transform parent)
    {
        enemyNameText = CreateEnemyHudText("EnemyName", parent, 24, FontStyles.Normal, TextAlignmentOptions.Left);
        AnchorTo(enemyNameText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(28f, -13f), new Vector2(205f, 44f));
        enemyNameText.rectTransform.pivot = new Vector2(0f, 1f);

        enemyLevelText = CreateEnemyHudText("EnemyLevel", parent, 22, FontStyles.Bold, TextAlignmentOptions.Right);
        AnchorTo(enemyLevelText.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-28f, -13f), new Vector2(185f, 44f));
        enemyLevelText.rectTransform.pivot = new Vector2(1f, 1f);

        enemyPowerText = CreateEnemyHudText("EnemyPower", parent, 20, FontStyles.Bold, TextAlignmentOptions.Right);
        AnchorTo(enemyPowerText.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-31f, -59f), new Vector2(180f, 42f));
        enemyPowerText.rectTransform.pivot = new Vector2(1f, 1f);

        GameObject portrait = CreatePanelBox("EnemyPortrait", parent, 0f);
        AnchorTo(portrait.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -225f), new Vector2(EnemyPortraitSize, EnemyPortraitSize));
        portrait.AddComponent<RectMask2D>();

        GameObject portraitImageObject = CreateLayoutObject("EnemyPortraitImage", portrait.transform);
        enemyPortraitImage = portraitImageObject.AddComponent<Image>();
        Sprite portraitSprite = Resources.Load<Sprite>(EnemyOrcPortraitResourcePath);
        enemyPortraitImage.sprite = portraitSprite;
        enemyPortraitImage.color = portraitSprite != null ? Color.white : Color.clear;
        enemyPortraitImage.preserveAspect = true;
        enemyPortraitDefaultColor = enemyPortraitImage.color;
        AnchorTo(enemyPortraitImage.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -12f), new Vector2(EnemyPortraitSize, 520f));

        TMP_Text portraitMark = CreateText("Mark", portrait.transform, "ENEMY", 42, FontStyles.Bold, TextAlignmentOptions.Center);
        portraitMark.color = portraitSprite == null
            ? new Color(0.42f, 0.31f, 0.20f, 0.72f)
            : Color.clear;
        AnchorTo(portraitMark.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        GameObject slots = BuildWeaponSlots(parent);
        AnchorTo(slots.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -606f), new Vector2(370f, 198f));

        BuildFramedResourceBar(parent, "HP", true, -726f);
        BuildFramedResourceBar(parent, "MP", false, -787f);
    }

    private TMP_Text CreateEnemyHudText(
        string objectName,
        Transform parent,
        int fontSize,
        FontStyles fontStyle,
        TextAlignmentOptions alignment)
    {
        TMP_Text text = CreateText(objectName, parent, "", fontSize, fontStyle, alignment);
        text.color = hudTextColor;
        Destroy(text.GetComponent<LayoutElement>());
        return text;
    }

    private GameObject BuildWeaponSlots(Transform parent)
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

        return slots;
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
        barBackground.color = enemyPanelUsesFrame
            ? Color.clear
            : new Color(0.018f, 0.012f, 0.01f, 1f);

        if (!enemyPanelUsesFrame)
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

    private void BuildFramedResourceBar(Transform parent, string label, bool isHp, float anchoredY)
    {
        GameObject bar = CreateLayoutObject(label + "Bar", parent);
        Image barBackground = bar.AddComponent<Image>();
        barBackground.color = Color.clear;
        AnchorTo(bar.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(40f, anchoredY), new Vector2(EnemyResourceBarWidth, EnemyResourceBarHeight));

        GameObject fill = CreateLayoutObject("Fill", bar.transform);
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = isHp
            ? new Color(0.68f, 0.04f, 0.03f, 1f)
            : new Color(0.02f, 0.18f, 0.72f, 1f);
        fillImage.type = Image.Type.Simple;
        fillImage.fillAmount = 1f;

        RectTransform fillRect = fill.GetComponent<RectTransform>();
        SetFillWidth(fillRect, 1f);

        TMP_Text valueText = CreateText("Value", bar.transform, "", 19, FontStyles.Bold, TextAlignmentOptions.Right);
        valueText.color = textColor;
        valueText.margin = new Vector4(0f, 0f, 12f, 0f);
        AnchorTo(valueText.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        Destroy(valueText.GetComponent<LayoutElement>());

        if (isHp)
        {
            enemyHpFill = fillImage;
            enemyHpFillRect = fillRect;
            enemyHpText = valueText;
        }
        else
        {
            enemyMpFill = fillImage;
            enemyMpFillRect = fillRect;
            enemyMpText = valueText;
        }
    }

    private GameObject CreatePanelBox(string objectName, Transform parent, float preferredHeight)
    {
        GameObject box = CreateLayoutObject(objectName, parent);
        Image image = box.AddComponent<Image>();
        image.color = enemyPanelUsesFrame ? Color.clear : portraitColor;

        if (!enemyPanelUsesFrame)
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
