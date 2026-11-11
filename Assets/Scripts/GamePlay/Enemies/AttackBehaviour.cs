using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackBehaviour : StateMachineBehaviour
{
    [Range(0f, 1f)] public float hitTime = 0.24f;   // 命中帧在 clip 中的归一化位置

    // 注意：StateMachineBehaviour 实例被所有使用该控制器的 Animator 共享，
    // 不能缓存 EnemyAttack，每次都要从传入的 animator 上取
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 状态一进入（攻击动画开始播放）就开始充能
        animator.GetComponentInParent<EnemyAttack>()?.BeginAttack();
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        EnemyAttack attack = animator.GetComponentInParent<EnemyAttack>();
        if (attack == null) return;

        // normalizedTime 已经包含攻速缩放，扇形填充和命中天然和动画同步
        attack.SetProgress(stateInfo.normalizedTime / Mathf.Max(hitTime, 0.01f));
        if (stateInfo.normalizedTime >= hitTime) attack.Strike();
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponentInParent<EnemyAttack>()?.EndAttack();
    }
}