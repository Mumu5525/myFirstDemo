using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionCheck : MonoBehaviour
{
    public Transform target;

    public Vector3 Get()
    {
        return new Vector3(target.position.x, target.position.y, target.position.z);
    }
}
