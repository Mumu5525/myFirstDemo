using UnityEngine;

/// <summary>
/// 运行时生成一张柔光贴图（径向渐变），供槽位辉光使用，无需美术素材。
/// 全局缓存一份。
/// </summary>
public static class GlowSpriteFactory
{
    static Sprite cached;

    public static Sprite Get()
    {
        if (cached != null) return cached;

        const int size = 128;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear,
            name = "ShopSoftGlow"
        };

        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
        float radius = size * 0.5f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center) / radius;
                float a = Mathf.Clamp01(1f - dist);
                a *= a;   // 二次衰减，边缘更柔和
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        }

        tex.Apply();
        cached = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        cached.name = "ShopSoftGlow";
        return cached;
    }
}