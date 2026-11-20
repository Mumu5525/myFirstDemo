using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Data/EnemyData")]
public class EnemyData : CharacterData
{
    [Header("Drops")]
    public int baseGold;

    // 以下三项经 CharacterStats.GetStat(AttackRange / AttackAngle / AttackDamage) 读取
    [Header("Attack")]
    [Tooltip("扇形半径，同时也是起手距离的基准")]
    public float attackRange = 1.2f;
    [Range(0f, 360f)]
    public float attackAngle = 120f;
    [Tooltip("基础伤害，等价于武器伤害；加成走 CharacterData 的 meleeDamage，不要两处各写一份")]
    public float attackDamage = 10f;
}
