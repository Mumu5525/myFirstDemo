using System.Collections.Generic;
using UnityEngine;

public class MeleeWeapon : Weapon
{
    protected override void PerformAttack(GameObject shooter, CharacterStats stats)
    {
        float charRange = stats != null ? stats.GetStat(StatType.MeleeRange) : 0f;
        float range = Data.meleeRange + charRange;

        // 伤害 = 武器基础 + 角色加成（含词条 + Buff）
        float charDmg = stats != null ? stats.GetStat(StatType.Damage) : 0f;
        float damage = Data.damage + charDmg;

        Vector3 center = shooter.transform.position + Vector3.up;
        Collider[] allHits = Physics.OverlapSphere(center, range, Data.enemyMask);

        Vector3 forward = shooter.transform.forward;
        forward.y = 0;
        forward.Normalize();

        float cos22_5 = Mathf.Cos(22.5f * Mathf.Deg2Rad);
        List<Collider> validHits = new();

        foreach (var hit in allHits)
        {
            Vector3 toTarget = (hit.transform.position - center);
            toTarget.y = 0;
            toTarget.Normalize();

            if (Vector3.Dot(forward, toTarget) >= cos22_5)
                validHits.Add(hit);
        }

        foreach (var h in validHits)
            h.GetComponentInParent<Health>()?.TakeDamage(damage, shooter);
    }
}