using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Data/CharacterData")]
public class CharacterData : ScriptableObject
{
    [Header("Movement")]
    public float speed = 3f;
    public float smoothing = 10f;
    public float rotationSpeed = 10f;

    [Header("Stats")]
    public float meleeDamage;    // 近战伤害加成（基础伤害来自武器/敌人数据）
    public float rangedDamage;   // 远程伤害加成
    public float maxHealth = 100f;
    public float defense;
    public float attackSpeed = 1f;
    public float criticalChance;
    public float criticalDamage = 1.5f;
    public float explosionDamage = 0f;
    public float explosionRadius = 0f;

    [Header("Ranged")]
    public float projectileCount = 1f;
    public float projectileSpeed = 20f;
    public float projectileSize = 1f;
    public float pierceCount = 1f;

    [Header("Melee")]
    public float meleeRange = 2f;
}