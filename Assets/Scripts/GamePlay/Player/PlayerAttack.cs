using UnityEngine;

[RequireComponent(typeof(PlayerInputReader))]
[RequireComponent(typeof(PlayerInventory))]
public class PlayerAttack : MonoBehaviour
{
    public Transform model;
    PlayerInputReader input;
    PlayerInventory inventory;

    void Awake()
    {
        input = GetComponent<PlayerInputReader>();
        inventory = GetComponent<PlayerInventory>();
    }

    void FixedUpdate()
    {
        if (!input.Attack) return;
        inventory.CurrentWeapon?.Attack(model.gameObject);
    }
}