using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 金币显示 UI。挂在包含 Text 的 GameObject 上，自动绑定玩家钱包。
/// </summary>
public class GoldUI : MonoBehaviour
{
    [SerializeField] Text goldText;
    PlayerInventory inventory;

    void Start()
    {
        if (goldText == null)
            goldText = GetComponent<Text>();

        if (goldText == null)
            Debug.LogError("[GoldUI] 没找到 Text 组件！请确保挂在了有 Text 的 GameObject 上");

        var player = PlayerLocator.Find();
        if (player == null)
            Debug.LogError("[GoldUI] 没找到玩家！请确保 Player 的 Tag 设为 Player");
        else
            inventory = player.GetComponent<PlayerInventory>();

        if (inventory == null)
            Debug.LogError("[GoldUI] 玩家身上没有 PlayerInventory 组件！");
        else
        {
            inventory.OnGoldChanged += UpdateDisplay;
            UpdateDisplay(inventory.Gold);
        }
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