using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StatType 
{
    Damage,
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
}
