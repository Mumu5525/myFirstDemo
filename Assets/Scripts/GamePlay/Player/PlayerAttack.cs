using UnityEngine;

[RequireComponent(typeof(PlayerInputReader))]
[RequireComponent(typeof(PlayerInventory))]
public class PlayerAttack : MonoBehaviour
{
    [SerializeField] Weapon fistWeapon;   // 没有装备武器时的兜底，不占武器槽

    PlayerInputReader input;
    PlayerInventory inventory;
    PlayerAnimator anim;

    void Awake()
    {
        input = GetComponent<PlayerInputReader>();
        inventory = GetComponent<PlayerInventory>();
        anim = GetComponent<PlayerAnimator>();
    }

    void Start() 
    {
        EventBus<OnWeaponAttack>.Subscribe(PlayFire);
    }
    void Update()
    {
        TryAttack();
    }

    void TryAttack()
    {
        Weapon weapon = inventory.CurrentWeapon ?? fistWeapon;
        if(weapon == null || weapon.Data == null) return;

        Vector3 aimPoint = transform.position + transform.forward * 10f;
        Camera cam = Camera.main;
        if(cam != null && MouseWorld.TryGetGroundPoint(cam, transform.position, out Vector3 groundPoint))
            aimPoint = groundPoint;

        weapon?.Attack(gameObject, aimPoint, input.AttackPressed, input.AttackHeld);
    }

    bool PlayFire(OnWeaponAttack o)
    {
        if(o.shooter != gameObject) return false;
        anim.PlayFire();
        return false;
    }
}