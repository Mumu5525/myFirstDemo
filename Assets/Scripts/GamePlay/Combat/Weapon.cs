using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    public WeaponData Data;
    float nextAttackTime;

    // 公共入口：先检查冷却，再交给子类做具体攻击
    public void Attack(Transform shooter)
    {
        if (Time.time < nextAttackTime) return;
        nextAttackTime = Time.time + 1f / Data.attackSpeed;
        PerformAttack(shooter);   // 子类实现
    }

    // 子类各自实现：近战 / 远程
    protected abstract void PerformAttack(Transform shooter);
}

