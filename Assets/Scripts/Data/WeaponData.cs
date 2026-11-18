using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public enum WeaponType{ Melee,Ranged}
public enum TriggerMode{Automatic, SemiAutomatic}
[CreateAssetMenu(fileName = "WeaponData", menuName = "Data/WeaponData")]
public class WeaponData : ScriptableObject
{
    [Header("基础")]
    public string displayName = "武器";
    [Tooltip("商店 UI 显示的图标")]
    public Sprite icon;
    public WeaponType type = WeaponType.Ranged;
    public GameObject weaponPrefab;  // 武器组件所在的 Prefab（商店购买时实例化用）
    public float damage = 0;
    public float attackSpeed;
    public float criticalChance;
    [Tooltip("商店基础价格")]
    public int basePrice = 100;

    [Header("远程(ranged)")]
    public TriggerMode triggerMode = TriggerMode.Automatic;
    public int pierceCount = 1;
    public int pelletCount = 1;
    [Tooltip("扇形总角度(度), 0 = 精准")]
    public float spreadAngle = 0;
    public float projectileSpeed = 20f;
    public float projectileLife = 5f;
    public Projectile projectilePrefab;

    [Header("近战")]
    public float meleeRange;

    [Header("人机工效")]
    [FormerlySerializedAs("ergonomics")]
    [Range(0.1f, 1f)]
    [Tooltip("开火时转向速度倍率，1 = 不减速")]
    public float turnSpeedMultiplier = 1f;
    [Range(0.1f, 1f)]
    [Tooltip("开火时移动速度倍率，1 = 不减速")]
    public float moveSpeedMultiplier = 1f;

    [Header("基础命中效果")]
    public OnHitEffect baseOnHitEffect;

    [Header("可命中成层级")]
    public LayerMask hitMask;
    public LayerMask enemyMask;
}
