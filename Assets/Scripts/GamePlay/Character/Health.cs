using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Health : MonoBehaviour
{
    float maxHealth;
    float currentHealth;
    CharacterStats stats;

    public bool IsDead => currentHealth <= 0f;

    public event System.Action OnDeath;

    void Awake()
    {
        stats = GetComponent<CharacterStats>();
    }

    public void Init(float max)
    {
        maxHealth = max;
        currentHealth = max;
    }

    public void TakeDamage(float amount)
    {
        if (IsDead) return;

        DispatchEvent(GameEventType.OnHit);

        // ─── 防御减免（衰减曲线，永远到不了 100%） ───
        float defense = stats != null ? stats.GetStat(StatType.Defense) : 0f;
        float defenseMult = 100f / (100f + Mathf.Max(0f, defense));

        // ─── 易伤倍率（0~正无穷%） ───
        float vuln = stats != null ? stats.GetStat(StatType.Vulnerability) : 0f;
        float vulnMult = 1f + Mathf.Max(0f, vuln) / 100f;

        float final = amount * defenseMult * vulnMult;
        currentHealth -= final;

        if (currentHealth <= 0f)
        {
            currentHealth = 0f;
            DispatchEvent(GameEventType.OnDeath);
            OnDeath?.Invoke();
        }
    }

    void DispatchEvent(GameEventType evt)
    {
        GetComponent<BuffManager>()?.DispatchEvent(evt);
        GetComponent<AffixManager>()?.DispatchEvent(evt);
    }
}

