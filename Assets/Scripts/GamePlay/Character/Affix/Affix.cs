using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 词条来源标记：武器词条换武器时移除，遗物词条常驻。
/// </summary>
public enum AffixSource { Weapon, Relic }

/// <summary>
/// 词条/效果抽象基类。武器词条和遗物效果都继承它。
///
/// 子类按需重写三个钩子：
/// 1. CollectModifiers —— 数值型：把 StatModifier 交给 CharacterStats
/// 2. OnEquip          —— 事件型：订阅 EventBus，或做一次性副作用
/// 3. OnUnequip        —— 事件型：撤销 OnEquip 里做的所有事
///
/// 三个钩子都有默认空实现，纯数据型子类一个都不用重写。
/// 和 Buff 的区别：Affix 跟随物品，没有持续时间，不会自己过期。
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

    /// <summary> 数值型词条重写：把自己产生的 StatModifier 交给调用方。 </summary>
    public virtual void CollectModifiers(StatType stat, List<StatModifier> output) { }

    /// <summary> 装备时触发。事件型词条在这里订阅 EventBus 或施加一次性效果。 </summary>
    public virtual void OnEquip(AffixContext owner) { }

    /// <summary> 卸下时触发。必须撤销 OnEquip 里做的所有事（退订、移除 Buff 等）。 </summary>
    public virtual void OnUnequip(AffixContext owner) { }
}

/// <summary>
/// 词条品质，和稀有度一一对应。
/// </summary>
public enum AffixTier
{
    T1 = 1,   // 普通
    T2 = 2,   // 稀有
    T3 = 3,   // 罕见
    T4 = 4,   // 史诗
    T5 = 5,   // 传说
    T6 = 6,   // 神话
}