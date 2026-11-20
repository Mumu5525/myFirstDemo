using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 让面板跟随鼠标，并按屏幕象限决定从鼠标哪一侧展开（四角跟随）。
/// 面板 pivot 建议设为 (0,0)，放在独立 Canvas 下，不要放在 LayoutGroup 或 Mask 子级。
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class MouseFollowPanel : MonoBehaviour
{
    [Tooltip("用于坐标转换的 Canvas。留空则自动查找父级 Canvas。")]
    [SerializeField] Canvas targetCanvas;

    [Tooltip("面板与屏幕边缘的最小间距")]
    [SerializeField] float edgeMargin = 8f;

    [Tooltip("方向切换死区。越大越不容易在屏幕中间来回切换。")]
    [SerializeField] float hysteresis = 40f;

    RectTransform rect;
    bool initialized;
    bool visible;
    bool placeLeft;   // 面板位于鼠标左侧
    bool placeBelow;  // 面板位于鼠标下方

    void Awake() => Initialize();

    void Initialize()
    {
        if (initialized) return;
        initialized = true;
        rect = (RectTransform)transform;
        if (targetCanvas == null) targetCanvas = GetComponentInParent<Canvas>(true);
    }

    /// <summary> 显示并立即更新到鼠标位置 </summary>
    public void Show()
    {
        Initialize();
        if (!gameObject.activeSelf) gameObject.SetActive(true);
        visible = true;

        // ContentSizeFitter 可能刚被内容撑开，先结算布局再定位
        LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
        UpdatePosition();
    }

    /// <summary> 隐藏（SetActive(false)）</summary>
    public void Hide()
    {
        if (!visible && !gameObject.activeSelf) return;
        visible = false;
        gameObject.SetActive(false);
    }

    void LateUpdate()
    {
        if (visible) UpdatePosition();
    }

    void UpdatePosition()
    {
        if (targetCanvas == null || rect == null) return;

        Vector2 mouse = Input.mousePosition;
        float halfW = Screen.width * 0.5f;
        float halfH = Screen.height * 0.5f;

        // 滞回：只有明确越过中分线 ± 死区才切换方向
        if (mouse.x > halfW + hysteresis) placeLeft = true;
        else if (mouse.x < halfW - hysteresis) placeLeft = false;

        if (mouse.y > halfH + hysteresis) placeBelow = true;
        else if (mouse.y < halfH - hysteresis) placeBelow = false;

        var canvasRect = (RectTransform)targetCanvas.transform;
        Camera cam = targetCanvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : targetCanvas.worldCamera;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, mouse, cam, out Vector2 local))
            return;

        Vector2 size = rect.rect.size;
        Vector2 bottomLeft = local;
        if (placeLeft) bottomLeft.x -= size.x;
        if (placeBelow) bottomLeft.y -= size.y;

        // 兜底：把面板限制在 Canvas 内
        Rect bounds = canvasRect.rect;
        float minX = bounds.xMin + edgeMargin;
        float maxX = bounds.xMax - edgeMargin - size.x;
        float minY = bounds.yMin + edgeMargin;
        float maxY = bounds.yMax - edgeMargin - size.y;

        if (maxX < minX) minX = maxX = (minX + maxX) * 0.5f;
        if (maxY < minY) minY = maxY = (minY + maxY) * 0.5f;

        bottomLeft.x = Mathf.Clamp(bottomLeft.x, minX, maxX);
        bottomLeft.y = Mathf.Clamp(bottomLeft.y, minY, maxY);

        // bottomLeft 是「面板左下角」的位置，换算回 localPosition 需要补偿 pivot
        Vector2 pivotOffset = new Vector2(rect.pivot.x * size.x, rect.pivot.y * size.y);
        Vector3 pos = bottomLeft + pivotOffset;
        pos.z = rect.localPosition.z;
        rect.localPosition = pos;
    }
}