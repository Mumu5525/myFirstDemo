using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 玩家血条。挂在一个有 Slider 组件的 GameObject 上。
/// 自动找到玩家，订阅 Health.OnHealthChanged。
/// </summary>
public class HealthBarUI : MonoBehaviour
{
    [SerializeField] Slider slider;
    [SerializeField] TextMeshProUGUI healthText; // 可选，显示 "75/100"

    Health playerHealth;

    void Start()
    {
        if (slider == null)
            slider = GetComponent<Slider>();

        var player = PlayerLocator.Find();
        if (player == null)
        {
            Debug.LogError("[HealthBarUI] 没找到玩家！");
            return;
        }

        playerHealth = player.GetComponent<Health>();
        if (playerHealth == null)
        {
            Debug.LogError("[HealthBarUI] 玩家身上没有 Health 组件！");
            return;
        }

        playerHealth.OnHealthChanged += UpdateDisplay;
        UpdateDisplay(playerHealth.CurrentHealth, playerHealth.MaxHealth);
    }

    void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged -= UpdateDisplay;
    }

    // 宽度比例：100HP → 400width
    const float HP_PER_WIDTH = 100f / 400f;

    void UpdateDisplay(float current, float max)
    {
        if (slider != null)
        {
            slider.maxValue = max;
            slider.value = current;

            // 根据最大生命调整 Slider 的宽度
            var rect = slider.GetComponent<RectTransform>();
            var size = rect.sizeDelta;
            size.x = Mathf.Min(max / HP_PER_WIDTH, 1500f); // 等价于 max / 100f * 400f
            rect.sizeDelta = size;
        }

        if (healthText != null)
            healthText.text = $"{(int)current} / {(int)max}";
    }
}