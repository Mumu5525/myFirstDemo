using UnityEngine;

public class PlayerAnimator : CharacterAnimator
{
    [Header("动画播放速度")]
    [Tooltip("动画以 1x 播放时对应的地面移动速度")]
    [SerializeField] float referenceSpeed = 6f;
    [SerializeField] float minScale = 0.25f;
    [SerializeField] float maxScale = 2f;

    public void SetLocomotion(Vector2 localVelocity)
    {
        SetFloat(AnimParams.Player.HorizontalSpeed, localVelocity.x, 0.1f);
        SetFloat(AnimParams.Player.VerticalSpeed,   localVelocity.y, 0.1f);

        // 用实际地面速度决定动画播放倍率
        float planarSpeed = localVelocity.magnitude;

        float speedScale = planarSpeed > 0.1f
            ? Mathf.Clamp(planarSpeed / referenceSpeed, minScale, maxScale)
            : 1f;   // 站着不动时保持正常速度，避免 idle 冻住

        SetFloat(AnimParams.Player.MoveSpeedScale, speedScale, 0.1f);
    }

    public void PlayHit()  => SetTrigger(AnimParams.Player.Hit);
    public void PlayFire() => SetTrigger(AnimParams.Player.Fire);
}
