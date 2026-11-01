using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 商店 HUD 主控制器。
/// 挂在常驻对象（Canvas）上，用 shopPanel 控制商店面板显隐。
/// Tab 开关 / Esc 关闭 / Q 刷新 / E 升级。
/// 打开商店时禁用玩家输入，关闭时恢复。
/// </summary>
public class ShopUI : MonoBehaviour
{
    // ─── 面板 ───
    [Header("面板")]
    [SerializeField] GameObject shopPanel;
    [SerializeField] bool startOpen = false;

    // ─── 顶部 ───
    [Header("顶部")]
    [SerializeField] TextMeshProUGUI goldText;
    [SerializeField] TextMeshProUGUI shopLevelText;

    // ─── 槽位 ───
    [Header("槽位")]
    [SerializeField] GameObject slotUIPrefab;
    [SerializeField] Transform relicSlotParent;    // 遗物槽父物体
    [SerializeField] Transform weaponSlotParent;   // 武器槽父物体

    [Header("槽位尺寸")]
    [SerializeField] Vector2 weaponSlotSize = new Vector2(210f, 110f);
    [SerializeField] Vector2 relicSlotSize  = new Vector2(120f, 120f);

    // ─── 详情 ───
    [Header("详情")]
    [SerializeField] ShopDetailPanelUI detailPanel;

    // ─── 底部 ───
    [Header("底部")]
    [SerializeField] GlowButton refreshButton;
    [SerializeField] GlowButton upgradeButton;
    [SerializeField] TextMeshProUGUI refreshCostText;
    [SerializeField] TextMeshProUGUI upgradeCostText;

    // ─── 玩法 ───
    [Header("玩法")]
    [SerializeField] Shop shop;
    [SerializeField] PlayerInventory inventory;
    [SerializeField] PlayerInputReader inputReader;

    ShopSlotUI[] weaponSlotUIs;
    ShopSlotUI[] relicSlotUIs;
    ShopSlot lastHovered;
    bool isOpen;

    // ═══════════════════════════════════════════
    //  生命周期
    // ═══════════════════════════════════════════

    void Start()
    {
        if (shop == null) shop = FindFirstObjectByType<Shop>();
        if (inventory == null) inventory = FindFirstObjectByType<PlayerInventory>();

        if (inputReader == null)
        {
            var player = PlayerLocator.Find();
            if (player != null) inputReader = player.GetComponent<PlayerInputReader>();
        }

        if (shopPanel == null)
        {
            Debug.LogError("[ShopUI] 没有指定 shopPanel！请把 ShopUI 挂在 Canvas 上，并把 ShopPanel 拖进面板字段。");
            return;
        }
        if (shop == null) { Debug.LogError("[ShopUI] 找不到 Shop！"); return; }

        // 事件
        shop.OnShopRefreshed    += OnShopRefreshed;
        shop.OnItemPurchased    += OnItemPurchased;
        shop.OnShopLevelChanged += OnLevelChanged;

        if (inventory != null) inventory.OnGoldChanged += OnGoldChanged;

        // 底部按钮
        if (refreshButton != null) refreshButton.GetComponent<Button>().onClick.AddListener(OnRefreshClick);
        if (upgradeButton != null) upgradeButton.GetComponent<Button>().onClick.AddListener(OnUpgradeClick);

        // 先填一次初始数值，否则打开前金币/等级是空的
        if (goldText != null && inventory != null) goldText.text = $"$ {inventory.Gold}";
        if (shopLevelText != null) shopLevelText.text = $"Lv.{shop.ShopLevel}";

        BuildSlotUIs();
        SetOpen(startOpen);
    }

    void OnDestroy()
    {
        if (shop != null)
        {
            shop.OnShopRefreshed    -= OnShopRefreshed;
            shop.OnItemPurchased    -= OnItemPurchased;
            shop.OnShopLevelChanged -= OnLevelChanged;

            foreach (var s in shop.weaponSlots)
                if (s != null) s.OnLockToggled -= OnAnyLockChanged;
            foreach (var s in shop.relicSlots)
                if (s != null) s.OnLockToggled -= OnAnyLockChanged;
        }

        if (inventory != null) inventory.OnGoldChanged -= OnGoldChanged;
    }

