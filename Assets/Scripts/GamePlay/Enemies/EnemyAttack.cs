using UnityEngine;


// 配合AttackBehaviour挂在EnemyAnimator上，实现敌人攻击逻辑
// 攻击节奏全部由动画驱动：前摇填充扇形 → 命中帧结算一次 → 后摇
public class EnemyAttack : MonoBehaviour
{

    Enemy enemy;
    [SerializeField] LayerMask targetMask = 1 << 3;              // 目标层，默认 Player
    [SerializeField] float defaultWindup = 0.2f;                  // 首次攻击前还没有实测前摇时的兜底值（秒）

    AttackSectorVisual sector;
    bool struck;      // 本次攻击是否已经结算过命中，防止命中帧被重复触发
    bool attacking;   // 攻击状态进行中，用来关掉起手检测
    float attackEnterTime;
    float lastWindup; // 上一次实测的前摇（秒），用于预测起手距离

    void Awake()
    {
        enemy = GetComponent<Enemy>();
        sector = AttackSectorVisual.Create(transform,
            enemy.Stats.GetStat(StatType.AttackRange),
            enemy.Stats.GetStat(StatType.AttackAngle));
    }

    void Update()
    {
        sector.SetShape(enemy.Stats.GetStat(StatType.AttackRange), enemy.Stats.GetStat(StatType.AttackAngle));   // 运行时改数据/加成也能即时反映
        if (!attacking) TryAttack();
    }

    // 前摇非零，所以要在进入攻击范围之前起手：
    // 让命中帧到来时，敌人与目标的距离正好落进攻击半径。
    void TryAttack()
    {
        Transform target = enemy.Target;
        if (target == null) return;

        Vector3 toTarget = target.position - transform.position;
        toTarget.y = 0f;
        float gap = toTarget.magnitude;
        if (gap < 0.001f) return;

        Vector3 flatVelocity = new Vector3(enemy.Rb.velocity.x, 0f, enemy.Rb.velocity.z);
        float closingSpeed = Vector3.Dot(flatVelocity, toTarget / gap);
        float windup = lastWindup > 0f ? lastWindup : defaultWindup;

        if (gap > enemy.Stats.GetStat(StatType.AttackRange) + Mathf.Max(0f, closingSpeed) * windup) return;

        enemy.Anim.SetAttackSpeed(Mathf.Max(0.01f, enemy.Stats.GetStat(StatType.AttackSpeed)));
        enemy.Anim.PlayAttack();
    }

    // 攻击状态进入
    public void BeginAttack()
    {
        attacking = true;
        attackEnterTime = Time.time;
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
        lastWindup = Time.time - attackEnterTime;   // 实测前摇，下一次起手判断用
        sector.SetFill(1f);
        DetectHit();
        sector.SetFill(0f);
        sector.SetVisible(false);
    }

    // 攻击被中断：状态退出 / 死亡 / 对象池回收，任何路径都必须走这里复位
    public void EndAttack()
    {
        attacking = false;
        struck = false;
        sector.SetFill(0f);
        sector.SetVisible(false);
    }

    // 扇形判定：先用球形查询圈出半径，再用水平夹角裁掉前方的扇形范围
    void DetectHit()
    {
        Vector3 origin = transform.position + Vector3.up;
        float range = enemy.Stats.GetStat(StatType.AttackRange);
        float halfAngle = enemy.Stats.GetStat(StatType.AttackAngle) * 0.5f;
        float damage = enemy.Stats.GetStat(StatType.AttackDamage);

        Collider[] hits = Physics.OverlapSphere(origin, range, targetMask, QueryTriggerInteraction.Ignore);
        foreach (var other in hits)
        {
            Vector3 toTarget = other.bounds.center - origin;
            toTarget.y = 0f;
            if (toTarget.sqrMagnitude > range * range) continue;
            if (Vector3.Angle(transform.forward, toTarget) > halfAngle) continue;

            DamageCalculator.DealDamage(enemy.gameObject, other.gameObject, damage, scaling: DamageScaling.Melee);
        }
    }
}