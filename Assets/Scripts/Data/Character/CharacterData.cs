using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 字段按"值如何进入公式"分三类，全部经 CharacterStats.GetStat 读取：
// 1. 直接值：作为自身属性直接使用
// 2. 倍率：乘在武器 / 动画的外部基础值上
// 3. 加法加成：加到武器 / OnHit / 攻击的外部基础值上
[CreateAssetMenu(fileName = "CharacterData", menuName = "Data/CharacterData")]
public class CharacterData : ScriptableObject
{
    [Header("Movement")]
    public float speed = 3f;              // 直接值：基础移速
    public float smoothing = 10f;         // 直接值：移动平滑量
    public float rotationSpeed = 10f;     // 直接值：转向速度

    [Header("Base Stats")]
    public float maxHealth = 100f;        // 直接值：基础生命
    public float defense;                 // 直接值：基础防御
    public float criticalChance;          // 直接值：暴击率，0~1
    public float criticalDamage = 1.5f;   // 直接值：暴击伤害倍率

    [Header("Multipliers")]
    [Tooltip("攻击速度倍率：玩家武器射速 × 此值，敌人攻击动画播放速度 × 此值。1 = 不变")]
    public float attackSpeed = 1f;

    [Header("Additive Bonuses")]
    public float meleeDamage;             // 加到武器基础伤害上
    public float rangedDamage;            // 加到武器、弹丸、爆炸的基础伤害上
    public float projectileCount = 1f;    // 加到武器 pelletCount 上，0 = 不加
    public float projectileSpeed = 20f;   // 加到武器 projectileSpeed 上
    public float pierceCount = 1f;        // 加到武器 pierceCount 上
    public float spreadAngle;             // 加到武器 spreadAngle 上，负数 = 更精准
    public float meleeRange = 2f;         // 加到武器 meleeRange 上
    public float explosionDamage;         // 加到 OnHit 爆炸的 damage 上
    public float explosionRadius;         // 加到 OnHit 爆炸的 radius 上
    public float projectileSize = 1f;     // 目前没有任何代码读取
}
