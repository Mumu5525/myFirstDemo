using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(RagDoll))]
public class EnemyDeath : MonoBehaviour
{
    NavMeshAgent agent;
    RagDoll rd;
    Health health;

    public Action OnExpire;

    public float ragdollDuration = 3.5f;

    void OnEnable() => health.OnDeath += HandleDeath;
    void OnDisable() => health.OnDeath -= HandleDeath;


    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<Health>();
        rd = GetComponent<RagDoll>();
    }

    void HandleDeath()
    {
        agent.enabled = false;
        LayerSetter.Set(transform, LayerMask.NameToLayer("DeadEnemy"));
        rd.Enable();
        StartCoroutine(ReturnAfterDelay(ragdollDuration));
    }


    IEnumerator ReturnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        OnExpire?.Invoke();
    }
}
