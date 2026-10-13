using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LayerSetter
{
    public static void Set(Transform t, int layer)
    {
        t.gameObject.layer = layer;
        foreach (Transform child in t)
            Set(child, layer);
    }
}