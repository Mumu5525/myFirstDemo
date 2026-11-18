using UnityEngine;

/// <summary>
/// 伤害数字生成器：订阅全局伤害/治疗事件，从对象池取数字并播放。
/// 场景里放一个即可，挂在 Screen Space - Overlay Canvas 下的某个节点上。
/// </summary>
public class DamagePopupSpawner : MonoBehaviour
{
    [Header("引用")]
    [SerializeField] DamagePopup popupPrefab;
    [SerializeField] Transform container;        // Canvas 下的 RectTransform，留空则用自身
    [SerializeField] int prewarm = 12;

    [Header("位置")]
    [SerializeField] float heightOffset = 2f;     // 数字从角色脚底往上偏移

    [Header("颜色")]
    [SerializeField] Color normalColor = Color.white;
    [SerializeField] Color criticalColor = new Color(1f, 0.2f, 0.2f);
    [SerializeField] Color healColor = new Color(0.2f, 1f, 0.4f);

    [Header("字号")]
    [SerializeField] float criticalSize = 1.4f;

    ObjectPool<DamagePopup> pool;

    void Awake()
    {
        if (popupPrefab == null)
        {
            Debug.LogError("[DamagePopupSpawner] 没有设置 popupPrefab");
            enabled = false;
            return;
        }

        if (container == null) container = transform;
        pool = new ObjectPool<DamagePopup>(popupPrefab, prewarm, container);
    }

    void OnEnable()
    {
        EventBus<OnUnitHit>.Subscribe(HandleHit);
        EventBus<OnUnitHealed>.Subscribe(HandleHealed);
    }

    void OnDisable()
    {
        EventBus<OnUnitHit>.Unsubscribe(HandleHit);
        EventBus<OnUnitHealed>.Unsubscribe(HandleHealed);
    }

    bool HandleHit(OnUnitHit e)
    {
        if (e.victim == null || e.damage <= 0f) return false;

        // 清场击杀一次会带走所有敌人，弹满屏 99999 没有意义
        if (e.type == DamageType.Environment) return false;

        Vector3 pos = e.victim.transform.position + Vector3.up * heightOffset;
        string text = Mathf.RoundToInt(e.damage).ToString();
        Color color = e.isCritical ? criticalColor : normalColor;
        float size = e.isCritical ? criticalSize : 1f;

        Play(pos, text, color, size);
        return false;
    }

    bool HandleHealed(OnUnitHealed e)
    {
        if (e.target == null) return false;

        Vector3 pos = e.target.transform.position + Vector3.up * heightOffset;

        // 满血治疗也要有反馈，但不能弹 "+0"
        string text = e.amount > 0f ? "+" + Mathf.RoundToInt(e.amount) : "满血";
        Play(pos, text, healColor, 1f);
        return false;
    }

    void Play(Vector3 pos, string text, Color color, float size)
    {
        DamagePopup popup = pool.Get();
        popup.Play(pos, text, color, size, ReturnPopup);
    }

    void ReturnPopup(DamagePopup popup)
    {
        pool.Return(popup);
    }
}