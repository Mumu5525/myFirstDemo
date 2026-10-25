using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 战利品生成器（静态工具类）。
/// 负责：挑武器/遗物 + 随机品质 + 抽词条。
/// 只依赖 LootTable + quality 数字，不依赖 ShopData。
/// </summary>
public static class LootGenerator
{
    /// <summary> 武器生成结果：数据 + 词条 </summary>
    public struct GeneratedWeapon
    {
        public WeaponData data;
        public List<Affix> affixes;
        public AffixTier rarity;
    }

    /// <summary> 遗物生成结果：数据 + 品质 + 内置效果 </summary>
    public struct GeneratedRelic
    {
        public RelicData data;
        public AffixTier tier;
        public List<RelicData.RelicEffect> effects;
    }

    /// <summary>
    /// 从武器池随机挑一把，随机品质，随机词条。
    /// 词条数 = (int)rarity（品质越高词条越多）。
    /// </summary>
    public static GeneratedWeapon RollWeapon(LootTable table, int quality)
    {
        if (table == null || table.weaponPool == null || table.weaponPool.Count == 0)
            return default;

        WeaponData wd = table.weaponPool[Random.Range(0, table.weaponPool.Count)];
        AffixTier rarity = RollRarity(table, quality);
        int affixCount = (int)rarity;
        List<Affix> affixes = table.affixPool != null
            ? table.affixPool.RollAndCreate(affixCount, rarity)
            : new List<Affix>();

        return new GeneratedWeapon { data = wd, affixes = affixes, rarity = rarity };
    }

    /// <summary>
    /// 从遗物池随机挑一个。品质用遗物自带的 tier，不随机。
    /// </summary>
    public static GeneratedRelic RollRelic(LootTable table)
    {
        if (table == null || table.relicPool == null || table.relicPool.Count == 0)
            return default;

        RelicData rd = table.relicPool[Random.Range(0, table.relicPool.Count)];
        return new GeneratedRelic { data = rd, tier = rd.tier, effects = rd.effects };
    }

    /// <summary>
    /// 根据 quality 数值加权随机品质。quality 越高，高品出现概率越大。
    /// </summary>
    static AffixTier RollRarity(LootTable table, int quality)
    {
        float total = 0f;
        for (int t = 1; t <= 5; t++)
            total += table.GetWeight((AffixTier)t, quality);

        float roll = Random.Range(0f, total);
        float cumulative = 0f;

        for (int t = 5; t >= 1; t--)
        {
            cumulative += table.GetWeight((AffixTier)t, quality);
            if (roll <= cumulative) return (AffixTier)t;
        }
        return AffixTier.T1;
    }
}