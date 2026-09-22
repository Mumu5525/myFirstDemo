using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponType{ Melee,Ranged}

[CreateAssetMenu(fileName = "WeaponData", menuName = "Data/WeaponData")]
public class WeaponData : ScriptableObject
{
    [Header("基础")]
    public string displayName = "武器";
    public WeaponType type = WeaponType.Melee;
    public float damage = 0;
    public float attackSpeed;
    public float criticalChance;

    [Header("远程(ranged)")]
    public int pierceCount = 1;
    public float projectileSpeed = 20f;
    public float projectileLife = 5f;

    [Header("近战")]
    public float meleeRange;
}
