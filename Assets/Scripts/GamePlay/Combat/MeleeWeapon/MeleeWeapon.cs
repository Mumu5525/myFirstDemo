using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeapon : Weapon
{
    protected override void PerformAttack(Transform shooter)
    {
        Vector3 center = shooter.position + Vector3.up;
        Collider[] allHits = Physics.OverlapSphere(center, Data.meleeRange, Data.enemyMask);

        Vector3 forward = shooter.forward;
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
            h.GetComponentInParent<Health>()?.TakeDamage(Data.damage);
        } 
    }
}

