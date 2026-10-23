using UnityEngine;

/// <summary>
/// 单个商店货架（MonoBehaviour）。挂在每个货架 GameObject 上。
/// 持有商品数据，提供锁定/解锁，点击时通知 Shop。
/// </summary>
public class ShopSlot : MonoBehaviour
{
    public ItemEntry item { get; private set; }
    public bool IsLocked { get; private set; }

    /// <summary> 玩家点击购买按钮时触发，Shop 订阅此事件来处理购买逻辑 </summary>
    public event System.Action<ShopSlot> OnBuyRequested;

    /// <summary> Shop 调用此方法填充商品 </summary>
    public void SetItem(ItemEntry newItem)
    {
        item = newItem;
    }

    /// <summary> Shop 调用此方法清空货架 </summary>
    public void Clear()
    {
        item = null;
    }

    /// <summary> 切换锁定状态，按钮/快捷键绑定此方法 </summary>
    public void ToggleLock()
    {
        IsLocked = !IsLocked;
    }

    /// <summary> UI 按钮绑定此方法 </summary>
    public void RequestBuy()
    {
        if (item != null)
            OnBuyRequested?.Invoke(this);
    }
}