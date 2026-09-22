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
        Camera camera = Camera.main;
        float cameraYaw = camera.transform.eulerAngles.y;
        Vector3 cameraForward = Quaternion.Euler(0, cameraYaw, 0) * Vector3.forward;
        Vector3 cameraRight   = Quaternion.Euler(0, cameraYaw, 0) * Vector3.right;
        cameraRight.y = 0;
        cameraRight.Normalize();

        Vector2 inputDir = input.Move;
        moveDirection = (cameraForward * inputDir.y + cameraRight * inputDir.x).normalized;

        Vector3 targetSpeed = moveDirection * data.speed;
        Vector3 currentSpeed = new Vector3(rb.velocity.x,0,rb.velocity.z);
        Vector3 flat = Vector3.MoveTowards(currentSpeed,targetSpeed,data.smoothing);

        rb.velocity = new Vector3(flat.x, rb.velocity.y, flat.z);
    }
}
