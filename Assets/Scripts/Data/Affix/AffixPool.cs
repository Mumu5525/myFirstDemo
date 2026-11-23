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
    /// 从不超过 maxTier 的 Tier 中抽 N 条，返回已创建好的 Affix 列表。
    /// maxTier 是等级上限；最高档的词条最多出现 1 条，其余槽位从更低的 Tier 抽。
    /// 每个槽位按 Tier 序号加权选档（Tier 越高越容易被选中），再在档内等概率。
    /// </summary>
    public List<Affix> RollAndCreate(int count, AffixTier maxTier = AffixTier.T6)
    {
        var result = new List<Affix>();
        var usedStats = new HashSet<StatType>();
        bool maxTierUsed = false;

        for (int i = 0; i < count; i++)
        {
            // 收集当前还有可用词条的 Tier（最高档出过一条后就排除）
            var availableTiers = new List<AffixTier>();
            for (int t = (int)maxTier; t >= (int)AffixTier.T1; t--)
            {
                var tier = (AffixTier)t;
                if (tier == maxTier && maxTierUsed) continue;
                if (GetUnusedCandidates(tier, usedStats).Count > 0)
                    availableTiers.Add(tier);
            }

            if (availableTiers.Count == 0) break;

            AffixTier chosen = PickWeighted(availableTiers);
            var candidates = GetUnusedCandidates(chosen, usedStats);
            var data = candidates[Random.Range(0, candidates.Count)];

            result.Add(CreateAffix(data));
            usedStats.Add(data.statType);
            if (chosen == maxTier) maxTierUsed = true;
        }

        // 按品质从高到低排序，UI 直接按顺序展示
        result.Sort((a, b) => ((int)b.Tier).CompareTo((int)a.Tier));
        return result;
    }

    /// <summary> 按 Tier 序号加权抽一个 Tier（T6 权重 6 … T1 权重 1）。 </summary>
    static AffixTier PickWeighted(List<AffixTier> tiers)
    {
        float total = 0f;
        foreach (var tier in tiers) total += (int)tier;

        float roll = Random.Range(0f, total);
        float cumulative = 0f;
        foreach (var tier in tiers)
        {
            cumulative += (int)tier;
            if (roll <= cumulative) return tier;
        }
        return tiers[tiers.Count - 1];
    }

    /// <summary> 某 Tier 里还没被用过 StatType 的候选词条。 </summary>
    List<AffixData> GetUnusedCandidates(AffixTier tier, HashSet<StatType> usedStats)
    {
        var candidates = new List<AffixData>();
        if (!byTier.TryGetValue(tier, out var list)) return candidates;

        foreach (var data in list)
        {
            if (data.behavior != AffixBehavior.Stat) continue;
            if (usedStats.Contains(data.statType)) continue;
            candidates.Add(data);
        }
        return candidates;
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

            AffixBehavior.OnHit => data.onHitEffect != null
                ? new OnHitAffix(data.id, data.displayName, data.tier, data.onHitEffect)
                : null,
        
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