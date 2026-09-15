using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunListener : MonoBehaviour
{
    void OnEnable()  => GameEvents.OnRunStateChanged += HandleRun;
    void OnDisable() => GameEvents.OnRunStateChanged -= HandleRun;

    void HandleRun(bool isRunning)
    {
        Debug.Log(isRunning ? "开始奔跑！" : "停止奔跑");
    }
}
