using System.Collections.Generic;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    [SerializeField] CharacterData data;
    [SerializeField] BuffManager buffManager;

    List<StatModifier> modifierCache = new List<StatModifier>();

    void Awake()
    {
        if (buffManager == null)
            buffManager = GetComponent<BuffManager>();
    }

    float GetBaseValue(StatType stat)
    {
        switch (stat)
        {
            case StatType.Damage:  return data.atk;
            case StatType.Health:  return data.health;
            case StatType.Speed:   return data.speed;
            case StatType.Defense: return data.defense;
            default:               return 0f;
        }
    }

    float CalculateFinal(StatType stat, float baseValue)
    {
        modifierCache.Clear();

        buffManager.CollectModifiers(stat, modifierCache);

        float addSum = 0f;
        float mulSum = 0f;
        float divProduct = 1f;

        foreach (var m in modifierCache)
        {
            switch (m.type)
            {
                case ModifierType.add:
                    addSum += m.value;
                    break;
                case ModifierType.multiply:
                    mulSum += m.value;
                    break;
                case ModifierType.divide:
                    divProduct *= m.value;
                    break;
            }
        }

        return (baseValue + addSum) * (1f + mulSum) / divProduct;
    }

    public float GetStat(StatType stat)
    {
        return CalculateFinal(stat, GetBaseValue(stat));
    }
}