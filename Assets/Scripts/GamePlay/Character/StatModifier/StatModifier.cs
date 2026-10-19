using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ModifierType
{
    add,
    multiply,
    divide
}

public struct StatModifier
{
    public ModifierType type { get; }
    public float value { get; }

    public StatModifier(ModifierType type, float value)
    {
        this.type = type;
        this.value = value;
    }
}
