using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 背包物品详情的显示组件（固定面板）。
/// 中间面板和替换模式的左侧面板共用同一套显示内容：
/// 名称、图标、加成后的基础属性、词条/效果、描述。
/// 替换模式下通过 compareTo 与另一把武器逐项比较：高亮绿、低亮红、相同不变色。
/// </summary>
public class InventoryDetailPanelUI : MonoBehaviour
{
    [Header("内容")]
    [SerializeField] Image detailIcon;
    [SerializeField] TextMeshProUGUI nameText;

    [Header("基础属性区")]
    [SerializeField] RectTransform attributeRoot;       // Stats/Viewport/Content
    [SerializeField] GameObject attributeRowPrefab;     // StatRow.prefab

    [Header("词条区")]
    [SerializeField] RectTransform statsRoot;           // Affixs/Viewport/Content
    [SerializeField] ShopStatRowUI statRowPrefab;       // AffixRow.prefab

    [Header("数值来源")]
    [SerializeField] CharacterStats characterStats;     // 留空自动查找玩家

    readonly List<GameObject> attributeRows = new();
    readonly List<ShopStatRowUI> affixRows = new();

    struct AttributeLine
    {
        public string text;
        public WeaponStatKey? key;
        public float value;
    }

    // ═══════════════════════════════════════════
    //  展示入口
    // ═══════════════════════════════════════════

    public void ShowWeapon(Weapon weapon, WeaponStats? compareTo = null)
    {
        if (weapon == null || weapon.Data == null) { Hide(); return; }

        // 背包武器的词条已经注册进 CharacterStats，这里不再重复添加
        WeaponStats stats = WeaponPreview.Calculate(weapon.Data, null, GetStats());
        ShowWeaponData(weapon.Data, weapon.Affixes, weapon.Rarity, stats, compareTo);
    }

    /// <summary> 商店待购商品（替换模式左侧面板） </summary>
    public void ShowItemEntry(ItemEntry item, WeaponStats? compareTo = null)
    {
        if (item == null) { Hide(); return; }

        if (item.kind == ItemEntry.Kind.Weapon)
        {
            if (item.weaponData == null) { Hide(); return; }

            // 待购武器尚未注册词条，预览时把自身词条临时计入
            WeaponStats stats = WeaponPreview.Calculate(item.weaponData, item.affixes, GetStats());
            ShowWeaponData(item.weaponData, item.affixes, item.rarity, stats, compareTo);
        }
        else
        {
            if (item.relicData == null) { Hide(); return; }
            ShowRelicData(item.relicData);
        }
    }

    public void ShowRelic(RelicData data)
    {
        if (data == null) { Hide(); return; }
        ShowRelicData(data);
    }

    // ═══════════════════════════════════════════
    //  具体填充
    // ═══════════════════════════════════════════

    void ShowWeaponData(WeaponData data, List<Affix> affixes, AffixTier rarity,
                        WeaponStats stats, WeaponStats? compareTo)
    {
        SetHeader(data.displayName, data.icon, ShopHUDTheme.RarityColor(rarity));

        var lines = new List<AttributeLine>();
        foreach (var l in WeaponPreview.BuildLines(stats))
        {
            lines.Add(new AttributeLine
            {
                text = l.text,
                key = l.key,
                value = l.value
            });
        }

        ShowAttributeRows(lines, compareTo);
        ShowAffixRows(BuildAffixRows(affixes));
    }

    void ShowRelicData(RelicData data)
    {
        SetHeader(data.displayName, data.icon, ShopHUDTheme.RarityColor(data.tier));
        ShowAffixRows(BuildRelicEffects(data));
    }

    static List<(string text, AffixTier? tier)> BuildAffixRows(List<Affix> affixes)
    {
        var lines = new List<(string text, AffixTier? tier)>();

        if (affixes == null || affixes.Count == 0)
        {
            lines.Add(("无附加词条", null));
            return lines;
        }

        foreach (var a in affixes)
            lines.Add(($"• {a.Name}", a.Tier));

        return lines;
    }

    static List<(string text, AffixTier? tier)> BuildRelicEffects(RelicData data)
    {
        var lines = new List<(string text, AffixTier? tier)>();

        if (data.effects == null || data.effects.Count == 0)
        {
            lines.Add(("无附加效果", null));
            return lines;
        }

        foreach (var e in data.effects)
            lines.Add((FormatStat(e.statType, e.modifierType, e.value), data.tier));

        return lines;
    }

