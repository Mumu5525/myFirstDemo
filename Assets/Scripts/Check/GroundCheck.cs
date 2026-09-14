using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    public Transform target;
    public float distance;
    public LayerMask groundLayer;

    public bool IsGrounded()
    {
        return Physics.Raycast(target.position, Vector3.down, distance,groundLayer);
    }
}
