using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 背包里的一个武器槽。由 InventoryUI 绑定数据，点击时通知 InventoryUI。
/// </summary>
public class InventoryWeaponSlotUI : MonoBehaviour,
    IPointerClickHandler, IPointerEnterHandler
{
    [Header("显示")]
    [SerializeField] Image icon;
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] Image border;
    [SerializeField] GameObject emptyMark;

    public event Action<InventoryWeaponSlotUI> Clicked;
    public event Action<InventoryWeaponSlotUI> Hovered;

    public int SlotIndex { get; private set; }
    public Weapon Weapon { get; private set; }

    public void Bind(int index, Weapon weapon)
    {
        SlotIndex = index;
        Weapon = weapon;

        bool hasWeapon = weapon != null && weapon.Data != null;

        if (icon != null)
        {
            icon.sprite = hasWeapon ? weapon.Data.icon : null;
            icon.enabled = hasWeapon;
        }

        if (nameText != null)
        {
            nameText.text = hasWeapon ? weapon.Data.displayName : "";
            nameText.enabled = hasWeapon;
        }

        if (border != null)
            border.color = hasWeapon
                ? ShopHUDTheme.RarityColor(weapon.Rarity)
                : ShopHUDTheme.EmptyBorder;

        if (emptyMark != null)
            emptyMark.SetActive(!hasWeapon);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        Clicked?.Invoke(this);
    }

    public void OnPointerEnter(PointerEventData eventData) => Hovered?.Invoke(this);
}