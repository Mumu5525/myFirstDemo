using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    Vector3 velocity;
    float life;
    float damage;
    int pierceLeft;
    Vector3 lastPosition;
    LayerMask enemyMask;
    LayerMask hitMask;
    Transform shooter;
    TrailRenderer Trail;
    ObjectPool<Projectile> pool;

    bool pendingReturn; // 命中后等一帧再回收，给 Trail 留出渲染时间

    void Awake()
    {
        Trail = GetComponent<TrailRenderer>();
        lastPosition = transform.position;
    }

    void FixedUpdate()
    {
        // 上一帧命中了，这一帧才回收，让 Trail 有时间渲染出完整轨迹
        if (pendingReturn)
        {
            pool.Return(this);
            return;
        }

        Vector3 currentPosition = lastPosition + velocity * Time.fixedDeltaTime;
        Vector3 direction = currentPosition - lastPosition;
        float distance = direction.magnitude;

        if (Physics.Raycast(lastPosition, direction, out RaycastHit hit, distance, hitMask))
        {
            // 先判断是不是发射者自己
            if (hit.transform == shooter || hit.transform.IsChildOf(shooter))
            {
                // 忽略自己，继续飞
                transform.position = currentPosition;
                lastPosition = currentPosition;
            }
            else
            {
                // 判断打中的是不是敌人
                bool isEnemy = (enemyMask.value & (1 << hit.collider.gameObject.layer)) != 0;

                if (isEnemy)
                {
                    hit.collider.gameObject.GetComponentInParent<Health>()?.TakeDamage(damage);
                    Debug.Log("命中敌人：" + hit.collider.name);

                    if (pierceLeft <= 0)
                    {
                        // 停在命中点，下一帧再回收
                        transform.position = hit.point;
                        lastPosition = hit.point;
                        velocity = Vector3.zero;
                        pendingReturn = true;
                        return;
                    }
                    else
                    {
                        pierceLeft--;
                        // 贯穿：从命中点稍前继续
                        lastPosition = hit.point + direction.normalized * 0.01f;
                        transform.position = lastPosition;
                        return;
                    }
                }
                else
                {
                    // 打中墙壁/地面，停在命中点，下一帧回收
                    Debug.Log("命中环境：" + hit.collider.name);
                    transform.position = hit.point;
                    lastPosition = hit.point;
                    velocity = Vector3.zero;
                    pendingReturn = true;
                    return;
                }
            }
        }
        else
        {
            // 没打中任何东西，正常移动
            transform.position = currentPosition;
            lastPosition = currentPosition;
        }

        life -= Time.fixedDeltaTime;
        if (life <= 0f) pool.Return(this);
    }

    public void Launch(Vector3 dir, WeaponData weapon, Transform shooter, ObjectPool<Projectile> pool, CharacterStats stats = null)
    {
        float effectiveSpeed = stats != null ? stats.GetStat(StatType.ProjectileSpeed) : weapon.projectileSpeed;
        float effectiveDamage = stats != null ? stats.GetStat(StatType.Damage) : weapon.damage;
        int effectivePierce = stats != null ? (int)stats.GetStat(StatType.PierceCount) : weapon.pierceCount;

        this.velocity = dir * effectiveSpeed;
        this.life = weapon.projectileLife;
        this.damage = effectiveDamage;
        this.pierceLeft = effectivePierce;
        this.hitMask = weapon.hitMask;
        this.enemyMask = weapon.enemyMask;
        this.shooter = shooter;
        this.pool = pool;

        this.pendingReturn = false;          // 复用时重置
        this.lastPosition = transform.position;
        if (Trail != null) Trail.Clear();
    }
}