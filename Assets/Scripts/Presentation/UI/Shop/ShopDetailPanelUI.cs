using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 商店右侧详情面板。
/// 显示最近一次悬停的货架物品；物品为空时隐藏内容、显示空状态提示。
/// 词条/效果按行显示，每行带一块略浅于底色的背景，行与行紧密排列。
/// </summary>
public class ShopDetailPanelUI : MonoBehaviour
{
    [Header("内容")]
    [SerializeField] Image             detailIcon;
    [SerializeField] TextMeshProUGUI   nameText;
    [SerializeField] TextMeshProUGUI   typeText;        // 武器 / 遗物
    // [SerializeField] TextMeshProUGUI   rarityText;      // 普通 / 稀有 / ...
    [SerializeField] TextMeshProUGUI   descriptionText; // 遗物描述（不参与词条行）
    [SerializeField] RectTransform     statsRoot;       // 词条行容器（Vertical Layout Group）
    [SerializeField] ShopStatRowUI     statRowPrefab;   // 一行词条
    [SerializeField] TextMeshProUGUI   priceText;

    [Header("空状态")]
    [SerializeField] TextMeshProUGUI   emptyHint;       // 例：悬停查看物品详情

    readonly List<ShopStatRowUI> rows = new();

    /// <summary> 显示一件商品；item 为 null 时显示空状态 </summary>
    public void Show(ItemEntry item, PlayerInventory inventory)
    {
        if (item == null) { Clear(); return; }

        var tier = item.kind == ItemEntry.Kind.Weapon ? item.rarity : item.relicTier;
        Color rarity = ShopHUDTheme.RarityColor(tier);

        if (nameText != null)
        {
            nameText.enabled = true;
            nameText.text = item.DisplayName;
            nameText.color = rarity;
        }

        if (typeText != null)
        {
            typeText.enabled = true;
            typeText.text = item.DisplayType;
        }

        // if (rarityText != null)
        // {
        //     rarityText.enabled = true;
        //     rarityText.text = ShopHUDTheme.RarityName(tier);
        //     rarityText.color = rarity;
        // }

        if (detailIcon != null)
        {
            var icon = item.kind == ItemEntry.Kind.Weapon
                ? item.weaponData?.icon
                : item.relicData?.icon;
            detailIcon.sprite  = icon;
            detailIcon.enabled = icon != null;
        }

        if (descriptionText != null)
        {
            string desc = item.kind == ItemEntry.Kind.Relic ? item.relicData?.description : null;
            descriptionText.enabled = !string.IsNullOrEmpty(desc);
            descriptionText.text = desc ?? "";
        }

        ShowRows(BuildRows(item));

        if (priceText != null)
        {
            priceText.enabled = true;
            priceText.text = $"$ {item.price}";
            bool canAfford = inventory != null && inventory.Gold >= item.price;
            priceText.color = canAfford ? ShopHUDTheme.CanAfford : ShopHUDTheme.CannotAfford;
        }

        if (emptyHint != null) emptyHint.enabled = false;
    }

    /// <summary> 清空并显示空状态 </summary>
    public void Clear()
    {
        if (detailIcon != null) detailIcon.enabled = false;
        if (nameText != null)   { nameText.enabled = false;   nameText.text = ""; }
        if (typeText != null)   { typeText.enabled = false;   typeText.text = ""; }
        // if (rarityText != null) { rarityText.enabled = false; rarityText.text = ""; }
        if (descriptionText != null) { descriptionText.enabled = false; descriptionText.text = ""; }
        if (priceText != null)  { priceText.enabled = false;  priceText.text = ""; }
        if (emptyHint != null)  emptyHint.enabled = true;

        ShowRows(null);
    }

    // ═══════════════════════════════════════════
    //  词条行
    // ═══════════════════════════════════════════

    void ShowRows(List<string> lines)
    {
        if (statsRoot == null || statRowPrefab == null) return;

        int count = lines?.Count ?? 0;

        while (rows.Count < count)
            rows.Add(Instantiate(statRowPrefab, statsRoot));

        for (int i = 0; i < rows.Count; i++)
        {
            bool show = i < count;
            rows[i].gameObject.SetActive(show);
            if (show) rows[i].SetText(lines[i]);
        }
    }

    List<string> BuildRows(ItemEntry item)
    {
        var lines = new List<string>();

        if (item.kind == ItemEntry.Kind.Weapon)
        {
            if (item.affixes == null || item.affixes.Count == 0)
                lines.Add("无附加词条");
            else
                foreach (var a in item.affixes)
                    lines.Add($"• {a.Name}");
        }
        else if (item.effects != null)
        {
            foreach (var e in item.effects)
                lines.Add($"• {FormatStat(e.statType, e.modifierType, e.value)}");
        }

        return lines;
    }

    static string FormatStat(StatType stat, ModifierType mod, float value)
    {
        string statName = stat.ToString();
        string sign = mod switch
        {
            ModifierType.add      => $"+{value}",
            ModifierType.multiply => $"×{(1f + value):F2}",
            ModifierType.divide   => $"÷{value}",
            _ => $"{value}"
        };
        return $"{statName} {sign}";
    }
}