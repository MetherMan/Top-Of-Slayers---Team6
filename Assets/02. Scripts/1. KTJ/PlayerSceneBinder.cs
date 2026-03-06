using TMPro;
using UnityEngine;

public class PlayerSceneBinder : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private PlayerMoveController moveController;
    [SerializeField] private SlashDashController slashDashController;
    [SerializeField] private ChainCombatController chainCombat;
    [SerializeField] private ChainVisualController chainVisual;
    [SerializeField] private DamageSystem damageSystem;
    [SerializeField] private PlayerHP playerHp;

    [Header("씬 참조")]
    [SerializeField] private VirtualJoystickController joystick;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private ChainUI chainUI;
    [SerializeField] private GameObject chainPanel;
    [SerializeField] private TextMeshProUGUI chainText;
    [SerializeField] private Transform darkenRoot;
    [SerializeField] private SpriteRenderer darkenSprite;

    [Header("자동 탐색")]
    [SerializeField] private bool autoFindSceneRefs = true;
    [SerializeField, Min(1)] private int maxBindFrames = 30;
    [SerializeField] private bool applyEquipmentStats = true;

    private EquipmentManager equipmentManager;
    private bool isEquipmentSubscribed;
    private int basePlayerMaxHp;
    private bool hasBasePlayerMaxHp;
    private int bindRetryCount;
    private bool keepBindingSceneRefs;

    private void Awake()
    {
        CachePlayerRefs();
        CacheBasePlayerHp();
    }

    private void OnEnable()
    {
        keepBindingSceneRefs = true;
        bindRetryCount = 0;
        TryBindSceneRefs();
        TryBindEquipment();
        ApplyEquipmentStats();
    }

    private void OnDisable()
    {
        keepBindingSceneRefs = false;

        if (isEquipmentSubscribed && equipmentManager != null)
        {
            equipmentManager.OnEquipmentChanged -= HandleEquipmentChanged;
            isEquipmentSubscribed = false;
        }
    }

    private void LateUpdate()
    {
        if (keepBindingSceneRefs)
        {
            TryBindSceneRefs();
        }

        if (!applyEquipmentStats || isEquipmentSubscribed) return;

        TryBindEquipment();
        if (isEquipmentSubscribed)
        {
            ApplyEquipmentStats();
        }
    }

    private void TryBindSceneRefs()
    {
        CachePlayerRefs();
        if (autoFindSceneRefs)
        {
            CacheSceneRefs();
        }

        ApplyBindings();
        if (IsBindReady())
        {
            keepBindingSceneRefs = false;
            return;
        }

        bindRetryCount++;
        if (bindRetryCount >= Mathf.Max(1, maxBindFrames))
        {
            keepBindingSceneRefs = false;
        }
    }

    private void CachePlayerRefs()
    {
        if (moveController == null) moveController = GetComponent<PlayerMoveController>();
        if (moveController == null) moveController = GetComponentInParent<PlayerMoveController>();
        if (moveController == null) moveController = GetComponentInChildren<PlayerMoveController>(true);
        if (moveController == null) moveController = FindObjectOfType<PlayerMoveController>();

        if (chainCombat == null) chainCombat = GetComponent<ChainCombatController>();
        if (chainCombat == null) chainCombat = GetComponentInParent<ChainCombatController>();
        if (chainCombat == null) chainCombat = GetComponentInChildren<ChainCombatController>(true);
        if (chainCombat == null) chainCombat = FindObjectOfType<ChainCombatController>();

        if (chainVisual == null) chainVisual = GetComponent<ChainVisualController>();
        if (chainVisual == null) chainVisual = GetComponentInParent<ChainVisualController>();
        if (chainVisual == null) chainVisual = GetComponentInChildren<ChainVisualController>(true);
        if (chainVisual == null) chainVisual = FindSceneChainVisual();

        if (slashDashController == null) slashDashController = GetComponent<SlashDashController>();
        if (slashDashController == null) slashDashController = GetComponentInParent<SlashDashController>();
        if (slashDashController == null) slashDashController = GetComponentInChildren<SlashDashController>(true);
        if (slashDashController == null) slashDashController = FindObjectOfType<SlashDashController>();

        if (damageSystem == null) damageSystem = GetComponent<DamageSystem>();
        if (damageSystem == null) damageSystem = GetComponentInParent<DamageSystem>();
        if (damageSystem == null) damageSystem = GetComponentInChildren<DamageSystem>(true);
        if (damageSystem == null) damageSystem = FindObjectOfType<DamageSystem>();

        if (playerHp == null) playerHp = GetComponent<PlayerHP>();
        if (playerHp == null) playerHp = GetComponentInParent<PlayerHP>();
        if (playerHp == null) playerHp = GetComponentInChildren<PlayerHP>(true);
        if (playerHp == null) playerHp = FindObjectOfType<PlayerHP>();
        CacheBasePlayerHp();
    }

    private void CacheBasePlayerHp()
    {
        if (hasBasePlayerMaxHp || playerHp == null) return;
        basePlayerMaxHp = Mathf.Max(1, playerHp.maxHP);
        hasBasePlayerMaxHp = true;
    }

    private void CacheSceneRefs()
    {
        if (joystick == null)
        {
            joystick = FindObjectOfType<VirtualJoystickController>();
        }

        if (cameraTransform == null)
        {
            CacheSceneCamera();
        }

        if (chainUI == null)
        {
            chainUI = FindSceneChainUI();
        }

        CacheChainUiObjects();
        CacheDarkOverlay();
    }

    private void CacheSceneCamera()
    {
        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
            return;
        }

        var camera = FindObjectOfType<Camera>();
        if (camera != null)
        {
            cameraTransform = camera.transform;
        }
    }

    private ChainVisualController FindSceneChainVisual()
    {
        var visuals = FindObjectsOfType<ChainVisualController>(true);
        if (visuals == null || visuals.Length == 0) return null;
        return visuals[0];
    }

    private ChainUI FindSceneChainUI()
    {
        var chainUis = FindObjectsOfType<ChainUI>(true);
        if (chainUis == null || chainUis.Length == 0) return null;
        return chainUis[0];
    }

    private void CacheChainUiObjects()
    {
        if (chainPanel == null && chainUI != null)
        {
            chainPanel = FindChainPanel(chainUI.transform);
        }
        if (chainText == null && chainPanel != null)
        {
            chainText = chainPanel.GetComponentInChildren<TextMeshProUGUI>(true);
        }
        if (chainText == null && chainUI != null)
        {
            chainText = FindChainText(chainUI.transform);
        }
        if (chainText == null)
        {
            chainText = FindSceneChainText();
        }
        if (chainPanel == null && chainText != null)
        {
            chainPanel = chainText.gameObject;
        }
    }

    private GameObject FindChainPanel(Transform root)
    {
        if (root == null) return null;

        var panels = root.GetComponentsInChildren<RectTransform>(true);
        for (int i = 0; i < panels.Length; i++)
        {
            var candidate = panels[i];
            if (candidate == null) continue;
            if (!IsChainUiName(candidate.name)) continue;
            return candidate.gameObject;
        }

        return null;
    }

    private TextMeshProUGUI FindChainText(Transform root)
    {
        if (root == null) return null;

        var texts = root.GetComponentsInChildren<TextMeshProUGUI>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            var candidate = texts[i];
            if (candidate == null) continue;
            if (!IsChainUiName(candidate.name)) continue;
            return candidate;
        }

        return null;
    }

    private TextMeshProUGUI FindSceneChainText()
    {
        var texts = FindObjectsOfType<TextMeshProUGUI>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            var candidate = texts[i];
            if (candidate == null) continue;
            if (!IsChainUiName(candidate.name)) continue;
            return candidate;
        }

        return null;
    }

    private void CacheDarkOverlay()
    {
        if (darkenSprite == null && chainVisual != null)
        {
            darkenSprite = FindDarkenSprite(chainVisual.transform);
        }
        if (darkenSprite == null)
        {
            darkenSprite = FindDarkenSprite(null);
        }

        if (darkenRoot == null && darkenSprite != null)
        {
            darkenRoot = darkenSprite.transform;
        }
        if (darkenSprite == null && darkenRoot != null)
        {
            darkenSprite = darkenRoot.GetComponent<SpriteRenderer>();
        }
    }

    private SpriteRenderer FindDarkenSprite(Transform root)
    {
        SpriteRenderer[] sprites = root != null
            ? root.GetComponentsInChildren<SpriteRenderer>(true)
            : FindObjectsOfType<SpriteRenderer>(true);

        for (int i = 0; i < sprites.Length; i++)
        {
            var candidate = sprites[i];
            if (candidate == null) continue;
            if (!IsDarkenSpriteName(candidate.name)) continue;
            return candidate;
        }

        return null;
    }

    private static bool IsChainUiName(string nameText)
    {
        if (string.IsNullOrEmpty(nameText)) return false;
        return nameText.IndexOf("chain", System.StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static bool IsDarkenSpriteName(string nameText)
    {
        if (string.IsNullOrEmpty(nameText)) return false;
        return nameText.IndexOf("chainblack", System.StringComparison.OrdinalIgnoreCase) >= 0
            || nameText.IndexOf("chaindark", System.StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private void ApplyBindings()
    {
        if (moveController != null)
        {
            moveController.BindSceneRefs(joystick, cameraTransform, chainCombat);
        }

        if (chainCombat != null)
        {
            chainCombat.BindSceneRefs(damageSystem, moveController);
        }

        if (chainVisual != null)
        {
            chainVisual.BindSceneRefs(chainUI, chainPanel, chainText, darkenRoot, darkenSprite, chainCombat, damageSystem);
        }
    }

    private void TryBindEquipment()
    {
        if (!applyEquipmentStats) return;
        if (isEquipmentSubscribed && equipmentManager != null) return;
        if (!EquipmentManager.HasInstance) return;

        equipmentManager = EquipmentManager.Instance;
        if (equipmentManager == null) return;

        equipmentManager.OnEquipmentChanged -= HandleEquipmentChanged;
        equipmentManager.OnEquipmentChanged += HandleEquipmentChanged;
        isEquipmentSubscribed = true;
    }

    private void HandleEquipmentChanged()
    {
        ApplyEquipmentStats();
    }

    private void ApplyEquipmentStats()
    {
        var spec = ResolveAttackSpec();
        int bonusAttack = 0;
        int bonusSpeed = 0;
        int bonusHp = 0;
        float bonusCritical = 0f;
        int bonusHeal = 0;

        if (applyEquipmentStats && equipmentManager == null)
        {
            if (EquipmentManager.HasInstance)
            {
                equipmentManager = EquipmentManager.Instance;
            }
        }

        if (applyEquipmentStats && equipmentManager != null)
        {
            AccumulateEquipmentStats(equipmentManager.weapon, ref bonusAttack, ref bonusSpeed, ref bonusHp, ref bonusCritical, ref bonusHeal);
            AccumulateEquipmentStats(equipmentManager.shoes, ref bonusAttack, ref bonusSpeed, ref bonusHp, ref bonusCritical, ref bonusHeal);
            AccumulateEquipmentStats(equipmentManager.gloves, ref bonusAttack, ref bonusSpeed, ref bonusHp, ref bonusCritical, ref bonusHeal);
            AccumulateEquipmentStats(equipmentManager.armor, ref bonusAttack, ref bonusSpeed, ref bonusHp, ref bonusCritical, ref bonusHeal);
            AccumulateEquipmentStats(equipmentManager.emblem, ref bonusAttack, ref bonusSpeed, ref bonusHp, ref bonusCritical, ref bonusHeal);
        }

        if (moveController != null)
        {
            moveController.SetPlayerMoveSpeed(spec != null ? spec.GetSpeed(0f) : 0f);
            moveController.SetEquipmentMoveSpeedBonus(bonusSpeed);
        }

        if (slashDashController != null)
        {
            slashDashController.SetPlayerCombatStats(
                spec != null ? spec.GetCritical() : 0f,
                spec != null ? spec.GetHeal() : 0);
            slashDashController.SetEquipmentCombatBonus(bonusAttack, bonusCritical, bonusHeal);
        }

        ApplyHpBonus(spec, bonusHp);
    }

    private void AccumulateEquipmentStats(
        InventoryItem inventoryItem,
        ref int bonusAttack,
        ref int bonusSpeed,
        ref int bonusHp,
        ref float bonusCritical,
        ref int bonusHeal)
    {
        if (inventoryItem == null) return;
        if (!(inventoryItem.item is EquipmentSO equipment)) return;

        int level = Mathf.Max(0, inventoryItem.enhancementLevel);
        bonusAttack += Mathf.Max(0, equipment.GetAttack(level));
        bonusSpeed += Mathf.Max(0, equipment.GetSpeed(level));
        bonusHp += Mathf.Max(0, equipment.GetHP(level));
        bonusCritical += Mathf.Max(0f, equipment.GetCritical(level));
        bonusHeal += Mathf.Max(0, equipment.GetHeal(level));
    }

    private void ApplyHpBonus(AttackSpecSO spec, int hpBonus)
    {
        if (playerHp == null) return;
        if (!hasBasePlayerMaxHp)
        {
            basePlayerMaxHp = Mathf.Max(1, playerHp.maxHP);
            hasBasePlayerMaxHp = true;
        }

        int configuredBaseHp = ResolveBasePlayerMaxHp(spec);
        int previousMaxHp = Mathf.Max(1, playerHp.maxHP);
        int nextMaxHp = Mathf.Max(1, configuredBaseHp + Mathf.Max(0, hpBonus));
        if (previousMaxHp == nextMaxHp)
        {
            playerHp.SetHpState(nextMaxHp, Mathf.Clamp(playerHp.currentHP, 0, nextMaxHp));
            return;
        }

        int nextHp = Mathf.Clamp(playerHp.currentHP, 0, previousMaxHp);
        int delta = nextMaxHp - previousMaxHp;
        if (delta > 0)
        {
            nextHp = Mathf.Min(nextHp + delta, nextMaxHp);
        }
        else
        {
            nextHp = Mathf.Min(nextHp, nextMaxHp);
        }

        playerHp.SetHpState(nextMaxHp, nextHp);
    }

    private AttackSpecSO ResolveAttackSpec()
    {
        if (slashDashController == null) return null;
        return slashDashController.Spec;
    }

    private int ResolveBasePlayerMaxHp(AttackSpecSO spec)
    {
        if (spec != null)
        {
            return spec.GetHP(basePlayerMaxHp);
        }

        return Mathf.Max(1, basePlayerMaxHp);
    }

    private bool IsBindReady()
    {
        if (moveController == null) return false;
        if (chainCombat == null) return false;
        if (damageSystem == null) return false;
        if (joystick == null) return false;
        if (cameraTransform == null) return false;
        if (chainVisual == null) return false;
        if (chainPanel == null) return false;
        if (chainText == null) return false;
        if (darkenRoot == null && darkenSprite == null) return false;
        return true;
    }
}
