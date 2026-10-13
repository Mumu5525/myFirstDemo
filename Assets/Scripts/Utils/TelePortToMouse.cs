using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TelePortToMouse : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (MouseWorld.TryGetGroundPoint(Camera.main, transform.position, out Vector3 dest))
                transform.position = dest;
        }
    }
}