    // ═══════════════════════════════════════════
    //  输入
    // ═══════════════════════════════════════════

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab)) { SetOpen(!isOpen); return; }
        if (Input.GetKeyDown(KeyCode.Escape) && isOpen) { SetOpen(false); return; }
        if (!isOpen) return;

        if (Input.GetKeyDown(KeyCode.Q)) shop?.PerformRefresh();
        if (Input.GetKeyDown(KeyCode.E)) shop?.Upgrade();
    }

    void SetOpen(bool open)
    {
        if (shopPanel == null) return;

        isOpen = open;
        shopPanel.SetActive(open);

        // 打开商店时禁用玩家输入（移动/攻击/QE 转镜头），关闭时恢复
        if (inputReader != null) inputReader.enabled = !open;

        if (open)
        {
            RefreshAllSlots();
            ShowDetail();
        }
    }

    // ═══════════════════════════════════════════
    //  槽位创建
    // ═══════════════════════════════════════════

    void BuildSlotUIs()
    {
        if (shop.data == null || slotUIPrefab == null) return;

        // 清理旧槽位 UI（只删带 ShopSlotUI 的，保留标题等装饰）
        if (weaponSlotParent != null)
            foreach (Transform t in weaponSlotParent)
                if (t.GetComponent<ShopSlotUI>() != null) Destroy(t.gameObject);
        if (relicSlotParent != null)
            foreach (Transform t in relicSlotParent)
                if (t.GetComponent<ShopSlotUI>() != null) Destroy(t.gameObject);

        // 遗物槽
        int relicCount = Mathf.Min(shop.relicSlots.Count, shop.data.relicSlotCount);
        relicSlotUIs = new ShopSlotUI[relicCount];
        for (int i = 0; i < relicCount; i++)
        {
            var ui = CreateSlotUI(relicSlotParent, relicSlotSize);
            ui.Bind(shop.relicSlots[i], inventory);
            relicSlotUIs[i] = ui;
        }

        // 武器槽
        int weaponCount = Mathf.Min(shop.weaponSlots.Count, shop.data.weaponSlotCount);
        weaponSlotUIs = new ShopSlotUI[weaponCount];
        for (int i = 0; i < weaponCount; i++)
        {
            var ui = CreateSlotUI(weaponSlotParent, weaponSlotSize);
            ui.Bind(shop.weaponSlots[i], inventory);
            weaponSlotUIs[i] = ui;
        }

        // 锁定变化 → 刷新底部按钮费用
        foreach (var s in shop.weaponSlots) if (s != null) s.OnLockToggled += OnAnyLockChanged;
        foreach (var s in shop.relicSlots)  if (s != null) s.OnLockToggled += OnAnyLockChanged;
    }

    ShopSlotUI CreateSlotUI(Transform parent, Vector2 size)
    {
        var go = Instantiate(slotUIPrefab, parent);
        go.GetComponent<RectTransform>().sizeDelta = size;

        // 父级挂了 Layout Group 时，实际尺寸由 LayoutElement 决定
        var layout = go.GetComponent<LayoutElement>();
        if (layout == null) layout = go.AddComponent<LayoutElement>();
        layout.preferredWidth  = size.x;
        layout.preferredHeight = size.y;

        var ui = go.GetComponent<ShopSlotUI>();
        ui.Hovered += OnSlotHovered;
        return ui;
    }

    void OnSlotHovered(ShopSlotUI ui)
    {
        lastHovered = ui.Slot;
        ShowDetail();
    }

    void ShowDetail()
    {
        var item = lastHovered?.item;
        if (item == null) detailPanel?.Clear();
        else              detailPanel?.Show(item, inventory);
    }

    // ═══════════════════════════════════════════
    //  事件响应
    // ═══════════════════════════════════════════

    void OnShopRefreshed()      => RefreshAllSlots();
    void OnItemPurchased(ItemEntry item) => RefreshAllSlots();

    void OnLevelChanged(int level)
    {
        if (shopLevelText != null)
            shopLevelText.text = $"Lv.{level}";
        UpdateBottomButtons();
    }

    void OnGoldChanged(int gold)
    {
        if (goldText != null)
            goldText.text = $"$ {gold}";

        foreach (var ui in weaponSlotUIs) ui?.RefreshVisuals();
        foreach (var ui in relicSlotUIs)  ui?.RefreshVisuals();

        UpdateBottomButtons();
        ShowDetail();
    }

    void OnAnyLockChanged(bool locked) => UpdateBottomButtons();

    void RefreshAllSlots()
    {
        foreach (var ui in weaponSlotUIs) ui?.Render();
        foreach (var ui in relicSlotUIs)  ui?.Render();

        UpdateBottomButtons();
        ShowDetail();
    }

    // ═══════════════════════════════════════════
    //  底部按钮
    // ═══════════════════════════════════════════

    void UpdateBottomButtons()
    {
        if (shop?.data == null) return;

        int lockedCount = 0;
        foreach (var s in shop.weaponSlots) if (s != null && s.IsLocked) lockedCount++;
        foreach (var s in shop.relicSlots)  if (s != null && s.IsLocked) lockedCount++;

        // 刷新
        int refreshCost = shop.data.GetRefreshCost(lockedCount);
        bool canRefresh = inventory != null && inventory.Gold >= refreshCost;
        if (refreshCostText != null)
            refreshCostText.text = $"刷新 [Q]\n{CostLine(refreshCost, canRefresh)}";
        refreshButton?.SetInteractable(canRefresh);

        // 升级
        int idx = shop.ShopLevel - 1;
        if (idx < shop.data.upgradeCosts.Length)
        {
            int upgradeCost = shop.data.upgradeCosts[idx];
            bool canUpgrade = inventory != null && inventory.Gold >= upgradeCost;
            if (upgradeCostText != null)
                upgradeCostText.text = $"升级 [E]\nLv.{shop.ShopLevel + 1}  {CostLine(upgradeCost, canUpgrade)}";
            upgradeButton?.SetInteractable(canUpgrade);
        }
        else
        {
            if (upgradeCostText != null) upgradeCostText.text = "升级 [E]\n已满级";
            upgradeButton?.SetInteractable(false);
        }
    }

    /// <summary> 第二行费用：买得起白色，买不起红色 </summary>
    static string CostLine(int cost, bool canAfford)
    {
        Color c = canAfford ? ShopHUDTheme.CanAfford : ShopHUDTheme.CannotAfford;
        return $"<color=#{ColorUtility.ToHtmlStringRGB(c)}>$ {cost}</color>";
    }

    void OnRefreshClick() => shop?.PerformRefresh();
    void OnUpgradeClick() => shop?.Upgrade();
}