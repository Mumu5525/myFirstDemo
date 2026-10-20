using System.Collections.Generic;

/// <summary>
/// 纯属性修正词条：装备后永久给角色加一项属性修正。
/// 行为由 AffixData 中的 (statType, modifierType, value) 决定。
/// </summary>
public class StatAffix : Affix
{
    readonly StatType targetStat;
    readonly StatModifier modifier;

    public StatAffix(string id, string name, AffixTier tier,
                     StatType targetStat, ModifierType modType, float value)
        : base(id, name, tier)
    {
        this.targetStat = targetStat;
        this.modifier = new StatModifier(modType, value);
    }

    public override void CollectModifiers(StatType stat, List<StatModifier> output)
    {
        if (stat == targetStat)
            output.Add(modifier);
    }
}