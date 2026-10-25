using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 词条管理器（MonoBehaviour）。
/// 挂在角色身上，管理所有已装备武器/遗物的词条。
/// </summary>
public class AffixManager : MonoBehaviour
{
    readonly List<Affix> activeAffixes = new();
    public int Count => activeAffixes.Count;

    /// <summary> 词条变更时触发。关心属性变化的系统（Health 等）订阅此事件。 </summary>
    public event System.Action OnAffixesChanged;

    public void EquipWeapon(List<Affix> newAffixes)
    {
        if (newAffixes == null) return;
        foreach (var a in newAffixes) a.Source = AffixSource.Weapon;
        activeAffixes.AddRange(newAffixes);
        OnAffixesChanged?.Invoke();                       // ① 先通知 → Health 同步 MaxHealth
        foreach (var a in newAffixes) a.OnEquip(gameObject); // ② 再调钩子，此时 MaxHealth 已最新
    }

    public void UnequipWeapon(List<Affix> affixes)
    {
        if (affixes == null) return;
        foreach (var a in affixes)
            if (a.Source == AffixSource.Weapon)
                activeAffixes.Remove(a);
        OnAffixesChanged?.Invoke();
        foreach (var a in affixes) a.OnUnequip(gameObject);
    }

    public void EquipRelic(List<Affix> newAffixes)
    {
        if (newAffixes == null) return;
        foreach (var a in newAffixes) a.Source = AffixSource.Relic;
        activeAffixes.AddRange(newAffixes);
        OnAffixesChanged?.Invoke();
        foreach (var a in newAffixes) a.OnEquip(gameObject);
    }

    public void UnequipRelic(Affix affix)
    {
        if (affix == null) return;
        if (affix.Source == AffixSource.Relic && activeAffixes.Remove(affix))
        {
            OnAffixesChanged?.Invoke();
            affix.OnUnequip(gameObject);
        }
    }

    public void CollectModifiers(StatType stat, List<StatModifier> output)
    {
        foreach (var affix in activeAffixes)
            affix.CollectModifiers(stat, output);
    }
}