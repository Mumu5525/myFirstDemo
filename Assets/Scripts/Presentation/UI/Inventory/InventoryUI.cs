using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 背包面板。Tab 开关，滑入滑出。
/// 武器槽 / 遗物槽按 PlayerInventory 的数据动态生成。
/// 详情用固定在屏幕上的面板显示：
/// - 中间面板：悬停哪个背包物品就显示哪个，离开后保留最后一件
/// - 左侧面板：只在武器替换模式显示商店待购武器
/// 武器槽在替换模式下可点击，正常打开时点击无反应。
/// </summary>
public class InventoryUI : MonoBehaviour
{
    [Header("面板")]
    [SerializeField] GameObject inventoryPannel;
    [SerializeField] AnimationCurve showCurve;
    [SerializeField] AnimationCurve hideCurve;
    [SerializeField] float animationSpeed = 1f;

    [Header("武器槽")]
    [SerializeField] PlayerInventory inventory;
    [SerializeField] InventoryWeaponSlotUI weaponSlotPrefab;
    [SerializeField] RectTransform weaponSlotRoot;

    [Header("遗物槽")]
    [SerializeField] InventoryRelicSlotUI relicSlotPrefab;
    [SerializeField] RectTransform relicSlotRoot;

    [Header("详情面板")]
    [SerializeField] InventoryDetailPanelUI centerDetailPanel;   // 中间：背包物品
    [SerializeField] InventoryDetailPanelUI replaceDetailPanel;  // 左侧：待购武器

    [Header("输入")]
    [SerializeField] PlayerInputReader inputReader;

    [Header("属性预览")]
    [SerializeField] CharacterStats characterStats;   // 留空自动从玩家身上查找

    readonly List<InventoryWeaponSlotUI> weaponSlots = new();
    readonly List<InventoryRelicSlotUI> relicSlots = new();

    bool isOpen;
    Coroutine animationRoutine;

    bool replaceMode;
    Action<int> replaceSelected;
    Action replaceCancelled;

    Weapon hoveredWeapon;
    ItemEntry pendingReplaceItem;

    // ═══════════════════════════════════════════
    //  生命周期
    // ═══════════════════════════════════════════

    void Start()
    {
        // 没配曲线时给一个可用的默认值，避免 Evaluate 空引用
        if (showCurve == null || showCurve.length == 0)
            showCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
        if (hideCurve == null || hideCurve.length == 0)
            hideCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        if (inventory == null) inventory = FindFirstObjectByType<PlayerInventory>();
        if (inputReader == null) inputReader = FindFirstObjectByType<PlayerInputReader>();
        if (characterStats == null && inventory != null)
            characterStats = inventory.GetComponent<CharacterStats>();

        if (inventory != null)
        {
            inventory.OnWeaponsChanged += RebuildWeaponSlots;
            inventory.OnRelicsChanged += RebuildRelicSlots;
        }

        RebuildWeaponSlots();
        RebuildRelicSlots();

        if (inventoryPannel != null) inventoryPannel.SetActive(false);
        SetPanelVisible(centerDetailPanel, false);
        SetPanelVisible(replaceDetailPanel, false);
    }

    void OnDestroy()
    {
        if (inventory != null)
        {
            inventory.OnWeaponsChanged -= RebuildWeaponSlots;
            inventory.OnRelicsChanged -= RebuildRelicSlots;
        }

        foreach (var slot in weaponSlots)
        {
            if (slot == null) continue;
            slot.Clicked -= OnWeaponSlotClicked;
            slot.Hovered -= OnWeaponSlotHovered;
        }

        foreach (var slot in relicSlots)
        {
            if (slot == null) continue;
            slot.Hovered -= OnRelicSlotHovered;
        }
    }

    void Update()
    {
        if (inputReader == null) return;

        if (inputReader.InventoryTogglePressed) { Toggle(); return; }
        if (inputReader.BackPressed && isOpen) { Close(); }
    }

    // ═══════════════════════════════════════════
    //  开关
    // ═══════════════════════════════════════════

