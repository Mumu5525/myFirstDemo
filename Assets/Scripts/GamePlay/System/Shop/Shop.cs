using System.Collections.Generic;
using UnityEngine;

public class Shop : MonoBehaviour
{
    [Header("配置")]
    public ShopData data;
    public LootTable lootTable;

    [Header("货架")]
    public List<ShopSlot> weaponSlots = new();
    public List<ShopSlot> relicSlots = new();

    [Header("玩家背包")]
    public PlayerInventory inventory;

    public int ShopLevel { get; private set; } = 1;

    public event System.Action<ItemEntry> OnItemPurchased;
    public event System.Action OnShopRefreshed;
    public event System.Action<int> OnShopLevelChanged;

    /// <summary> 武器槽已满时触发，UI 应打开背包让玩家选择要替换的武器 </summary>
    public event System.Action<ItemEntry> OnWeaponReplaceRequested;

    ItemEntry pendingWeaponItem;
    ShopSlot pendingWeaponSlot;

    void Start() => RefreshAll();

    void OnEnable()
    {
        foreach (var s in weaponSlots) s.OnBuyRequested += TryBuy;
        foreach (var s in relicSlots) s.OnBuyRequested += TryBuy;
        EventBus<OnWaveFinished>.Subscribe(OnWaveFinished);
    }

    void OnDisable()
    {
        foreach (var s in weaponSlots) s.OnBuyRequested -= TryBuy;
        foreach (var s in relicSlots) s.OnBuyRequested -= TryBuy;
        EventBus<OnWaveFinished>.Unsubscribe(OnWaveFinished);
    }

    // ─── 货架 ───

    // ─── 波次结束：免费刷新 ───

    bool OnWaveFinished(OnWaveFinished e)
    {
        RefreshAll();
        return false;
    }

    // ─── 刷新 ───

    public void RefreshAll()
    {
        for (int i = 0; i < weaponSlots.Count; i++)
        {
            if (weaponSlots[i].IsLocked) continue;

            var roll = LootGenerator.RollWeapon(lootTable, ShopLevel);
            if (roll.data == null) { weaponSlots[i].Clear(); continue; }

            int price = CalcWeaponPrice(roll.data, roll.rarity, roll.affixes);
            weaponSlots[i].SetItem(new ItemEntry
            {
                kind = ItemEntry.Kind.Weapon,
                weaponData = roll.data,
                affixes = roll.affixes,
                rarity = roll.rarity,
                price = price
            });
        }

        for (int i = 0; i < relicSlots.Count; i++)
        {
            if (relicSlots[i].IsLocked) continue;

            var roll = LootGenerator.RollRelic(lootTable);
            if (roll.data == null) { relicSlots[i].Clear(); continue; }

            int price = CalcRelicPrice(roll.data, roll.tier);
            relicSlots[i].SetItem(new ItemEntry
            {
                kind = ItemEntry.Kind.Relic,
                relicData = roll.data,
                effects = roll.effects,
                relicTier = roll.tier,
                price = price
            });
        }

        OnShopRefreshed?.Invoke();
    }

    public void PerformRefresh()
    {
        int lockedCount = 0;
        foreach (var s in weaponSlots) if (s.IsLocked) lockedCount++;
        foreach (var s in relicSlots) if (s.IsLocked) lockedCount++;

        if (inventory != null && !inventory.SpendGold(data.GetRefreshCost(lockedCount)))
        {
            Debug.Log("金币不足，无法刷新");
            return;
        }
        RefreshAll();
    }

    // ─── 购买 ───

    public void TryBuy(ShopSlot slot)
    {
        if (slot == null || slot.item == null) return;
        if (inventory == null) return;

        var item = slot.item;

        if (item.kind == ItemEntry.Kind.Weapon)
        {
            // 先检查再扣钱：否则金币扣了、武器却生成不出来
            if (item.weaponData == null || item.weaponData.weaponPrefab == null)
            {
                Debug.LogError($"[Shop] {item.DisplayName} 没有配置 weaponPrefab，无法购买");
                return;
            }

            if (!inventory.CanAddWeapon())
            {
                // 槽位已满：先不扣钱，打开背包等待玩家选择要替换的武器
                if (inventory.Gold < item.price)
                {
                    Debug.Log("金币不足");
                    return;
                }

                pendingWeaponItem = item;
                pendingWeaponSlot = slot;
                OnWeaponReplaceRequested?.Invoke(item);
                return;
            }
        }

        if (!inventory.SpendGold(item.price))
        {
            Debug.Log("金币不足");
            return;
        }

        if (item.kind == ItemEntry.Kind.Weapon)
            InstantiateWeaponForInventory(item);
        else
            DeliverRelic(item);

        slot.Clear();

        // 购买后重置锁定：否则刷新会跳过锁定槽，导致该槽永久空置
        if (slot.IsLocked) slot.ToggleLock();

        OnItemPurchased?.Invoke(item);
    }

