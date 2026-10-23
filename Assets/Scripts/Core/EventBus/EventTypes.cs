using UnityEngine;

// ──────────────────────────────────────────────
// 所有全局事件结构体集中定义在一个文件中，方便查找和维护。
// 每个事件是一个纯 struct，放在 EventBus<T> 中传递。
// ──────────────────────────────────────────────

/// <summary> 游戏运行状态切换（暂停/继续） </summary>
public struct OnRunStateChanged { public bool isRunning; }

/// <summary> 一波结束 </summary>
public struct OnWaveFinished { public int waveNumber; }

/// <summary> 任意单位死亡（敌人或玩家） </summary>
public struct OnAnyDeath
{
    public GameObject victim;
    public GameObject killer;
}

/// <summary> 相机旋转输入（+90° 或 -90°） </summary>
public struct OnCameraRotate { public float degrees; }

// ──────────────────────────────────────────────
// Phase 2+ 预定义的事件（暂不接入，先留好 struct）
// ──────────────────────────────────────────────

/// <summary> 单位受到伤害前（可用于 Buff/护盾修改伤害值） </summary>
public struct BeforeUnitHit
{
    public GameObject victim;
    public GameObject attacker;
    public float rawDamage;
    public float finalDamage;   // handler 可以修改此字段
}

////// <summary> 单位受到伤害后（可用于 Buff 防止后续伤害/反伤等其他事件） </summary>
public struct AfterUnitHit
{
    public GameObject victim;
    public GameObject attacker;
    public float rawDamage;
    public float finalDamage;
}

/// <summary> 单位即将死亡（返回 true 可阻止死亡，如护盾/复活） </summary>
public struct OnUnitPreDeath
{
    public GameObject victim;
    public GameObject killer;
}

/// <summary> 单位已死亡（仅通知，不可阻止） </summary>
public struct OnUnitDied
{
    public GameObject victim;
    public GameObject killer;
}

/// <summary> 敌人被生成 </summary>
public struct OnEnemySpawned
{
    public GameObject enemy;
    public int waveNumber;
}

/// <summary> 敌人从场景移除（死亡或超时清场） </summary>
public struct OnEnemyRemoved
{
    public GameObject enemy;
    public bool wasKilled;  // true=死亡, false=超时回收
}