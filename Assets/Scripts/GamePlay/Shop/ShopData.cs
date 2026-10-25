using UnityEngine;

/// <summary>
/// 商店配置（ScriptableObject）。槽位、费用、定价乘数。
/// 品质权重和词条数量已移至 LootTable。
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
    [Tooltip("升级到第N级的费用 = upgradeCosts[N-1]")]
    public int[] upgradeCosts = new int[] { 100, 150, 225, 340, 510, 765, 1150, 1725, 2590 };
    public int maxLevel = 10;

    [Header("武器品质价格倍率")]
    [Tooltip("暂定：basePrice × 倍率（下标 0 不用，1=T1，5=T5）")]
    public float[] qualityPriceMultipliers = new float[] { 0, 1f, 1.5f, 2f, 3f, 4f };

    [Header("词条附加价格（按 Tier）")]
    [Tooltip("暂定：每个词条的附加价格（下标 0 不用，1=T1，5=T5）")]
    public int[] affixTierPrices = new int[] { 0, 10, 20, 35, 50, 100 };

    /// <summary> 根据锁定数量获取本次刷新费用 </summary>
    public int GetRefreshCost(int lockedCount)
    {
        return baseRefreshCost + lockedCount * refreshCostPerLock;
    }
}