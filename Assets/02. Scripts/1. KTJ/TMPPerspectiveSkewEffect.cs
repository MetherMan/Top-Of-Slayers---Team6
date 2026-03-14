using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class TMPPerspectiveSkewEffect : MonoBehaviour
{
    [SerializeField] private float shear = -34f;
    [SerializeField, Min(0f)] private float perspectiveInset = 18f;
    [SerializeField] private bool recedeRight = true;

    private TMP_Text textComponent;
    private bool isDirty = true;

    public float Shear
    {
        get => shear;
        set
        {
            if (Mathf.Approximately(shear, value)) return;
            shear = value;
            Refresh();
        }
    }

    public float PerspectiveInset
    {
        get => perspectiveInset;
        set
        {
            if (Mathf.Approximately(perspectiveInset, value)) return;
            perspectiveInset = Mathf.Max(0f, value);
            Refresh();
        }
    }

    public bool RecedeRight
    {
        get => recedeRight;
        set
        {
            if (recedeRight == value) return;
            recedeRight = value;
            Refresh();
        }
    }

    public void Refresh()
    {
        isDirty = true;

        if (textComponent == null)
        {
            textComponent = GetComponent<TMP_Text>();
        }

        if (textComponent != null)
        {
            textComponent.havePropertiesChanged = true;
        }
    }

    private void Awake()
    {
        textComponent = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        if (textComponent == null)
        {
            textComponent = GetComponent<TMP_Text>();
        }

        TMPro_EventManager.TEXT_CHANGED_EVENT.Add(HandleTextChanged);
        Refresh();
    }

    private void OnDisable()
    {
        TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(HandleTextChanged);
        RestoreBaseMesh();
    }

    private void OnValidate()
    {
        Refresh();
    }

    private void LateUpdate()
    {
        if (!isActiveAndEnabled) return;
        if (textComponent == null) return;
        if (!isDirty && !textComponent.havePropertiesChanged) return;

        ApplyPerspective();
    }

    private void HandleTextChanged(Object changedObject)
    {
        if (changedObject == textComponent)
        {
            Refresh();
        }
    }

    private void ApplyPerspective()
    {
        textComponent.ForceMeshUpdate();
        var textInfo = textComponent.textInfo;
        if (textInfo == null || textInfo.characterCount == 0)
        {
            isDirty = false;
            return;
        }

        var hasVisibleCharacter = false;
        var minX = float.MaxValue;
        var maxX = float.MinValue;
        var minY = float.MaxValue;
        var maxY = float.MinValue;

        for (var i = 0; i < textInfo.characterCount; i++)
        {
            var character = textInfo.characterInfo[i];
            if (!character.isVisible) continue;

            hasVisibleCharacter = true;
            var bottomLeft = character.bottomLeft;
            var topRight = character.topRight;

            if (bottomLeft.x < minX) minX = bottomLeft.x;
            if (topRight.x > maxX) maxX = topRight.x;
            if (bottomLeft.y < minY) minY = bottomLeft.y;
            if (topRight.y > maxY) maxY = topRight.y;
        }

        if (!hasVisibleCharacter)
        {
            isDirty = false;
            return;
        }

        if (!PerspectiveWarpUtility.TryBuildQuad(
                minX,
                maxX,
                minY,
                maxY,
                shear,
                perspectiveInset,
                recedeRight,
                out var warpedBottomLeft,
                out var warpedTopLeft,
                out var warpedTopRight,
                out var warpedBottomRight,
                out var minBounds,
                out var maxBounds))
        {
            isDirty = false;
            return;
        }

        for (var i = 0; i < textInfo.characterCount; i++)
        {
            var character = textInfo.characterInfo[i];
            if (!character.isVisible) continue;

            var materialIndex = character.materialReferenceIndex;
            var vertexIndex = character.vertexIndex;
            var vertices = textInfo.meshInfo[materialIndex].vertices;

            vertices[vertexIndex] = PerspectiveWarpUtility.Warp(vertices[vertexIndex], minBounds, maxBounds, warpedBottomLeft, warpedTopLeft, warpedTopRight, warpedBottomRight);
            vertices[vertexIndex + 1] = PerspectiveWarpUtility.Warp(vertices[vertexIndex + 1], minBounds, maxBounds, warpedBottomLeft, warpedTopLeft, warpedTopRight, warpedBottomRight);
            vertices[vertexIndex + 2] = PerspectiveWarpUtility.Warp(vertices[vertexIndex + 2], minBounds, maxBounds, warpedBottomLeft, warpedTopLeft, warpedTopRight, warpedBottomRight);
            vertices[vertexIndex + 3] = PerspectiveWarpUtility.Warp(vertices[vertexIndex + 3], minBounds, maxBounds, warpedBottomLeft, warpedTopLeft, warpedTopRight, warpedBottomRight);
        }

        for (var i = 0; i < textInfo.meshInfo.Length; i++)
        {
            var meshInfo = textInfo.meshInfo[i];
            meshInfo.mesh.vertices = meshInfo.vertices;
            textComponent.UpdateGeometry(meshInfo.mesh, i);
        }

        textComponent.havePropertiesChanged = false;
        isDirty = false;
    }

    private void RestoreBaseMesh()
    {
        if (textComponent == null) return;

        textComponent.ForceMeshUpdate();
        var textInfo = textComponent.textInfo;
        if (textInfo == null) return;

        for (var i = 0; i < textInfo.meshInfo.Length; i++)
        {
            var meshInfo = textInfo.meshInfo[i];
            meshInfo.mesh.vertices = meshInfo.vertices;
            textComponent.UpdateGeometry(meshInfo.mesh, i);
        }

        textComponent.havePropertiesChanged = false;
        isDirty = false;
    }
}
