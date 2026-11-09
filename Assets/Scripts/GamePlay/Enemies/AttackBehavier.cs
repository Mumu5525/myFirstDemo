using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackBehaviour : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 状态一进入（攻击动画开始播放）就开启攻击碰撞体
        animator.GetComponentInParent<EnemyAttack>()?.OnAttackStart();
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 状态退出（攻击动画播放结束 / 切换走）时关闭攻击碰撞体
        animator.GetComponentInParent<EnemyAttack>()?.OnAttackEnd();
    }
}