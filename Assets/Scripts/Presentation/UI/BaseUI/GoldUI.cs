using TMPro;
using UnityEngine;

/// <summary>
/// 金币显示 UI。挂在包含 TextMeshProUGUI 的 GameObject 上，自动绑定玩家钱包。
/// </summary>
public class GoldUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI goldText;
    PlayerInventory inventory;

    void Start()
    {
        if (goldText == null)
            goldText = GetComponent<TextMeshProUGUI>();

        if (goldText == null)
        {
            Debug.LogError("[GoldUI] 没找到 TextMeshProUGUI 组件！");
            return;
        }

        var player = PlayerLocator.Find();
        if (player == null)
        {
            Debug.LogError("[GoldUI] 没找到玩家！Tag 是否设为 Player？");
            return;
        }

        inventory = player.GetComponent<PlayerInventory>();
        if (inventory == null)
        {
            Debug.LogError("[GoldUI] 玩家身上没有 PlayerInventory 组件！");
            return;
        }

        inventory.OnGoldChanged += UpdateDisplay;
        UpdateDisplay(inventory.Gold);
    }

    void OnDestroy()
    {
        if (inventory != null)
            inventory.OnGoldChanged -= UpdateDisplay;
    }

    void UpdateDisplay(int gold)
    {
        if (goldText != null)
            goldText.text = $"{gold}";
    }
}