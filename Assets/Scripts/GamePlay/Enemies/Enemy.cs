using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{

    /// <summary> 当前追踪目标（仅移动用），由生成方注入 </summary>
    public Transform Target { get; set; }

    Health health;
    RagDoll ragdoll;
    EnemyDeath death;
    NavMeshAgent agent;
    Rigidbody rb;
    Animator anim;
    EnemyData data;
    CharacterStats stats;

    // 防御性懒加载：消费方在 Awake/OnEnable 阶段访问也不会拿到 null，
    // 不依赖本脚本 Awake 与组件排列顺序的先后。
    public Health Health => health != null ? health : (health = GetComponent<Health>());
    public RagDoll Ragdoll => ragdoll != null ? ragdoll : (ragdoll = GetComponent<RagDoll>());
    public EnemyDeath Death => death != null ? death : (death = GetComponent<EnemyDeath>());
    public NavMeshAgent Agent => agent != null ? agent : (agent = GetComponent<NavMeshAgent>());
    public Rigidbody Rb => rb != null ? rb : (rb = GetComponent<Rigidbody>());
    public Animator Anim => anim != null ? anim : (anim = GetComponentInChildren<Animator>());
    public CharacterStats Stats => stats != null ? stats : (stats = GetComponent<CharacterStats>());
    public EnemyData Data => data != null ? data : (data = Stats.GetRawData<EnemyData>());

    // 对象池复用的统一入口
    public void ReSpawn(Vector3 position, Transform target = null)
    {
        Target = target;
        Stats.ClearBuff();
        Rb.position = position;         // 重新设置位置
        Health.Reset();                 // 血量从 CharacterStats 读取
        Agent.nextPosition = position;
        Agent.Warp(position);           // 放到 NavMesh 上
        Agent.enabled = true;
        Ragdoll.Disable();              // 退出布娃娃（这里才会把 Animator 重新启用）
        Anim.Rebind();                  // 重置到默认状态；不会触发 OnStateExit，所以复位攻击要单独做
        Anim.Update(0f);
        GetComponent<EnemyAttack>()?.EndAttack();   // 兜底：清掉上一次残留的攻击表现
        LayerSetter.Set(transform, LayerMask.NameToLayer("Enemy"));
    }
}

