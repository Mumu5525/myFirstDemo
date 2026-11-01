using TMPro;
using UnityEngine;

/// <summary>
/// 详情面板里的一行词条 / 效果：深灰底 + 一行文本。
/// 由 ShopDetailPanelUI 按需实例化并复用。
/// </summary>
public class ShopStatRowUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI label;

    public void SetText(string text)
    {
        if (label != null) label.text = text;
    }
}