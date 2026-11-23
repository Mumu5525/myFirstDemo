using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 靶子实时伤害统计：挂在靶子上，订阅自身 Health.OnDamaged。
/// 维护一个滚动时间窗口，周期性通过 EventBus 发布 OnDummyStats 给 UI 显示。
///
/// DPS = 窗口内总伤害 / 窗口实际跨度。
/// 窗口未满时用实际跨度（至少 1 秒，避免首发瞬间爆表），窗口满了之后按固定窗口滑动。
/// 停手超过 resetSeconds 后，下一次命中视为新的一次测试，清空累计数据。
/// </summary>
[RequireComponent(typeof(Health))]
public class DummyDamageTracker : MonoBehaviour
{
    [Header("统计窗口")]
    [Tooltip("滚动 DPS 的时间窗口（秒）")]
    [SerializeField] float windowSeconds = 5f;

    [Tooltip("UI 刷新间隔（秒）")]
    [SerializeField] float pushInterval = 0.1f;

    [Tooltip("停手多久后，下一次命中开始新的一次统计")]
    [SerializeField] float resetSeconds = 5f;

    [Header("靶子回血")]
    [Tooltip("未受到伤害多久后开始回血（秒）")]
    [SerializeField] float regenDelay = 3f;

    [Tooltip("每秒回复的血量")]
    [SerializeField] float regenPerSecond = 20000f;

    struct Hit
    {
        public float time;
        public float damage;
    }

    readonly Queue<Hit> windowHits = new Queue<Hit>();
    float windowDamage;

    // 本次测试累计
    float totalDamage;
    int hitCount;
    int critCount;
    float maxHit;

    float lastHitTime;
    float pushTimer;
    float regenTimer;

    Health health;

    void Awake()
    {
        health = GetComponent<Health>();
    }

    void OnEnable()
    {
        if (health != null) health.OnDamaged += HandleDamaged;
        EventBus<OnUnitPreDeath>.Subscribe(HandlePreDeath);
    }

    void OnDisable()
    {
        if (health != null) health.OnDamaged -= HandleDamaged;
        EventBus<OnUnitPreDeath>.Unsubscribe(HandlePreDeath);
    }

    /// <summary> 把血量补回来，Health 检测到血量 > 0 就会取消死亡。 </summary>
    bool HandlePreDeath(OnUnitPreDeath e)
    {
        if (e.victim != gameObject) return false;
        health.Revive(1f);
        return false;
    }

    void HandleDamaged(float amount, DamageType type, bool isCritical)
    {
        float now = Time.time;

        // 停手过久：这次命中开始新的一轮测试
        if (hitCount > 0 && now - lastHitTime > resetSeconds)
            ResetSession();

        windowHits.Enqueue(new Hit { time = now, damage = amount });
        windowDamage += amount;

        totalDamage += amount;
        hitCount++;
        if (isCritical) critCount++;
        maxHit = Mathf.Max(maxHit, amount);
        lastHitTime = now;
    }

    void Update()
    {
        float now = Time.time;

        // 淘汰滑出窗口的命中
        while (windowHits.Count > 0 && now - windowHits.Peek().time > windowSeconds)
            windowDamage -= windowHits.Dequeue().damage;

        // 未受伤 regenDelay 秒后开始回血，每秒回复一次
        if (health != null && now - lastHitTime > regenDelay
            && health.CurrentHealth < health.MaxHealth)
        {
            regenTimer += Time.deltaTime;
            if (regenTimer >= 1f)
            {
                regenTimer -= 1f;
                health.Heal(regenPerSecond);
            }
        }
        else
        {
            regenTimer = 0f;
        }

        pushTimer += Time.deltaTime;
        if (pushTimer < pushInterval) return;
        pushTimer = 0f;

        EventBus<OnDummyStats>.Publish(new OnDummyStats
        {
            dummy = gameObject,
            dps = CurrentDps(now),
            totalDamage = totalDamage,
            hitCount = hitCount,
            critCount = critCount,
            maxHit = maxHit,
            windowSeconds = windowSeconds,
        });
    }

    float CurrentDps(float now)
    {
        if (windowDamage <= 0f || windowHits.Count == 0) return 0f;

        // 窗口未满时用实际跨度，最少按 1 秒算，避免第一发瞬间 DPS 爆表
        float span = Mathf.Clamp(now - windowHits.Peek().time, 1f, windowSeconds);
        return windowDamage / span;
    }

    void ResetSession()
    {
        windowHits.Clear();
        windowDamage = 0f;
        totalDamage = 0f;
        hitCount = 0;
        critCount = 0;
        maxHit = 0f;
    }
}