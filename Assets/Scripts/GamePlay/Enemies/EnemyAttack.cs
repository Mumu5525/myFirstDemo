using UnityEngine;


// 配合AttackBehaviour挂在EnemyAnimator上，实现敌人攻击逻辑
// 攻击节奏全部由动画驱动：前摇填充扇形 → 命中帧结算一次 → 后摇
public class EnemyAttack : MonoBehaviour
{
    const string AttackTrigger = "Attack";
    const string AtkSpeed = "AttackSpeed";

    Enemy enemy;
    [SerializeField] LayerMask targetMask = 1 << 3;              // 目标层，默认 Player
    [SerializeField] float attackRange = 1.2f;                    // 扇形半径
    [SerializeField, Range(0f, 360f)] float attackAngle = 120f;   // 扇形总角度

    AttackSectorVisual sector;
    bool struck;   // 本次攻击是否已经结算过命中，防止命中帧被重复触发

    void Awake()
    {
        enemy = GetComponent<Enemy>();
        sector = AttackSectorVisual.Create(transform, attackRange, attackAngle);
    }

    void Update()
    {
        TryAttack();
    }

    void TryAttack()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            enemy.Anim.SetFloat(AtkSpeed, enemy.Stats.GetStat(StatType.AttackSpeed));
            enemy.Anim.SetTrigger(AttackTrigger);
        }
    }

    // 攻击状态进入
    public void BeginAttack()
    {
        struck = false;
        sector.SetFill(0f);
        sector.SetVisible(true);
    }

    // 由 AttackBehaviour 每帧推进，progress: 0~1
    public void SetProgress(float progress)
    {
        sector.SetFill(progress);
    }

    // 命中帧：结算一次扇形伤害
    public void Strike()
    {
        if (struck) return;
        struck = true;
        sector.SetFill(1f);
        DetectHit();
        sector.SetFill(0f);
        sector.SetVisible(false);
    }

    // 攻击被中断：状态退出 / 死亡 / 对象池回收，任何路径都必须走这里复位
    public void EndAttack()
    {
        struck = false;
        sector.SetFill(0f);
        sector.SetVisible(false);
    }

    // 扇形判定：先用球形查询圈出半径，再用水平夹角裁掉前方的扇形范围
    void DetectHit()
    {
        Vector3 origin = transform.position + Vector3.up;
        float damage = enemy.Stats.GetStat(StatType.Damage);

        Collider[] hits = Physics.OverlapSphere(origin, attackRange, targetMask, QueryTriggerInteraction.Ignore);
        foreach (var other in hits)
        {
            Vector3 toTarget = other.bounds.center - origin;
            toTarget.y = 0f;
            if (toTarget.sqrMagnitude > attackRange * attackRange) continue;
            if (Vector3.Angle(transform.forward, toTarget) > attackAngle * 0.5f) continue;

            Debug.Log($"{other.name} 被攻击了");
            DamageCalculator.DealDamage(enemy.gameObject, other.gameObject, damage);
        }
    }
}