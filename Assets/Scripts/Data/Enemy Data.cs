using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyData : ScriptableObject
{
    [Header("移动")]
    public float speed;
    public float smoothing;

    [Header("转向")]
    public float rotarionSpeed;

    [Header("属性")]
    public float atk;
    public float health;

}
