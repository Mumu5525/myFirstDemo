using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 生成配置（ScriptableObject）。
/// 武器池、遗物池、词条池、品质权重——全部集中在这里。
/// LootGenerator 只需要一个 LootTable + quality 数字即可生成。
/// </summary>
[CreateAssetMenu(fileName = "LootTable", menuName = "Data/LootTable")]
public class LootTable : ScriptableObject
{
    [Header("武器池")]
    public List<WeaponData> weaponPool = new();
    [Header("遗物池")]
    public List<RelicData> relicPool = new();
    [Header("词条池")]
    public AffixPool affixPool;

    // ─── 品质权重 ───
    [System.Serializable]
    public class RarityWeight
    {
        [Tooltip("对应品质")]
        public AffixTier tier;
        [Tooltip("基础权重")]
        public float baseWeight;
        [Tooltip("每升一级增加的权重")]
        public float perLevelBonus;
    }

    [Header("品质概率")]
    public RarityWeight[] rarityWeights = new RarityWeight[]
    {
        new() { tier = AffixTier.T1, baseWeight = 50, perLevelBonus = -4 },
        new() { tier = AffixTier.T2, baseWeight = 25, perLevelBonus =  0 },
        new() { tier = AffixTier.T3, baseWeight = 15, perLevelBonus =  3 },
        new() { tier = AffixTier.T4, baseWeight =  8, perLevelBonus =  5 },
        new() { tier = AffixTier.T5, baseWeight =  0, perLevelBonus =  1 },
    };

    /// <summary> 根据品质等级和品质目标，获取当前权重 </summary>
    public float GetWeight(AffixTier tier, int quality)
    {
        foreach (var rw in rarityWeights)
        {
            if (rw.tier == tier)
                return Mathf.Max(1f, rw.baseWeight + rw.perLevelBonus * (quality - 1));
        }
        return 0f;
    }
}