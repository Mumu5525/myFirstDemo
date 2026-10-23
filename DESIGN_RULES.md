# 项目设计规范

---

## 一、核心原则

### 1. 不阿谀奉承
当用户提出不合理的设计方案、错误的推论、荒谬的假设时，**必须直接指出**，并给出自己的分析和推荐方案。不允许顺着用户的错误思路往下推。

### 2. 职责单一，边界清晰
每个类有且仅有一个职责。违反时必须纠正。

### 3. 用事件协调，不要用方法调用耦合
A 系统不需要知道 B 系统的存在。A 发事件，B 订阅事件。不要让 A 去调用 B 的方法，不要让 A 在生成对象时去 B 那里"注册"。

**反面案例**：金币系统给 WaveManager 加 Register 方法，让 WaveManager 生成敌人时主动注册。
**正确做法**：Health 发 OnAnyDeath 静态事件，GoldSystem 订阅一次全局生效。WaveManager 不知道金币系统的存在。

### 4. 先问"能不做吗"，再动手
每加一个新类、新方法、新参数之前，先问：能不能用已有的东西解决？能不能少改一个文件？用户提的需求是最终效果，不是实现方式。实现方式由我来判断，选改动最小的那个。

### 5. 能静态就不要挂载
如果一个系统不需要 Inspector 配置、不需要访问场景引用、只是纯逻辑协调，用静态类。不需要强制用户去场景里挂 GameObject。

### 6. 提需求时的互动规则
当用户提出一个需求时，必须先：
1. **指出可能存在的问题和不合理的地方** — 设计缺陷、职责越界、耦合风险、边界条件遗漏
2. **表达自己的观点** — 推荐方案是什么，为什么
3. **询问用户的意见** — 确认后再动手

不要直接听从用户的第一版方案。用户在提需求时说的往往是"想达到的效果"，不是"最佳实现方式"。拆解需求、挑战假设、给出更好的方案，才是我的价值。

---

## 二、各系统职责

### CharacterStats — 属性计算器
- **职责**：根据 CharacterData（基础值） + BuffManager（临时修正） + AffixManager（永久词条修正），计算出角色当帧的最终属性值。
- **不负责**：伤害计算、减伤公式、任何"两个角色之间的交互逻辑"。

### Health — 扣血/回血 / 死亡通知
- **职责**：接收已算好的最终伤害值，加减血量。死亡时触发 OnDeath / OnAnyDeath 事件。
- **不负责**：伤害公式、暴击判定、金币发放。

### Weapon / Projectile — 伤害计算
- **职责**：读取攻击方和目标方的 CharacterStats，计算最终伤害，传给 Health.TakeDamage。
- **不负责**：扣血（交给 Health）。

### Buff — 临时效果
- **职责**：有生命周期的临时修改器。通过 BuffManager 注入 CharacterStats。
- **特点**：会随时间过期、会被移除、会叠加刷新。

### Affix — 永久词条
- **职责**：武器/遗物生成时固定，不随时间变化。通过 AffixManager 注入 CharacterStats。

### GoldSystem — 金币系统（静态类）
- **职责**：订阅 Health.OnAnyDeath，判定击杀者是玩家时计算金币并发放。
- **不负责**：金币存储（交给 PlayerInventory）、UI 展示。

### PlayerInventory — 玩家背包（MonoBehaviour，挂在玩家身上）
- **职责**：存金币、武器（最多2把）、遗物（无上限）。提供 AddGold / SpendGold / AddWeapon / RemoveWeapon / AddRelic 等方法，变动时通知 UI。
- **不负责**：装备生效（交给 PlayerEquipment）、购买流程（Shop 调它）。

### PlayerEquipment — 装备协调器（MonoBehaviour，挂在玩家身上）
- **职责**：订阅 PlayerInventory 的武器/遗物变动事件，更新 AffixManager 的词条和 PlayerAttack 的武器列表。
- **不负责**：存储数据（交给 PlayerInventory）。

---

## 三、事件命名规范

| 事件 | 类型 | 用途 |
|------|------|------|
| `Health.OnDeath` | 实例事件 | 自己的死亡回调（EnemyDeath 用） |
| `Health.OnDeathWithInfo` | 实例事件 | 自己的死亡回调 + 击杀者信息 |
| `Health.OnAnyDeath` | **静态事件** | 全局死亡通知（金币系统、统计系统用） |
| `PlayerInventory.OnGoldChanged` | 实例事件 | 金币变动通知 UI |
| `PlayerInventory.OnWeaponsChanged` | 实例事件 | 武器变动通知 PlayerEquipment |
| `PlayerInventory.OnRelicsChanged` | 实例事件 | 遗物变动通知 PlayerEquipment |

---

## 四、代码风格

- 中文注释可以接受，但 API 名称和枚举值用英文
- 新功能先问"应该放在哪个类里"，确认职责边界后再写
- 每次讨论后如果产生了新的设计决策，更新本文档
- **少改文件，用事件解耦**