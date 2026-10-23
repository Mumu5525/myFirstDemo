using UnityEngine;

public class Health : MonoBehaviour
{
    float maxHealth;
    float currentHealth;
    CharacterStats stats;

    public bool IsDead => currentHealth <= 0f;

    /// <summary> 内层通知：仅自身组件使用（PlayerDeath/EnemyDeath 放布娃娃等）。实例级，不走全局广播。 </summary>
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

    public void TakeDamage(float amount, GameObject attacker = null)
    {
        if (IsDead) return;

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
            // ─── 外层通知：EventBus<OnUnitDied> 全局广播，供跨系统监听（GoldSystem/WaveManager 等） ───
            EventBus<OnUnitDied>.Publish(new OnUnitDied { victim = gameObject, killer = attacker });
            OnDeath?.Invoke();
        }
    }
}
