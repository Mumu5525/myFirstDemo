using UnityEngine;

/// <summary>
/// 统一伤害结算入口：所有伤害都从这里走。
/// 负责"打多少血"的计算，Health 只负责扣血和死亡状态。
/// </summary>
public static class DamageCalculator
{
    /// <param name="source">伤害来源，可以为 null（如清场直接击杀）</param>
    /// <param name="target">伤害目标，伤害结算后会找到它身上的 Health</param>
    /// <param name="baseDamage">基础伤害：武器伤害 / 拳头伤害 / 敌人基础攻击，不含角色加成</param>
    /// <param name="type">伤害来源类型，供表现层过滤</param>
    /// <param name="scaling">吃哪一项角色加成；None 表示不吃加成</param>
    public static void DealDamage(GameObject source, GameObject target, float baseDamage,
        DamageType type = DamageType.Direct, DamageScaling scaling = DamageScaling.None)
    {
        if (target == null) return;

        Health health = target.GetComponentInParent<Health>();
        if (health == null || health.IsDead) return;

        CharacterStats targetStats = health.GetComponent<CharacterStats>();
        CharacterStats sourceStats = source != null ? source.GetComponentInParent<CharacterStats>() : null;
        float finalDamage = baseDamage;
        bool isCritical = false;

        // ─── 来源侧加成（基础值 + 对应的近战/远程加成） ───
        if (sourceStats != null)
        {
            if (scaling == DamageScaling.Melee)
                finalDamage += sourceStats.GetStat(StatType.MeleeDamage);
            else if (scaling == DamageScaling.Ranged)
                finalDamage += sourceStats.GetStat(StatType.RangedDamage);

            float chance = sourceStats.GetStat(StatType.CriticalChance);
            if (Random.value < chance)      // 约定 chance 为 0~1
            {
                finalDamage *= sourceStats.GetStat(StatType.CriticalDamage);
                isCritical = true;
            }
        }

        // ─── 目标侧减免 ───
        if (targetStats != null)
        {
            // 防御减免（衰减曲线，永远到不了 100%）
            float defense = targetStats.GetStat(StatType.Defense);
            finalDamage *= 100f / (100f + Mathf.Max(0f, defense));

            // 易伤倍率（0~正无穷%）
            float vulnerability = targetStats.GetStat(StatType.Vulnerability);
            finalDamage *= 1f + Mathf.Max(0f, vulnerability) / 100f;
        }

        if (finalDamage <= 0f) return;

        health.ApplyDamage(finalDamage, source, type, isCritical);
    }
}