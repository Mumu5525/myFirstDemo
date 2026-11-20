using System.Collections.Generic;
using UnityEngine;

/// <summary> 详情面板里参与比较的基础属性 </summary>
public enum WeaponStatKey
{
    Damage,
    AttackSpeed,
    CriticalChance,
    PelletCount,
    PierceCount,
    SpreadAngle,
    MeleeRange
}

/// <summary> 一把武器在当前角色加成下的最终数值 </summary>
public struct WeaponStats
{
    public WeaponType type;
    public float damage;
    public float attackSpeed;
    public float criticalChance;
    public int pelletCount;
    public int pierceCount;
    public float spreadAngle;
    public float meleeRange;

    /// <summary> 取出用于比较的值；该武器类型没有这项时返回 false。 </summary>
    public bool TryGet(WeaponStatKey key, out float value)
    {
        switch (key)
        {
            case WeaponStatKey.Damage:         value = damage;         return true;
            case WeaponStatKey.AttackSpeed:    value = attackSpeed;    return true;
            case WeaponStatKey.CriticalChance: value = criticalChance; return true;

            case WeaponStatKey.PelletCount:    value = pelletCount;    return type == WeaponType.Ranged;
            case WeaponStatKey.PierceCount:    value = pierceCount;    return type == WeaponType.Ranged;
            case WeaponStatKey.SpreadAngle:    value = spreadAngle;    return type == WeaponType.Ranged;
            case WeaponStatKey.MeleeRange:     value = meleeRange;     return type == WeaponType.Melee;

            default: value = 0f; return false;
        }
    }
}

public struct WeaponStatLine
{
    public WeaponStatKey key;
    public float value;
    public string text;
}

/// <summary>
/// 按攻击实现里的真实公式，把 WeaponData 转成“加成后数值”。
/// 背包武器：extraAffixes 传 null（词条已注册进 CharacterStats）。
/// 商店待购武器：extraAffixes 传 item.affixes（尚未注册，需要临时计入）。
/// </summary>
public static class WeaponPreview
{
    public static WeaponStats Calculate(WeaponData data, List<Affix> extraAffixes, CharacterStats stats)
    {
        var result = new WeaponStats { type = data.type };

        // 伤害：DealDamage 里 baseDamage + 对应近战/远程加成
        float damageBonus = 0f;
        if (stats != null)
        {
            damageBonus = data.type == WeaponType.Ranged
                ? stats.GetStat(StatType.RangedDamage, extraAffixes)
                : stats.GetStat(StatType.MeleeDamage, extraAffixes);
        }
        result.damage = data.damage + damageBonus;

        // 攻速：Weapon.Attack 里 Data.attackSpeed × AttackSpeed 倍率
        float atkSpeed = stats != null ? stats.GetStat(StatType.AttackSpeed, extraAffixes) : 1f;
        result.attackSpeed = data.attackSpeed * atkSpeed;

        // 暴击率：武器自带 + 角色 CriticalChance（0~1）
        float charCrit = stats != null
            ? stats.GetStat(StatType.CriticalChance, extraAffixes)
            : 0f;
        result.criticalChance = data.criticalChance + charCrit;

        // 弹丸数：RangedWeapon 里 pelletCount + round(ProjectileCount)，至少 1
        int extraPellets = stats != null
            ? Mathf.RoundToInt(stats.GetStat(StatType.ProjectileCount, extraAffixes))
            : 0;
        result.pelletCount = Mathf.Max(1, data.pelletCount + extraPellets);

        // 穿透：Projectile 里 pierceCount + (int)PierceCount
        float extraPierce = stats != null ? stats.GetStat(StatType.PierceCount, extraAffixes) : 0f;
        result.pierceCount = data.pierceCount + (int)extraPierce;

        // 散射角：RangedWeapon 里 spreadAngle + SpreadAngle，最低 0
        float extraSpread = stats != null ? stats.GetStat(StatType.SpreadAngle, extraAffixes) : 0f;
        result.spreadAngle = Mathf.Max(0f, data.spreadAngle + extraSpread);

        // 近战范围：MeleeWeapon 里 meleeRange + MeleeRange
        float extraRange = stats != null ? stats.GetStat(StatType.MeleeRange, extraAffixes) : 0f;
        result.meleeRange = data.meleeRange + extraRange;

        return result;
    }

    public static List<WeaponStatLine> BuildLines(WeaponStats s)
    {
        var lines = new List<WeaponStatLine>
        {
            new WeaponStatLine
            {
                key = WeaponStatKey.Damage, value = s.damage,
                text = $"伤害  {s.damage:F0}"
            },
            new WeaponStatLine
            {
                key = WeaponStatKey.AttackSpeed, value = s.attackSpeed,
                text = $"攻击速度  {s.attackSpeed:F2}"
            },
            new WeaponStatLine
            {
                key = WeaponStatKey.CriticalChance, value = s.criticalChance,
                text = $"暴击率  {s.criticalChance * 100f:F0}%"
            },
        };

        if (s.type == WeaponType.Ranged)
        {
            lines.Add(new WeaponStatLine
            {
                key = WeaponStatKey.PelletCount, value = s.pelletCount,
                text = $"弹丸数  {s.pelletCount}"
            });
            lines.Add(new WeaponStatLine
            {
                key = WeaponStatKey.PierceCount, value = s.pierceCount,
                text = $"穿透  {s.pierceCount}"
            });

            // 散射角始终显示（0 也是有效值，比较时 0 更优）
            lines.Add(new WeaponStatLine
            {
                key = WeaponStatKey.SpreadAngle, value = s.spreadAngle,
                text = $"散射角  {s.spreadAngle:F0}°"
            });
        }
        else
        {
            lines.Add(new WeaponStatLine
            {
                key = WeaponStatKey.MeleeRange, value = s.meleeRange,
                text = $"近战范围  {s.meleeRange:F1}"
            });
        }

        return lines;
    }
}