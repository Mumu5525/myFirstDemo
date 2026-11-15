using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputReader : MonoBehaviour
{
    PlayerControls controls;

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
    public bool SwitchP => controls.Player.SwitchWeaponPrevious.WasPressedThisFrame()
                            || Mouse.current.scroll.ReadValue().y > 0f;
    public bool SwitchN => controls.Player.SwitchWeaponNext.WasPressedThisFrame()
                            || Mouse.current.scroll.ReadValue().y < 0f;
    #endregion

    void Awake() 
    {
        controls = new PlayerControls();
    }

    void OnEnable()
    {
        controls.Enable();
    }

    void OnDisable()
    {
        controls.Disable();
    }

    void Update()
    {
        if (controls.Camera.TurnLeft.WasPressedThisFrame())
            EventBus<OnCameraRotate>.Publish(new OnCameraRotate(){degrees = 90f});
        if (controls.Camera.TurnRight.WasPressedThisFrame())
            EventBus<OnCameraRotate>.Publish(new OnCameraRotate(){degrees = -90f});
    }
}
