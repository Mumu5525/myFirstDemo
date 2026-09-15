using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopDownCamera : MonoBehaviour
{
    [Header("跟随目标")]
    public Transform target;

    [Header("相机偏移")]
    public Vector3 offset = new Vector3(0, 8f, -8f);

    [Header("相机平滑度")]
    public float smoothing = 0.15f;

    [Header("相机缩放")]
    [Range(0.5f,2f)]
    public float zoom = 1f;

    Vector3 velocity;

    void LateUpdate()
    {
        if(target == null) return;

        Vector3 targetPos = target.position + offset * zoom;
        transform.position = Vector3.SmoothDamp(transform.position,targetPos, ref velocity, smoothing);
    }
}
