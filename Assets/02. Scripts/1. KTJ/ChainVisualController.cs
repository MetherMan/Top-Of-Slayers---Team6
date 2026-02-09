using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class ChainVisualController : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private ChainCombatController chainCombat;
    [SerializeField] private DamageSystem damageSystem;
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

    private int lastChain = -1;
    private Tween darkenTween;
    private bool isChainActive;
    private Vector3 darkenBaseScale = Vector3.one;
    private int pendingTextRefreshFrames;

    private void Awake()
    {
        if (chainCombat == null) chainCombat = GetComponent<ChainCombatController>();
        if (chainCombat == null) chainCombat = GetComponentInParent<ChainCombatController>();
        if (damageSystem == null) damageSystem = GetComponent<DamageSystem>();
        if (damageSystem == null) damageSystem = GetComponentInParent<DamageSystem>();
        if (chainUI == null) chainUI = GetComponentInChildren<ChainUI>(true);

        if (chainTextRoot == null && chainText != null) chainTextRoot = chainText.rectTransform;
        if (chainTextRoot == null && chainPanel != null) chainTextRoot = chainPanel.GetComponent<RectTransform>();
        if (chainTextGroup == null && chainPanel != null) chainTextGroup = chainPanel.GetComponent<CanvasGroup>();

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
            lastChain = -1;
            pendingTextRefreshFrames = 0;
            return;
        }

        PlayDarken(true);
    }

    private void HandleDamageApplied(DamageSystem.DamageResult result)
    {
        if (result.IsDead)
        {
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
            if (chainCombat != null) chainCombat.OnSlowStateChanged -= HandleSlowStateChanged;
            chainCombat = externalChainCombat;
            if (isActiveAndEnabled) chainCombat.OnSlowStateChanged += HandleSlowStateChanged;
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
        if (darkenGroup == null && darkenRoot != null) darkenGroup = darkenRoot.GetComponent<CanvasGroup>();
        if (darkenGraphic == null && darkenRoot != null) darkenGraphic = darkenRoot.GetComponent<Graphic>();
        if (darkenSprite == null && darkenRoot != null) darkenSprite = darkenRoot.GetComponent<SpriteRenderer>();
        if (darkenRoot != null) darkenBaseScale = darkenRoot.localScale;
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
        ResetDarkenImmediate();
        isChainActive = false;
        lastChain = -1;
        pendingTextRefreshFrames = 0;
    }
}
