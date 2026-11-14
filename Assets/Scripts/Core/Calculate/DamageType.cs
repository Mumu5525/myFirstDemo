/// <summary> 伤害来源类型。伤害管线只记录事实，具体表现由订阅者决定。 </summary>
public enum DamageType
{
    Direct,           // 近战、子弹等实体攻击
    DamageOverTime,   // 流血、中毒、灼烧
    Environment       // 清场、陷阱、坠落
}

/// <summary> 伤害吃哪一项角色加成。None 表示不吃角色加成（如环境伤害）。 </summary>
public enum DamageScaling
{
    None,
    Melee,
    Ranged
}