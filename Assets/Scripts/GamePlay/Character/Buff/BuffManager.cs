using System.Collections.Generic;
using UnityEngine;

public class BuffManager : MonoBehaviour
{
    List<Buff> activeBuffs = new List<Buff>();
    BuffContext ctx;

    public int Count => activeBuffs.Count;

    void Awake()
    {
        ctx = new BuffContext { Owner = gameObject };
    }

    void Update()
    {
        // 副本遍历，防止 OnUpdate/OnRemove 回调里修改 activeBuffs 导致 Collection was modified
        var snapshot = new List<Buff>(activeBuffs);
        for (int i = snapshot.Count - 1; i >= 0; i--)
        {
            var buff = snapshot[i];
            buff.Tick(Time.deltaTime, ctx);
            if (buff.IsExpired)
            {
                buff.OnRemove(ctx);
                activeBuffs.Remove(buff);
            }
        }
    }

    public void AddBuff(Buff buff)
    {
        var existing = activeBuffs.Find(b => b.Id == buff.Id);
        if (existing != null)
        {
            switch (existing.GetStackPolicy())
            {
                case StackPolicy.RefreshDuration:
                    existing.Refresh();
                    return;
                case StackPolicy.Ignore:
                    return;
                case StackPolicy.Replace:
                    existing.OnRemove(ctx);
                    activeBuffs.Remove(existing);
                    break;
            }
        }

        activeBuffs.Add(buff);
        buff.OnApply(ctx);
    }

    public void RemoveBuff(Buff buff)
    {
        if (activeBuffs.Remove(buff))
            buff.OnRemove(ctx);
    }

    public void RemoveBuffById(string id)
    {
        var buff = activeBuffs.Find(b => b.Id == id);
        if (buff != null)
            RemoveBuff(buff);
    }

    public bool HasBuff(string id)
    {
        return activeBuffs.Exists(b => b.Id == id);
    }

    public void CollectModifiers(StatType stat, List<StatModifier> output)
    {
        foreach (var buff in activeBuffs)
            buff.CollectModifiers(stat, output, ctx);
    }
}