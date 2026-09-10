using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{


    [Header("Movement")]
    public float movespeed;

    public Transform orientation;

    // 垂直Input
    float horizontalInput;
    // 水平Input
    float verticalInput;

    public float jumpForce = 8f;
    public Transform groundCheck;
    public float groundCheckDistance = 0.2f;
    //移动方向
    Vector3 moveDirection;

    [Header("移动平滑度(越低越平滑)")]
    public float smoothing = 50f;

    [Header("转向速度")]
    public float rotationSpeed = 10f;

    Rigidbody rb;

    Animator anim;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        Inputs();
        anim.SetFloat("Speed",new Vector3(rb.velocity.x, 0, rb.velocity.z).magnitude);
        anim.SetBool("Grounded",IsGrounded());
        if(Input.GetKeyDown(KeyCode.Space) && IsGrounded())
        {
            Jump();
        }
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

    private void Jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(groundCheck.position, Vector3.down, groundCheckDistance);
    }
    

    private void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if(moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }

        Vector3 targetSpeed = moveDirection.normalized * movespeed;
        Vector3 currentSpeed = new Vector3(rb.velocity.x,0,rb.velocity.z);
        Vector3 flat = Vector3.MoveTowards(currentSpeed,targetSpeed,smoothing);
        rb.velocity = new Vector3(flat.x, rb.velocity.y, flat.z);
    }
}
