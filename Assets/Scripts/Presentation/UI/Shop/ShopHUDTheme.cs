using UnityEngine;

/// <summary>
/// 商店 HUD 主题色与样式常量。
/// 全局统一引用，方便后续换皮。
/// </summary>
public static class ShopHUDTheme
{
    // ─── 主色调（底色全局统一）───
    public static readonly Color BgDark       = Hex("#0F0B09");   // 页面 / 面板 / 货架 / 按钮 底色
    public static readonly Color AccentCyan   = Hex("#00F0FF");   // 备用强调色
    public static readonly Color AccentOrange = Hex("#FF8C00");   // 备用强调色
    public static readonly Color TextPrimary  = Hex("#FFFFFF");   // 主文字（白）
    public static readonly Color TextDim      = Hex("#FFFFFF8C"); // 次要文字（白 55%）
    public static readonly Color PanelBg      = Hex("#0F0B09");   // 面板背景
    public static readonly Color RowBg        = Hex("#1A1714");   // 词条行背景（略浅于底色）
    public static readonly Color LockColor    = Hex("#FFFFFF");   // 锁定图标（白）

    // ─── 槽位状态 ───
    public static readonly Color EmptyBorder  = Hex("#5A6675");   // 空槽/售出：浅灰边框

    // ─── 品质色（T1~T6）───
    public static readonly Color[] RarityColors =
    {
        Hex("#F2F5F8"), // T1 普通 白
        Hex("#44D17A"), // T2 稀有 绿
        Hex("#3D9BFF"), // T3 罕见 蓝
        Hex("#B26BFF"), // T4 史诗 紫
        Hex("#FF8C00"), // T5 传说 橙
        Hex("#FF3B3B"), // T6 神话 红
    };

    static readonly string[] RarityNames = { "普通", "稀有", "罕见", "史诗", "传说", "神话" };

    // ─── 状态色 ───
    public static readonly Color CanAfford    = Hex("#ffffff");
    public static readonly Color CannotAfford = Hex("#FF4444");
    public static readonly Color ButtonNormal = Hex("#0F0B09");
    public static readonly Color ButtonHover  = Hex("#0F0B09");

    // ─── 字体与间距 ───
    public const float SlotWidth  = 130f;
    public const float SlotHeight = 130f;
    public const float PanelPadding = 16f;
    public const float ButtonHeight  = 44f;

    static Color Hex(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out var c);
        return c;
    }

    /// <summary> 根据品质返回颜色 </summary>
    public static Color RarityColor(AffixTier tier)
    {
        int i = Mathf.Clamp((int)tier - 1, 0, RarityColors.Length - 1);
        return RarityColors[i];
    }

    /// <summary> 根据品质返回显示名 </summary>
    public static string RarityName(AffixTier tier)
    {
        int i = Mathf.Clamp((int)tier - 1, 0, RarityNames.Length - 1);
        return RarityNames[i];
    }
}