    static string FormatStat(StatType stat, ModifierType mod, float value)
    {
        string sign = mod switch
        {
            ModifierType.add      => $"+{value}",
            ModifierType.multiply => $"×{(1f + value):F2}",
            ModifierType.divide   => $"÷{value}",
            _ => $"{value}"
        };
        return $"{stat}  {sign}";
    }

    // ═══════════════════════════════════════════
    //  清空
    // ═══════════════════════════════════════════

    /// <summary> 只清空内容，不隐藏面板。面板显隐由 InventoryUI 控制。 </summary>
    public void Hide()
    {
        if (detailIcon != null) detailIcon.enabled = false;
        if (nameText != null) { nameText.enabled = false; nameText.text = ""; }

        ShowAttributeRows(null, null);
        ShowAffixRows(null);
    }

    void SetHeader(string displayName, Sprite icon, Color color)
    {
        if (nameText != null)
        {
            nameText.enabled = true;
            nameText.text = displayName;
            nameText.color = color;
        }

        if (detailIcon != null)
        {
            detailIcon.sprite = icon;
            detailIcon.enabled = icon != null;
        }
    }


    CharacterStats GetStats()
    {
        if (characterStats == null)
        {
            var player = PlayerLocator.Find();
            if (player != null) characterStats = player.GetComponent<CharacterStats>();
        }
        return characterStats;
    }

    // ═══════════════════════════════════════════
    //  行
    // ═══════════════════════════════════════════

    void ShowAttributeRows(List<AttributeLine> lines, WeaponStats? compareTo)
    {
        if (attributeRoot == null || attributeRowPrefab == null) return;

        int count = lines?.Count ?? 0;

        while (attributeRows.Count < count)
            attributeRows.Add(Instantiate(attributeRowPrefab, attributeRoot));

        for (int i = 0; i < attributeRows.Count; i++)
        {
            bool show = i < count;
            attributeRows[i].SetActive(show);
            if (!show) continue;

            var tmp = attributeRows[i].GetComponentInChildren<TextMeshProUGUI>(true);
            if (tmp == null) continue;

            tmp.text = lines[i].text;
            tmp.color = ResolveColor(lines[i], compareTo);
        }
    }

    static Color ResolveColor(AttributeLine line, WeaponStats? compareTo)
    {
        Color normal = ShopHUDTheme.TextPrimary;

        if (compareTo == null || line.key == null) return normal;
        if (!compareTo.Value.TryGet(line.key.Value, out float other)) return normal;

        // 按显示精度比较，避免“显示一样却变色”
        WeaponStatKey key = line.key.Value;
        float self = DisplayValue(key, line.value);
        other = DisplayValue(key, other);

        if (Mathf.Approximately(self, other)) return normal;

        // 散射角越小越精准，其余属性越大越好
        bool higherIsBetter = key != WeaponStatKey.SpreadAngle;
        bool selfIsBetter = higherIsBetter ? self > other : self < other;

        return selfIsBetter ? ShopHUDTheme.StatBetter : ShopHUDTheme.StatWorse;
    }

    static float DisplayValue(WeaponStatKey key, float value)
    {
        switch (key)
        {
            case WeaponStatKey.Damage:         return Mathf.Round(value);
            case WeaponStatKey.AttackSpeed:    return Mathf.Round(value * 100f);
            case WeaponStatKey.CriticalChance: return Mathf.Round(value * 100f);
            case WeaponStatKey.PelletCount:
            case WeaponStatKey.PierceCount:    return Mathf.Round(value);
            case WeaponStatKey.SpreadAngle:    return Mathf.Round(value);
            case WeaponStatKey.MeleeRange:     return Mathf.Round(value * 10f);
            default:                           return value;
        }
    }

    void ShowAffixRows(List<(string text, AffixTier? tier)> lines)
    {
        if (statsRoot == null || statRowPrefab == null) return;

        int count = lines?.Count ?? 0;

        while (affixRows.Count < count)
            affixRows.Add(Instantiate(statRowPrefab, statsRoot));

        for (int i = 0; i < affixRows.Count; i++)
        {
            bool show = i < count;
            affixRows[i].gameObject.SetActive(show);
            if (show) affixRows[i].SetText(lines[i].text, lines[i].tier);
        }
    }
}