using UnityEngine;

public class RangedWeapon : Weapon
{
    ObjectPool<Projectile> pool;
    Transform poolParent;

    void Awake()
    {
        poolParent = new GameObject("projectiles").transform;
    }

    /// <summary>
    /// 商店购买时先 Instantiate 再 Setup，Awake 里 Data 还是 null，
    /// 所以对象池必须延迟到第一次攻击时创建。
    /// </summary>
    ObjectPool<Projectile> GetPool()
    {
        if (pool == null && Data != null && Data.projectilePrefab != null)
            pool = new ObjectPool<Projectile>(Data.projectilePrefab, 10, poolParent);
        return pool;
    }

    protected override void PerformAttack(GameObject shooter, CharacterStats stats)
    {
        var weaponPool = GetPool();
        if (weaponPool == null) return;

        Vector3 dir = shooter.transform.forward;
        dir.y = 0; dir.Normalize();
        Vector3 muzzle = shooter.transform.position + Vector3.up + dir * 0.5f;

        Projectile p = weaponPool.Get();
        p.transform.position = muzzle;
        p.transform.rotation = Quaternion.LookRotation(dir);
        p.Launch(dir, Data, shooter, pool, stats);
    }
}