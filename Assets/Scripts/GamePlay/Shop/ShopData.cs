using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 商店配置（ScriptableObject）。所有数值参数在此配置，Shop 运行时读取。
/// </summary>
[CreateAssetMenu(fileName = "ShopData", menuName = "Data/ShopData")]
public class ShopData : ScriptableObject
{
    [Header("货架")]
    public int weaponSlotCount = 2;
    public int relicSlotCount = 3;

    [Header("刷新")]
    [Tooltip("基础刷新费用")]
    public int baseRefreshCost = 10;
    [Tooltip("每锁定一个货架，刷新费用增加")]
    public int refreshCostPerLock = 5;

    [Header("升级")]
    [Tooltip("升到第N级的费用 = baseUpgradeCost * multiplier^(N-1)")]
    public int baseUpgradeCost = 100;
    public float upgradeCostMultiplier = 1.5f;
    public int maxLevel = 10;

    // ─── 品质权重 ───
    [System.Serializable]
    public class RarityWeight
    {
        [Tooltip("对应品质")]
        public AffixTier tier;
        [Tooltip("商店Lv1时的权重")]
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

    // ─── 品质倍率 ───
    [Header("武器品质价格倍率")]
    public float[] qualityPriceMultipliers = new float[] { 0, 1f, 1.5f, 2f, 3f, 4f }; // [0]=未使用, [1]=T1, ...

    // ─── 词条价格 ───
    [Header("词条附加价格（按Tier）")]
    public int[] affixTierPrices = new int[] { 0, 10, 20, 35, 50, 100 }; // [0]=未使用, [1]=T1, ...

    // ─── 词条数量 ───
    [Header("各品质词条数量")]
    public int[] affixCountByTier = new int[] { 0, 1, 2, 3, 4, 5 }; // [0]=未使用, [1]=T1, ...

    /// <summary> 根据商店等级和目标品质，获取当前权重 </summary>
    public float GetWeight(AffixTier tier, int shopLevel)
    {
        foreach (var rw in rarityWeights)
        {
            if (rw.tier == tier)
                return Mathf.Max(1f, rw.baseWeight + rw.perLevelBonus * (shopLevel - 1));
        }
        return 0f;
    }

    /// <summary> 根据商店等级获取升级费用 </summary>
    public int GetUpgradeCost(int shopLevel)
    {
        return Mathf.RoundToInt(baseUpgradeCost * Mathf.Pow(upgradeCostMultiplier, shopLevel - 1));
    }

    /// <summary> 根据锁定数量获取本次刷新费用 </summary>
    public int GetRefreshCost(int lockedCount)
    {
        return baseRefreshCost + lockedCount * refreshCostPerLock;
    }
}