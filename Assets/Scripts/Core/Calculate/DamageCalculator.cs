using UnityEngine;

/// <summary>
/// 统一伤害结算入口：所有伤害都从这里走。
/// 负责"打多少血"的计算，Health 只负责扣血和死亡状态。
/// </summary>
public static class DamageCalculator
{
    /// <param name="source">伤害来源，可以为 null（如清场直接击杀），目前只用于死亡事件的击杀者</param>
    /// <param name="target">伤害目标，伤害结算后会找到它身上的 Health</param>
    /// <param name="rawDamage">结算前的原始伤害（来源侧加成前后都可以，取决于调用方）</param>
    public static void DealDamage(GameObject source, GameObject target, float rawDamage)
    {
        if (target == null || rawDamage <= 0f) return;

        Health health = target.GetComponentInParent<Health>();
        if (health == null || health.IsDead) return;

        CharacterStats targetStats = health.GetComponent<CharacterStats>();
        float finalDamage = rawDamage;

        if (targetStats != null)
        {
            // ─── 防御减免（衰减曲线，永远到不了 100%） ───
            float defense = targetStats.GetStat(StatType.Defense);
            finalDamage *= 100f / (100f + Mathf.Max(0f, defense));

            // ─── 易伤倍率（0~正无穷%） ───
            float vulnerability = targetStats.GetStat(StatType.Vulnerability);
            finalDamage *= 1f + Mathf.Max(0f, vulnerability) / 100f;
        }

        // ─── 来源侧伤害增益/减益（暴击、等级补正等）接在这里 ───
        // CharacterStats sourceStats = source != null ? source.GetComponentInParent<CharacterStats>() : null;

        health.ApplyDamage(finalDamage, source);
    }
}