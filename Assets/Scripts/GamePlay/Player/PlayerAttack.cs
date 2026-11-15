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

    void Update()
    {
        Weapon weapon = inventory.CurrentWeapon ?? fistWeapon;
        if(weapon == null || weapon.Data == null) return;

        Vector3 aimPoint = model.position + model.forward * 10f;
        Camera cam = Camera.main;
        if(cam != null && MouseWorld.TryGetGroundPoint(cam, transform.position, out Vector3 groundPoint))
            aimPoint = groundPoint;

        weapon?.Attack(model.gameObject, aimPoint, input.AttackPressed, input.AttackHeld);
    }
}