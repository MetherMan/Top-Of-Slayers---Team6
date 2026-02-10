using UnityEngine;

public partial class TargetingLineVisualizer
{
    private Gradient BuildSolidGradient(Color color)
    {
        var gradient = new Gradient();
        colorKeys.Clear();
        colorKeys.Add(new GradientColorKey(color, 0f));
        colorKeys.Add(new GradientColorKey(color, 1f));
        alphaKeys.Clear();
        alphaKeys.Add(new GradientAlphaKey(color.a, 0f));
        alphaKeys.Add(new GradientAlphaKey(color.a, 1f));
        gradient.SetKeys(colorKeys.ToArray(), alphaKeys.ToArray());
        return gradient;
    }

    private void EnsureDefaultLineMaterial(LineRenderer renderer)
    {
        if (renderer == null) return;
        if (renderer.sharedMaterial != null) return;

        var shader = Shader.Find("Sprites/Default");
        if (shader == null) return;

        renderer.material = new Material(shader);
    }

    private void ApplySolidLineMaterial()
    {
        if (line == null) return;

        if (solidLineMaterial == null)
        {
            EnsureDefaultLineMaterial(line);
            solidLineMaterial = line.sharedMaterial;
        }

        if (solidLineMaterial != null && line.sharedMaterial != solidLineMaterial)
        {
            line.sharedMaterial = solidLineMaterial;
        }

        line.textureMode = LineTextureMode.Stretch;
    }

    private void ApplyDottedLineMaterial(float length, float worldWidth)
    {
        if (line == null) return;
        EnsureDottedLineMaterial();
        if (dottedLineMaterial == null)
        {
            ApplySolidLineMaterial();
            return;
        }

        if (line.sharedMaterial != dottedLineMaterial)
        {
            line.sharedMaterial = dottedLineMaterial;
        }

        // 월드 두께 기준으로 타일 길이를 제한해 점이 길쭉해지는 현상을 방지한다.
        var safeWidth = Mathf.Max(minLineWidth, worldWidth);
        var minAllowed = safeWidth * 0.85f;
        var maxAllowed = safeWidth * 1.15f;
        var density = Mathf.Clamp(dotRepeatPerMeter, 0.8f, 1.2f);
        var tileLength = safeWidth * Mathf.Clamp(dotTileSizeMultiplier / density, 0.9f, 1.1f);
        var userMin = Mathf.Clamp(dotMinWorldSize, minAllowed, maxAllowed);
        tileLength = Mathf.Clamp(tileLength, userMin, maxAllowed);
        tileLength = Mathf.Max(0.005f, tileLength);

        var repeat = Mathf.Clamp(length / tileLength, 1f, 240f);
        line.textureMode = LineTextureMode.Tile;
        line.sharedMaterial.mainTextureScale = new Vector2(repeat, 1f);
    }

    private void EnsureDottedLineMaterial()
    {
        if (dottedLineMaterial != null && dottedTexture != null) return;

        var shader = Shader.Find("Sprites/Default");
        if (shader == null) return;

        if (dottedTexture == null)
        {
            dottedTexture = BuildDottedTexture();
        }

        if (dottedLineMaterial == null)
        {
            dottedLineMaterial = new Material(shader);
        }

        dottedLineMaterial.mainTexture = dottedTexture;
    }

    private Texture2D BuildDottedTexture()
    {
        const int size = 64;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false, true);
        texture.wrapMode = TextureWrapMode.Repeat;
        texture.filterMode = FilterMode.Bilinear;

        var radius = Mathf.Clamp(Mathf.RoundToInt(size * dotFillRatio * 0.5f), 1, (size / 2) - 1);
        var feather = Mathf.Max(1f, size * 0.06f);
        var center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                var dx = x - center.x;
                var dy = y - center.y;
                var distance = Mathf.Sqrt(dx * dx + dy * dy);
                var alpha = Mathf.Clamp01((radius + feather - distance) / feather);
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply(false, true);
        return texture;
    }

    private void OnDestroy()
    {
        if (dottedLineMaterial != null)
        {
            Destroy(dottedLineMaterial);
        }

        if (dottedTexture != null)
        {
            Destroy(dottedTexture);
        }
    }
}
