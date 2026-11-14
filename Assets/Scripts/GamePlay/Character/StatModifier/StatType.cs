using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StatType
{
    // 注意：此枚举按整数序列化进 AffixData / RelicData 资产，只能末尾追加，不要删除或重排。
    MeleeDamage,     // 近战伤害加成（复用原 Damage 的 0 号位置）
    MaxHealth,
    Speed,
    Defense,
    Vulnerability,   // 易伤：受到伤害额外增加的比例（%），0~正无穷
    AttackSpeed,
    CriticalChance,
    CriticalDamage,
    ProjectileCount,
    ProjectileSpeed,
    ProjectileSize,
    PierceCount,
    MeleeRange,
    ExplosionRadius,
    ExplosionDamage,
    RangedDamage,    // 远程伤害加成
}
