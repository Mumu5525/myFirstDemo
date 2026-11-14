using UnityEngine;

[RequireComponent(typeof(PlayerInputReader))]
[RequireComponent(typeof(PlayerInventory))]
public class PlayerAttack : MonoBehaviour
{
    public Transform model;
    [SerializeField] Weapon fistWeapon;   // 没有装备武器时的兜底，不占武器槽

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
        Weapon weapon = inventory.CurrentWeapon ?? fistWeapon;
        weapon?.Attack(model.gameObject);
    }
}