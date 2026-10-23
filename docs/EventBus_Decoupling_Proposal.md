# 事件总线解耦方案 — 任务书

> 项目：《吸血鬼幸存者》风格 Roguelite 游戏  
> 目标：通过统一的事件总线系统，降低模块间的直接耦合  
> 创建日期：2026-10-21

---

## 一、现状概述

### 1.1 当前事件系统

项目已有 `GameEvents` 静态类，包含 5 个全局事件：

| 事件 | 发布方 | 订阅方 |
|------|--------|--------|
| `OnRunStateChanged(bool)` | — | — |
| `OnWaveFinished(int)` | WaveManager | Shop |
| `OnAnyDeath(DeathInfo)` | Health | GoldSystem |
| `OnCameraRotateLeft()` | PlayerInputReader | TopDownCamera |
| `OnCameraRotateRight()` | PlayerInputReader | TopDownCamera |

### 1.2 当前架构图

```
┌─────────────────────────────────────────────────────────────┐
│                     直接 GetComponent 耦合                    │
│                                                              │
│  GoldSystem ──→ PlayerInventory (GetComponentInParent)       │
│  GoldSystem ──→ CharacterData  (GetComponent)                │
│  Health ──────→ BuffManager    (GetComponent)                │
│  Health ──────→ AffixManager   (GetComponent)                │
│  WaveManager ─→ Health.TakeDamage()                         │
│  Projectile ──→ Health.TakeDamage()                         │
│  MeleeWeapon ─→ Health.TakeDamage()                         │
│  PlayerAttack → PlayerInputReader (GetComponent)             │
│  EnemyDeath ──→ LayerSetter     (静态调用)                   │
│  PlayerDeath ─→ PlayerMove.enabled = false                  │
│                                                              │
│                    C# event 耦合（同GameObject）              │
│  PlayerEquipment ←→ PlayerInventory (OnWeaponsChanged)       │
│  PlayerEquipment ←→ PlayerInventory (OnRelicsChanged)        │
│  Health ─────────→ PlayerDeath    (OnDeath event)            │
│  Health ─────────→ EnemyDeath     (OnDeath event)            │
│  WaveManager ←─── EnemyDeath.OnExpire Action                 │
└─────────────────────────────────────────────────────────────┘
```

---

## 二、待解耦系统清单

### 🔴 一级 — 高耦合，强烈推荐解耦

| # | 耦合点 | 当前写法 | 建议事件名 | 优先级 |
|---|--------|---------|-----------|--------|
| 1 | GoldSystem ↔ PlayerInventory | `GetComponentInParent<PlayerInventory>()` 后直接调 `AddGold()` | `OnGoldAwarded(GoldAwardInfo)` | P0 |
| 2 | GoldSystem ↔ CharacterData | `GetComponent<CharacterData>()` 读 `baseGold` | 合入 #1，GoldAwardInfo 携带金额 | P0 |
| 3 | Health ↔ BuffManager / AffixManager | `GetComponent<BuffManager>()?.DispatchEvent()` | `OnUnitHit(HitInfo)`, `OnUnitDeath(DeathInfo)` | P0 |
| 4 | WaveManager ↔ 敌人存活管理 | 直接调 `Health.TakeDamage(99999f)`；手动维护 `aliveEnemies` 列表 | `OnEnemySpawned`, `OnEnemyRemoved` | P0 |
| 5 | WaveManager ↔ 波次结束 | `GameEvents.WaveFinished(wave)` | 保持，升级为强类型 | P1 |

### 🟡 二级 — 中度耦合，推荐解耦

| # | 耦合点 | 当前写法 | 建议事件名 | 优先级 |
|---|--------|---------|-----------|--------|
| 6 | PlayerInputReader ↔ TopDownCamera | `GameEvents.CameraRotateLeft/Right()` | `OnCameraRotate(CameraRotateDir)` | P1 |
| 7 | PlayerEquipment ↔ PlayerInventory | C# event `OnWeaponsChanged` | `OnWeaponEquipped`, `OnRelicEquipped` | P2 |
| 8 | PlayerAttack ↔ PlayerInputReader | 直接读 `input.Attack` | `OnAttackInput`, `OnSwitchWeapon` | P2 |
| 9 | Projectile / Melee ↔ Health | 直接调 `TakeDamage()` | 统一伤害管线 → `RequestDamage`/`OnDamageDealt` | P1 |
| 10 | EnemyDeath ↔ WaveManager | `OnExpire` Action 回调 | `OnEnemyRemoved` | P1 |

