using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Health))]
public class EnemyDeath : MonoBehaviour
{
    Enemy enemy;

    public float ragdollDuration = 3.5f;

    void OnEnable()
    {
        enemy.Health.OnDeath += HandleDeath;
    }
    void OnDisable()
    {
        enemy.Health.OnDeath -= HandleDeath;
    }


    void Awake()
    {
        enemy = GetComponent<Enemy>();
    }

    void HandleDeath()
    {
        enemy.Agent.enabled = false;
        GetComponent<EnemyAttack>()?.EndAttack();   // 死亡会停掉 Animator，OnStateExit 不会再回调
        LayerSetter.Set(transform, LayerMask.NameToLayer("DeadEnemy"));
        enemy.Ragdoll.Enable();
        StartCoroutine(ReturnAfterDelay(ragdollDuration));
    }


    IEnumerator ReturnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        EventBus<OnEnemyRemoved>.Publish(new OnEnemyRemoved { enemy = gameObject, wasKilled = true });
    }
}
