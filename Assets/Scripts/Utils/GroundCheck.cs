using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    Transform target;
    public float distance;
    public LayerMask groundLayer;


    void Awake()
    {
        target = GetComponent<Transform>();
    }
    public bool IsGrounded()
    {
        return Physics.Raycast(target.position, Vector3.down, distance,groundLayer);
    }
}
