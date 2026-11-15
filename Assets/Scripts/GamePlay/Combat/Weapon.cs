using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    [SerializeField] WeaponData data;
    List<Affix> affixes;

    /// <summary> 武器数据。Inspector 可预置（初始武器），Shop 生成时会用 Setup 覆盖。 </summary>
    public WeaponData Data => data;
    /// <summary> 此武器携带的词条，在装备/卸下时用于增删 AffixManager。 </summary>
    public List<Affix> Affixes => affixes;
    float nextAttackTime;

    /// <summary> 初始化武器数据和词条（由 Shop 等生成方调用）。 </summary>
    public void Setup(WeaponData weaponData, List<Affix> weaponAffixes)
    {
        data = weaponData;
        affixes = weaponAffixes;
        OnSetup();
    }

    public void Attack(GameObject shooter, Vector3 aimPoint, bool pressedThisFrame, bool held)
    {
        if (Time.time < nextAttackTime) return;
        
        bool wantsFire = Data.triggerMode == TriggerMode.SemiAutomatic
            ? pressedThisFrame
            : held;
        if(!wantsFire) return;

        CharacterStats stats = shooter.GetComponentInParent<CharacterStats>();
        float charBonus = stats != null ? stats.GetStat(StatType.AttackSpeed) : 1f;
        float effectiveAtkSpeed = Data.attackSpeed * charBonus;
        nextAttackTime = Time.time + 1f / Mathf.Max(0.01f, effectiveAtkSpeed);
        PerformAttack(shooter, stats, aimPoint);
    }

    protected virtual void OnSetup() { }
    protected abstract void PerformAttack(GameObject shooter, CharacterStats stats, Vector3 aimPoint);
}