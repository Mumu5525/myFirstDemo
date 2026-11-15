using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ExplosionOnHit", menuName = "Data/OnHit/Explosion")]
public class ExplosionOnHit : OnHitEffect
{
    public float radius = 1f;
    public float damage = 0f;
    [Tooltip("与武器的 enemyMask 保持一致")]
    public LayerMask enemyMask;

    public override void Apply(GameObject source, GameObject target, Vector3 point)
    {
        CharacterStats stats = source != null ? source.GetComponentInParent<CharacterStats>() : null;
        float r   = radius + (stats != null ? stats.GetStat(StatType.ExplosionRadius) : 0f);
        float dmg = damage + (stats != null ? stats.GetStat(StatType.ExplosionDamage) : 0f);

        Collider[] hits = Physics.OverlapSphere(point, r, enemyMask, QueryTriggerInteraction.Ignore);
        HashSet<Health> damaged = new();

        foreach (Collider col in hits)
        {
            Health health = col.GetComponentInParent<Health>();
            if (health == null || health.IsDead || !damaged.Add(health)) continue;

            // 防止自己炸自己（多个 Collider 的情况）
            if (source != null && health.transform.IsChildOf(source.transform)) continue;

            DamageCalculator.DealDamage(source, health.gameObject, dmg, scaling: DamageScaling.Ranged);
        }
    }
}
