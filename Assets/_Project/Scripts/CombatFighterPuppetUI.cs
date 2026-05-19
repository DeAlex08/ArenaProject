using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum CombatFighterPose
{
    Idle,
    AttackLeft,
    AttackRight,
    Block,
    Dodge,
    Hit,
    CritHit,
    Death
}

public class CombatFighterPuppetUI : MonoBehaviour
{
    private const string PoseResourceRoot = "Combat/FighterPoses/";

    [Header("Future Pose Sprites")]
    [SerializeField] private Sprite idleSprite;
    [SerializeField] private Sprite attackLeftSprite;
    [SerializeField] private Sprite attackRightSprite;
    [SerializeField] private Sprite blockSprite;
    [SerializeField] private Sprite dodgeSprite;
    [SerializeField] private Sprite hitSprite;
    [SerializeField] private Sprite critHitSprite;
    [SerializeField] private Sprite deathSprite;

    private RectTransform rootRect;
    private RectTransform poseRootRect;
    private RectTransform modularRootRect;
    private Image poseImage;
    private TMP_Text poseLabel;

    private Vector2 rootStartPosition;
    private bool facingRight = true;
    private bool nextAttackUsesRightPose = true;
    private bool isDead;

    private Color baseColor = new Color(0.10f, 0.095f, 0.085f, 1f);
    private Color limbColor = new Color(0.15f, 0.13f, 0.11f, 1f);
    private Color weaponColor = new Color(0.42f, 0.33f, 0.24f, 1f);

    public RectTransform RootRect => rootRect;

    public void Initialize(bool faceRight, Color bodyColor, Color accentColor, Color bladeColor)
    {
        facingRight = faceRight;
        baseColor = bodyColor;
        limbColor = accentColor;
        weaponColor = bladeColor;

        BuildIfNeeded();
        ResetPose();
    }

    public void AssignPoseSprites(
        Sprite idle,
        Sprite attackLeft,
        Sprite attackRight,
        Sprite block,
        Sprite dodge,
        Sprite hit,
        Sprite critHit,
        Sprite death)
    {
        idleSprite = idle;
        attackLeftSprite = attackLeft;
        attackRightSprite = attackRight;
        blockSprite = block;
        dodgeSprite = dodge;
        hitSprite = hit;
        critHitSprite = critHit;
        deathSprite = death;
        SetPose(isDead ? CombatFighterPose.Death : CombatFighterPose.Idle);
    }

    public void ResetPose()
    {
        BuildIfNeeded();

        isDead = false;
        rootRect.anchoredPosition = rootStartPosition;
        rootRect.localEulerAngles = Vector3.zero;
        poseRootRect.localEulerAngles = Vector3.zero;
        SetPose(CombatFighterPose.Idle);
    }

    public void CaptureCurrentPoseAsRest()
    {
        BuildIfNeeded();
        rootStartPosition = rootRect.anchoredPosition;
        nextAttackUsesRightPose = facingRight;
        ResetPose();
    }

    public IEnumerator PlayAttack()
    {
        BuildIfNeeded();

        if (isDead)
            yield break;

        CombatFighterPose attackPose = nextAttackUsesRightPose
            ? CombatFighterPose.AttackRight
            : CombatFighterPose.AttackLeft;
        nextAttackUsesRightPose = !nextAttackUsesRightPose;

        float direction = GetFacingDirection();
        Vector2 start = rootRect.anchoredPosition;

        SetPose(attackPose);
        yield return MoveRoot(start, start + new Vector2(direction * -12f, 4f), 0.05f);
        yield return MoveRoot(rootRect.anchoredPosition, start + new Vector2(direction * 58f, 0f), 0.12f);
        yield return new WaitForSeconds(0.04f);
        yield return MoveRoot(rootRect.anchoredPosition, start, 0.13f);
        SetPose(CombatFighterPose.Idle);
    }

