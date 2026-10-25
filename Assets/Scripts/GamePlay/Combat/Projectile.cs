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
    GameObject shooter;
    TrailRenderer Trail;
    ObjectPool<Projectile> pool;
    bool pendingReturn;

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
                    hit.collider.gameObject.GetComponentInParent<Health>()?.TakeDamage(damage, shooter);
                    if (pierceLeft <= 0)
                    {
                        transform.position = hit.point;
                        lastPosition = hit.point;
                        velocity = Vector3.zero;
                        pendingReturn = true;
                        return;
                    }
                    else
                    {
                        pierceLeft--;
                        lastPosition = hit.point + direction.normalized * 0.01f;
                        transform.position = lastPosition;
                        return;
                    }
                }
                else
                {
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
            transform.position = currentPosition;
            lastPosition = currentPosition;
        }

        life -= Time.fixedDeltaTime;
        if (life <= 0f) pool.Return(this);
    }

    public void Launch(Vector3 dir, WeaponData weapon, GameObject shooter, ObjectPool<Projectile> pool, CharacterStats stats = null)
    {
        float charSpeed = stats != null ? stats.GetStat(StatType.ProjectileSpeed) : 0f;
        float charDmg = stats != null ? stats.GetStat(StatType.Damage) : 0f;
        float charPierce = stats != null ? stats.GetStat(StatType.PierceCount) : 0f;

        this.velocity = dir * (weapon.projectileSpeed + charSpeed);
        this.life = weapon.projectileLife;
        this.damage = weapon.damage + charDmg;
        this.pierceLeft = weapon.pierceCount + (int)charPierce;
        this.hitMask = weapon.hitMask;
        this.enemyMask = weapon.enemyMask;
        this.shooter = shooter;
        this.pool = pool;

        this.pendingReturn = false;
        this.lastPosition = transform.position;
        if (Trail != null) Trail.Clear();
    }
}