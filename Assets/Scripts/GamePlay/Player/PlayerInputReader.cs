using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputReader : MonoBehaviour
{
    PlayerControls controls;
    public Vector2 Move => controls.Player.Move.ReadValue<Vector2>();
    public bool Attack => controls.Player.Attack.IsPressed();
    public bool switchP => controls.Player.SwitchWeaponPrevious.WasPerformedThisFrame()
                            || Mouse.current.scroll.ReadValue().y > 0f;
    public bool switchN => controls.Player.SwitchWeaponNext.WasPerformedThisFrame()
                            || Mouse.current.scroll.ReadValue().y < 0f;

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
