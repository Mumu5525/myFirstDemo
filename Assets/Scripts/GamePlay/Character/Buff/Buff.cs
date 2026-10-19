using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Buff : MonoBehaviour
{
    public string Name{ get; protected set; }
    public string Id { get; protected set; }
    public string Info{ get; protected set; }
    public float Duration { get; protected set; }   // <=0 表示永久
    public float Remaining { get; protected set; }
    public bool IsExpired => Duration > 0 && Remaining <= 0f;
    public object Source { get; protected set; }     // 来源（武器/遗物/敌人）

    protected Buff(string name, string id, string info, float duration, object source)
    {
        Name = name;
        Id = id;
        Info = info;
        Duration = duration;
        Remaining = duration;
        Source = source;
    }

    // ---- 生命周期钩子，默认空实现 ----
    public virtual void OnApply(BuffContext ctx) { }
    public virtual void OnUpdate(float dt, BuffContext ctx) { }
    public virtual void OnRemove(BuffContext ctx) { }

    // ---- 属性修正钩子：返回对某属性的修正，默认无修正 ----
    // StatModifier 里带 类型(加法/乘法/覆盖) 和 数值
    public virtual void CollectModifiers(StatType stat, List<StatModifier> output, BuffContext ctx) { }

    // ---- 事件钩子：默认不处理 ----
    public virtual void OnEvent(GameEventType evt, BuffContext ctx) { }

    // ---- 驱动：由 BuffManager 每帧调用 ----
    public void Tick(float dt, BuffContext ctx)
    {
        if (Duration > 0)
        {
            Remaining -= dt;
            if (Remaining < 0) Remaining = 0;
        }
        OnUpdate(dt, ctx);
    }

    // 重置持续时间（供 BuffManager 在 RefreshDuration 策略时调用）
    public void Refresh() => Remaining = Duration;

    // 可选：同类 Buff 叠加策略
    public virtual StackPolicy GetStackPolicy() => StackPolicy.RefreshDuration;
}

public enum StackPolicy { RefreshDuration, StackIndependent, Ignore, Replace }