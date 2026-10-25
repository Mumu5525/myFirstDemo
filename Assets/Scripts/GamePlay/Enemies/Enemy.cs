using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public CharacterData baseData;                 // SO 模板（Inspector 拖）
    public CharacterData Data { get; private set; } // 运行时副本，buff 改这个

    /// <summary> 当前追踪目标（仅移动用），由生成方注入 </summary>
    public Transform Target { get; set; }

    Health health;
    RagDoll ragdoll;
    EnemyDeath death;
    NavMeshAgent agent;
    Rigidbody rb;

    void Awake()
    {
        Data = Instantiate(baseData);   // ← 克隆：改副本不影响资产
        health = GetComponent<Health>();
        ragdoll = GetComponent<RagDoll>();
        death = GetComponent<EnemyDeath>();
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
    }

    // 对象池复用的统一入口
    public void ReSpawn(Vector3 position, Transform target = null)
    {
        Target = target;

        rb.position = position;         // 重新设置位置
        agent.nextPosition = position;
        
        Data = Instantiate(baseData);   // ← 重建副本，清掉上一局的 buff 残留
        health.Reset();                 // 血量从 CharacterStats 读取
        agent.Warp(position);           // 放到 NavMesh 上
        agent.enabled = true;
        ragdoll.Disable();              // 退出布娃娃
        LayerSetter.Set(transform, LayerMask.NameToLayer("Enemy"));
    }
}

