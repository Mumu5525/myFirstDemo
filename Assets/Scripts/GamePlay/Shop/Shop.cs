using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 商店主控制器（MonoBehaviour）。挂在商店 UI 根节点上。
/// 管理刷新、购买、升级、锁定等流程。数值参数读取 ShopData。
///
/// 职责边界：
///   - RefreshAll()     纯生成商品，不处理金币 —— 供波次结束等外部事件免费调用
///   - PerformRefresh() 扣钱后 RefreshAll —— UI 刷新按钮绑这个
///   - 不主动调用任何其他系统；通过 GameEvents 接收波次结束通知
/// </summary>
public class Shop : MonoBehaviour
{
    [Header("配置")]
    public ShopData data;
    public AffixPool affixPool;

    [Header("武器池")]
    public List<WeaponData> weaponPool = new();
    [Header("遗物池")]
    public List<RelicData> relicPool = new();

    [Header("货架")]
    public List<ShopSlot> weaponSlots = new();
    public List<ShopSlot> relicSlots = new();

    [Header("玩家背包")]
    public PlayerInventory inventory;

    public int ShopLevel { get; private set; } = 1;

    /// <summary> 购买成功后触发 </summary>
    public event System.Action<ItemEntry> OnItemPurchased;

    void Start()
    {
        // 首次打开商店：免费刷满货架
        RefreshAll();
    }

    void OnEnable()
    {
        foreach (var s in weaponSlots) s.OnBuyRequested += TryBuy;
        foreach (var s in relicSlots)  s.OnBuyRequested += TryBuy;

        EventBus<OnWaveFinished>.Subscribe(OnWaveFinished);
    }

    void OnDisable()
    {
        foreach (var s in weaponSlots) s.OnBuyRequested -= TryBuy;
        foreach (var s in relicSlots)  s.OnBuyRequested -= TryBuy;

        EventBus<OnWaveFinished>.Unsubscribe(OnWaveFinished);
    }

    // ─── 波次结束：免费刷新 ───

    bool OnWaveFinished(OnWaveFinished e)
    {
        RefreshAll();
        return false;
    }

    // ─── 刷新 ───

    /// <summary> 纯生成商品，不扣钱。供首次打开、波次结束等场景直接调用。 </summary>
    public void RefreshAll()
    {
        // 生成武器
        for (int i = 0; i < weaponSlots.Count; i++)
        {
            if (weaponSlots[i].IsLocked) continue;
            if (weaponPool.Count == 0) { weaponSlots[i].Clear(); continue; }

            WeaponData wd = weaponPool[Random.Range(0, weaponPool.Count)];
            AffixTier rarity = RollRarity();
            int affixCount = data.affixCountByTier[(int)rarity];
            List<Affix> affixes = affixPool != null
                ? affixPool.RollAndCreate(affixCount, AffixTier.T1)
                : new List<Affix>();
            int price = CalcWeaponPrice(wd, rarity, affixes);

            weaponSlots[i].SetItem(new ItemEntry
            {
                kind = ItemEntry.Kind.Weapon,
                weaponData = wd,
                affixes = affixes,
                rarity = rarity,
                price = price
            });
        }

        // 生成遗物
        for (int i = 0; i < relicSlots.Count; i++)
        {
            if (relicSlots[i].IsLocked) continue;
            if (relicPool.Count == 0) { relicSlots[i].Clear(); continue; }

            RelicData rd = relicPool[Random.Range(0, relicPool.Count)];
            relicSlots[i].SetItem(new ItemEntry
            {
                kind = ItemEntry.Kind.Relic,
                relicData = rd,
                price = rd.price
            });
        }
    }

    /// <summary> 手动刷新：扣钱后调 RefreshAll。UI 刷新按钮绑此方法。 </summary>
    public void PerformRefresh()
    {
        int lockedCount = CountLocked();
        int cost = data.GetRefreshCost(lockedCount);

        if (inventory != null && !inventory.SpendGold(cost))
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

        var purchased = slot.item;

        // 武器购买：先检查背包容量
        if (purchased.kind == ItemEntry.Kind.Weapon && !inventory.CanAddWeapon())
        {
            Debug.Log("武器槽已满（最多2把），请先替换或出售现有武器");
            return;
        }

        // 扣钱
        if (!inventory.SpendGold(purchased.price))
        {
            Debug.Log("金币不足");
            return;
        }

        // 加入背包
        if (purchased.kind == ItemEntry.Kind.Weapon)
            inventory.AddWeapon(purchased);
        else
            inventory.AddRelic(purchased);

        slot.Clear();
        OnItemPurchased?.Invoke(purchased);
    }

    // ─── 升级 ───

    public void Upgrade()
    {
        if (ShopLevel >= data.maxLevel) return;
        int cost = data.GetUpgradeCost(ShopLevel);
        if (inventory != null && !inventory.SpendGold(cost)) return;
        ShopLevel++;
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

    // ─── 品质随机 ───

    AffixTier RollRarity()
    {
        float total = 0f;
        for (int t = 1; t <= 5; t++)
            total += data.GetWeight((AffixTier)t, ShopLevel);

        float roll = Random.Range(0f, total);
        float cumulative = 0f;

        for (int t = 5; t >= 1; t--)
        {
            cumulative += data.GetWeight((AffixTier)t, ShopLevel);
            if (roll <= cumulative) return (AffixTier)t;
        }
        return AffixTier.T1;
    }

    // ─── 锁定 ───

    int CountLocked()
    {
        int count = 0;
        foreach (var s in weaponSlots) if (s.IsLocked) count++;
        foreach (var s in relicSlots)  if (s.IsLocked) count++;
        return count;
    }
}