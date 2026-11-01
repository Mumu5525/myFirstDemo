using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    Rigidbody rb;
    Animator anim;
    GroundCheck groundCheck;
    PlayerInputReader input;
    [SerializeField] CharacterData data;

    public Transform model;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
        groundCheck = GetComponent<GroundCheck>();
        input = GetComponent<PlayerInputReader>();
    }
    void Update()
    {
        PlayAnimation();
    }

    private void FixedUpdate()
    {
        PlayerMove();
        PlayerRotate();
    }
    
    private void PlayerMove()
    {
        Vector3 moveDirection;

        //获取相机方向
        Camera camera = Camera.main;
        float cameraYaw = camera.transform.eulerAngles.y;
        Vector3 cameraForward = Quaternion.Euler(0, cameraYaw, 0) * Vector3.forward;
        Vector3 cameraRight   = Quaternion.Euler(0, cameraYaw, 0) * Vector3.right;
        cameraRight.y = 0;
        cameraRight.Normalize();

        //获取输入方向
        Vector2 inputDir = input.Move;
        moveDirection = (cameraForward * inputDir.y + cameraRight * inputDir.x).normalized;

        //计算目标速度
        Vector3 targetSpeed = moveDirection * data.speed;
        Vector3 currentSpeed = new Vector3(rb.velocity.x,0,rb.velocity.z);
        Vector3 flat = Vector3.MoveTowards(currentSpeed,targetSpeed,data.smoothing);

        //移动玩家
        rb.velocity = new Vector3(flat.x, rb.velocity.y, flat.z);
    }

    void PlayerRotate()
    {
        //获取相机方向
        Camera camera = Camera.main;
        
        Ray ray = camera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if(MouseWorld.TryGetGroundPoint(camera, model.position, out Vector3 hitPoint))  
        {
            Vector3 lookDir = hitPoint - model .position;
            lookDir.y = 0;

            if(lookDir != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDir);
                model.rotation = Quaternion.Slerp(model.rotation, targetRotation, data.rotationSpeed * Time.deltaTime);
            }
        }
    }

    void PlayAnimation()
    {
        //计算动画参数
        Vector3 flatVel = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        Vector3 localVel = model.InverseTransformDirection(flatVel);

        //更新动画参数
        anim.SetFloat("HorizontalSpeed", localVel.x, 0.1f, Time.deltaTime);
        anim.SetFloat("VerticalSpeed", localVel.z, 0.1f, Time.deltaTime);
        anim.SetFloat("AnimaSpeed",data.speed/6f);
        anim.SetBool("Grounded",groundCheck.IsGrounded());
    }
}
