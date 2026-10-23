using UnityEngine;

/// <summary>
/// 遗物模板（ScriptableObject）。效果固定，效果系统后续实现。
/// </summary>
[CreateAssetMenu(fileName = "RelicData", menuName = "Data/RelicData")]
public class RelicData : ScriptableObject
{
    public string displayName = "遗物";
    [TextArea(1, 3)]
    public string description;
    public int price = 50;
}