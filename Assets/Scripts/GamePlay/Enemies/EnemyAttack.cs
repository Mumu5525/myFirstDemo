using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class EnemyAttack : MonoBehaviour
{
    const string AttackTrigger = "Attack";
    const string AtkSpeed = "AttackSpeed";

    Enemy enemy;
    [SerializeField] Collider AttackCollider;
    float attackSpeed;
    float damage;

    // 本次攻击动画区间内已经命中过的目标，避免同一目标被重复伤害
    readonly HashSet<Collider> alreadyHit = new HashSet<Collider>();

    void Awake()
    {
        enemy = GetComponent<Enemy>();
        AttackCollider.isTrigger = true;
    }

    void Start()
    {
        attackSpeed = enemy.Stats.GetStat(StatType.AttackSpeed);
        damage = enemy.Stats.GetStat(StatType.Damage);
    }

    void Update()
    {
        TryAttack();
    }

    void TryAttack()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            enemy.Anim.SetFloat(AtkSpeed, attackSpeed);
            enemy.Anim.SetTrigger(AttackTrigger);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"{other.name} 被攻击了");
        if (!other.CompareTag("Player")) return;
        if (alreadyHit.Contains(other)) return;
        alreadyHit.Add(other);
        other.gameObject.GetComponent<Health>()?.TakeDamage(damage, enemy.gameObject);
        
    }

    public void OnAttackStart()
    {
        alreadyHit.Clear();
        attackSpeed = enemy.Stats.GetStat(StatType.AttackSpeed);
        AttackCollider.enabled = true;
    }

    public void OnAttackEnd()
    {
        alreadyHit.Clear();
        AttackCollider.enabled = false;
    }
}