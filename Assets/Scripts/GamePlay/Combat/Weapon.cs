using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    WeaponData data;
    List<Affix> affixes;

    /// <summary> 武器数据（运行时设置，只读）。 </summary>
    public WeaponData Data => data;
    /// <summary> 此武器携带的词条，在装备/卸下时用于增删 AffixManager。 </summary>
    public List<Affix> Affixes => affixes;
    float nextAttackTime;

    /// <summary> 初始化武器数据和词条（由 Shop 等生成方调用）。 </summary>
    public void Setup(WeaponData weaponData, List<Affix> weaponAffixes)
    {
        data = weaponData;
        affixes = weaponAffixes;
    }

    public void Attack(GameObject shooter)
    {
        if (Time.time < nextAttackTime) return;

        CharacterStats stats = shooter.GetComponentInParent<CharacterStats>();
        float charBonus = stats != null ? stats.GetStat(StatType.AttackSpeed) : 0f;
        float effectiveAtkSpeed = Data.attackSpeed + charBonus;
        nextAttackTime = Time.time + 1f / effectiveAtkSpeed;
        PerformAttack(shooter, stats);
    }

    protected abstract void PerformAttack(GameObject shooter, CharacterStats stats);
}