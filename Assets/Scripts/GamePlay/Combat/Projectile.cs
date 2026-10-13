using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    Rigidbody rb;
    Vector3 velocity;
    float life;
    float damage;
    int pierceLeft;
    LayerMask enemyMask;
    Transform shooter;
    ObjectPool<Projectile> pool;
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
       rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
       life -= Time.fixedDeltaTime; 
       if(life <= 0f) pool.Return(this);
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.transform == shooter || other.transform.IsChildOf(shooter)) return;
        bool isEnemy = (enemyMask.value & (1 << other.gameObject.layer)) != 0;
        if (isEnemy)
        {
            if (pierceLeft <= 0) pool.Return(this);
            else pierceLeft--;
            Debug.Log("命中" + other.name);
            other.GetComponentInParent<Health>()?.TakeDamage(damage);
        }
        else pool.Return(this);
    }

    public void Launch(Vector3 dir,WeaponData weapon, LayerMask mask, Transform shooter, ObjectPool<Projectile> pool)
    {
        this.velocity = dir * weapon.projectileSpeed;
        this.life = weapon.projectileLife;
        this.damage = weapon.damage;
        this.pierceLeft = weapon.pierceCount;
        this.enemyMask = mask;
        this.shooter = shooter;
        this.pool = pool;
    }
}
