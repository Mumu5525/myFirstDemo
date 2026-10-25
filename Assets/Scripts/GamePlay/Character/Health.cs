using UnityEngine;

[RequireComponent(typeof(CharacterStats))]
[RequireComponent(typeof(AffixManager))]
public class Health : MonoBehaviour
{
    CharacterStats stats;
    AffixManager affixManager;

    public bool IsDead => currentHealth <= 0f;
    float currentHealth;
    float maxHealth;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    /// <summary> 血量变化（受伤/治疗/重置）。给 UI 等展示层用。 </summary>
    public event System.Action<float, float> OnHealthChanged; // current, max

    /// <summary> 内层通知：仅自身组件使用（PlayerDeath/EnemyDeath 放布娃娃等）。实例级，不走全局广播。 </summary>
    public event System.Action OnDeath;

    void Awake()
    {
        stats = GetComponent<CharacterStats>();
        affixManager = GetComponent<AffixManager>();
    }

    void Start()
    {
        Reset();
    }

    void OnEnable()
    {
        if (affixManager != null)
            affixManager.OnAffixesChanged += UpdateMaxHealth;
    }

    void OnDisable()
    {
        if (affixManager != null)
            affixManager.OnAffixesChanged -= UpdateMaxHealth;
    }

    /// <summary> 满血复活：刷新 MaxHealth，回满 CurrentHealth。 </summary>
    public void Reset()
    {
        maxHealth = stats != null ? stats.GetStat(StatType.MaxHealth) : 0f;
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    /// <summary> 只刷新 MaxHealth（词条/Buff 变更时自动调用）。当前血量不变。 </summary>
    public void UpdateMaxHealth()
    {
        float newMax = stats != null ? stats.GetStat(StatType.MaxHealth) : 0f;
        float clampedCurrent = Mathf.Min(currentHealth, newMax);

        if (Mathf.Approximately(maxHealth, newMax) && Mathf.Approximately(currentHealth, clampedCurrent))
            return;

        maxHealth = newMax;
        currentHealth = clampedCurrent;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    /// <summary> 治疗（当前血量 + amount）。 </summary> ///
    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    /// <summary>受伤</summary>
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
        currentHealth = Mathf.Max(0f, currentHealth - final);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0f)
        {
            // ─── 外层通知：EventBus<OnUnitDied> 全局广播，供跨系统监听（GoldSystem/WaveManager 等） ───
            EventBus<OnUnitDied>.Publish(new OnUnitDied { victim = gameObject, killer = attacker });
            OnDeath?.Invoke();
        }
    }
}