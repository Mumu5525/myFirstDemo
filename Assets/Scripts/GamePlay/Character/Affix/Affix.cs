using System.Collections.Generic;

/// <summary>
/// 词条来源标记：武器词条换武器时移除，遗物词条常驻。
/// </summary>
public enum AffixSource { Weapon, Relic }

/// <summary>
/// 词条抽象基类。
/// 和 Buff 不同：Affix 在武器/遗物生成时就固定，没有生命周期，不随时间变化。
/// </summary>
public abstract class Affix
{
    public string Id    { get; protected set; }
    public string Name  { get; protected set; }
    public AffixTier Tier { get; protected set; }
    public AffixSource Source { get; set; }

    protected Affix(string id, string name, AffixTier tier)
    {
        Id   = id;
        Name = name;
        Tier = tier;
    }

    /// <summary>
    /// 把自己产生的 StatModifier 交给调用方。
    /// 纯属性词条重写这个就够了。
    /// </summary>
    public abstract void CollectModifiers(StatType stat, List<StatModifier> output);
}

/// <summary>
/// 词条品质，和稀有度一一对应。
/// </summary>
public enum AffixTier
{
    T1 = 1,   // 普通
    T2 = 2,   // 稀有
    T3 = 3,   // 史诗
    T4 = 4,   // 传说
    T5 = 5,   // 神话
}