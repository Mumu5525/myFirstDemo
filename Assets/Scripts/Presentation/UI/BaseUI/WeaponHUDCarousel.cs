using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 横向武器 HUD 轮播。
/// 每把武器生成一张卡片，首尾相接形成无限列表；当前武器始终吸附在容器中心。
/// 切枪（滚轮/N-P）时由 PlayerInventory.OnWeaponsChanged 驱动滚动。
/// </summary>
public class WeaponHUDCarousel : MonoBehaviour
{
    [Header("引用")]
    [SerializeField] WeaponHUDUI cardPrefab;
    [Tooltip("留空自动从 Player Tag 上查找")]
    [SerializeField] PlayerInventory inventory;

    [Header("布局")]
    [Tooltip("相邻卡片的水平间距")]
    [SerializeField] float spacing = 350f;
    [Tooltip("吸附速度，越大越快")]
    [SerializeField] float snapSpeed = 12f;

    // 三份副本：中间一份用于显示，左右两份用于无缝循环
    const int COPIES = 3;

    readonly List<WeaponHUDUI> cards = new();
    readonly List<int> stripIndices = new();
    readonly List<Weapon> weapons = new();

    float animatedIndex;
    float targetIndex;

    void Awake()
    {
        if (GetComponent<RectMask2D>() == null)
            gameObject.AddComponent<RectMask2D>();
    }

    void Start()
    {
        if (inventory == null)
        {
            var player = PlayerLocator.Find();
            if (player != null)
                inventory = player.GetComponent<PlayerInventory>();
        }

        if (inventory == null || cardPrefab == null)
        {
            Debug.LogError("[WeaponHUDCarousel] 缺少 PlayerInventory 或卡片 prefab！");
            enabled = false;
            return;
        }

        inventory.OnWeaponsChanged += OnWeaponsChanged;
        Rebuild();
        SnapToCurrentInstant();
    }

    void OnDestroy()
    {
        if (inventory != null)
            inventory.OnWeaponsChanged -= OnWeaponsChanged;
    }

    void Update()
    {
        if (cards.Count == 0) return;

        animatedIndex = Mathf.Lerp(animatedIndex, targetIndex,
            1f - Mathf.Exp(-snapSpeed * Time.unscaledDeltaTime));

        if (Mathf.Abs(animatedIndex - targetIndex) < 0.001f)
            animatedIndex = targetIndex;

        Recenter();
        ApplyPositions();
    }

    void OnWeaponsChanged()
    {
        // 增删/替换武器：重建卡片并直接对准当前武器
        if (!SameWeaponSet())
        {
            Rebuild();
            SnapToCurrentInstant();
            return;
        }

        // 仅切换武器：平滑滚到新武器
        ScrollToCurrent();
    }

    bool SameWeaponSet()
    {
        int count = 0;
        foreach (var w in inventory.Weapons)
            if (w != null) count++;

        if (count != weapons.Count) return false;

        int i = 0;
        foreach (var w in inventory.Weapons)
            if (w != null)
            {
                if (weapons[i] != w) return false;
                i++;
            }

        return true;
    }

    void Rebuild()
    {
        foreach (var card in cards)
        {
            if (card == null) continue;
            card.gameObject.SetActive(false);
            Destroy(card.gameObject);
        }

        cards.Clear();
        stripIndices.Clear();
        weapons.Clear();

        foreach (var w in inventory.Weapons)
            if (w != null)
                weapons.Add(w);

        int n = weapons.Count;
        if (n == 0) return;

        for (int copy = 0; copy < COPIES; copy++)
        {
            for (int i = 0; i < n; i++)
            {
                var card = Instantiate(cardPrefab, transform);
                card.Bind(weapons[i]);

                var rt = (RectTransform)card.transform;
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);

                cards.Add(card);
                stripIndices.Add(copy * n + i);
            }
        }
    }

    void SnapToCurrentInstant()
    {
        int n = weapons.Count;
        if (n == 0) return;

        animatedIndex = targetIndex = n + CurrentListIndex();
        ApplyPositions();
    }

    void ScrollToCurrent()
    {
        int n = weapons.Count;
        if (n == 0) return;

        targetIndex = NearestStripIndex(CurrentListIndex());
    }

    int CurrentListIndex()
    {
        var current = inventory.CurrentWeapon;
        for (int i = 0; i < weapons.Count; i++)
            if (weapons[i] == current)
                return i;

        return 0;
    }

    /// <summary> 在三份副本中选离当前动画位置最近的一份，避免为了切一把枪滚过整条列表。 </summary>
    int NearestStripIndex(int listIndex)
    {
        int n = weapons.Count;
        int best = n + listIndex;
        float bestDist = Mathf.Abs(best - animatedIndex);

        for (int copy = 0; copy < COPIES; copy++)
        {
            int candidate = copy * n + listIndex;
            float dist = Mathf.Abs(candidate - animatedIndex);
            if (dist < bestDist)
            {
                best = candidate;
                bestDist = dist;
            }
        }

        return best;
    }

    /// <summary> 动画越过中间一份后整体平移 N 格。三份卡片内容相同，所以视觉上没有跳变。 </summary>
    void Recenter()
    {
        int n = weapons.Count;
        if (n == 0) return;

        if (animatedIndex < n)
        {
            animatedIndex += n;
            targetIndex += n;
        }
        else if (animatedIndex >= 2 * n)
        {
            animatedIndex -= n;
            targetIndex -= n;
        }
    }

    void ApplyPositions()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            var rt = (RectTransform)cards[i].transform;
            rt.anchoredPosition = new Vector2((stripIndices[i] - animatedIndex) * spacing, 0f);
        }
    }
}