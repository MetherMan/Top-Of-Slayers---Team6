using System.Collections;
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

    private Coroutine bindRoutine;
    private EquipmentManager equipmentManager;
    private bool isEquipmentSubscribed;
    private int basePlayerMaxHp;
    private bool hasBasePlayerMaxHp;

    private void Awake()
    {
        ResolveLocalRefs();
    }

    private void OnEnable()
    {
        if (bindRoutine != null)
        {
            StopCoroutine(bindRoutine);
        }

        bindRoutine = StartCoroutine(BindRoutine());
        TryBindEquipment();
        ApplyEquipmentStats();
    }

    private void OnDisable()
    {
        if (bindRoutine != null)
        {
            StopCoroutine(bindRoutine);
            bindRoutine = null;
        }

        if (isEquipmentSubscribed && equipmentManager != null)
        {
            equipmentManager.OnEquipmentChanged -= HandleEquipmentChanged;
            isEquipmentSubscribed = false;
        }
    }

    private void LateUpdate()
    {
        if (!applyEquipmentStats) return;
        if (isEquipmentSubscribed) return;

        TryBindEquipment();
        if (isEquipmentSubscribed)
        {
            ApplyEquipmentStats();
        }
    }

    private IEnumerator BindRoutine()
    {
        for (int i = 0; i < maxBindFrames; i++)
        {
            ResolveLocalRefs();
            if (autoFindSceneRefs)
            {
                ResolveSceneRefs();
            }

            ApplyBindings();
            TryBindEquipment();
            ApplyEquipmentStats();
            if (IsBindReady())
            {
                bindRoutine = null;
                yield break;
            }

            yield return null;
        }

        bindRoutine = null;
    }

    private void ResolveLocalRefs()
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
        if (chainVisual == null)
        {
            var visuals = FindObjectsOfType<ChainVisualController>(true);
            if (visuals != null && visuals.Length > 0) chainVisual = visuals[0];
        }

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
        if (!hasBasePlayerMaxHp && playerHp != null)
        {
            basePlayerMaxHp = Mathf.Max(1, playerHp.maxHP);
            hasBasePlayerMaxHp = true;
        }
    }

    private void ResolveSceneRefs()
    {
        if (joystick == null)
        {
            joystick = FindObjectOfType<VirtualJoystickController>();
        }

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
        if (cameraTransform == null)
        {
            var camera = FindObjectOfType<Camera>();
            if (camera != null) cameraTransform = camera.transform;
        }

        if (chainUI == null)
        {
            var chainUIs = FindObjectsOfType<ChainUI>(true);
            if (chainUIs != null && chainUIs.Length > 0)
            {
                chainUI = chainUIs[0];
            }
        }

        if (chainPanel == null && chainUI != null)
        {
            var chainPanels = chainUI.GetComponentsInChildren<RectTransform>(true);
            for (int i = 0; i < chainPanels.Length; i++)
            {
                var candidate = chainPanels[i];
                if (candidate == null) continue;
                if (!candidate.name.ToLower().Contains("chain")) continue;
                chainPanel = candidate.gameObject;
                break;
            }
        }
        if (chainText == null && chainPanel != null)
        {
            chainText = chainPanel.GetComponentInChildren<TextMeshProUGUI>(true);
        }
        if (chainText == null)
        {
            var texts = FindObjectsOfType<TextMeshProUGUI>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                var candidate = texts[i];
                if (candidate == null) continue;
                var lowerName = candidate.name.ToLower();
                if (!lowerName.Contains("chain")) continue;
                chainText = candidate;
                break;
            }
        }
        if (chainPanel == null && chainText != null)
        {
            chainPanel = chainText.gameObject;
        }

        if (darkenRoot == null)
        {
            var sprites = FindObjectsOfType<SpriteRenderer>(true);
            for (int i = 0; i < sprites.Length; i++)
            {
                var candidate = sprites[i];
                if (candidate == null) continue;
                var lowerName = candidate.name.ToLower();
                if (!lowerName.Contains("chainblack") && !lowerName.Contains("chaindark")) continue;
                darkenSprite = candidate;
                darkenRoot = candidate.transform;
                break;
            }
        }
        if (darkenSprite == null && darkenRoot != null)
        {
            darkenSprite = darkenRoot.GetComponent<SpriteRenderer>();
        }
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
        if (!applyEquipmentStats) return;
        if (equipmentManager == null)
        {
            if (!EquipmentManager.HasInstance) return;
            equipmentManager = EquipmentManager.Instance;
        }

        if (equipmentManager == null) return;

        int bonusAttack = 0;
        int bonusSpeed = 0;
        int bonusHp = 0;
        float bonusCritical = 0f;
        int bonusHeal = 0;

        AccumulateEquipmentStats(equipmentManager.weapon, ref bonusAttack, ref bonusSpeed, ref bonusHp, ref bonusCritical, ref bonusHeal);
        AccumulateEquipmentStats(equipmentManager.shoes, ref bonusAttack, ref bonusSpeed, ref bonusHp, ref bonusCritical, ref bonusHeal);
        AccumulateEquipmentStats(equipmentManager.gloves, ref bonusAttack, ref bonusSpeed, ref bonusHp, ref bonusCritical, ref bonusHeal);
        AccumulateEquipmentStats(equipmentManager.armor, ref bonusAttack, ref bonusSpeed, ref bonusHp, ref bonusCritical, ref bonusHeal);
        AccumulateEquipmentStats(equipmentManager.emblem, ref bonusAttack, ref bonusSpeed, ref bonusHp, ref bonusCritical, ref bonusHeal);

        if (moveController != null)
        {
            moveController.SetEquipmentMoveSpeedBonus(bonusSpeed);
        }

        if (slashDashController != null)
        {
            slashDashController.SetEquipmentCombatBonus(bonusAttack, bonusCritical, bonusHeal);
        }

        ApplyHpBonus(bonusHp);
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

    private void ApplyHpBonus(int hpBonus)
    {
        if (playerHp == null) return;
        if (!hasBasePlayerMaxHp)
        {
            basePlayerMaxHp = Mathf.Max(1, playerHp.maxHP);
            hasBasePlayerMaxHp = true;
        }

        int previousMaxHp = Mathf.Max(1, playerHp.maxHP);
        int nextMaxHp = Mathf.Max(1, basePlayerMaxHp + Mathf.Max(0, hpBonus));
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