### 🟢 三级 — 轻耦合，可选解耦

| # | 耦合点 | 当前写法 | 建议事件名 | 优先级 |
|---|--------|---------|-----------|--------|
| 11 | EnemyDeath ↔ LayerSetter | 静态调用层名 | `OnEnemyDeathBegin`, `OnEnemyDeathEnd` | P3 |
| 12 | PlayerDeath ↔ PlayerMove | `move.enabled = false` | `OnPlayerDeath`, `OnPlayerRevive` | P3 |
| 13 | BuffManager ↔ GameEventType | `DispatchEvent(GameEventType.OnHit)` | 合入统一事件总线 | P3 |
| 14 | Shop ↔ PlayerInventory | 直接调用 `SpendGold/AddWeapon/AddRelic` | `OnItemPurchased`（已有） | P3 |

---

## 三、事件总线升级目标

### 3.1 从静态 weak-type 升级为强类型 EventBus

```
现状：GameEvents 是静态类 + 裸 Action 委托
        → 无法按类型分发、无法做优先级、无生命周期管理

目标：EventBus<T> 泛型总线
        → 按事件 struct 分发
        → 支持优先级排序
        → 支持拦截（如护盾拦截伤害）
        → 天然解耦：发布方不引用订阅方，订阅方不引用发布方
```

### 3.2 目标架构图

```
                          ┌─────────────────────┐
                          │     EventBus<T>      │
                          │   (泛型事件总线)      │
                          └──────┬───┬───┬──────┘
                                 │   │   │
              ┌──────────────────┼───┼───┼──────────────────┐
              │                  │   │   │                  │
              ▼                  ▼   ▼   ▼                  ▼
    ┌─────────────┐   ┌─────────────┐   ┌─────────────┐   ┌─────
    │ PlayerAttack │   │  GoldSystem │   │  WaveManager │   │ ...
    │ 发送:         │   │ 订阅:       │   │ 发送:        │   │
    │ RequestDamage│   │ OnUnitDeath │   │ OnWaveFinish │   │
    └─────────────┘   └─────────────┘   └─────────────┘   └─────
```

---

## 四、实施计划

### Phase 1：基础设施（P0）
- [ ] 实现 `EventBus<T>` 泛型类（注册/注销/触发/优先级/拦截）
- [ ] 定义核心事件结构体：`HitInfo`, `DeathInfo`, `GoldAwardInfo`, `WaveInfo`, `CameraInputInfo`
- [ ] 迁移现有 5 个 `GameEvents` 事件到新总线
- [ ] 废弃旧的 `GameEventType` enum（其功能被事件 struct 替代）

### Phase 2：伤害管线解耦（P0）
- [ ] Health 不再直接调 `BuffManager.DispatchEvent` / `AffixManager.DispatchEvent`
- [ ] BuffManager 和 AffixManager 自行订阅 `OnUnitHit` / `OnUnitDeath`
- [ ] GoldSystem 不再 `GetComponent` PlayerInventory，改为订阅 `OnUnitDeath`
- [ ] WaveManager 不再 `GetComponent<Health>.TakeDamage()`，改为发布 `KillAllEnemies` / 订阅 `OnEnemyRemoved`

### Phase 3：战斗输入解耦（P1）
- [ ] PlayerInputReader 发布 `OnAttackInput` / `OnSwitchWeapon` 事件
- [ ] PlayerAttack 订阅输入事件（不再 `GetComponent<PlayerInputReader>`）
- [ ] Projectile / MeleeWeapon 发布 `OnDamageDealt`，不再直接调 `Health.TakeDamage`

### Phase 4：装备经济解耦（P2）
- [ ] PlayerInventory 的变更通过全局事件发布
- [ ] 新增 UI / 音效 / 成就系统可独立订阅

### Phase 5：表现层解耦（P3）
- [ ] LayerSetter、RagDoll、动画等通过事件驱动

---

## 五、验收标准

1. **零 GetComponent 跨模块调用**：除工具类和同模块内部，不再有 `GetComponent<X>` 引用其他系统
2. **新增系统零代码修改**：如加"击杀统计成就"，只需订阅 `OnUnitDeath`，不修改 Health/WaveManager/Enemy
3. **伤害可拦截**：护盾 Buff 可在事件总线的 pre-handler 阶段阻止伤害
4. **事件可重放**：调试/回放时重放事件序列即可复现游戏状态