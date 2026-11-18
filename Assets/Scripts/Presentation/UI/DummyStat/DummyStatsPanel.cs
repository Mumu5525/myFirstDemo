using TMPro;
using UnityEngine;

/// <summary>
/// 靶子统计面板：订阅 DummyDamageTracker 发布的 OnDummyStats，刷新一块 TMP_Text。
/// 挂在 Canvas 下，绑定一个 TMP_Text 即可。
/// </summary>
public class DummyStatsPanel : MonoBehaviour
{
    [SerializeField] TMP_Text label;

    void OnEnable()
    {
        EventBus<OnDummyStats>.Subscribe(HandleStats);
    }

    void OnDisable()
    {
        EventBus<OnDummyStats>.Unsubscribe(HandleStats);
    }

    bool HandleStats(OnDummyStats e)
    {
        if (label == null) return false;

        float critRate = e.hitCount > 0 ? e.critCount * 100f / e.hitCount : 0f;
        float avgHit = e.hitCount > 0 ? e.totalDamage / e.hitCount : 0f;

        label.text = $"DPS {e.dps:F1}（{e.windowSeconds:F0}s）\n"
                   + $"总伤 {e.totalDamage:F0}\n"
                   + $"命中 {e.hitCount}\n"
                   + $"暴击 {critRate:F0}%\n"
                   + $"最高 {e.maxHit:F0}\n"
                   + $"平均 {avgHit:F0}";

        return false;
    }
}