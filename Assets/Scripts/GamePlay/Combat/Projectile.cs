using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    Vector3 velocity;
    float life;
    float damage;
    float criticalChance;
    int pierceLeft;
    bool pendingReturn;
    Vector3 lastPosition;
    LayerMask enemyMask;
    LayerMask hitMask;
    GameObject shooter;
    TrailRenderer Trail;
    ObjectPool<Projectile> pool;
    List<OnHitEffect> onHitEffects;


    void Awake()
    {
        Trail = GetComponent<TrailRenderer>();
        lastPosition = transform.position;
    }

    void FixedUpdate()
    {
        if (pendingReturn) { pool.Return(this); return; }

        Vector3 currentPosition = lastPosition + velocity * Time.fixedDeltaTime;
        Vector3 direction = currentPosition - lastPosition;
        float distance = direction.magnitude;

        if (Physics.Raycast(lastPosition, direction, out RaycastHit hit, distance, hitMask))
        {
            if (hit.collider.gameObject == shooter)
            {
                transform.position = currentPosition;
                lastPosition = currentPosition;
            }
            else
            {
                bool isEnemy = (enemyMask.value & (1 << hit.collider.gameObject.layer)) != 0;
                if (isEnemy)
                {
                    if (pierceLeft <= 0)
                    {
                        transform.position = hit.point;
                        Impact(hit.collider, hit.point, true);
                        lastPosition = hit.point;
                        velocity = Vector3.zero;
                        pendingReturn = true;
                        return;
                    }
                    else
                    {
                        Impact(hit.collider, hit.point, true);
                        pierceLeft--;
                        lastPosition = hit.point + direction.normalized * 0.01f;
                        transform.position = lastPosition;
                        return;
                    }
                }
                else
                {
                    transform.position = hit.point;
                    Impact(hit.collider, hit.point, false);
                    lastPosition = hit.point;
                    velocity = Vector3.zero;
                    pendingReturn = true;
                    return;
                }
            }
        }
        else
        {
            transform.position = currentPosition;
            lastPosition = currentPosition;
        }

        life -= Time.fixedDeltaTime;
        if (life <= 0f) pool.Return(this);
    }

    public void Launch(Vector3 dir, WeaponData weapon, GameObject shooter, ObjectPool<Projectile> pool, CharacterStats stats = null, List<OnHitEffect> onHitEffects = null)
    {
        float charSpeed = stats != null ? stats.GetStat(StatType.ProjectileSpeed) : 0f;
        float charPierce = stats != null ? stats.GetStat(StatType.PierceCount) : 0f;

        this.velocity = dir * (weapon.projectileSpeed + charSpeed);
        this.life = weapon.projectileLife;
        this.damage = weapon.damage;   // 补在 life 赋值旁边
        this.criticalChance = weapon.criticalChance;
        this.pierceLeft = weapon.pierceCount + (int)charPierce;
        this.hitMask = weapon.hitMask;
        this.enemyMask = weapon.enemyMask;
        this.shooter = shooter;
        this.pool = pool;
        this.onHitEffects = onHitEffects;

        this.pendingReturn = false;
        this.lastPosition = transform.position;
        if (Trail != null) Trail.Clear();
    }

    void Impact(Collider other, Vector3 point, bool isEnemy)
    {
        // 先结算直击伤害
        if (isEnemy)
            DamageCalculator.DealDamage(shooter, other.gameObject, damage,
                scaling: DamageScaling.Ranged, extraCriticalChance: criticalChance);

        // 再结算附加效果伤害
        if (onHitEffects != null)
            foreach (var e in onHitEffects)
                e.Apply(shooter, other.gameObject, point);
    }
}