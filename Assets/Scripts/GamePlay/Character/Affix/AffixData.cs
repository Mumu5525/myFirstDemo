using UnityEngine;

/// <summary>
/// 词条模板（ScriptableObject）。
/// 在 Inspector 里配置一条词条的定义，运行时通过 AffixInstance 创建真实实例。
/// </summary>
[CreateAssetMenu(fileName = "AffixData", menuName = "Data/AffixData")]
public class AffixData : ScriptableObject
{
    [Header("基础")]
    public string displayName = "词条";
    public string id;
    public AffixTier tier = AffixTier.T1;

    [Header("行为类型")]
    public AffixBehavior behavior = AffixBehavior.Stat;

    // ─── 纯属性词条字段 ───
    [Header("属性修正（behavior = Stat 时生效）")]
    public StatType statType;
    public ModifierType modifierType = ModifierType.add;
    public float value;

    // ─── 触发器词条字段（预留） ───
    [Header("触发（behavior = Trigger 时生效）")]
    public GameEventType triggerEvent;
    [Tooltip("触发后执行的效果，暂时由代码判断")]
    public string triggerEffectId;

    // ─── 描述 ───
    [Header("显示")]
    [TextArea(1, 3)]
    public string description;
}

/// <summary>
/// 词条行为模式：决定运行时创建什么类型的 Affix 子类。
/// </summary>
public enum AffixBehavior
{
    /// <summary> 纯属性修正 → StatAffix </summary>
    Stat,
    /// <summary> 事件触发 → TriggerAffix（暂未实现） </summary>
    Trigger,
}