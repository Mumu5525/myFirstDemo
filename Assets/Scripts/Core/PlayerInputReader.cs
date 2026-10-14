using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputReader : MonoBehaviour
{
    PlayerControls controls;
    public Vector2 Move => controls.Player.Move.ReadValue<Vector2>();
    public bool Attack => controls.Player.Attack.IsPressed();
    public bool SwitchP => controls.Player.SwitchWeaponPrevious.WasPressedThisFrame()
                            || Mouse.current.scroll.ReadValue().y > 0f;
    public bool SwitchN => controls.Player.SwitchWeaponNext.WasPressedThisFrame()
                            || Mouse.current.scroll.ReadValue().y < 0f;

    public bool TurnLeft => controls.Camera.TurnLeft.WasPressedThisFrame();
    public bool TurnRight => controls.Camera.TurnRight.WasPressedThisFrame();

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
}
