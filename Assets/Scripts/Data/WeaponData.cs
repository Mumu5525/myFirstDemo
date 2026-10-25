using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponType{ Melee,Ranged}

[CreateAssetMenu(fileName = "WeaponData", menuName = "Data/WeaponData")]
public class WeaponData : ScriptableObject
{
    [Header("基础")]
    public string displayName = "武器";
    public WeaponType type = WeaponType.Ranged;
    public GameObject weaponPrefab;  // 武器组件所在的 Prefab（商店购买时实例化用）
    public float damage = 0;
    public float attackSpeed;
    public float criticalChance;
    [Tooltip("商店基础价格")]
    public int basePrice = 100;

    [Header("远程(ranged)")]
    public int pierceCount = 1;
    public float projectileSpeed = 20f;
    public float projectileLife = 5f;
    public Projectile projectilePrefab;

    [Header("近战")]
    public float meleeRange;

    [Header("可命中成层级")]
    public LayerMask hitMask;
    public LayerMask enemyMask;
}
