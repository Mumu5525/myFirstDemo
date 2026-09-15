using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public Transform orientation;
    public Transform model;
    Rigidbody rb;
    Animator anim;
    GroundCheck groundCheck;
    Vector3 moveDirection;

    // 垂直Input
    float horizontalInput;
    // 水平Input
    float verticalInput;

    public PlayerData data;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
        groundCheck = GetComponent<GroundCheck>();
    }
    void Update()
    {
        Inputs();
        Vector3 flatVel = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        Vector3 localVel = model.InverseTransformDirection(flatVel);
        anim.SetFloat("MoveX", localVel.x, 0.1f, Time.deltaTime);
        anim.SetFloat("MoveZ", localVel.z, 0.1f, Time.deltaTime);
        anim.SetFloat("AnimaSpeed",data.speed/4.8f);
        anim.SetBool("Grounded",groundCheck.IsGrounded());
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void Inputs()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
    }
    
    private void MovePlayer()
    {
        moveDirection = new Vector3(horizontalInput, 0, verticalInput);
        Vector3 targetSpeed = moveDirection.normalized * data.speed;
        Vector3 currentSpeed = new Vector3(rb.velocity.x,0,rb.velocity.z);
        Vector3 flat = Vector3.MoveTowards(currentSpeed,targetSpeed,data.smoothing);
        rb.velocity = new Vector3(flat.x, rb.velocity.y, flat.z);
    }
}
