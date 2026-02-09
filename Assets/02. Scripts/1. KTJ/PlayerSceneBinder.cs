using System.Collections;
using TMPro;
using UnityEngine;

public class PlayerSceneBinder : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private PlayerMoveController moveController;
    [SerializeField] private ChainCombatController chainCombat;
    [SerializeField] private ChainVisualController chainVisual;
    [SerializeField] private DamageSystem damageSystem;

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

    private Coroutine bindRoutine;

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
    }

    private void OnDisable()
    {
        if (bindRoutine != null)
        {
            StopCoroutine(bindRoutine);
            bindRoutine = null;
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

        if (chainCombat == null) chainCombat = GetComponent<ChainCombatController>();
        if (chainCombat == null) chainCombat = GetComponentInParent<ChainCombatController>();
        if (chainCombat == null) chainCombat = GetComponentInChildren<ChainCombatController>(true);

        if (chainVisual == null) chainVisual = GetComponent<ChainVisualController>();
        if (chainVisual == null) chainVisual = GetComponentInParent<ChainVisualController>();
        if (chainVisual == null) chainVisual = GetComponentInChildren<ChainVisualController>(true);
        if (chainVisual == null)
        {
            var visuals = FindObjectsOfType<ChainVisualController>(true);
            if (visuals != null && visuals.Length > 0) chainVisual = visuals[0];
        }

        if (damageSystem == null) damageSystem = GetComponent<DamageSystem>();
        if (damageSystem == null) damageSystem = GetComponentInParent<DamageSystem>();
        if (damageSystem == null) damageSystem = GetComponentInChildren<DamageSystem>(true);
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
                if (!candidate.name.ToLower().Contains("chainblack")) continue;
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

        if (chainVisual != null)
        {
            chainVisual.BindSceneRefs(chainUI, chainPanel, chainText, darkenRoot, darkenSprite, chainCombat, damageSystem);
        }
    }

    private bool IsBindReady()
    {
        if (moveController == null) return false;
        if (joystick == null) return false;
        if (cameraTransform == null) return false;
        if (chainVisual == null) return false;
        if (chainPanel == null) return false;
        if (chainText == null) return false;
        if (darkenRoot == null && darkenSprite == null) return false;
        return true;
    }
}