    public void Toggle()
    {
        if (isOpen) Close();
        else Open();
    }

    public void Open()
    {
        if (isOpen) return;
        isOpen = true;

        // 背包模式只启用 Inventory + Basic
        inputReader?.SetMode(InputMode.Inventory);

        SetPanelVisible(centerDetailPanel, true);

        // 打开时默认展示当前装备武器，避免中间面板空白
        hoveredWeapon = inventory != null ? inventory.CurrentWeapon : null;
        RefreshDetailPanels();

        StartAnimation(true);
    }

    /// <summary>
/// 关闭背包。restoreInputMode = false 时由调用方（ShopUI 的替换流程）负责恢复输入模式。
/// </summary>
public void Close(bool restoreInputMode = true)
{
    if (!isOpen) return;
    isOpen = false;

    bool wasReplace = replaceMode;

    // 玩家自行关闭背包 = 放弃待替换
    if (wasReplace) EndReplaceMode(true);

    SetPanelVisible(centerDetailPanel, false);
    SetPanelVisible(replaceDetailPanel, false);

    pendingReplaceItem = null;
    hoveredWeapon = null;

    // 替换结束时的输入模式由 ShopUI.EndReplace 恢复为 Shop
    if (restoreInputMode && !wasReplace)
        inputReader?.SetMode(InputMode.Gameplay);

    StartAnimation(false);
}

    // ═══════════════════════════════════════════
    //  槽位生成
    // ═══════════════════════════════════════════

    void RebuildWeaponSlots()
    {
        if (inventory == null || weaponSlotPrefab == null || weaponSlotRoot == null) return;

        while (weaponSlots.Count < inventory.Weapons.Length)
        {
            var slot = Instantiate(weaponSlotPrefab, weaponSlotRoot);
            slot.Clicked += OnWeaponSlotClicked;
            slot.Hovered += OnWeaponSlotHovered;
            weaponSlots.Add(slot);
        }

        for (int i = 0; i < weaponSlots.Count; i++)
        {
            bool used = i < inventory.Weapons.Length;
            weaponSlots[i].gameObject.SetActive(used);
            if (used) weaponSlots[i].Bind(i, inventory.Weapons[i]);
        }
    }

    void RebuildRelicSlots()
    {
        if (inventory == null || relicSlotPrefab == null || relicSlotRoot == null) return;

        int count = inventory.RelicCount;

        while (relicSlots.Count < count)
        {
            var slot = Instantiate(relicSlotPrefab, relicSlotRoot);
            slot.Hovered += OnRelicSlotHovered;
            relicSlots.Add(slot);
        }

        for (int i = 0; i < relicSlots.Count; i++)
        {
            bool used = i < count;
            relicSlots[i].gameObject.SetActive(used);
            if (used) relicSlots[i].Bind(i, inventory.GetRelicData(i));
        }
    }

    // ═══════════════════════════════════════════
    //  悬停详情（离开后保留最后一件）
    // ═══════════════════════════════════════════

    void OnWeaponSlotHovered(InventoryWeaponSlotUI slot)
    {
        hoveredWeapon = slot.Weapon;
        RefreshDetailPanels();
    }

    void OnRelicSlotHovered(InventoryRelicSlotUI slot)
    {
        hoveredWeapon = null;

        if (centerDetailPanel != null)
        {
            if (slot.Data != null) centerDetailPanel.ShowRelic(slot.Data);
            else centerDetailPanel.Hide();
        }

        // 悬停遗物时不做武器比较，但左侧待购武器仍然显示
        if (replaceMode && pendingReplaceItem != null)
            replaceDetailPanel?.ShowItemEntry(pendingReplaceItem);
    }

