using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    [SerializeField] WeaponData data;
    [Tooltip("左手握持点（武器 prefab 内的子物体）")]
    [SerializeField] Transform leftHandGrip;
    List<Affix> affixes;
    float nextAttackTime;
    bool firing;

    public WeaponData Data => data;
    public List<Affix> Affixes => affixes;
    public Transform LeftHandGrip => leftHandGrip;

    /// <summary> 开火时的人机工效倍率。 </summary>
    public float TurnSpeedMultiplier => firing && Data != null ? Data.turnSpeedMultiplier : 1f;
    public float MoveSpeedMultiplier => firing && Data != null ? Data.moveSpeedMultiplier : 1f;

    /// <summary> 初始化武器数据和词条（由 Shop 等生成方调用）。 </summary>
    public void Setup(WeaponData weaponData, List<Affix> weaponAffixes)
    {
        data = weaponData;
        affixes = weaponAffixes;
        OnSetup();
    }

    public void Attack(GameObject shooter, Vector3 aimPoint, bool pressedThisFrame, bool held)
    {
        // 冷却期间也要记录，开火减速覆盖整个按住扳机的时间
        firing = held;
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
        EventBus<OnWeaponAttack>.Publish(new OnWeaponAttack{shooter = shooter});
    }

    protected virtual void OnSetup() { }
    protected abstract void PerformAttack(GameObject shooter, CharacterStats stats, Vector3 aimPoint);
}