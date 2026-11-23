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
    public T GetRawData<T>() where T : CharacterData => data as T;

    // ─── 三合一求值 ───

    float GetBaseValue(StatType stat)
    {
        // 攻击相关字段在 EnemyData 上，玩家没有这些值
        EnemyData enemyData = data as EnemyData;

        switch (stat)
        {
            case StatType.MeleeDamage:      return data.meleeDamage;
            case StatType.RangedDamage:     return data.rangedDamage;
            case StatType.MaxHealth:        return data.maxHealth;
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
            case StatType.SpreadAngle:      return data.spreadAngle;
            case StatType.Smoothing:        return data.smoothing;
            case StatType.RotationSpeed:    return data.rotationSpeed;
            case StatType.AttackRange:      return enemyData != null ? enemyData.attackRange : 0f;
            case StatType.AttackAngle:      return enemyData != null ? enemyData.attackAngle : 0f;
            case StatType.AttackDamage:     return enemyData != null ? enemyData.attackDamage : 0f;
            case StatType.ReloadSpeed:      return 1f;
            default:                        return 0f;
        }
    }

    /// <summary> 三合一：base + affix + buff </summary>
    public float GetStat(StatType stat) => GetStat(stat, null);

    /// <summary>
    /// 三合一：base + affix + buff，额外叠加临时词条（用于未拥有的武器预览）。
    /// extraAffixes 为 null 时行为与无参版本完全一致。
    /// </summary>
    public float GetStat(StatType stat, IEnumerable<Affix> extraAffixes)
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

        if (extraAffixes != null)
        {
            foreach (var a in extraAffixes)
                a.CollectModifiers(stat, modifierCache);
        }

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

    public void ClearBuff()
    {
        buffManager.Reset();
    }
}