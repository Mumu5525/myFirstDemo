using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameEvents
{
    public static event Action<bool> OnRunStateChanged;
    public static void RunStateChanged(bool isRunning) => OnRunStateChanged?. Invoke(isRunning); 
}
