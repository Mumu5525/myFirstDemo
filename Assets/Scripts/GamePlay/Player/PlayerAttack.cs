using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(PlayerInputReader))]
public class PlayerAttack : MonoBehaviour
{
    ObjectPool<Projectile> pool;
    public Projectile prefab;
    public WeaponData[] weaponData;
    public PlayerData playerData;
    public Transform model;
    public LayerMask enemylayer;
    float nextAttackTime;

    PlayerInputReader input;
    int currentindex = 1;
    // Start is called before the first frame update
    void Awake()
    {
        input = GetComponent<PlayerInputReader>();
        Transform parent = new GameObject("projectiles").transform;
        pool = new ObjectPool<Projectile>(prefab, 10, parent);
    }

    void Update()
    {
        switchWeapon();
    }

    void FixedUpdate()
    {
        if (input.Attack)
            TryAttack();
    }

    void TryAttack()
    {
        if (weaponData == null || weaponData.Length == 0) return;
        WeaponData weapon = weaponData[currentindex];
        if (weapon == null) return;
        if (Time.time < nextAttackTime) return;

        nextAttackTime = Time.time + 1f / weapon.attackSpeed;
        switch (weapon.type)
        {
            case WeaponType.Melee:
                MeleeAttack(weapon);
                break;
            case WeaponType.Ranged:
                RangedAttack(weapon);
                break;
            default:
                break;
        }
    }

    //TODO : 近战攻击
    void MeleeAttack(WeaponData weapon)
    {
        Vector3 center = model.position + Vector3.up;
        Collider[] allHits = Physics.OverlapSphere(center, weapon.meleeRange, enemylayer);

        Vector3 forward = model.forward;
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

        Debug.Log($"近战！命中 {validHits.Count} 个目标，伤害：" + weapon.damage);
        foreach (var h in validHits) Debug.Log("  - " + h.name);
    }

    void RangedAttack(WeaponData weapon)
    {
        Vector3 dir = model.forward;
        dir.y = 0;
        dir.Normalize();
        Vector3 muzzle = model.position + Vector3.up + dir * 0.5f;
        Projectile p = pool.Get();
        p.transform.position = muzzle;
        p.transform.rotation = Quaternion.LookRotation(dir);
        p.Launch(dir, weapon, enemylayer, model, pool);
    }

    //TODO: 切换武器
    void switchWeapon()
    {
        int count = weaponData.Length == 0 ? 0 : weaponData.Length;
        if (count == 0) return;
        if (input.SwitchP)
        {
            currentindex = (currentindex - 1 + count) % count;
            Debug.Log("切换至上一把武器，当前武器：" + weaponData[currentindex].type);
        } 
        if (input.SwitchN)
        {
            currentindex = (currentindex + 1) % count;
            Debug.Log("切换至下一把武器，当前武器：" + weaponData[currentindex].type);
        }
    }
}