    /// <summary> 根据当前悬停武器和待购武器刷新两个面板，并互相作为比较对象。 </summary>
    void RefreshDetailPanels()
    {
        if (centerDetailPanel == null) return;

        if (hoveredWeapon == null || hoveredWeapon.Data == null)
        {
            centerDetailPanel.Hide();
            return;
        }

        WeaponStats? middle = WeaponPreview.Calculate(
            hoveredWeapon.Data, null, characterStats);

        WeaponStats? pending = null;
        if (replaceMode && pendingReplaceItem != null &&
            pendingReplaceItem.kind == ItemEntry.Kind.Weapon &&
            pendingReplaceItem.weaponData != null)
        {
            pending = WeaponPreview.Calculate(
                pendingReplaceItem.weaponData, pendingReplaceItem.affixes, characterStats);
        }

        centerDetailPanel.ShowWeapon(hoveredWeapon, pending);

        if (replaceMode && pendingReplaceItem != null)
            replaceDetailPanel?.ShowItemEntry(pendingReplaceItem, middle);
    }

    // ═══════════════════════════════════════════
    //  武器替换
    // ═══════════════════════════════════════════

    /// <summary>
    /// 进入替换模式并打开背包。
    /// 左侧面板展示 pendingItem；玩家点击武器槽时调用 onSlotSelected(槽位下标)；
    /// 玩家直接关闭背包时调用 onCancelled。
    /// </summary>
    public void OpenForWeaponReplace(ItemEntry pendingItem, Action<int> onSlotSelected, Action onCancelled)
    {
        pendingReplaceItem = pendingItem;
        replaceMode = true;
        replaceSelected = onSlotSelected;
        replaceCancelled = onCancelled;

        if (!isOpen) Open();
        else RefreshDetailPanels();

        SetPanelVisible(replaceDetailPanel, true);
    }

    /// <summary> 退出替换模式。notifyCancelled = false 表示替换成功，不需要通知取消。 </summary>
    public void EndReplaceMode(bool notifyCancelled)
    {
        var cancelled = replaceCancelled;
        bool wasReplace = replaceMode;

        replaceMode = false;
        replaceSelected = null;
        replaceCancelled = null;
        pendingReplaceItem = null;

        if (wasReplace && notifyCancelled) cancelled?.Invoke();

        SetPanelVisible(replaceDetailPanel, false);
    }

    void OnWeaponSlotClicked(InventoryWeaponSlotUI slot)
    {
        // 正常打开背包时点击武器不产生任何效果
        if (!replaceMode || replaceSelected == null) return;
        replaceSelected(slot.SlotIndex);
    }

    // ═══════════════════════════════════════════
    //  面板显隐
    // ═══════════════════════════════════════════

    static void SetPanelVisible(InventoryDetailPanelUI panel, bool visible)
    {
        if (panel != null) panel.gameObject.SetActive(visible);
    }

    // ═══════════════════════════════════════════
    //  动画
    // ═══════════════════════════════════════════

    void StartAnimation(bool show)
    {
        if (inventoryPannel == null) return;

        if (animationRoutine != null)
            StopCoroutine(animationRoutine);

        animationRoutine = StartCoroutine(show ? ShowPannel(inventoryPannel) : HidePannel(inventoryPannel));
    }

    IEnumerator ShowPannel(GameObject pannel)
    {
        RectTransform rt = pannel.GetComponent<RectTransform>();
        float moveDistance = rt.rect.width;
        float timer = 0f;
        pannel.SetActive(true);
        while (timer < 1f)
        {
            timer += Time.deltaTime * animationSpeed;
            rt.anchoredPosition = new Vector2(moveDistance * showCurve.Evaluate(timer), rt.anchoredPosition.y);
            yield return null;
        }
    }

    IEnumerator HidePannel(GameObject pannel)
    {
        RectTransform rt = pannel.GetComponent<RectTransform>();
        float moveDistance = rt.rect.width;
        float timer = 0f;

        while (timer < 1f)
        {
            timer += Time.deltaTime * animationSpeed;
            rt.anchoredPosition = new Vector2(moveDistance * hideCurve.Evaluate(timer), rt.anchoredPosition.y);
            yield return null;
        }

        pannel.SetActive(false);
    }
}