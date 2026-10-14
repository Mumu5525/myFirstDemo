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
            if (hit.transform != shooter && !hit.transform.IsChildOf(shooter))
            {
                hit.collider.gameObject.GetComponentInParent<Health>()?.TakeDamage(damage);
                Debug.Log("命中" + hit.collider.name);

                if (pierceLeft <= 0)
                {
                    pool.Return(this);
                    return;
                }
                else
                {
                    pierceLeft--;
                }
            }
        }

        transform.position = currentPosition;
        lastPosition = currentPosition;

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
        if(Trail != null) Trail.Clear();
    }
}
