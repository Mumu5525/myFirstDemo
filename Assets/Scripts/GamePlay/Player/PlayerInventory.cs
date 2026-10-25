using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 玩家背包。掌管金币、武器、遗物。
/// 武器切换也在此处理（需要 PlayerInputReader）。
/// </summary>
[RequireComponent(typeof(PlayerInputReader))]
public class PlayerInventory : MonoBehaviour
{
    PlayerInputReader input;
    AffixManager affixManager;

    // ─── 金币 ───

    public int Gold { get; private set; }
    public event System.Action<int> OnGoldChanged;

    public void AddGold(int amount)
    {
        if (amount <= 0) return;
        Gold += amount;
        OnGoldChanged?.Invoke(Gold);
    }

    public bool SpendGold(int amount)
    {
        if (amount <= 0) return false;
        if (Gold < amount) return false;
        Gold -= amount;
        OnGoldChanged?.Invoke(Gold);
        return true;
    }

    // ─── 武器 ───

    [field: SerializeField]
    public Weapon[] Weapons { get; private set; }
    public int CurrentIndex { get; private set; }
    public Weapon CurrentWeapon
        => (Weapons.Length > 0 && Weapons[CurrentIndex] != null) ? Weapons[CurrentIndex] : null;
    public int WeaponCount => CountWeapons();
    public int MaxWeaponSlots { get; private set; }
    public event System.Action OnWeaponsChanged;

    public bool CanAddWeapon() => WeaponCount < MaxWeaponSlots;

    void Update()
    {
        if (WeaponCount <= 1) return;
        if (input.SwitchN) CycleNext();
        if (input.SwitchP) CyclePrev();
    }

    void CycleNext()
    {
        for (int i = 1; i <= MaxWeaponSlots; i++)
        {
            int next = (CurrentIndex + i) % MaxWeaponSlots;
            if (Weapons[next] != null) { CurrentIndex = next; return; }
        }
    }

    void CyclePrev()
    {
        for (int i = 1; i <= MaxWeaponSlots; i++)
        {
            int prev = (CurrentIndex - i + MaxWeaponSlots) % MaxWeaponSlots;
            if (Weapons[prev] != null) { CurrentIndex = prev; return; }
        }
    }

    int CountWeapons()
    {
        int c = 0;
        foreach (var w in Weapons) if (w != null) c++;
        return c;
    }

    /// <summary> 添加武器。词条从 weapon.Affixes 路由到 AffixManager。 </summary>
    public bool AddWeapon(Weapon weapon)
    {
        if (weapon == null) return false;
        if (!CanAddWeapon()) return false;

        for (int i = 0; i < Weapons.Length; i++)
        {
            if (Weapons[i] == null)
            {
                Weapons[i] = weapon;
                affixManager?.EquipWeapon(weapon.Affixes);

                if (CurrentWeapon == null)
                    CurrentIndex = i;

                OnWeaponsChanged?.Invoke();
                return true;
            }
        }
        return false;
    }

    /// <summary> 替换指定槽位。 </summary>
    public Weapon ReplaceWeapon(int slotIndex, Weapon newWeapon)
    {
        if (slotIndex < 0 || slotIndex >= Weapons.Length) return null;
        if (newWeapon == null) return null;

        var old = Weapons[slotIndex];
        if (old != null)
            affixManager?.UnequipWeapon(old.Affixes);

        affixManager?.EquipWeapon(newWeapon.Affixes);
        Weapons[slotIndex] = newWeapon;
        OnWeaponsChanged?.Invoke();
        return old;
    }

    /// <summary> 卸下武器，词条从 Weapon.Affixes 注销。 </summary>
    public Weapon RemoveWeapon(int index)
    {
        if (index < 0 || index >= Weapons.Length) return null;
        var weapon = Weapons[index];
        if (weapon == null) return null;

        affixManager?.UnequipWeapon(weapon.Affixes);
        Weapons[index] = null;

        // 如果卸下的是当前武器，找下一把
        if (CurrentIndex == index && WeaponCount > 0)
            CycleNext(); // next after removal

        OnWeaponsChanged?.Invoke();
        return weapon;
    }

    // ─── 遗物 ───

    class RelicSlot
    {
        public RelicData data;
        public List<Affix> affixes;
    }

    List<RelicSlot> relics = new();

    public int RelicCount => relics.Count;
    public event System.Action OnRelicsChanged;

    public RelicData GetRelicData(int index)
        => index >= 0 && index < relics.Count ? relics[index]?.data : null;

    public void AddRelic(RelicData data, List<Affix> affixes)
    {
        if (data == null) return;
        relics.Add(new RelicSlot { data = data, affixes = affixes });
        affixManager?.EquipRelic(affixes);
        OnRelicsChanged?.Invoke();
    }

    // ─── 初始化 ───

    void Awake()
    {
        input = GetComponent<PlayerInputReader>();
        affixManager = GetComponent<AffixManager>();
        var charData = GetComponent<CharacterStats>()?.GetRawData();
        Gold = charData?.initialGold ?? 0;
        MaxWeaponSlots = charData?.maxWeaponSlots ?? 2;

        // Inspector 没拖武器 → 创建空数组；拖了 → 保留
        if (Weapons == null || Weapons.Length == 0)
            Weapons = new Weapon[MaxWeaponSlots];
    }

    void Start()
    {
        // 初始武器的词条注册
        foreach (var w in Weapons)
            if (w != null)
                affixManager?.EquipWeapon(w.Affixes);

        OnWeaponsChanged?.Invoke();
    }
}