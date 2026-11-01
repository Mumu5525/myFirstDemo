using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 按钮：底色固定，悬停时白色外描边泛光。
/// 需要同物体上有 Button + Image；发光描边是可选的子物体。
/// </summary>
[RequireComponent(typeof(Button), typeof(Image))]
public class GlowButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("底色")]
    [SerializeField] Color fill         = ShopHUDTheme.ButtonNormal;
    [SerializeField] Color fillDisabled = new Color(0.05f, 0.04f, 0.03f, 0.55f);

    [Header("外发光（可选）")]
    [SerializeField] Image glowOutline;
    [SerializeField] Color glowNormal = new Color(1f, 1f, 1f, 0f);
    [SerializeField] Color glowHover  = new Color(1f, 1f, 1f, 0.55f);

    [Header("过渡")]
    [SerializeField] float speed = 12f;

    Image targetImage;
    Button button;
    Color targetFill;
    Color targetGlow;
    bool hovered;
    bool interactable = true;

    void Awake()
    {
        targetImage = GetComponent<Image>();
        button = GetComponent<Button>();
        targetFill = fill;
        targetGlow = glowNormal;
    }

    void Update()
    {
        float t = 1f - Mathf.Exp(-speed * Time.deltaTime);

        if (targetImage != null)
            targetImage.color = Color.Lerp(targetImage.color, targetFill, t);
        if (glowOutline != null)
            glowOutline.color = Color.Lerp(glowOutline.color, targetGlow, t);
    }

    public void OnPointerEnter(PointerEventData e) { hovered = true; Refresh(); }
    public void OnPointerExit(PointerEventData e)  { hovered = false; Refresh(); }

    void Refresh()
    {
        targetFill = interactable ? fill : fillDisabled;
        targetGlow = (hovered && interactable) ? glowHover : glowNormal;
    }

    /// <summary> 手动设置是否可交互（禁用时底色变暗、不发光） </summary>
    public void SetInteractable(bool value)
    {
        interactable = value;
        if (button != null) button.interactable = value;
        if (!value) hovered = false;
        Refresh();
    }
}