using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class UIPerspectiveSkewEffect : BaseMeshEffect
{
    [SerializeField] private float shear = -34f;
    [SerializeField, Min(0f)] private float perspectiveInset = 18f;
    [SerializeField] private bool recedeRight = true;

    private readonly List<UIVertex> vertices = new List<UIVertex>();

    public float Shear
    {
        get => shear;
        set
        {
            if (Mathf.Approximately(shear, value)) return;
            shear = value;
            SetDirty();
        }
    }

    public float PerspectiveInset
    {
        get => perspectiveInset;
        set
        {
            if (Mathf.Approximately(perspectiveInset, value)) return;
            perspectiveInset = Mathf.Max(0f, value);
            SetDirty();
        }
    }

    public bool RecedeRight
    {
        get => recedeRight;
        set
        {
            if (recedeRight == value) return;
            recedeRight = value;
            SetDirty();
        }
    }

    public void Refresh()
    {
        SetDirty();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        SetDirty();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        SetDirty();
    }

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive()) return;
        if (vh.currentVertCount <= 0) return;

        vertices.Clear();
        vh.GetUIVertexStream(vertices);
        if (vertices.Count == 0) return;

        if (!PerspectiveWarpUtility.TryBuildQuad(vertices, shear, perspectiveInset, recedeRight,
                out var bottomLeft, out var topLeft, out var topRight, out var bottomRight,
                out var minBounds, out var maxBounds))
        {
            return;
        }

        for (var i = 0; i < vertices.Count; i++)
        {
            var vertex = vertices[i];
            var warped = PerspectiveWarpUtility.Warp(
                vertex.position,
                minBounds,
                maxBounds,
                bottomLeft,
                topLeft,
                topRight,
                bottomRight);

            vertex.position = warped;
            vertices[i] = vertex;
        }

        vh.Clear();
        vh.AddUIVertexTriangleStream(vertices);
    }

    private void SetDirty()
    {
        if (graphic != null)
        {
            graphic.SetVerticesDirty();
        }
    }
}

internal static class PerspectiveWarpUtility
{
    public static bool TryBuildQuad(
        IReadOnlyList<UIVertex> vertices,
        float shear,
        float perspectiveInset,
        bool recedeRight,
        out Vector3 bottomLeft,
        out Vector3 topLeft,
        out Vector3 topRight,
        out Vector3 bottomRight,
        out Vector2 minBounds,
        out Vector2 maxBounds)
    {
        var minX = float.MaxValue;
        var maxX = float.MinValue;
        var minY = float.MaxValue;
        var maxY = float.MinValue;

        for (var i = 0; i < vertices.Count; i++)
        {
            var position = vertices[i].position;
            if (position.x < minX) minX = position.x;
            if (position.x > maxX) maxX = position.x;
            if (position.y < minY) minY = position.y;
            if (position.y > maxY) maxY = position.y;
        }

        return TryBuildQuad(minX, maxX, minY, maxY, shear, perspectiveInset, recedeRight,
            out bottomLeft, out topLeft, out topRight, out bottomRight, out minBounds, out maxBounds);
    }

    public static bool TryBuildQuad(
        float minX,
        float maxX,
        float minY,
        float maxY,
        float shear,
        float perspectiveInset,
        bool recedeRight,
        out Vector3 bottomLeft,
        out Vector3 topLeft,
        out Vector3 topRight,
        out Vector3 bottomRight,
        out Vector2 minBounds,
        out Vector2 maxBounds)
    {
        minBounds = new Vector2(minX, minY);
        maxBounds = new Vector2(maxX, maxY);

        bottomLeft = Vector3.zero;
        topLeft = Vector3.zero;
        topRight = Vector3.zero;
        bottomRight = Vector3.zero;

        var width = maxX - minX;
        var height = maxY - minY;
        if (width <= 0.0001f || height <= 0.0001f)
        {
            return false;
        }

        var insetStrength = perspectiveInset <= 1f
            ? Mathf.Clamp01(perspectiveInset)
            : Mathf.Clamp01(perspectiveInset / Mathf.Max(width, height));
        var normalizedShear = Mathf.Abs(shear) <= 1f
            ? Mathf.Clamp(shear, -0.3f, 0.3f)
            : Mathf.Clamp(shear / 90f, -0.3f, 0.3f);

        var horizontalInset = width * Mathf.Lerp(0f, 0.36f, insetStrength);
        var verticalInset = height * Mathf.Lerp(0f, 0.2f, insetStrength);
        var verticalShift = height * normalizedShear;

        if (recedeRight)
        {
            bottomLeft = new Vector3(minX, minY, 0f);
            topLeft = new Vector3(minX, maxY, 0f);
            bottomRight = new Vector3(maxX - horizontalInset, minY + verticalInset + verticalShift, 0f);
            topRight = new Vector3(maxX - horizontalInset, maxY - verticalInset + verticalShift, 0f);
        }
        else
        {
            bottomLeft = new Vector3(minX + horizontalInset, minY + verticalInset - verticalShift, 0f);
            topLeft = new Vector3(minX + horizontalInset, maxY - verticalInset - verticalShift, 0f);
            bottomRight = new Vector3(maxX, minY, 0f);
            topRight = new Vector3(maxX, maxY, 0f);
        }

        var originalCenter = new Vector3((minX + maxX) * 0.5f, (minY + maxY) * 0.5f, 0f);
        var warpedCenter = (bottomLeft + topLeft + topRight + bottomRight) * 0.25f;
        var centerOffset = originalCenter - warpedCenter;

        bottomLeft += centerOffset;
        topLeft += centerOffset;
        topRight += centerOffset;
        bottomRight += centerOffset;

        return true;
    }

    public static Vector3 Warp(
        Vector3 point,
        Vector2 minBounds,
        Vector2 maxBounds,
        Vector3 bottomLeft,
        Vector3 topLeft,
        Vector3 topRight,
        Vector3 bottomRight)
    {
        var x01 = Mathf.InverseLerp(minBounds.x, maxBounds.x, point.x);
        var y01 = Mathf.InverseLerp(minBounds.y, maxBounds.y, point.y);

        var left = Vector3.Lerp(bottomLeft, topLeft, y01);
        var right = Vector3.Lerp(bottomRight, topRight, y01);
        return Vector3.Lerp(left, right, x01);
    }
}
