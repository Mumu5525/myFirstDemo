using TMPro;
using UnityEngine;

/// <summary>
/// 单个伤害数字：屏幕空间飘字，生命周期结束后归还对象池。
/// 要求父节点是一个 Screen Space - Overlay Canvas 下的 RectTransform。
/// </summary>
public class DamagePopup : MonoBehaviour
{
    [SerializeField] TMP_Text label;
    [SerializeField] CanvasGroup canvasGroup;

    [Header("运动")]
    [SerializeField] float lifetime = 0.8f;
    [SerializeField] float gravity = 900f;          // 像素/秒²
    [SerializeField] float minSpeed = 160f;         // 像素/秒
    [SerializeField] float maxSpeed = 300f;
    [SerializeField, Range(0f, 90f)] float maxAngle = 50f;   // 以正上方为基准的左右偏角

    [Header("缩放")]
    [SerializeField] float popScale = 1.3f;
    [SerializeField] float popDuration = 0.12f;

    [Header("淡出")]
    [SerializeField, Range(0f, 1f)] float fadeStart = 0.45f; // 生命周期百分比

    RectTransform rect;
    RectTransform container;
    Camera cam;

    Vector3 worldAnchor;
    Vector2 screenOffset;
    Vector2 velocity;
    float elapsed;
    float baseFontSize;
    System.Action<DamagePopup> onFinished;

    void Awake()
    {
        rect = (RectTransform)transform;
        container = transform.parent as RectTransform;
        cam = Camera.main;

        if (label == null) label = GetComponentInChildren<TMP_Text>();
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        baseFontSize = label != null ? label.fontSize : 36f;
    }

    public void Play(Vector3 anchor, string text, Color color, float sizeMultiplier, System.Action<DamagePopup> onFinished)
    {
        this.worldAnchor = anchor;
        this.onFinished = onFinished;

        elapsed = 0f;
        screenOffset = Random.insideUnitCircle * 18f;      // 同位置重叠时错开
        velocity = RandomDirection() * Random.Range(minSpeed, maxSpeed);

        label.text = text;
        label.color = color;
        label.fontSize = baseFontSize * sizeMultiplier;

        canvasGroup.alpha = 1f;
        rect.localScale = Vector3.one * popScale;

        UpdateScreenPosition();
    }

    void Update()
    {
        elapsed += Time.deltaTime;

        velocity.y -= gravity * Time.deltaTime;
        screenOffset += velocity * Time.deltaTime;

        // 出现时放大一下再回弹
        float popT = Mathf.Clamp01(elapsed / popDuration);
        rect.localScale = Vector3.one * Mathf.Lerp(popScale, 1f, popT);

        // 后半段淡出
        float life01 = elapsed / lifetime;
        canvasGroup.alpha = 1f - Mathf.InverseLerp(fadeStart, 1f, life01);

        UpdateScreenPosition();

        if (elapsed >= lifetime)
        {
            var done = onFinished;
            onFinished = null;
            done?.Invoke(this);
        }
    }

    Vector2 RandomDirection()
    {
        float angle = Random.Range(-maxAngle, maxAngle);
        return Quaternion.Euler(0f, 0f, angle) * Vector2.up;
    }

    void UpdateScreenPosition()
    {
        if (cam == null) cam = Camera.main;
        if (cam == null || container == null) return;

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(cam, worldAnchor) + screenOffset;

        // Overlay Canvas 的 cam 参数传 null
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                container, screenPoint, null, out Vector2 local))
        {
            rect.anchoredPosition = local;
        }
    }
}