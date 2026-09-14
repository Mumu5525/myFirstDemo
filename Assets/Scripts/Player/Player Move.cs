using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public Transform model;
    public LayerMask groundLayer;
    public Transform orientation;
    Rigidbody rb;
    Animator anim;
    GroundCheck groundCheck;
    Camera mainCamera;
    //移动方向
    Vector3 moveDirection;

    // 垂直Input
    float horizontalInput;
    // 水平Input
    float verticalInput;

    public PlayerData data;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
        groundCheck = GetComponent<GroundCheck>();
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        Inputs();
        AimAtMouse();
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

    void AimAtMouse()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if(Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
        {
            Vector3 lookDir = hit.point - model .position;
            lookDir.y = 0;

            if(lookDir != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDir);
                model.rotation = Quaternion.Slerp(model.rotation, targetRotation, data.rotationSpeed * Time.deltaTime);
            }
        }
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
