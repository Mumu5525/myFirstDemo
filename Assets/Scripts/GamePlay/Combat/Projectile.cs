using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Projectile : MonoBehaviour
{
    Vector3 velocity;
    float life;
    float damage;
    int pierceLeft;
    Vector3 lastPosition;
    LayerMask enemyMask;
    Transform shooter;
    TrailRenderer Trail;
    ObjectPool<Projectile> pool;

    void Awake()
    {
        Trail = GetComponent<TrailRenderer>();
        lastPosition = transform.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 currentPosition = lastPosition + velocity * Time.fixedDeltaTime;
        Vector3 direction = currentPosition - lastPosition;
        float distance = direction.magnitude;

        if (Physics.Raycast(lastPosition, direction, out RaycastHit hit, distance, enemyMask))
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
                        pool.Return(this);
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
                    // 打中墙壁/地面，直接停
                    Debug.Log("命中环境：" + hit.collider.name);
                    pool.Return(this);
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

    public void Launch(Vector3 dir, WeaponData weapon, LayerMask mask, Transform shooter, ObjectPool<Projectile> pool)
    {
        this.velocity = dir * weapon.projectileSpeed;
        this.life = weapon.projectileLife;
        this.damage = weapon.damage;
        this.pierceLeft = weapon.pierceCount;
        this.enemyMask = mask;
        this.shooter = shooter;
        this.pool = pool;
        this.lastPosition = transform.position;
        if (Trail != null) Trail.Clear();
    }
}
