using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeapon : Weapon
{
    protected override void PerformAttack(GameObject shooter)
    {
        CharacterStats stats = GetStats(shooter.transform);
        float range = stats != null ? stats.GetStat(StatType.MeleeRange) : Data.meleeRange;
        float damage = stats != null ? stats.GetStat(StatType.Damage) : Data.damage;

        Vector3 center = shooter.transform.position + Vector3.up;
        Collider[] allHits = Physics.OverlapSphere(center, range, Data.enemyMask);

        Vector3 forward = shooter.transform.forward;
        forward.y = 0;
        forward.Normalize();

        float cos22_5 = Mathf.Cos(22.5f * Mathf.Deg2Rad);  // 预计算余弦值

        List<Collider> validHits = new List<Collider>();
        foreach (var hit in allHits)
        {
            Vector3 toTarget = (hit.transform.position - center);
            toTarget.y = 0;
            toTarget.Normalize();

            if (Vector3.Dot(forward, toTarget) >= cos22_5)  // 点积判断
            {
                validHits.Add(hit);
            }
        }

        Debug.Log($"近战！命中 {validHits.Count} 个目标");
        foreach (var h in validHits)
        {
            // TODO: 接入 Buff/护盾 拦截伤害时，改为：
            //   var e = new BeforeUnitHit { rawDamage = damage, finalDamage = damage };
            //   EventBus<BeforeUnitHit>.Publish(e);
            //   h.GetComponentInParent<Health>()?.TakeDamage(e.finalDamage, shooter);
            h.GetComponentInParent<Health>()?.TakeDamage(damage, shooter);
        }
    }
}

