using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 泛型事件总线。
/// 
/// 用法：
///   // 定义事件（一个 struct，放在任何文件里）
///   public struct OnUnitDied { public GameObject victim; public GameObject killer; }
/// 
///   // 发布
///   EventBus&lt;OnUnitDied&gt;.Publish(new OnUnitDied { victim = go, killer = attacker });
/// 
///   // 订阅
///   EventBus&lt;OnUnitDied&gt;.Subscribe(HandleDeath);
///   EventBus&lt;OnUnitDied&gt;.Unsubscribe(HandleDeath);
/// 
/// 特性：
///   - 基于 struct 的强类型事件，天然 value-type 无 null
///   - 支持优先级排序（priority 越大越先执行）
///   - 支持拦截（handler 返回 true 表示已消耗，后续 handler 不再执行）
///   - 发布时做 snapshot 遍历，防止 handler 里修改订阅列表导致异常
/// </summary>
public static class EventBus<T> where T : struct
{
    /// <summary> handler 返回 true 表示事件已被消耗，后续 handler 不再执行。返回 false（默认）继续传递。 </summary>
    public delegate bool Handler(T args);

    struct Entry
    {
        public Handler handler;
        public int priority;
        public int id;         // 递增序号，同 priority 按注册先后排序
    }

    static List<Entry> entries = new List<Entry>();
    static int nextId;

    // 发布时可能触发 subscribe/unsubscribe，用 snapshot 防迭代异常
    static List<Entry> snapshot = new List<Entry>();

    // ─── 订阅 / 退订 ───

    /// <summary> 按默认优先级 0 订阅。 </summary>
    public static void Subscribe(Handler handler)
    {
        Subscribe(handler, priority: 0);
    }

    /// <summary> 按指定优先级订阅。priority 越高越先执行。 </summary>
    public static void Subscribe(Handler handler, int priority)
    {
        if (handler == null) return;
        entries.Add(new Entry { handler = handler, priority = priority, id = nextId++ });
        // 保持有序：priority 降序，同 priority 按注册先后
        entries.Sort((a, b) =>
        {
            int c = b.priority.CompareTo(a.priority);
            return c != 0 ? c : a.id.CompareTo(b.id);
        });
    }

    /// <summary> 退订。注意：同一个 handler 注册多次则全部移除。 </summary>
    public static void Unsubscribe(Handler handler)
    {
        if (handler == null) return;
        entries.RemoveAll(e => e.handler == handler);
    }

    // ─── 发布 ───

    /// <summary>
    /// 发布事件给所有订阅者。如果某个 handler 返回 true（消耗事件），
    /// 后续低优先级的 handler 不再执行。
    /// 
    /// 返回 true 表示事件被消耗（被某个 handler 拦截）。
    /// </summary>
    public static bool Publish(T args)
    {
        if (entries.Count == 0) return false;

        // snapshot 避免 handler 内 subscribe/unsubscribe 导致 Collection was modified
        snapshot.Clear();
        snapshot.AddRange(entries);

        bool consumed = false;
        for (int i = 0; i < snapshot.Count; i++)
        {
            var entry = snapshot[i];
            try
            {
                if (entry.handler(args))
                {
                    consumed = true;
                    break;   // 事件被消耗，停止传递
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[EventBus<{typeof(T).Name}>] Handler threw: {ex}");
            }
        }

        return consumed;
    }

    // ─── 调试 ───

    public static int HandlerCount => entries.Count;

    public static void Clear()
    {
        entries.Clear();
        nextId = 0;
    }

    /// <summary> 仅供调试：列出所有 handler 的类型名和优先级。 </summary>
    public static string Dump()
    {
        var lines = new List<string>();
        for (int i = 0; i < entries.Count; i++)
        {
            var e = entries[i];
            string target = e.handler.Target?.GetType().Name ?? "static";
            string method = e.handler.Method.Name;
            lines.Add($"#{i} pri={e.priority} id={e.id} {target}.{method}");
        }
        return string.Join("\n", lines);
    }
}