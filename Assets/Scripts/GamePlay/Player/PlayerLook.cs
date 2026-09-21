using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    public LayerMask groundLayer;
    Camera mainCamera;
    public Transform model;
    public PlayerData data;

    void Start()
    {
        mainCamera = Camera.main;
    }
    void Update()
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
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
}
