using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 背包里的一个遗物槽。由 InventoryUI 绑定数据，悬停时通知 InventoryUI。
/// 遗物没有点击替换流程，所以只实现悬停事件。
/// </summary>
public class InventoryRelicSlotUI : MonoBehaviour, IPointerEnterHandler
{
    [Header("显示")]
    [SerializeField] Image icon;
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] Image border;
    [SerializeField] GameObject emptyMark;

    public event Action<InventoryRelicSlotUI> Hovered;

    public int SlotIndex { get; private set; }
    public RelicData Data { get; private set; }

    public void Bind(int index, RelicData data)
    {
        SlotIndex = index;
        Data = data;

        bool hasRelic = data != null;

        if (icon != null)
        {
            icon.sprite = hasRelic ? data.icon : null;
            icon.enabled = hasRelic;
        }

        if (nameText != null)
        {
            nameText.text = hasRelic ? data.displayName : "";
            nameText.enabled = hasRelic;
        }

        if (border != null)
            border.color = hasRelic
                ? ShopHUDTheme.RarityColor(data.tier)
                : ShopHUDTheme.EmptyBorder;

        if (emptyMark != null)
            emptyMark.SetActive(!hasRelic);
    }

    public void OnPointerEnter(PointerEventData eventData) => Hovered?.Invoke(this);
}