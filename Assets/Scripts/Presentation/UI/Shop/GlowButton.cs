using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 商店按钮的底色控制：正常 / 禁用两种颜色。
/// 只负责颜色和可交互状态，不做任何悬停特效。
/// 需要同物体上有 Button + Image。
/// </summary>
[RequireComponent(typeof(Button), typeof(Image))]
public class GlowButton : MonoBehaviour
{
    [Header("底色")]
    [SerializeField] Color fill         = ShopHUDTheme.ButtonNormal;
    [SerializeField] Color fillDisabled = new Color(0.05f, 0.04f, 0.03f, 0.55f);

    [Header("过渡")]
    [SerializeField] float speed = 12f;

    Image targetImage;
    Button button;
    Color targetFill;

    void Awake()
    {
        targetImage = GetComponent<Image>();
        button = GetComponent<Button>();
        targetFill = fill;
    }

    void Update()
    {
        if (targetImage == null) return;

        float t = 1f - Mathf.Exp(-speed * Time.deltaTime);
        targetImage.color = Color.Lerp(targetImage.color, targetFill, t);
    }

    /// <summary> 手动设置是否可交互（禁用时底色变暗） </summary>
    public void SetInteractable(bool value)
    {
        if (button != null) button.interactable = value;
        targetFill = value ? fill : fillDisabled;
    }
}