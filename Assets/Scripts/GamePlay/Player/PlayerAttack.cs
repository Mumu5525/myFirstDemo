using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(PlayerInputReader))]
public class PlayerAttack : MonoBehaviour
{
    ObjectPool<Projectile> pool;
    // public Projectile prefab;
    // public WeaponData[] weaponData;
    public Weapon[] weapons;
    public CharacterData playerData;
    public Transform model;
    public LayerMask enemylayer;
    // float nextAttackTime;

    PlayerInputReader input;
    int currentindex = 0;
    // Start is called before the first frame update
    void Awake()
    {
        input = GetComponent<PlayerInputReader>();
        Transform parent = new GameObject("projectiles").transform;
    }

    void Update()
    {
        SwitchWeapon();
    }

    void FixedUpdate()
    {
        if (input.Attack)
            TryAttack();
    }

    void TryAttack()
    {
        if (weapons == null || weapons.Length == 0) return;
        Weapon weapon = weapons[currentindex];
        if (weapon == null) return;

        weapon.Attack(model.gameObject);

    }

    void SwitchWeapon()
    {
        int count = weapons.Length == 0 ? 0 : weapons.Length;
        if (count == 0) return;
        if (input.SwitchP)
        {
            currentindex = (currentindex - 1 + count) % count;
        } 
        if (input.SwitchN)
        {
            currentindex = (currentindex + 1) % count;
        }
    }
}
