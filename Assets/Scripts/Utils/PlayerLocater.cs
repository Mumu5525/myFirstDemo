using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerLocator
{
    static Transform cached;
    static float lastScanTime = -999f;
    const float scanInterval = 0.1f;

    public static Transform Find()
    {
        if (cached == null && Time.time - lastScanTime >= scanInterval)
        {
            GameObject go = GameObject.FindGameObjectWithTag("Player");
            if (go != null) cached = go.transform;
        }
        return cached;
    }

    public static void Clear() => cached = null;
}
