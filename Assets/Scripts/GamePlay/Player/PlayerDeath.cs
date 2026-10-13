using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(RagDoll))]
public class PlayerDeath : MonoBehaviour
{
    Health health;
    PlayerMove move;
    RagDoll rd;

    void OnEnable() => health.OnDeath += HandleDeath;
    void OnDisable() => health.OnDeath -= HandleDeath;

    void Awake()
    {
        health = GetComponent<Health>();
        move = GetComponent<PlayerMove>();
        rd = GetComponent<RagDoll>();
    }

    void HandleDeath()
    {
        rd.Enable();
        move.enabled = false;
        //TODO 死亡后的逻辑
    }
}
