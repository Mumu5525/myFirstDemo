using System.Collections.Generic;
using UnityEngine;

// OnHitAffix.cs
public class OnHitAffix : Affix
{
    public OnHitEffect Effect { get; }

    public OnHitAffix(string id, string name, AffixTier tier, OnHitEffect effect)
        : base(id, name, tier)
    {
        Effect = effect;
    }

    public override void CollectModifiers(StatType stat, List<StatModifier> output) { }
}
