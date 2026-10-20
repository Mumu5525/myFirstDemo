using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 词条管理器（MonoBehaviour）。
/// 挂在角色身上，管理所有已装备武器/遗物的词条。
/// </summary>
public class AffixManager : MonoBehaviour
{
    readonly List<Affix> activeAffixes = new();

    AffixContext ctx;

    void Awake()
    {
        ctx = new AffixContext { Owner = transform };
    }

    // ─── 装备 / 卸下 ───

    /// <summary> 装备一件武器：只替换武器来源的旧词条，遗物词条保留 </summary>
    public void EquipWeapon(List<Affix> newAffixes)
    {
        activeAffixes.RemoveAll(a => a.Source == AffixSource.Weapon);
        foreach (var a in newAffixes) a.Source = AffixSource.Weapon;
        activeAffixes.AddRange(newAffixes);
    }

    public void UnequipWeapon()
    {
        activeAffixes.RemoveAll(a => a.Source == AffixSource.Weapon);
    }

    /// <summary> 拾取遗物：加入常驻词条 </summary>
    public void EquipRelic(List<Affix> newAffixes)
    {
        foreach (var a in newAffixes) a.Source = AffixSource.Relic;
        activeAffixes.AddRange(newAffixes);
    }

    /// <summary> 移除指定遗物词条 </summary>
    public void UnequipRelic(Affix affix)
    {
        if (affix.Source == AffixSource.Relic)
            activeAffixes.Remove(affix);
    }

    // ─── 属性收集 ───

    public void CollectModifiers(StatType stat, List<StatModifier> output)
    {
        foreach (var affix in activeAffixes)
            affix.CollectModifiers(stat, output);
    }

    // ─── 事件分发 ───

    public void DispatchEvent(GameEventType evt)
    {
        foreach (var affix in activeAffixes)
            affix.OnEvent(evt, ctx);
    }

    // ─── 查询 ───

    public int Count => activeAffixes.Count;
}