using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    [SerializeField] WeaponData data;
    [Tooltip("左手握持点（武器 prefab 内的子物体）")]
    [SerializeField] Transform leftHandGrip;
    [Tooltip("这把武器实例的品质（动态生成，不放在 WeaponData 上）")]
    [SerializeField] AffixTier rarity = AffixTier.T1;
    List<Affix> affixes;
    float nextAttackTime;
    bool firing;

    int currentAmmo;
    bool reloading;
    float reloadEndTime;
    float reloadDuration;
    CharacterStats ownerStats;

    public WeaponData Data => data;
    public List<Affix> Affixes => affixes;
    public AffixTier Rarity => rarity;
    public Transform LeftHandGrip => leftHandGrip;

    CharacterStats Stats
    {
        get
        {
            if (ownerStats == null)
                ownerStats = GetComponentInParent<CharacterStats>();
            return ownerStats;
        }
    }

    /// <summary> 有效弹匣容量 = 武器基础值 + 角色词条/Buff。0 = 无弹匣。 </summary>
    public int MagazineSize
    {
        get
        {
            if (Data == null || Data.magazineSize <= 0) return 0;
            float bonus = Stats != null ? Stats.GetStat(StatType.MagazineSize) : 0f;
            return Mathf.Max(1, Mathf.RoundToInt(Data.magazineSize + bonus));
        }
    }

    public bool UsesAmmo => MagazineSize > 0;
    public bool HasInfiniteAmmo => Stats != null && Stats.GetStat(StatType.InfiniteAmmo) > 0f;
    public int CurrentAmmo => currentAmmo;
    public bool IsReloading => reloading;

    /// <summary> 装填进度 0~1，用于 UI 按换弹时间平滑回填。 </summary>
    public float ReloadProgress
    {
        get
        {
            if (!reloading || Data == null) return 0f;
            return Mathf.Clamp01(1f - (reloadEndTime - Time.time) / Mathf.Max(0.0001f, reloadDuration));
        }
    }

    /// <summary> 弹药变化（当前弹药，弹匣容量）。装填开始和结束时也会触发。 </summary>
    public event System.Action<int, int> OnAmmoChanged;

    /// <summary> 开火时的人机工效倍率。 </summary>
    public float TurnSpeedMultiplier => firing && Data != null ? Data.turnSpeedMultiplier : 1f;
    public float MoveSpeedMultiplier => firing && Data != null ? Data.moveSpeedMultiplier : 1f;

    protected virtual void Awake() => ResetAmmo();

    protected virtual void OnEnable() => TickReload();

    void Update() => TickReload();

    /// <summary> 初始化武器数据和词条（由 Shop 等生成方调用）。 </summary>
    public void Setup(WeaponData weaponData, List<Affix> weaponAffixes, AffixTier weaponRarity = AffixTier.T1)
    {
        data = weaponData;
        affixes = weaponAffixes;
        rarity = weaponRarity;
        ResetAmmo();
        OnSetup();
    }

    public void Attack(GameObject shooter, Vector3 aimPoint, bool pressedThisFrame, bool held)
    {
        TickReload();

        // 冷却期间也要记录，开火减速覆盖整个按住扳机的时间
        firing = held;
        if (Time.time < nextAttackTime) return;

        bool wantsFire = Data.triggerMode == TriggerMode.SemiAutomatic
            ? pressedThisFrame
            : held;
        if(!wantsFire) return;

        // 无限弹药不消耗、不换弹
        bool consumesAmmo = UsesAmmo && !HasInfiniteAmmo;

        // 装填中或弹匣为空时不能开火
        if (consumesAmmo)
        {
            if (reloading) return;
            if (currentAmmo <= 0) { StartReload(); return; }
        }

        CharacterStats stats = shooter.GetComponentInParent<CharacterStats>();
        float charBonus = stats != null ? stats.GetStat(StatType.AttackSpeed) : 1f;
        float effectiveAtkSpeed = Data.attackSpeed * charBonus;
        nextAttackTime = Time.time + 1f / Mathf.Max(0.01f, effectiveAtkSpeed);

        if (consumesAmmo)
        {
            currentAmmo--;
            OnAmmoChanged?.Invoke(currentAmmo, MagazineSize);
            if (currentAmmo <= 0)
                StartReload();
        }

        PerformAttack(shooter, stats, aimPoint);
        EventBus<OnWeaponAttack>.Publish(new OnWeaponAttack{shooter = shooter});
    }

    /// <summary> 把弹药补满。装备词条后由 PlayerInventory 调用，让初始弹药吃到加成。 </summary>
    public void ResetAmmo()
    {
        currentAmmo = MagazineSize;
        reloading = false;
        reloadEndTime = 0f;
        reloadDuration = 0f;
        OnAmmoChanged?.Invoke(currentAmmo, MagazineSize);
    }

    void StartReload()
    {
        if (!UsesAmmo || reloading) return;

        if (HasInfiniteAmmo)
        {
            currentAmmo = MagazineSize;
            OnAmmoChanged?.Invoke(currentAmmo, MagazineSize);
            return;
        }

        reloading = true;
        float speed = Stats != null ? Stats.GetStat(StatType.ReloadSpeed) : 1f;
        reloadDuration = Data.reloadTime / Mathf.Max(0.05f, speed);
        reloadEndTime = Time.time + reloadDuration;
        OnAmmoChanged?.Invoke(currentAmmo, MagazineSize);
    }

    void TickReload()
    {
        if (HasInfiniteAmmo)
        {
            if (reloading || currentAmmo != MagazineSize)
            {
                reloading = false;
                currentAmmo = MagazineSize;
                OnAmmoChanged?.Invoke(currentAmmo, MagazineSize);
            }
            return;
        }

        // 词条/Buff 可能降低容量，把超出上限的弹药收回
        if (currentAmmo > MagazineSize)
        {
            currentAmmo = MagazineSize;
            OnAmmoChanged?.Invoke(currentAmmo, MagazineSize);
        }

        if (!reloading || Time.time < reloadEndTime) return;
        reloading = false;
        currentAmmo = MagazineSize;
        OnAmmoChanged?.Invoke(currentAmmo, MagazineSize);
    }

    protected virtual void OnSetup() { }
    protected abstract void PerformAttack(GameObject shooter, CharacterStats stats, Vector3 aimPoint);
}