using UnityEngine;
using UnityEngine.InputSystem;

public enum InputMode
{
    Gameplay,
    Shop,
    Inventory
}

/// <summary>
/// 统一输入入口。所有按键都从 PlayerControls 的 Action Map 读取，
/// 通过 SetMode 只启用当前模式需要的 Map，实现商店/背包输入互斥。
/// </summary>
public class PlayerInputReader : MonoBehaviour
{
    PlayerControls controls;
    InputMode mode = InputMode.Gameplay;

    #region 玩家移动输入
    public Vector2 Move => controls.Player.Move.ReadValue<Vector2>();
    #endregion

    #region 玩家鼠标位置输入
    public Vector2 MousePosition => controls.Player.MousePosition.ReadValue<Vector2>();
    #endregion

    #region 玩家攻击输入
    public bool AttackPressed => controls.Player.Attack.WasPressedThisFrame();
    public bool AttackHeld => controls.Player.Attack.IsPressed();
    #endregion

    #region 玩家滚轮输入
    public bool SwitchP => controls.Player.SwitchWeaponScroll.ReadValue<float>() > 0f;
    public bool SwitchN => controls.Player.SwitchWeaponScroll.ReadValue<float>() < 0f;
    #endregion

    #region UI 快捷键
    public bool ShopTogglePressed      => controls.Shop.OpenClose.WasPressedThisFrame();
    public bool ShopRefreshPressed     => controls.Shop.Refresh.WasPressedThisFrame();
    public bool ShopUpgradePressed     => controls.Shop.Upgrade.WasPressedThisFrame();
    public bool InventoryTogglePressed => controls.Inventory.OpenClose.WasPressedThisFrame();
    public bool BackPressed            => controls.Basic.Back.WasPressedThisFrame();
    public bool ShiftHeld              => controls.Basic.Shift.IsPressed();
    #endregion

    void Awake()
    {
        controls = new PlayerControls();
    }

    void OnEnable()
    {
        SetMode(mode);
    }

    void OnDisable()
    {
        controls.Disable();
    }

    /// <summary>
    /// 切换输入模式。
    /// 注意：Shop / Inventory 两个开关 Action 所在的 Map 不要在开关界面时反复
    /// Disable→Enable，否则按键仍按住时会被重新判定为按下，造成界面闪烁。
    /// </summary>
    public void SetMode(InputMode newMode)
    {
        mode = newMode;

        bool gameplay = newMode == InputMode.Gameplay;
        bool shop     = newMode == InputMode.Shop;
        bool inventory = newMode == InputMode.Inventory;

        SetMapEnabled(controls.Player, gameplay);
        SetMapEnabled(controls.Camera, gameplay);
        SetMapEnabled(controls.Basic, true);

        // 两个开关始终可用（Gameplay 下可打开，打开后也可用同一键关闭）
        SetMapEnabled(controls.Shop, gameplay || shop);
        SetMapEnabled(controls.Inventory, gameplay || inventory);

        // 只有商店界面里才响应 Q/E
        SetActionEnabled(controls.Shop.Refresh, shop);
        SetActionEnabled(controls.Shop.Upgrade, shop);
    }

    static void SetMapEnabled(InputActionMap map, bool enabled)
    {
        if (enabled)
        {
            if (!map.enabled) map.Enable();
        }
        else
        {
            if (map.enabled) map.Disable();
        }
    }

    static void SetActionEnabled(InputAction action, bool enabled)
    {
        if (enabled)
        {
            if (!action.enabled) action.Enable();
        }
        else
        {
            if (action.enabled) action.Disable();
        }
    }

    void Update()
    {
        if (controls.Camera.TurnLeft.WasPressedThisFrame())
            EventBus<OnCameraRotate>.Publish(new OnCameraRotate { degrees = 90f });
        if (controls.Camera.TurnRight.WasPressedThisFrame())
            EventBus<OnCameraRotate>.Publish(new OnCameraRotate { degrees = -90f });
    }
}