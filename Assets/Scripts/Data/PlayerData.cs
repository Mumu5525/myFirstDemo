using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Data/PlayerData")]
public class PlayerData : ScriptableObject
{
    [Header("移动")]
    public float speed = 3.8f;
    public float runningSpeed = 5f;
    public float smoothing = 10f;

    [Header("转向")]
    public float rotationSpeed = 10f;
}
