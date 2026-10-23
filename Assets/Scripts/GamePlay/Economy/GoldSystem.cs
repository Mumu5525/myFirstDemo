using UnityEngine;

/// <summary>
/// 金币系统（静态类，无需挂载）。
/// 订阅 EventBus&lt;OnAnyDeath&gt;，判定击杀者是玩家时计算金币并发放。
/// </summary>
public static class GoldSystem
{
    [RuntimeInitializeOnLoadMethod]
    static void Init()
    {
        EventBus<OnUnitDied>.Subscribe(HandleDeath);
    }

    static bool HandleDeath(OnUnitDied e)
    {
        if (e.killer == null || e.victim == null) return false;

        var inventory = e.killer.GetComponentInParent<PlayerInventory>();
        if (inventory == null) return false;

        var enemy = e.victim.GetComponent<Enemy>();
        if (enemy == null || enemy.Data == null) return false;

        int gold = enemy.Data.baseGold;
        inventory.AddGold(gold);
        return false;
    }
}