    // ─── 武器交付：实例化 prefab → 绑数据 → 入背包 ───

    void InstantiateWeaponForInventory(ItemEntry item)
    {
        var weapon = CreateWeapon(item);
        inventory.AddWeapon(weapon);
    }

    Weapon CreateWeapon(ItemEntry item)
    {
        var prefab = item.weaponData.weaponPrefab;
        var weaponObj = Instantiate(prefab, inventory.WeaponMount);
        var weapon = weaponObj.GetComponent<Weapon>();
        weapon.Setup(item.weaponData, item.affixes, item.rarity);
        return weapon;
    }

    // ─── 槽满替换 ───

    /// <summary> 用待替换的商品替换背包指定槽位。成功后扣钱、清空商店槽位。 </summary>
    public bool CompletePendingWeaponReplace(int slotIndex)
    {
        if (pendingWeaponItem == null || pendingWeaponSlot == null) return false;
        if (inventory == null) return false;
        if (slotIndex < 0 || slotIndex >= inventory.Weapons.Length) return false;

        if (inventory.Gold < pendingWeaponItem.price)
        {
            CancelPendingWeaponReplace();
            return false;
        }

        var item = pendingWeaponItem;
        var slot = pendingWeaponSlot;
        pendingWeaponItem = null;
        pendingWeaponSlot = null;

        if (!inventory.SpendGold(item.price)) return false;

        var weapon = CreateWeapon(item);
        var old = inventory.ReplaceWeapon(slotIndex, weapon);
        if (old != null) Destroy(old.gameObject);

        slot.Clear();
        if (slot.IsLocked) slot.ToggleLock();

        OnItemPurchased?.Invoke(item);
        return true;
    }

    /// <summary> 放弃待替换（玩家关闭背包或选择失败）。不扣钱，商品保留在货架上。 </summary>
    public void CancelPendingWeaponReplace()
    {
        pendingWeaponItem = null;
        pendingWeaponSlot = null;
    }

    // ─── 遗物交付：内置效果 → StatAffix → 入背包 ───

    void DeliverRelic(ItemEntry item)
    {
        var affixes = new List<Affix>();
        if (item.effects != null)
        {
            foreach (var e in item.effects)
            {
                affixes.Add(new StatAffix(
                    $"{item.relicData.name}_{e.statType}",
                    item.relicData.displayName,
                    item.relicTier,
                    e.statType,
                    e.modifierType,
                    e.value
                ));
            }
        }
        inventory.AddRelic(item.relicData, affixes);
    }

    // ─── 升级 ───

    public void Upgrade()
    {
        int idx = ShopLevel - 1;
        if (idx >= data.upgradeCosts.Length) return;

        if (inventory != null && !inventory.SpendGold(data.upgradeCosts[idx]))
        {
            Debug.Log("金币不足，无法升级");
            return;
        }

        ShopLevel++;
        OnShopLevelChanged?.Invoke(ShopLevel);
    }

    // ─── 定价 ───

    int CalcWeaponPrice(WeaponData wd, AffixTier rarity, List<Affix> affixes)
    {
        float multiplier = data.qualityPriceMultipliers[(int)rarity];
        int basePrice = Mathf.RoundToInt(wd.basePrice * multiplier);

        int affixTotal = 0;
        foreach (var a in affixes)
            affixTotal += data.affixTierPrices[(int)a.Tier];

        return basePrice + affixTotal;
    }

    int CalcRelicPrice(RelicData rd, AffixTier tier)
    {
        float multiplier = data.qualityPriceMultipliers[(int)tier];
        return Mathf.RoundToInt(rd.price * multiplier);
    }

}