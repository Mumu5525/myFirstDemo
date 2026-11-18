using UnityEngine;
using UnityEngine.Animations.Rigging;

/// <summary>
/// 切换武器时把左手 IK 目标切到当前武器的左握把。
/// 右手使用场景中的固定目标，不参与切换。
/// </summary>
public class IKController : MonoBehaviour
{
    [SerializeField] TwoBoneIKConstraint leftHandConstraint;
    [SerializeField] RigBuilder rigBuilder;

    PlayerInventory inventory;

    void Awake()
    {
        inventory = GetComponentInParent<PlayerInventory>();
        if (rigBuilder == null)
            rigBuilder = GetComponentInParent<RigBuilder>();
    }

    void OnEnable()
    {
        if (inventory != null)
            inventory.OnWeaponsChanged += Refresh;
    }

    void OnDisable()
    {
        if (inventory != null)
            inventory.OnWeaponsChanged -= Refresh;
    }

    void Refresh()
    {
        Weapon weapon = inventory != null ? inventory.CurrentWeapon : null;

        leftHandConstraint.weight = weapon != null ? 1f : 0f;
        if (weapon == null || rigBuilder == null) return;

        // target 引用变了，必须重建 PlayableGraph 才会生效
        leftHandConstraint.data.target = weapon.LeftHandGrip;
        rigBuilder.Build();
    }
}