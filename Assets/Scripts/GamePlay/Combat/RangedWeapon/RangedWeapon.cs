using UnityEngine;

public class RangedWeapon : Weapon
{
    ObjectPool<Projectile> pool;

    void Awake()
    {
        Transform parent = new GameObject("projectiles").transform;
        pool = new ObjectPool<Projectile>(Data.projectilePrefab, 10, parent);
    }

    protected override void PerformAttack(GameObject shooter, CharacterStats stats)
    {
        Vector3 dir = shooter.transform.forward;
        dir.y = 0; dir.Normalize();
        Vector3 muzzle = shooter.transform.position + Vector3.up + dir * 0.5f;

        Projectile p = pool.Get();
        p.transform.position = muzzle;
        p.transform.rotation = Quaternion.LookRotation(dir);
        p.Launch(dir, Data, shooter, pool, stats);
    }
}