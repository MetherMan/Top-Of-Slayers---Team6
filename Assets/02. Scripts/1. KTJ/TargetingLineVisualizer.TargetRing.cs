using UnityEngine;

public partial class TargetingLineVisualizer
{
    private void EnsureTargetRing()
    {
        if (!useTargetRing) return;
        if (targetRing != null)
        {
            targetRing.loop = true;
            targetRing.useWorldSpace = true;
            targetRing.positionCount = Mathf.Max(8, targetRingSegments);
            targetRing.enabled = false;
            EnsureDefaultLineMaterial(targetRing);
            targetRing.textureMode = LineTextureMode.Stretch;
            return;
        }

        var ringObject = new GameObject("TargetRing");
        ringObject.transform.SetParent(transform, false);
        targetRing = ringObject.AddComponent<LineRenderer>();
        targetRing.loop = true;
        targetRing.useWorldSpace = true;
        targetRing.positionCount = Mathf.Max(8, targetRingSegments);
        targetRing.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        targetRing.receiveShadows = false;
        targetRing.enabled = false;
        targetRing.textureMode = LineTextureMode.Stretch;

        EnsureDefaultLineMaterial(targetRing);
    }

    private void UpdateTargetRing(Transform target, float baseWidth, Color color)
    {
        if (!useTargetRing || targetRing == null) return;
        if (target == null)
        {
            targetRing.enabled = false;
            return;
        }

        var segmentCount = Mathf.Max(8, targetRingSegments);
        if (targetRing.positionCount != segmentCount)
        {
            targetRing.positionCount = segmentCount;
        }

        var center = target.position + Vector3.up * targetRingHeight;
        var step = 360f / segmentCount;
        for (int i = 0; i < segmentCount; i++)
        {
            var rad = Mathf.Deg2Rad * (i * step);
            var point = center + new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)) * targetRingRadius;
            targetRing.SetPosition(i, point);
        }

        var ringWidth = Mathf.Max(0.005f, baseWidth * targetRingWidthMultiplier);
        targetRing.startWidth = ringWidth;
        targetRing.endWidth = ringWidth;
        targetRing.colorGradient = BuildSolidGradient(color);
        targetRing.enabled = true;
    }
}
