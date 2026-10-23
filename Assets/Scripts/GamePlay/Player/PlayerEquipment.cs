using UnityEngine;

/// <summary>
/// 装备协调器（MonoBehaviour）。挂在玩家身上。
/// 订阅 PlayerInventory 的变动事件，负责：
///   - 武器变更时更新 PlayerAttack.weapons 和 AffixManager
///   - 遗物变更时更新 AffixManager
/// </summary>
[RequireComponent(typeof(PlayerInventory))]
public class PlayerEquipment : MonoBehaviour
{
    PlayerInventory inventory;
    PlayerAttack playerAttack;
    AffixManager affixManager;
    Weapon[] weaponPrefabs;  // 武器 Prefab 池，按 WeaponData 匹配

    void Awake()
    {
        inventory = GetComponent<PlayerInventory>();
        playerAttack = GetComponent<PlayerAttack>();
        affixManager = GetComponent<AffixManager>();
    }

    void OnEnable()
    {
        inventory.OnWeaponsChanged += RefreshWeapons;
        inventory.OnRelicsChanged  += RefreshRelics;
    }

    void OnDisable()
    {
        inventory.OnWeaponsChanged -= RefreshWeapons;
        inventory.OnRelicsChanged  -= RefreshRelics;
    }

    void RefreshWeapons()
    {
        // 重建 PlayerAttack 的武器数组
        // TODO：根据 WeaponData 实例化对应 Weapon Prefab
        // 临时：只更新词条到 AffixManager
        var allWeaponAffixes = new System.Collections.Generic.List<Affix>();
        for (int i = 0; i < inventory.MaxWeaponSlots; i++)
        {
            var weapon = inventory.GetWeapon(i);
            if (weapon?.affixes != null)
                allWeaponAffixes.AddRange(weapon.affixes);
        }
        affixManager?.EquipWeapon(allWeaponAffixes);
    }

    void RefreshRelics()
    {
        // 遗物效果：清掉旧遗物词条，重新注入
        var allRelicAffixes = new System.Collections.Generic.List<Affix>();
        for (int i = 0; i < inventory.RelicCount; i++)
        {
            var relic = inventory.GetRelic(i);
            // 遗物当前没有词条，等遗物效果系统做好后再填
        }

        // TODO: 遗物系统做好后，调用 affixManager.EquipRelic(allRelicAffixes)
    }
}