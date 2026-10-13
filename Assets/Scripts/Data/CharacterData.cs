using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Data/CharacterData")]
public class CharacterData : ScriptableObject
{
    [Header("移动")]
    public float speed = 3f;
    public float smoothing = 10f;

    [Header("转向")]
    public float rotationSpeed = 10f;

    [Header("属性")]
    public float atk;
    public float health;

}