    public IEnumerator PlayBlock()
    {
        BuildIfNeeded();

        if (isDead)
            yield break;

        SetPose(CombatFighterPose.Block);
        yield return PulsePose(new Vector2(GetFacingDirection() * 8f, 0f), 0.08f);
        yield return new WaitForSeconds(0.12f);
        SetPose(CombatFighterPose.Idle);
    }

    public IEnumerator PlayDodge()
    {
        BuildIfNeeded();

        if (isDead)
            yield break;

        Vector2 start = rootRect.anchoredPosition;
        float direction = facingRight ? -1f : 1f;

        SetPose(CombatFighterPose.Dodge);
        yield return MoveRoot(start, start + new Vector2(direction * 64f, 12f), 0.11f);
        yield return MoveRoot(rootRect.anchoredPosition, start, 0.14f);
        SetPose(CombatFighterPose.Idle);
    }

    public IEnumerator PlayHit(bool strong)
    {
        BuildIfNeeded();

        if (isDead)
            yield break;

        float shakeDistance = strong ? 24f : 13f;
        float direction = facingRight ? -1f : 1f;
        Vector2 start = rootRect.anchoredPosition;

        SetPose(strong ? CombatFighterPose.CritHit : CombatFighterPose.Hit);
        SetPoseColor(strong ? new Color(0.98f, 0.34f, 0.12f, 1f) : new Color(0.68f, 0.12f, 0.08f, 1f));
        yield return MoveRoot(start, start + new Vector2(direction * shakeDistance, 0f), 0.04f);
        yield return MoveRoot(rootRect.anchoredPosition, start + new Vector2(-direction * shakeDistance * 0.65f, 0f), 0.04f);
        yield return MoveRoot(rootRect.anchoredPosition, start, 0.05f);
        SetPose(CombatFighterPose.Idle);
    }

    public IEnumerator PlayDeath()
    {
        BuildIfNeeded();

        if (isDead)
            yield break;

        isDead = true;
        SetPose(CombatFighterPose.Death);
        yield return MoveRoot(rootRect.anchoredPosition, rootStartPosition + new Vector2(0f, -22f), 0.16f);
    }

    public void Flash(Color color)
    {
        SetPoseColor(color);
    }

    public void RestoreColor()
    {
        SetPose(isDead ? CombatFighterPose.Death : CombatFighterPose.Idle);
    }

    private IEnumerator PulsePose(Vector2 offset, float duration)
    {
        Vector2 start = poseRootRect.anchoredPosition;
        Vector2 target = start + offset;

        yield return MovePoseRoot(start, target, duration);
        yield return MovePoseRoot(target, start, duration);
    }

    private IEnumerator MoveRoot(Vector2 from, Vector2 to, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = duration > 0f ? Mathf.Clamp01(elapsed / duration) : 1f;
            rootRect.anchoredPosition = Vector2.Lerp(from, to, t);
            yield return null;
        }

