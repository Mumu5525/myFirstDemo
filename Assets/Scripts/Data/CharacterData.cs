using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Data/CharacterData")]
public class CharacterData : ScriptableObject
{
    [Header("移动")]
    public float speed = 3f;
    public float smoothing = 10f;

    [Header("转向")]
    public float rotationSpeed = 10f;

    [Header("属性")]
    public float atk;
    public float health;
    public float defense;
    public float attackSpeed = 1f;
    public float criticalChance;
    public float criticalDamage = 1.5f;

    [Header("投射物")]
    public float projectileCount = 1f;
    public float projectileSpeed = 20f;
    public float projectileSize = 1f;
    public float pierceCount = 1f;
    public float meleeRange = 2f;

    [Header("爆炸")]
    public float explosionRadius = 3f;
    public float explosionDamage = 10f;

}