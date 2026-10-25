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

        if (item.kind == ItemEntry.Kind.Weapon && !inventory.CanAddWeapon())
        {
            Debug.Log("武器槽已满");
            return;
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
        OnItemPurchased?.Invoke(item);
    }

    // ─── 武器交付：实例化 prefab → 绑数据 → 入背包 ───

    void InstantiateWeaponForInventory(ItemEntry item)
    {
        var prefab = item.weaponData.weaponPrefab;
        var weaponObj = Instantiate(prefab, inventory.transform);
        var weapon = weaponObj.GetComponent<Weapon>();
        weapon.Setup(item.weaponData, item.affixes);
        if (prefab == null)
        {
            Debug.LogError($"[Shop] {item.weaponData.displayName} 没有 weaponPrefab！");
            return;
        }

        inventory.AddWeapon(weapon);
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