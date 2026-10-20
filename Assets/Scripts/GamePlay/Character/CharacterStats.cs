using System.Collections.Generic;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    [SerializeField] CharacterData data;
    [SerializeField] BuffManager buffManager;
    [SerializeField] AffixManager affixManager;

    List<StatModifier> modifierCache = new List<StatModifier>();

    void Awake()
    {
        if (buffManager == null)
            buffManager = GetComponent<BuffManager>();
        if (affixManager == null)
            affixManager = GetComponent<AffixManager>();
    }

    float GetBaseValue(StatType stat)
    {
        switch (stat)
        {
            case StatType.Damage:           return data.atk;
            case StatType.Health:           return data.health;
            case StatType.Speed:            return data.speed;
            case StatType.Defense:          return data.defense;
            case StatType.Vulnerability:    return 0f;               // 易伤来自 debuff，没有基础值
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

    float CalculateFinal(StatType stat, float baseValue)
    {
        modifierCache.Clear();

        buffManager.CollectModifiers(stat, modifierCache);
        if (affixManager != null)
            affixManager.CollectModifiers(stat, modifierCache);

        float addSum = 0f;
        float mulSum = 0f;
        float divProduct = 1f;

        foreach (var m in modifierCache)
        {
            switch (m.type)
            {
                case ModifierType.add:
                    addSum += m.value;
                    break;
                case ModifierType.multiply:
                    mulSum += m.value;
                    break;
                case ModifierType.divide:
                    divProduct *= m.value;
                    break;
            }
        }

        return Mathf.Max(0f, (baseValue + addSum) * (1f + mulSum) / divProduct);
    }

    public float GetStat(StatType stat)
    {
        return CalculateFinal(stat, GetBaseValue(stat));
    }
}