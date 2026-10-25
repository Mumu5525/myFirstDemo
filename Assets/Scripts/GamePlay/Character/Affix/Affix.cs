using System.Collections.Generic;
using UnityEngine;

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

    // ---- 生命周期钩子，默认空实现（和 Buff 的 OnApply/OnRemove 对称） ----

    /// <summary> 装备时触发。子类重写来实现非数值效果（回血、加Buff等）。 </summary>
    public virtual void OnEquip(GameObject owner) { }

    /// <summary> 卸下时触发。子类重写来撤销效果（移除Buff等）。 </summary>
    public virtual void OnUnequip(GameObject owner) { }
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