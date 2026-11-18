using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 屏幕边缘红色闪烁。挂在全屏 UI Image 上。
/// 没有贴图时会在运行时生成一张中心透明的径向渐变图。
/// </summary>
[RequireComponent(typeof(Image))]
public class ScreenEdgeFlash : MonoBehaviour
{
    [SerializeField] Image image;
    [SerializeField] Color flashColor = new Color(1f, 0f, 0f, 0.55f);
    [SerializeField] float fadeIn = 0.05f;
    [SerializeField] float hold = 0.06f;
    [SerializeField] float fadeOut = 0.25f;

    float currentAlpha;
    Coroutine routine;

    void Awake()
    {
        if (image == null) image = GetComponent<Image>();
        if (image.sprite == null) image.sprite = CreateVignetteSprite(256);

        image.raycastTarget = false;
        SetAlpha(0f);
    }

    void OnDisable()
    {
        if (routine != null) StopCoroutine(routine);
        routine = null;
        SetAlpha(0f);
    }

    public void Play()
    {
        if (!isActiveAndEnabled) return;

        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(FlashRoutine());
    }

    IEnumerator FlashRoutine()
    {
        yield return Fade(currentAlpha, flashColor.a, fadeIn);
        yield return new WaitForSeconds(hold);
        yield return Fade(flashColor.a, 0f, fadeOut);
        routine = null;
    }

    IEnumerator Fade(float from, float to, float duration)
    {
        if (duration <= 0f)
        {
            SetAlpha(to);
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            SetAlpha(Mathf.Lerp(from, to, t / duration));
            yield return null;
        }
        SetAlpha(to);
    }

    void SetAlpha(float a)
    {
        currentAlpha = a;
        Color c = flashColor;
        c.a = a;
        image.color = c;
    }

    /// <summary> 生成中心透明、边缘不透明的径向渐变图。 </summary>
    static Sprite CreateVignetteSprite(int size)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        float maxDist = center.magnitude;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), center) / maxDist;
                // d < 0.45 完全透明；d 到 1 时边缘完全显示
                float a = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.45f, 1f, d));
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
}
