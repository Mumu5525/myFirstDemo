using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> 命中效果基类。一次命中 = 一次 Apply。 </summary>
public abstract class OnHitEffect : ScriptableObject
{
    /// <param name="source">开火者（用于属性加成、伤害归属）</param>
    /// <param name="target">被命中的对象（可能是敌人，也可能是墙）</param>
    /// <param name="point">命中点</param>
    public abstract void Apply(GameObject source, GameObject target, Vector3 point);
}
