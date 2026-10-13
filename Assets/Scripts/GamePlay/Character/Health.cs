using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Health : MonoBehaviour
{
    float maxHealth;
    float currentHealth;
    public bool IsDead => currentHealth <= 0f;

    public event System.Action OnDeath;

    public void Init(float max)
    {
        maxHealth = max;
        currentHealth = max;
    }

    public void TakeDamage(float amount)
    {
        if (IsDead) return;
        currentHealth -= amount;
        if (currentHealth <= 0f)
        {
            currentHealth = 0f;
            OnDeath?.Invoke();
        }
    }
}

