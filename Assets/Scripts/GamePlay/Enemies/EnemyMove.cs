using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class EnemyMove : MonoBehaviour
{
    Enemy enemy;

    void Awake()
    {
        enemy = GetComponent<Enemy>();
    }

    void Start()
    {
        enemy.Agent.updatePosition = false;
        enemy.Agent.updateRotation = false;
        enemy.Agent.speed = enemy.Stats != null ? enemy.Stats.GetStat(StatType.Speed) : enemy.Data.speed;
    }
    void FixedUpdate()
    {
        CalculateSpeed();
        FaceMoveDirection();
    }

    void Update()
    {
        Move();
    }

    void FaceMoveDirection()
    {
        Vector3 flatVelocity = new Vector3(enemy.Rb.velocity.x, 0 ,enemy.Rb.velocity.z);

        if(flatVelocity.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(flatVelocity);
            enemy.Rb.MoveRotation(Quaternion.Slerp(enemy.Rb.rotation, targetRotation, enemy.Data.rotationSpeed * Time.fixedDeltaTime));
        }
    }

    void Move()
    {
        enemy.Anim.SetFloat("Speed",enemy.Rb.velocity.magnitude);

        Transform target = enemy.Target;
        if (target != null && enemy.Agent.enabled)
        {
            enemy.Agent.SetDestination(target.position);
        }
    }

    void CalculateSpeed()
    {
        enemy.Rb.velocity = new Vector3(0, enemy.Rb.velocity.y, 0);
        enemy.Rb.angularVelocity = Vector3.zero;

        Vector3 currentVelocity = enemy.Rb.velocity;
        Vector3 desireVelocity = enemy.Agent.desiredVelocity;
        Vector3 flat = Vector3.MoveTowards(currentVelocity, desireVelocity, enemy.Data.smoothing);
        enemy.Rb.velocity = new Vector3(flat.x, enemy.Rb.velocity.y, flat.z);

        enemy.Agent.nextPosition = enemy.Rb.position;
    }
}
