using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public static class MouseWorld
{
    // 获取鼠标指向的、经过 referencePoint 的水平面上的世界坐标
    public static bool TryGetGroundPoint(Camera camera, Vector3 referencePoint, out Vector3 point)
    {
        Ray ray = camera.ScreenPointToRay(Mouse.current.position.ReadValue());
        Plane plane = new Plane(Vector3.up, referencePoint);   // 数学平面，不依赖碰撞体

        if (plane.Raycast(ray, out float enter))
        {
            point = ray.GetPoint(enter);
            return true;
        }

        point = default;
        return false;
    }
}
