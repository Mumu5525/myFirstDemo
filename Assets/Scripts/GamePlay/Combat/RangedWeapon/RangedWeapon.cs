using System.Collections.Generic;
using UnityEngine;

public class RangedWeapon : Weapon
{
    [Tooltip("枪口位置")]
    [SerializeField] Transform muzzle;
    ObjectPool<Projectile> pool;
    Transform poolParent;

    List<OnHitEffect> onHitCache;
    bool onHitBuilt;

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

    List<OnHitEffect> GetOnHitEffects()
    {
        if (onHitBuilt) return onHitCache;
        onHitBuilt = true;

        var list = new List<OnHitEffect>();
        if (Data != null && Data.baseOnHitEffect != null)
            list.Add(Data.baseOnHitEffect);
        if (Affixes != null)
            foreach (var a in Affixes)
                if (a is OnHitAffix o && o.Effect != null)
                    list.Add(o.Effect);

        onHitCache = list.Count > 0 ? list : null;
        return onHitCache;
    }

    protected override void OnSetup() => onHitBuilt = false;

    protected override void PerformAttack(GameObject shooter, CharacterStats stats, Vector3 aimPoint)
    {
        var projectilePool = GetPool();
        if (projectilePool == null) return;

        Vector3 forward = shooter.transform.forward;
        forward.y = 0;
        forward.Normalize();

        Vector3 muzzlePos = muzzle != null
            ? muzzle.position
            : shooter.transform.position + Vector3.up + forward * 0.5f;

        Vector3 aimDir = aimPoint - muzzlePos;
        aimDir.y = 0;

        // 阈值 = 枪口到玩家中心的水平距离，随枪口位置自适应
        Vector3 muzzleToPlayer = shooter.transform.position - muzzlePos;
        muzzleToPlayer.y = 0f;
        float minAimDistSqr = muzzleToPlayer.sqrMagnitude;

        // 鼠标压在枪口附近时方向会剧烈抖动，回退到角色朝向
        if (aimDir.sqrMagnitude < minAimDistSqr)
            aimDir = forward;
        aimDir.Normalize();

        float spread = Data.spreadAngle;
        int pellets = Data.pelletCount;
        if (stats != null)
        {
            spread += stats.GetStat(StatType.SpreadAngle);
            pellets += Mathf.RoundToInt(stats.GetStat(StatType.ProjectileCount));
        }
        spread = Mathf.Max(0f, spread);
        pellets = Mathf.Max(1, pellets);

        float half = spread * 0.5f;

        for (int i = 0; i < pellets; i++)
        {
            float angle;
            if (pellets == 1)
            {
                angle = Random.Range(-half, half);
            }
            else
            {
                // 均匀铺开 + 少量抖动，避免霰弹聚成一团
                float t = (i + 0.5f) / pellets;
                angle = Mathf.Lerp(-half, half, t) + Random.Range(-half, half) * 0.15f;
            }

            Vector3 dir = Quaternion.Euler(0f, angle, 0f) * aimDir;

            List<OnHitEffect> onHit = GetOnHitEffects();
            Projectile p = projectilePool.Get();
            p.transform.position = muzzlePos;
            p.transform.rotation = Quaternion.LookRotation(dir);
            p.Launch(dir, Data, shooter, projectilePool, stats, onHit);
        }
    }
}