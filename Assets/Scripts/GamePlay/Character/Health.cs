using UnityEngine;

[RequireComponent(typeof(CharacterStats))]
[RequireComponent(typeof(AffixManager))]
public class Health : MonoBehaviour
{
    CharacterStats stats;
    AffixManager affixManager;
    BuffManager buffManager;

    enum Status
    {
        Alive,
        Dead
    }

    float currentHealth;
    float maxHealth;
    Status status = Status.Alive;
    GameObject latestAttacker = null;


    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsDead => status == Status.Dead;
    public GameObject LatestAttacker => latestAttacker;

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

    void setHealth(float amount)
    {
        currentHealth = Mathf.Min(maxHealth, amount);
    }

    void setMaxHealth(float amount)
    {
        maxHealth = Mathf.Max(0f, amount);
    }

    /// <summary> 满血复活：刷新 MaxHealth，回满 CurrentHealth，并重置死亡状态（对象池复用必需）。 </summary>
    public void Reset()
    {
        status = Status.Alive;
        latestAttacker = null;
        float tempMax = stats != null ? stats.GetStat(StatType.MaxHealth) : 0f;
        setMaxHealth(tempMax);
        setHealth(tempMax);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    /// <summary> 只刷新 MaxHealth（词条/Buff 变更时自动调用）。当前血量不变。 </summary>
    public void UpdateMaxHealth()
    {
        float newMax = stats != null ? stats.GetStat(StatType.MaxHealth) : 0f;  // 获取最新的 MaxHealth 值
        float clampedCurrent = Mathf.Min(currentHealth, newMax);  // 确保当前血量不超过 MaxHealth

        if (Mathf.Approximately(maxHealth, newMax) && Mathf.Approximately(currentHealth, clampedCurrent))
            return;

        setMaxHealth(newMax);
        setHealth(clampedCurrent);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    /// <summary> 治疗（当前血量 + amount）。 </summary> ///
    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    /// <summary>
    /// 扣除已结算好的最终伤害。
    /// 伤害计算统一在 DamageCalculator 里做，这里只负责血量状态和死亡通知。
    /// </summary>
    public void ApplyDamage(float amount, GameObject attacker = null)
    {
        if (IsDead || amount <= 0f) return;

        currentHealth = Mathf.Max(0f, currentHealth - amount);
        if (attacker != null) latestAttacker = attacker;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0f) Die();
    }

    void Die()
    {
        status = Status.Dead;

        // ─── 外层通知：EventBus<OnUnitDied> 全局广播，供跨系统监听（GoldSystem/WaveManager 等） ───
        EventBus<OnUnitDied>.Publish(new OnUnitDied { victim = gameObject, killer = latestAttacker });
        OnDeath?.Invoke();
    }
}