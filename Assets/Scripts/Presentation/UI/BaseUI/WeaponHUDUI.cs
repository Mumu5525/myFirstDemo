using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 武器 HUD 卡片。由 WeaponHUDCarousel 实例化并绑定某一把武器，
/// 只负责显示这把武器的图标、名称、品质和弹药。
/// </summary>
public class WeaponHUDUI : MonoBehaviour
{
    [Header("显示")]
    [SerializeField] Slider ammoSlider;
    [SerializeField] Image icon;
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] Image border;

    Weapon boundWeapon;

    /// <summary> 绑定要显示的武器。传 null 会隐藏卡片。 </summary>
    public void Bind(Weapon weapon)
    {
        Unbind();

        if (weapon == null || weapon.Data == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);
        boundWeapon = weapon;
        boundWeapon.OnAmmoChanged += UpdateAmmo;

        if (icon != null)
        {
            icon.sprite = weapon.Data.icon;
            icon.enabled = weapon.Data.icon != null;
        }

        if (nameText != null)
            nameText.text = weapon.Data.displayName;

        if (border != null)
            border.color = ShopHUDTheme.RarityColor(weapon.Rarity);

        if (ammoSlider != null)
            ammoSlider.interactable = false;

        RefreshAmmo();
    }

    void OnDestroy() => Unbind();

    void Update()
    {
        if (boundWeapon == null)
            return;

        RefreshAmmo();
    }

    void RefreshAmmo()
    {
        if (ammoSlider == null || boundWeapon == null)
            return;

        // 词条/Buff 可能改变弹容量，每帧同步上限
        int max = Mathf.Max(1, boundWeapon.MagazineSize);
        ammoSlider.maxValue = max;

        if (!boundWeapon.UsesAmmo)
        {
            ammoSlider.value = 0f;
            return;
        }

        if (boundWeapon.HasInfiniteAmmo)
        {
            ammoSlider.value = max;
            return;
        }

        // 装填中按换弹时间平滑回填，其余时候显示剩余弹药
        ammoSlider.value = boundWeapon.IsReloading
            ? max * boundWeapon.ReloadProgress
            : Mathf.Min(boundWeapon.CurrentAmmo, max);
    }

    void UpdateAmmo(int current, int max)
    {
        if (ammoSlider == null) return;

        // 无弹匣武器（如拳头）保持 0 格
        ammoSlider.maxValue = Mathf.Max(1, max);
        ammoSlider.value = max > 0 ? Mathf.Min(current, max) : 0f;
    }

    void Unbind()
    {
        if (boundWeapon != null)
        {
            boundWeapon.OnAmmoChanged -= UpdateAmmo;
            boundWeapon = null;
        }
    }
}