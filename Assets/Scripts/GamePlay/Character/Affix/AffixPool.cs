using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 词条池（ScriptableObject）。
/// 在 Inspector 里把所有 AffixData 拖进来，运行时按 Tier 随机抽取。
/// </summary>
[CreateAssetMenu(fileName = "AffixPool", menuName = "Data/AffixPool")]
public class AffixPool : ScriptableObject
{
    [Header("词条池（全部拖入）")]
    public List<AffixData> allAffixes = new();

    // 按 Tier 缓存
    Dictionary<AffixTier, List<AffixData>> byTier;

    void OnEnable()
    {
        RebuildCache();
    }

    void OnValidate()
    {
        // 编辑器里改了列表后自动重建缓存
        RebuildCache();
    }

    void RebuildCache()
    {
        byTier = new Dictionary<AffixTier, List<AffixData>>();
        foreach (var affix in allAffixes)
        {
            if (affix == null) continue;
            if (!byTier.ContainsKey(affix.tier))
                byTier[affix.tier] = new List<AffixData>();
            byTier[affix.tier].Add(affix);
        }
    }

    // ─── 抽取 ───

    /// <summary>
    /// 从指定 Tier 中随机抽一条词条模板。
    /// 不保证 Tier 池非空时返回 null。
    /// </summary>
    public AffixData Roll(AffixTier tier)
    {
        if (!byTier.TryGetValue(tier, out var list) || list.Count == 0)
        {
            Debug.LogWarning($"AffixPool: 没有 Tier{tier} 的词条，降级到 T1");
            list = byTier.GetValueOrDefault(AffixTier.T1);
            if (list == null || list.Count == 0) return null;
        }
        return list[Random.Range(0, list.Count)];
    }

    /// <summary>
    /// 从指定 Tier 中随机抽 N 条，同一家族不会重复出现（同一 statType 只取一条）。
    /// </summary>
    public List<AffixData> RollMulti(AffixTier tier, int count)
    {
        if (!byTier.TryGetValue(tier, out var candidates) || candidates.Count == 0)
            return new List<AffixData>();

        var shuffled = new List<AffixData>(candidates);
        Shuffle(shuffled);

        var result = new List<AffixData>();
        var usedStats = new HashSet<StatType>();

        foreach (var affix in shuffled)
        {
            if (result.Count >= count) break;
            if (affix.behavior != AffixBehavior.Stat) continue;
            if (usedStats.Contains(affix.statType)) continue;   // 同类型不重复

            result.Add(affix);
            usedStats.Add(affix.statType);
        }
        return result;
    }

    /// <summary>
    /// 从任意 Tier 中抽 N 条，高 Tier 优先。返回已创建好的 Affix 列表。
    /// minTier 限制最低品质。
    /// </summary>
    public List<Affix> RollAndCreate(int count, AffixTier minTier = AffixTier.T1)
    {
        var result = new List<Affix>();
        var usedStats = new HashSet<StatType>();

        // 从高 Tier 往低 Tier 尝试，凑够 count
        for (int t = (int)AffixTier.T5; t >= (int)minTier && result.Count < count; t--)
        {
            var tier = (AffixTier)t;
            if (!byTier.TryGetValue(tier, out var list) || list.Count == 0) continue;

            var shuffled = new List<AffixData>(list);
            Shuffle(shuffled);

            foreach (var data in shuffled)
            {
                if (result.Count >= count) break;
                if (data.behavior != AffixBehavior.Stat) continue;
                if (usedStats.Contains(data.statType)) continue;

                result.Add(CreateAffix(data));
                usedStats.Add(data.statType);
            }
        }
        return result;
    }

    // ─── 工具 ───

    public static Affix CreateAffix(AffixData data)
    {
        return data.behavior switch
        {
            AffixBehavior.Stat => new StatAffix(
                data.id, data.displayName, data.tier, 
                data.statType, data.modifierType, data.value),
            // AffixBehavior.Trigger => ...  // 将来扩展
            _ => null
        };
    }

    static void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}