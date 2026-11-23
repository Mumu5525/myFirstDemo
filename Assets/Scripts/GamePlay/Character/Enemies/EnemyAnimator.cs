using UnityEngine;

/// <summary> 敌人动画的语义接口。 </summary>
public class EnemyAnimator : CharacterAnimator
{
    public void SetMoveSpeed(float speed)
    {
        SetFloat(AnimParams.Enemy.Speed, speed);
    }

    public void SetAttackSpeed(float speed)
    {
        SetFloat(AnimParams.Enemy.AttackSpeed, speed);
    }

    public void PlayAttack()
    {
        SetTrigger(AnimParams.Enemy.Attack);
    }
}