        rootRect.anchoredPosition = to;
    }

    private IEnumerator MovePoseRoot(Vector2 from, Vector2 to, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = duration > 0f ? Mathf.Clamp01(elapsed / duration) : 1f;
            poseRootRect.anchoredPosition = Vector2.Lerp(from, to, t);
            yield return null;
        }

        poseRootRect.anchoredPosition = to;
    }

    private void BuildIfNeeded()
    {
        if (rootRect != null)
            return;

        rootRect = GetComponent<RectTransform>();

        if (rootRect == null)
            rootRect = gameObject.AddComponent<RectTransform>();

        BuildPoseRoot();
        BuildHiddenModularRoot();
        LoadDefaultPoseSpritesIfNeeded();
        rootStartPosition = rootRect.anchoredPosition;
    }

    private void BuildPoseRoot()
    {
        GameObject poseRoot = new GameObject("PoseRoot", typeof(RectTransform));
        poseRoot.transform.SetParent(transform, false);
        poseRootRect = poseRoot.GetComponent<RectTransform>();
        poseRootRect.anchorMin = new Vector2(0.5f, 0.5f);
        poseRootRect.anchorMax = new Vector2(0.5f, 0.5f);
        poseRootRect.pivot = new Vector2(0.5f, 0.5f);
        poseRootRect.anchoredPosition = Vector2.zero;
        poseRootRect.sizeDelta = new Vector2(380f, 330f);

        GameObject imageObject = new GameObject("PoseImage", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        imageObject.transform.SetParent(poseRoot.transform, false);
        RectTransform imageRect = imageObject.GetComponent<RectTransform>();
        imageRect.anchorMin = Vector2.zero;
        imageRect.anchorMax = Vector2.one;
        imageRect.offsetMin = Vector2.zero;
        imageRect.offsetMax = Vector2.zero;

        poseImage = imageObject.GetComponent<Image>();
        poseImage.preserveAspect = true;
        poseImage.raycastTarget = false;

        Outline outline = imageObject.AddComponent<Outline>();
        outline.effectColor = new Color(0.04f, 0.025f, 0.015f, 1f);
        outline.effectDistance = new Vector2(2f, -2f);
        outline.useGraphicAlpha = true;

        GameObject labelObject = new GameObject("PoseLabel", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        labelObject.transform.SetParent(poseRoot.transform, false);
        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        poseLabel = labelObject.GetComponent<TMP_Text>();
        poseLabel.alignment = TextAlignmentOptions.Center;
        poseLabel.fontSize = 23f;
        poseLabel.fontStyle = FontStyles.Bold;
        poseLabel.color = new Color(0.96f, 0.89f, 0.70f, 1f);
        poseLabel.textWrappingMode = TextWrappingModes.Normal;
    }

    private void BuildHiddenModularRoot()
    {
        // The modular body-part hierarchy is kept for future 2D puppet sprites,
        // but Combat Playback v1 uses pose sprites/placeholders as the visible layer.
        GameObject modularRoot = new GameObject("ModularPuppetRoot", typeof(RectTransform));
        modularRoot.transform.SetParent(transform, false);
        modularRootRect = modularRoot.GetComponent<RectTransform>();
        modularRootRect.anchorMin = new Vector2(0.5f, 0.5f);
        modularRootRect.anchorMax = new Vector2(0.5f, 0.5f);
        modularRootRect.pivot = new Vector2(0.5f, 0.5f);
        modularRootRect.anchoredPosition = Vector2.zero;
        modularRootRect.sizeDelta = new Vector2(190f, 330f);

        CreateModularPart("Body", modularRoot.transform);
        CreateModularPart("Head", modularRoot.transform);
        CreateModularPart("LeftArm", modularRoot.transform);
        CreateModularPart("RightArm", modularRoot.transform);
        CreateModularPart("LeftWeapon", modularRoot.transform);
        CreateModularPart("RightWeapon", modularRoot.transform);
        CreateModularPart("Legs", modularRoot.transform);

        modularRoot.SetActive(false);
    }

    private void CreateModularPart(string partName, Transform parent)
    {
        GameObject part = new GameObject(partName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        part.transform.SetParent(parent, false);
        Image image = part.GetComponent<Image>();
        image.color = partName.Contains("Weapon") ? weaponColor : limbColor;
    }

    private void SetPose(CombatFighterPose pose)
    {
        BuildIfNeeded();

        Sprite sprite = GetSpriteForPose(pose);
        poseImage.sprite = sprite;
        poseImage.color = sprite != null ? Color.white : GetPlaceholderColor(pose);
        poseImage.transform.localScale = new Vector3(facingRight ? 1f : -1f, 1f, 1f);

        bool showLabel = sprite == null;
        poseLabel.gameObject.SetActive(showLabel);

        if (showLabel)
            poseLabel.text = BuildPoseLabel(pose);
    }

    private void SetPoseColor(Color color)
    {
        if (poseImage != null)
            poseImage.color = color;
    }

    private Sprite GetSpriteForPose(CombatFighterPose pose)
    {
        switch (pose)
        {
            case CombatFighterPose.AttackLeft:
                return attackLeftSprite;
            case CombatFighterPose.AttackRight:
                return attackRightSprite;
            case CombatFighterPose.Block:
                return blockSprite;
            case CombatFighterPose.Dodge:
                return dodgeSprite;
            case CombatFighterPose.Hit:
                return hitSprite;
            case CombatFighterPose.CritHit:
                return critHitSprite;
            case CombatFighterPose.Death:
                return deathSprite;
            default:
                return idleSprite;
        }
    }

    private Color GetPlaceholderColor(CombatFighterPose pose)
    {
        switch (pose)
        {
            case CombatFighterPose.AttackLeft:
            case CombatFighterPose.AttackRight:
                return Color.Lerp(baseColor, weaponColor, 0.45f);
            case CombatFighterPose.Block:
                return Color.Lerp(baseColor, new Color(0.55f, 0.62f, 0.65f, 1f), 0.42f);
            case CombatFighterPose.Dodge:
                return Color.Lerp(baseColor, new Color(0.42f, 0.56f, 0.85f, 1f), 0.35f);
            case CombatFighterPose.Hit:
                return new Color(0.62f, 0.12f, 0.08f, 1f);
            case CombatFighterPose.CritHit:
                return new Color(0.92f, 0.26f, 0.10f, 1f);
            case CombatFighterPose.Death:
                return new Color(0.18f, 0.15f, 0.13f, 1f);
            default:
                return baseColor;
        }
    }

    private string BuildPoseLabel(CombatFighterPose pose)
    {
        string arrow = facingRight ? ">" : "<";

        switch (pose)
        {
            case CombatFighterPose.AttackLeft:
                return arrow + "\nATTACK\nLEFT";
            case CombatFighterPose.AttackRight:
                return arrow + "\nATTACK\nRIGHT";
            case CombatFighterPose.CritHit:
                return "CRIT\nHIT";
            default:
                return pose.ToString().ToUpperInvariant();
        }
    }

    private float GetFacingDirection()
    {
        return facingRight ? 1f : -1f;
    }

    private void LoadDefaultPoseSpritesIfNeeded()
    {
        idleSprite = idleSprite != null ? idleSprite : LoadPoseSprite("FighterPose_Idle");
        attackLeftSprite = attackLeftSprite != null ? attackLeftSprite : LoadPoseSprite("FighterPose_AttackLeft");
        attackRightSprite = attackRightSprite != null ? attackRightSprite : LoadPoseSprite("FighterPose_AttackRight");
        blockSprite = blockSprite != null ? blockSprite : LoadPoseSprite("FighterPose_Block");
        dodgeSprite = dodgeSprite != null ? dodgeSprite : LoadPoseSprite("FighterPose_Dodge");
        deathSprite = deathSprite != null ? deathSprite : LoadPoseSprite("FighterPose_Death");

        // The first supplied sheet has no dedicated Hit/CritHit frames yet.
        // Reusing Block keeps v1 sprite-based during hit reactions while flash/shake carries the impact.
        hitSprite = hitSprite != null ? hitSprite : blockSprite;
        critHitSprite = critHitSprite != null ? critHitSprite : blockSprite;
    }

    private Sprite LoadPoseSprite(string resourceName)
    {
        string resourcePath = PoseResourceRoot + resourceName;
        Sprite importedSprite = Resources.Load<Sprite>(resourcePath);

        if (importedSprite != null)
            return importedSprite;

        Texture2D texture = Resources.Load<Texture2D>(resourcePath);

        if (texture == null)
        {
            Debug.LogWarning("CombatFighterPuppetUI could not load pose asset: " + resourcePath);
            return null;
        }

        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            100f);
        sprite.name = resourceName;
        return sprite;
    }
}
