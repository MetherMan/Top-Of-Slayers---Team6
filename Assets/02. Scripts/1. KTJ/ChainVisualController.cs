using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class ChainVisualController : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private ChainCombatController chainCombat;
    [SerializeField] private DamageSystem damageSystem;
    [SerializeField] private HitSequenceController hitSequence;
    [SerializeField] private ChainUI chainUI;
    [SerializeField] private GameObject chainPanel;
    [SerializeField] private TextMeshProUGUI chainText;

    [Header("체인 텍스트")]
    [SerializeField] private string chainTextFormat = "Chain x {0}";
    [SerializeField] private RectTransform chainTextRoot;
    [SerializeField] private CanvasGroup chainTextGroup;
    [SerializeField, Min(0f)] private float chainTextFadeIn = 0.08f;
    [SerializeField, Min(0f)] private float chainTextFadeOut = 0.12f;
    [SerializeField, Min(0f)] private float chainTextPunchScale = 0.15f;
    [SerializeField, Min(0f)] private float chainTextPunchDuration = 0.12f;
    [SerializeField] private Ease chainTextEase = Ease.OutBack;

    [Header("체인 타이머 바")]
    [SerializeField] private Image chainTimerBarFillImage;
    [SerializeField] private Image chainTimerBarBackgroundImage;
    [SerializeField] private bool autoCreateChainTimerBar = true;
    [SerializeField, Min(0f)] private float chainTimerBarTopMargin = 0f;
    [SerializeField, Min(0f)] private float chainTimerBarSidePadding = 0f;
    [SerializeField, Min(4f)] private float chainTimerBarHeight = 20f;
    [SerializeField] private Color chainTimerBarColor = new Color(1f, 0.85f, 0.15f, 0.95f);
    [SerializeField] private Color chainTimerBarBackgroundColor = new Color(0.08f, 0.08f, 0.1f, 0.7f);
    [SerializeField, Range(0f, 1f)] private float chainTimerBarEmptyAlpha = 0.18f;

    [Header("체인 마일스톤 비트")]
    [SerializeField] private bool useChainMilestoneBeat = true;
    [SerializeField] private bool useMilestoneHitStop = true;
    [SerializeField, Min(0f)] private float milestoneTextPunchScale = 0.26f;
    [SerializeField, Min(0f)] private float milestoneTextPunchDuration = 0.14f;
    [SerializeField] private Color milestoneTextFlashColor = new Color(1f, 0.45f, 0.2f, 1f);
    [SerializeField, Min(0.01f)] private float milestoneTextFlashReturn = 0.16f;
    [SerializeField] private Color milestoneTimerBarFlashColor = new Color(1f, 0.35f, 0.2f, 1f);
    [SerializeField, Min(0.01f)] private float milestoneTimerBarFlashReturn = 0.14f;

    [Header("체인 처치 프리팹")]
    [SerializeField] private GameObject chainKillPrefab;
    [SerializeField, Min(0f)] private float chainKillPrefabHeightOffset = 0.2f;
    [SerializeField, Min(0f)] private float chainKillPrefabAutoDestroyTime = 2f;

    [Header("체인 처치 피니시 비트")]
    [SerializeField] private bool useChainKillFinishBeat = true;
    [SerializeField] private bool useKillFinishHitStop = true;
    [SerializeField, Min(0f)] private float killFinishTextPunchScale = 0.34f;
    [SerializeField, Min(0f)] private float killFinishTextPunchDuration = 0.18f;
    [SerializeField] private Color killFinishTextFlashColor = new Color(1f, 0.28f, 0.2f, 1f);
    [SerializeField, Min(0.01f)] private float killFinishTextFlashReturn = 0.18f;

    [Header("피해량 텍스트")]
    [SerializeField] private bool useDamageText = true;
    [SerializeField] private Camera damageTextCamera;
    [SerializeField] private Vector3 damageTextOffset = new Vector3(0f, 0.14f, 0f);
    [SerializeField, Min(0f)] private float damageTextRandomHorizontal = 0.16f;
    [SerializeField, Min(0f)] private float damageTextRiseDistance = 0.72f;
    [SerializeField, Min(0.05f)] private float damageTextDuration = 0.42f;
    [SerializeField, Min(0.01f)] private float damageTextScale = 0.18f;
    [SerializeField, Min(1f)] private float damageTextFontSize = 7f;
    [SerializeField, Range(1f, 3f)] private float damageTextPopScaleMultiplier = 1.75f;
    [SerializeField, Min(0f)] private float damageTextPopDuration = 0.09f;
    [SerializeField, Min(0f)] private float damageTextDriftDistance = 0.14f;
    [SerializeField, Min(1)] private int damageTextBigHitThreshold = 30;
    [SerializeField, Range(0f, 1f)] private float damageTextAmountScaleWeight = 0.62f;
    [SerializeField, Min(0f)] private float damageTextBigHitExtraScale = 0.26f;
    [SerializeField] private TMP_FontAsset damageTextFontAsset;
    [SerializeField] private Color damageTextColor = new Color(1f, 1f, 1f, 1f);
    [SerializeField] private Color damageTextBigHitColor = new Color(1f, 0.28f, 0.28f, 1f);
    [SerializeField] private Color killDamageTextColor = new Color(1f, 0.36f, 0.28f, 1f);

    [Header("배경 어둡게")]
    [SerializeField] private CanvasGroup darkenGroup;
    [SerializeField] private Graphic darkenGraphic;
    [SerializeField] private SpriteRenderer darkenSprite;
    [SerializeField] private Transform darkenRoot;
    [SerializeField, Range(0f, 1f)] private float darkenAlpha = 0.5f;
    [SerializeField, Min(0f)] private float darkenFadeTime = 0.12f;
    [SerializeField] private Ease darkenFadeEase = Ease.OutQuad;
    [SerializeField] private bool useDarkenBurst = true;
    [SerializeField, Min(0f)] private float darkenStartScale = 0.9f;
    [SerializeField, Min(0f)] private float darkenOvershootScale = 1.05f;
    [SerializeField, Min(0f)] private float darkenBurstTime = 0.08f;
    [SerializeField, Min(0f)] private float darkenSettleTime = 0.08f;
    [SerializeField] private bool useUnscaledTime = true;

    [Header("지형 어둡게")]
    [SerializeField] private bool useEnvironmentDarken = true;
    [SerializeField] private bool useSpriteDarkenWithEnvironment;
    [SerializeField] private bool environmentOnlyStaticRenderers;
    [SerializeField] private LayerMask environmentLayerMask = ~0;
    [SerializeField, Range(0f, 1f)] private float environmentDarkenStrength = 0.22f;

    private int lastChain = -1;
    private Tween darkenTween;
    private bool isChainActive;
    private Vector3 darkenBaseScale = Vector3.one;
    private Vector3 chainTextBaseScale = Vector3.one;
    private int pendingTextRefreshFrames;
    private Color chainTextBaseColor = Color.white;
    private Tween chainTextColorTween;
    private Tween chainTimerBarColorTween;

    private void Awake()
    {
        if (chainCombat == null) chainCombat = GetComponent<ChainCombatController>();
        if (chainCombat == null) chainCombat = GetComponentInParent<ChainCombatController>();
        if (chainCombat == null) chainCombat = FindObjectOfType<ChainCombatController>();
        if (damageSystem == null) damageSystem = GetComponent<DamageSystem>();
        if (damageSystem == null) damageSystem = GetComponentInParent<DamageSystem>();
        if (damageSystem == null) damageSystem = FindObjectOfType<DamageSystem>();
        if (hitSequence == null) hitSequence = GetComponent<HitSequenceController>();
        if (hitSequence == null) hitSequence = GetComponentInParent<HitSequenceController>();
        if (hitSequence == null) hitSequence = FindObjectOfType<HitSequenceController>();
        if (chainUI == null) chainUI = GetComponentInChildren<ChainUI>(true);

        if (chainTextRoot == null && chainText != null) chainTextRoot = chainText.rectTransform;
        if (chainTextRoot == null && chainPanel != null) chainTextRoot = chainPanel.GetComponent<RectTransform>();
        if (chainTextGroup == null && chainPanel != null) chainTextGroup = chainPanel.GetComponent<CanvasGroup>();
        if (chainTextRoot != null) chainTextBaseScale = chainTextRoot.localScale;
        if (damageTextFontAsset == null && chainText != null) damageTextFontAsset = chainText.font;
        if (chainText != null) chainTextBaseColor = chainText.color;
        EnsureChainTimerBar();

        if (darkenRoot == null)
        {
            if (darkenGroup != null) darkenRoot = darkenGroup.transform;
            else if (darkenGraphic != null) darkenRoot = darkenGraphic.transform;
            else if (darkenSprite != null) darkenRoot = darkenSprite.transform;
        }
        if (darkenGraphic == null && darkenGroup == null && darkenRoot != null)
        {
            darkenGraphic = darkenRoot.GetComponent<Graphic>();
        }
        if (darkenSprite == null && darkenGroup == null && darkenRoot != null)
        {
            darkenSprite = darkenRoot.GetComponent<SpriteRenderer>();
        }

        if (darkenRoot != null) darkenBaseScale = darkenRoot.localScale;

        ForceResetVisualState();
    }

    private void OnEnable()
    {
        ForceResetVisualState();

        if (chainCombat != null)
        {
            chainCombat.OnSlowStateChanged += HandleSlowStateChanged;
            chainCombat.OnChainMilestoneReached += HandleChainMilestoneReached;
            HandleSlowStateChanged(chainCombat.IsSlowActive);
        }
        if (damageSystem != null)
        {
            damageSystem.OnDamageApplied += HandleDamageApplied;
        }
    }

    private void OnDisable()
    {
        if (chainCombat != null)
        {
            chainCombat.OnSlowStateChanged -= HandleSlowStateChanged;
            chainCombat.OnChainMilestoneReached -= HandleChainMilestoneReached;
        }
        if (damageSystem != null)
        {
            damageSystem.OnDamageApplied -= HandleDamageApplied;
        }

        KillTweens();
        ForceResetVisualState();
    }

    private void HandleSlowStateChanged(bool isActive)
    {
        if (isActive == isChainActive)
        {
            if (isActive)
            {
                UpdateChainText();
            }
            return;
        }

        isChainActive = isActive;
        if (!isActive)
        {
            PlayDarken(false);
            HideChain();
            ResetChainBeatImmediate();
            ResetChainTimerBarImmediate();
            lastChain = -1;
            pendingTextRefreshFrames = 0;
            return;
        }

        PlayDarken(true);
    }

    private void HandleDamageApplied(DamageSystem.DamageResult result)
    {
        TrySpawnDamageText(result);

        if (result.IsDead)
        {
            PlayKillFinishBeat();
            TrySpawnChainKillPrefab(result.Target);
            pendingTextRefreshFrames = 0;
            return;
        }

        pendingTextRefreshFrames = 3;
        UpdateChainText();
    }

    public void BindSceneRefs(
        ChainUI externalChainUI,
        GameObject externalChainPanel,
        TextMeshProUGUI externalChainText,
        Transform externalDarkenRoot,
        SpriteRenderer externalDarkenSprite,
        ChainCombatController externalChainCombat = null,
        DamageSystem externalDamageSystem = null)
    {
        if (externalChainCombat != null && chainCombat != externalChainCombat)
        {
            if (chainCombat != null)
            {
                chainCombat.OnSlowStateChanged -= HandleSlowStateChanged;
                chainCombat.OnChainMilestoneReached -= HandleChainMilestoneReached;
            }
            chainCombat = externalChainCombat;
            if (isActiveAndEnabled)
            {
                chainCombat.OnSlowStateChanged += HandleSlowStateChanged;
                chainCombat.OnChainMilestoneReached += HandleChainMilestoneReached;
            }
        }

        if (externalDamageSystem != null && damageSystem != externalDamageSystem)
        {
            if (damageSystem != null) damageSystem.OnDamageApplied -= HandleDamageApplied;
            damageSystem = externalDamageSystem;
            if (isActiveAndEnabled) damageSystem.OnDamageApplied += HandleDamageApplied;
        }

        if (externalChainUI != null) chainUI = externalChainUI;
        if (externalChainPanel != null) chainPanel = externalChainPanel;
        if (externalChainText != null) chainText = externalChainText;
        if (externalDarkenRoot != null) darkenRoot = externalDarkenRoot;
        if (externalDarkenSprite != null) darkenSprite = externalDarkenSprite;

        if (chainTextRoot == null && chainText != null) chainTextRoot = chainText.rectTransform;
        if (chainTextRoot == null && chainPanel != null) chainTextRoot = chainPanel.GetComponent<RectTransform>();
        if (chainTextGroup == null && chainPanel != null) chainTextGroup = chainPanel.GetComponent<CanvasGroup>();
        if (chainTextRoot != null) chainTextBaseScale = chainTextRoot.localScale;
        if (chainText != null) chainTextBaseColor = chainText.color;
        EnsureChainTimerBar();
        if (darkenGroup == null && darkenRoot != null) darkenGroup = darkenRoot.GetComponent<CanvasGroup>();
        if (darkenGraphic == null && darkenRoot != null) darkenGraphic = darkenRoot.GetComponent<Graphic>();
        if (darkenSprite == null && darkenRoot != null) darkenSprite = darkenRoot.GetComponent<SpriteRenderer>();

        if (darkenRoot != null)
        {
            darkenBaseScale = darkenRoot.localScale;
        }
    }

    private void UpdateChainText()
    {
        if (chainCombat == null) return;
        if (!chainCombat.IsSlowActive) return;
        var chain = chainCombat.CurrentChain;
        if (chain <= 0) return;
        if (chain == lastChain && IsChainVisible()) return;
        lastChain = chain;
        ShowChain(chain);
    }

    private void LateUpdate()
    {
        UpdateChainTimerBar();

        if (pendingTextRefreshFrames <= 0) return;

        UpdateChainText();
        if (IsChainVisible())
        {
            pendingTextRefreshFrames = 0;
            return;
        }

        pendingTextRefreshFrames--;
    }

    private void ForceResetVisualState()
    {
        HideChainImmediate();
        ResetChainBeatImmediate();
        ResetChainTimerBarImmediate();
        ResetDarkenImmediate();
        isChainActive = false;
        lastChain = -1;
        pendingTextRefreshFrames = 0;
    }

    private void HandleChainMilestoneReached(int chain)
    {
        PlayChainMilestoneBeat(chain);
    }
}
