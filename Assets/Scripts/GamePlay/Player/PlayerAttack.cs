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

    void FixedUpdate()
    {
        if(input.Attack)
            TryAttack();
    }

    void TryAttack()
    {
        if(weaponData == null || weaponData.Length == 0) return;
        WeaponData weapon = weaponData[currentindex];
        if(weapon == null) return;
        if(Time.time < nextAttackTime) return;

        nextAttackTime = Time.time + 1f/weapon.AttackSpeed;
        switch (weapon.type)
        {
            case WeaponType.Melee :
                break;
            case WeaponType.Ranged :
                RangedAttack(weapon);
                break;
            default :
                break;
        }
    }

    //TODO : 近战攻击

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
        if (input.switchP)
        {
            
        }
    }
}
