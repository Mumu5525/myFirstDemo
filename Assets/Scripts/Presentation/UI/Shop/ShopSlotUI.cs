using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 商店槽位 UI（只显示图标 / 价格 / 品质 / 锁定）。
/// 悬停：边框不透明度提高 + 同色泛光；底色和尺寸都不变。
/// 左键 = 购买；Shift + 左键 = 锁定 / 解锁。
/// </summary>
public class ShopSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    // ─── 显示 ───
    [Header("显示")]
    [SerializeField] Image             panelBg;       // 内底（铺满槽位，负责接收鼠标）
    [SerializeField] Image[]           borderParts;   // 边框
    [SerializeField] Image             glow;          // 背后柔光（贴图运行时生成）
    [SerializeField] Image             itemIcon;      // 物品图标
    [SerializeField] TextMeshProUGUI   priceText;     // 价格
    [SerializeField] Image             lockIcon;      // 右上角锁图标
    [SerializeField] GameObject        emptyMark;     // 可选：空槽占位

    // ─── 动画 ───
    [Header("边框 / 泛光")]
    [SerializeField] float lerpSpeed         = 12f;
    [SerializeField] float borderAlphaNormal = 0.4f;
    [SerializeField] float borderAlphaHover  = 1f;
    [SerializeField] float glowAlphaNormal   = 0f;
    [SerializeField] float glowAlphaHover    = 0.55f;

    // ─── 数据 ───
    ShopSlot slot;
    PlayerInventory inventory;
    bool hovered;

    Color targetBorder, targetGlow, targetText;

    public ShopSlot Slot => slot;

    /// <summary> 鼠标进入时触发，ShopUI 用它更新右侧详情 </summary>
    public event Action<ShopSlotUI> Hovered;

    bool HasItem => slot != null && slot.item != null;

    // ═══════════════════════════════════════════
    //  生命周期
    // ═══════════════════════════════════════════

    void Awake()
    {
        if (glow != null)
        {
            glow.sprite = GlowSpriteFactory.Get();
            glow.raycastTarget = false;
        }
    }

    public void Bind(ShopSlot shopSlot, PlayerInventory playerInventory)
    {
        if (slot != null)
            slot.OnLockToggled -= OnLockToggled;

        slot      = shopSlot;
        inventory = playerInventory;

        if (slot != null)
        {
            slot.OnLockToggled += OnLockToggled;
            OnLockToggled(slot.IsLocked);
        }

        Render();
    }

    void OnDestroy()
    {
        if (slot != null)
            slot.OnLockToggled -= OnLockToggled;
    }

    void OnLockToggled(bool locked) => UpdateLockIcon();

    // ═══════════════════════════════════════════
    //  渲染
    // ═══════════════════════════════════════════

    public void Render()
    {
        bool empty = !HasItem;

        if (itemIcon != null) itemIcon.enabled = !empty;
        if (priceText != null) priceText.enabled = !empty;
        if (emptyMark != null) emptyMark.SetActive(empty);

        if (!empty)
        {
            var item = slot.item;
            var icon = item.kind == ItemEntry.Kind.Weapon
                ? item.weaponData?.icon
                : item.relicData?.icon;

            if (itemIcon != null)
            {
                itemIcon.sprite  = icon;
                itemIcon.enabled = icon != null;
            }

            if (priceText != null)
                priceText.text = $"$ {item.price}";
        }

        UpdateLockIcon();
        ComputeTargets();
        SnapToTargets();
    }

    void UpdateLockIcon()
    {
        if (lockIcon == null) return;
        bool show = slot != null && slot.IsLocked && HasItem;
        lockIcon.enabled = show;
        if (show) lockIcon.color = ShopHUDTheme.LockColor;
    }

    // ═══════════════════════════════════════════
    //  颜色过渡
    // ═══════════════════════════════════════════

    void ComputeTargets()
    {
        bool empty = !HasItem;

        if (empty)
        {
            targetBorder = ShopHUDTheme.EmptyBorder;
            targetGlow   = new Color(0f, 0f, 0f, 0f);
            targetText   = ShopHUDTheme.TextDim;
            return;
        }

        var item = slot.item;
        var tier = item.kind == ItemEntry.Kind.Weapon ? item.rarity : item.relicTier;
        Color rarity = ShopHUDTheme.RarityColor(tier);
        bool canAfford = inventory != null && inventory.Gold >= item.price;

        // 悬停只提亮边框 + 同色泛光，不换底、不放大
        targetBorder = new Color(rarity.r, rarity.g, rarity.b,
                                 hovered ? borderAlphaHover : borderAlphaNormal);
        targetGlow   = new Color(rarity.r, rarity.g, rarity.b,
                                 hovered ? glowAlphaHover : glowAlphaNormal);
        targetText   = canAfford ? ShopHUDTheme.CanAfford : ShopHUDTheme.CannotAfford;
    }

    void SnapToTargets()
    {
        ApplyColors(targetBorder, targetGlow, targetText);
    }

    void ApplyColors(Color border, Color glowColor, Color text)
    {
        if (borderParts != null)
            foreach (var b in borderParts)
                if (b != null) b.color = border;
        if (glow != null) glow.color = glowColor;
        if (priceText != null) priceText.color = text;
    }

    void Update()
    {
        // 指数平滑，帧率无关
        float t = 1f - Mathf.Exp(-lerpSpeed * Time.deltaTime);

        if (borderParts != null)
            foreach (var b in borderParts)
                if (b != null) b.color = Color.Lerp(b.color, targetBorder, t);
        if (glow != null) glow.color = Color.Lerp(glow.color, targetGlow, t);
        if (priceText != null) priceText.color = Color.Lerp(priceText.color, targetText, t);
    }

    /// <summary> 金币变化时由 ShopUI 调用，重算“买得起/买不起”的颜色 </summary>
    public void RefreshVisuals()
    {
        ComputeTargets();
        SnapToTargets();
    }

    // ═══════════════════════════════════════════
    //  鼠标交互
    // ═══════════════════════════════════════════

    public void OnPointerEnter(PointerEventData e)
    {
        hovered = true;
        ComputeTargets();
        Hovered?.Invoke(this);
    }

    public void OnPointerExit(PointerEventData e)
    {
        hovered = false;
        ComputeTargets();
    }

    public void OnPointerClick(PointerEventData e)
    {
        if (!HasItem) return;
        if (e.button != PointerEventData.InputButton.Left) return;

        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            slot.ToggleLock();
        else
            slot.RequestBuy();
    }
}