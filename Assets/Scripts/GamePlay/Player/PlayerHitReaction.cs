using UnityEngine;

/// <summary> 玩家受击表现：只有实体攻击触发上半身受击动画。 </summary>
[RequireComponent(typeof(Health))]
public class PlayerHitReaction : MonoBehaviour
{
    [Header("受击动画")]
    [SerializeField] float hitCooldown = 0.3f;

    [Header("受击反馈")]
    [SerializeField] CameraShake cameraShake;
    [SerializeField] ScreenEdgeFlash screenEdgeFlash;

    Health health;
    PlayerAnimator anim;
    float lastHitTime;

    void Awake()
    {
        health = GetComponent<Health>();
        anim = GetComponentInChildren<PlayerAnimator>();   // Animator 在 model 子物体上
    }

    void OnEnable()  => health.OnDamaged += PlayHit;
    void OnDisable() => health.OnDamaged -= PlayHit;

    void PlayHit(float amount, DamageType type, bool isCritical)
    {
        //只有实体攻击才有反馈
        if (type != DamageType.Direct) return;       // 流血/环境伤害不播
        
        //冷却 防止多段伤害多次播放
        if (Time.time - lastHitTime < hitCooldown) return;
        lastHitTime = Time.time;

        //动画
        anim.PlayHit();
        if(cameraShake != null) cameraShake.Shake();
        if(screenEdgeFlash != null) screenEdgeFlash.Play();
    }
}
