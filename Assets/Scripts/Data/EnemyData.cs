using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Data/EnemyData")]
public class EnemyData : ScriptableObject
{
    [Header("移动")]
    public float speed = 2f;
    public float smoothing = 10f;

    [Header("转向")]
    public float rotarionSpeed = 10f;

    [Header("属性")]
    public float atk;
    public float health;

}
