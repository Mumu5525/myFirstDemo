using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopDownCamera : MonoBehaviour
{
    public Transform target;
    public float distance = 11.3f;      // 杆子长度（相机到中心点的距离，控制远近）
    public float pitch = 45f;           // 杆子俯仰角（越大越接近俯视）
    public float rotateSmooth = 0.15f;  // 环绕旋转平滑
    public float maxFollowSpeed = 25f;    // 追赶速度上限（越远越快，但封顶）
    public float followSharpness = 1f;    // 速度随距离增长的急切程度

    Vector3 orbitCenter;      // 环绕中心（平滑跟着玩家）
    float yaw;                // 当前环绕角
    float targetYaw;
    float yawVelocity;

    void Awake()
    {
        if (target == null) target = PlayerLocator.Find();
        if (target != null) orbitCenter = target.position;
    }

    void OnEnable()
    {
        EventBus<OnCameraRotate>.Subscribe(Rotate);
    }

    void OnDisable()
    {
        EventBus<OnCameraRotate>.Unsubscribe(Rotate);
    }

    bool Rotate(OnCameraRotate e)
    {
        targetYaw += e.degrees;
        return false;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 1. 环绕中心平滑跟随玩家
        Vector3 delta = target.position - orbitCenter;
        float speed = Mathf.Min(delta.magnitude * followSharpness, maxFollowSpeed);
        orbitCenter = Vector3.MoveTowards(orbitCenter, target.position, speed * Time.deltaTime);

        // 2. 环绕角平滑旋转
        yaw = Mathf.SmoothDampAngle(yaw, targetYaw, ref yawVelocity, rotateSmooth);

        // 3. 相机位置 = 中心点沿杆子反方向退 distance
        Quaternion rot = Quaternion.Euler(pitch, yaw, 0);
        transform.position = orbitCenter - (rot * Vector3.forward) * distance;

        // 4. 相机朝向 = 杆子朝向
        transform.rotation = rot;
    }
}
