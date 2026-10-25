using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 遗物模板（ScriptableObject）。
/// 自带品质和内置效果（类似词条，但不随机）。
/// </summary>
[CreateAssetMenu(fileName = "RelicData", menuName = "Data/RelicData")]
public class RelicData : ScriptableObject
{
    [System.Serializable]
    public struct RelicEffect
    {
        public StatType statType;
        public ModifierType modifierType;
        public float value;
    }

    public string displayName = "遗物";
    [TextArea(1, 3)]
    public string description;
    [Tooltip("基础价格，商店根据品质进一步调价")]
    public int price = 50;
    [Tooltip("遗物品质（固定，不随机）")]
    public AffixTier tier = AffixTier.T1;
    [Tooltip("内置效果（类似词条，购买时转换为运行时 Affix）")]
    public List<RelicEffect> effects = new();
}