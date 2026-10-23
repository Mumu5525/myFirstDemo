using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 玩家背包（MonoBehaviour）。挂在玩家身上。
/// 整合金币、武器（最多2把）、遗物（无上限）。
/// </summary>
public class PlayerInventory : MonoBehaviour
{
    [Header("初始金币")]
    public int startGold = 0;

    // ─── 金币 ───

    public int Gold { get; private set; }
    public event System.Action<int> OnGoldChanged;


    void Awake()
    {
        Gold = startGold;
    }

    /// <summary> 加钱。由 GoldSystem 在敌人被玩家击杀后调用。 </summary>
    public void AddGold(int amount)
    {
        Debug.Log($"AddGold调用");
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

    const int MaxWeapons = 2;

    ItemEntry[] weapons = new ItemEntry[MaxWeapons];
    int weaponCount;

    public int WeaponCount => weaponCount;
    public int MaxWeaponSlots => MaxWeapons;
    public event System.Action OnWeaponsChanged;

    public ItemEntry GetWeapon(int index)
    {
        return index >= 0 && index < MaxWeapons ? weapons[index] : null;
    }

    public bool CanAddWeapon() => weaponCount < MaxWeapons;

    public bool AddWeapon(ItemEntry item)
    {
        if (item == null || item.kind != ItemEntry.Kind.Weapon) return false;
        if (!CanAddWeapon()) return false;

        for (int i = 0; i < MaxWeapons; i++)
        {
            if (weapons[i] == null)
            {
                weapons[i] = item;
                weaponCount++;
                OnWeaponsChanged?.Invoke();
                return true;
            }
        }
        return false;
    }

    /// <summary> 替换指定槽位的武器，返回被替换的旧武器（覆盖UI用） </summary>
    public ItemEntry ReplaceWeapon(int slotIndex, ItemEntry newItem)
    {
        if (slotIndex < 0 || slotIndex >= MaxWeapons) return null;
        if (newItem == null || newItem.kind != ItemEntry.Kind.Weapon) return null;

        var old = weapons[slotIndex];
        weapons[slotIndex] = newItem;
        if (old == null) weaponCount++;
        OnWeaponsChanged?.Invoke();
        return old;
    }

    public ItemEntry RemoveWeapon(int index)
    {
        if (index < 0 || index >= MaxWeapons) return null;
        var removed = weapons[index];
        if (removed == null) return null;
        weapons[index] = null;
        weaponCount--;
        OnWeaponsChanged?.Invoke();
        return removed;
    }

    // ─── 遗物 ───

    List<ItemEntry> relics = new();

    public int RelicCount => relics.Count;
    public event System.Action OnRelicsChanged;

    public ItemEntry GetRelic(int index)
    {
        return index >= 0 && index < relics.Count ? relics[index] : null;
    }

    public void AddRelic(ItemEntry item)
    {
        if (item == null || item.kind != ItemEntry.Kind.Relic) return;
        relics.Add(item);
        OnRelicsChanged?.Invoke();
    }

    public bool RemoveRelic(ItemEntry item)
    {
        if (relics.Remove(item))
        {
            OnRelicsChanged?.Invoke();
            return true;
        }
        return false;
    }
}