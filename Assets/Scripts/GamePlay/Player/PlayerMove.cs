using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public Transform model;
    public PlayerData data;
    Rigidbody rb;
    Animator anim;
    GroundCheck groundCheck;
    Vector3 moveDirection;
    PlayerInputReader input;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
        groundCheck = GetComponent<GroundCheck>();
        input = GetComponent<PlayerInputReader>();
    }
    void Update()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        Vector3 localVel = model.InverseTransformDirection(flatVel);
        anim.SetFloat("MoveX", localVel.x, 0.1f, Time.deltaTime);
        anim.SetFloat("MoveZ", localVel.z, 0.1f, Time.deltaTime);
        anim.SetFloat("AnimaSpeed",data.speed/6f);
        anim.SetBool("Grounded",groundCheck.IsGrounded());
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }
    
    private void MovePlayer()
    {
        moveDirection = new Vector3(input.Move.x, 0, input.Move.y);
        Vector3 targetSpeed = moveDirection.normalized * data.speed;
        Vector3 currentSpeed = new Vector3(rb.velocity.x,0,rb.velocity.z);
        Vector3 flat = Vector3.MoveTowards(currentSpeed,targetSpeed,data.smoothing);
        rb.velocity = new Vector3(flat.x, rb.velocity.y, flat.z);
    }
}
