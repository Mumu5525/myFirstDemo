using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    public WeaponData Data;
    float nextAttackTime;

    // 公共入口：先检查冷却，再交给子类做具体攻击
    public void Attack(GameObject shooter)
    {
        if (Time.time < nextAttackTime) return;

        // 从 shooter 身上找 CharacterStats，有 buff/affix 就用修正后的攻速
        CharacterStats stats = shooter.GetComponentInParent<CharacterStats>();
        float effectiveAtkSpeed = stats != null ? stats.GetStat(StatType.AttackSpeed) : Data.attackSpeed;
        nextAttackTime = Time.time + 1f / effectiveAtkSpeed;
        PerformAttack(shooter);   // 子类实现
    }

    /// <summary> 从 shooter 查找 CharacterStats，供子类读取被 buff/affix 修正后的属性 </summary>
    protected CharacterStats GetStats(Transform shooter) => shooter.GetComponentInParent<CharacterStats>();

    // 子类各自实现：近战 / 远程
    protected abstract void PerformAttack(GameObject shooter);
}

