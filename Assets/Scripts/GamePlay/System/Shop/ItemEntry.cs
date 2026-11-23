using System.Collections.Generic;

/// <summary>
/// 商店商品运行时数据。包装一件武器或遗物，以及已生成的词条/效果和最终价格。
/// </summary>
public class ItemEntry
{
    public enum Kind { Weapon, Relic }
    public Kind kind;

    // ─── 武器 ───
    public WeaponData weaponData;
    public List<Affix> affixes;
    public AffixTier rarity;  // 武器品质

    // ─── 遗物 ───
    public RelicData relicData;
    public List<RelicData.RelicEffect> effects;  // 遗物内置效果
    public AffixTier relicTier;                   // 遗物品质

    // ─── 通用 ───
    public int price;

    // ─── 快捷属性 ───
    public string DisplayName => kind == Kind.Weapon ? weaponData.displayName : relicData.displayName;
    public string DisplayType => kind == Kind.Weapon ? "武器" : "遗物";
}