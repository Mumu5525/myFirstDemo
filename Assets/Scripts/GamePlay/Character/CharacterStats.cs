using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BuffManager))]
[RequireComponent(typeof(AffixManager))]
public class CharacterStats : MonoBehaviour
{
    [SerializeField] CharacterData data;
    [SerializeField] BuffManager buffManager;
    [SerializeField] AffixManager affixManager;

    List<StatModifier> modifierCache = new();

    void Awake()
    {
        if (buffManager == null)
            buffManager = GetComponent<BuffManager>();
        if (affixManager == null)
            affixManager = GetComponent<AffixManager>();
    }

    /// <summary> 返回原始 CharacterData（供 Inventory 读初始值等） </summary>
    public CharacterData GetRawData() => data;

    // ─── 三合一求值 ───

    float GetBaseValue(StatType stat)
    {
        switch (stat)
        {
            case StatType.Damage:           return data.atk;
            case StatType.MaxHealth:           return data.health;
            case StatType.Speed:            return data.speed;
            case StatType.Defense:          return data.defense;
            case StatType.Vulnerability:    return 0f;
            case StatType.AttackSpeed:      return data.attackSpeed;
            case StatType.CriticalChance:   return data.criticalChance;
            case StatType.CriticalDamage:   return data.criticalDamage;
            case StatType.ProjectileCount:  return data.projectileCount;
            case StatType.ProjectileSpeed:  return data.projectileSpeed;
            case StatType.ProjectileSize:   return data.projectileSize;
            case StatType.PierceCount:      return data.pierceCount;
            case StatType.MeleeRange:       return data.meleeRange;
            case StatType.ExplosionRadius:  return data.explosionRadius;
            case StatType.ExplosionDamage:  return data.explosionDamage;
            default:                        return 0f;
        }
    }

    /// <summary> 三合一：base + affix + buff </summary>
    public float GetStat(StatType stat)
    {
        // 防御性懒加载：Awake 顺序不确定时也能正常工作
        if (buffManager == null)
            buffManager = GetComponent<BuffManager>();
        if (affixManager == null)
            affixManager = GetComponent<AffixManager>();

        float baseValue = GetBaseValue(stat);
        modifierCache.Clear();
        if (buffManager != null)
            buffManager.CollectModifiers(stat, modifierCache);
        if (affixManager != null)
            affixManager.CollectModifiers(stat, modifierCache);

        float addSum = 0f, mulSum = 0f, divProduct = 1f;
        foreach (var m in modifierCache)
        {
            switch (m.type)
            {
                case ModifierType.add:      addSum += m.value; break;
                case ModifierType.multiply: mulSum += m.value; break;
                case ModifierType.divide:   divProduct *= m.value; break;
            }
        }
        return Mathf.Max(0f, (baseValue + addSum) * (1f + mulSum) / divProduct);
    }
}