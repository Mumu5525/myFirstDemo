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
    public EnemyData Data;
    public GameObject target;

    void Awake()
    {
        groundCheck = GetComponent<GroundCheck>();
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        agent.updatePosition = false;
        agent.updateRotation = false;
    }
    void FixedUpdate()
    {
        rb.velocity = new Vector3(0, rb.velocity.y, 0);
        rb.angularVelocity = Vector3.zero;

        Vector3 currentVelocity = rb.velocity;
        Vector3 desireVelocity = agent.desiredVelocity;
        Vector3 flat = Vector3.MoveTowards(currentVelocity,desireVelocity,Data.smoothing);
        rb.velocity = new Vector3(flat.x, rb.velocity.y, flat.z);

        agent.nextPosition = rb.position;
    }

    void Update()
    {
        if(target != null )
            agent.SetDestination(target.transform.position);
    }
}
