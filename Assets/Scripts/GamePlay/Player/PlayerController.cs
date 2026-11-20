using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    Rigidbody rb;
    GroundCheck groundCheck;
    PlayerInputReader input;
    PlayerInventory inventory;
    PlayerAnimator anim;
    CharacterStats stats;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<PlayerAnimator>();
        groundCheck = GetComponent<GroundCheck>();
        input = GetComponent<PlayerInputReader>();
        inventory = GetComponent<PlayerInventory>();
        stats = GetComponent<CharacterStats>();
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

    /// <summary> 当前武器开火时的人机工效倍率。 </summary>
    float TurnMultiplier
        => inventory != null && inventory.CurrentWeapon != null
            ? inventory.CurrentWeapon.TurnSpeedMultiplier
            : 1f;

    float MoveMultiplier
        => inventory != null && inventory.CurrentWeapon != null
            ? inventory.CurrentWeapon.MoveSpeedMultiplier
            : 1f;
    
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
        Vector3 targetSpeed = moveDirection * (stats.GetStat(StatType.Speed) * MoveMultiplier);
        Vector3 currentSpeed = new Vector3(rb.velocity.x,0,rb.velocity.z);
        Vector3 flat = Vector3.MoveTowards(currentSpeed,targetSpeed,stats.GetStat(StatType.Smoothing));

        //移动玩家
        rb.velocity = new Vector3(flat.x, rb.velocity.y, flat.z);
    }

    void PlayerRotate()
    {
        //朝向完全由代码控制，清掉碰撞产生的角冲量，避免与 MoveRotation 打架
        rb.angularVelocity = Vector3.zero;

        //获取相机方向
        Camera camera = Camera.main;
        Ray ray = camera.ScreenPointToRay(input.MousePosition);
        if(MouseWorld.TryGetGroundPoint(camera, transform.position, out Vector3 hitPoint))  
        {
            Vector3 lookDir = hitPoint - transform.position;
            lookDir.y = 0;

            if(lookDir != Vector3.zero)
            {
                // 人机工效：开火时降低转向速度
                float turnSpeed = stats.GetStat(StatType.RotationSpeed) * TurnMultiplier;

                Quaternion targetRotation = Quaternion.LookRotation(lookDir);
                Quaternion slerpRptation = Quaternion.Slerp(rb.rotation, targetRotation, turnSpeed * Time.deltaTime);
                rb.MoveRotation(slerpRptation);
            }
        }
    }

    void PlayAnimation()
    {
        //计算动画参数
        Vector3 flatVel = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        Vector3 localVel = transform.InverseTransformDirection(flatVel);

        //更新动画参数
        anim.SetLocomotion(new Vector2(localVel.x, localVel.z));
    }
}
