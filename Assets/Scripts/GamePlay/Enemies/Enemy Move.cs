using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;


[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
public class EnemyMove : MonoBehaviour
{
    NavMeshAgent agent;
    Rigidbody rb;
    GroundCheck groundCheck;
    Animator anim;
    Enemy enemy;
    CharacterStats stats;

    void Awake()
    {
        groundCheck = GetComponent<GroundCheck>();
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
        enemy = GetComponent<Enemy>();
        stats = GetComponent<CharacterStats>();
    }

    void Start()
    {
        agent.updatePosition = false;
        agent.updateRotation = false;
        agent.speed = stats != null ? stats.GetStat(StatType.Speed) : enemy.Data.speed;
    }
    void FixedUpdate()
    {
        rb.velocity = new Vector3(0, rb.velocity.y, 0);
        rb.angularVelocity = Vector3.zero;

        Vector3 currentVelocity = rb.velocity;
        Vector3 desireVelocity = agent.desiredVelocity;
        Vector3 flat = Vector3.MoveTowards(currentVelocity,desireVelocity,enemy.Data.smoothing);
        rb.velocity = new Vector3(flat.x, rb.velocity.y, flat.z);

        agent.nextPosition = rb.position;

        FaceMoveDirection();
    }

    void Update()
    {
        anim.SetFloat("Speed",rb.velocity.magnitude);
        anim.SetFloat("AnimationSpeed", rb.velocity.magnitude/4.5f);

        Transform target = enemy.Target;
        if (target != null && agent.enabled)
        {
            agent.SetDestination(target.position);
        }
    }

    void FaceMoveDirection()
    {
        Vector3 flatVelocity = new Vector3(rb.velocity.x, 0 ,rb.velocity.z);

        if(flatVelocity.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(flatVelocity);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, enemy.Data.rotationSpeed * Time.fixedDeltaTime));
        }
    }
}
