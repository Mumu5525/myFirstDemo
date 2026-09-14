using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Data/PlayerData")]
public class PlayerData : ScriptableObject
{
    [Header("移动")]
    public float speed = 1f;
    public float runningSpeed = 12f;
    public float smoothing = 10f;

    [Header("地面检测")]
    public float groundCheckDistance = 0.4f; 

    [Header("转向")]
    public float rotationSpeed = 10f;
}
