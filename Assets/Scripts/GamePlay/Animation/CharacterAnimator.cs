using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Animator 的统一访问层。每个角色一个，不做全局单例。
/// 只负责参数、触发、层权重、启用状态；不放游戏逻辑。
/// </summary>
public class CharacterAnimator : MonoBehaviour
{
    /// <summary>
    /// 动画状态机
    /// </summary>
    Animator anim;


    /// <summary>
    /// 动画状态机外部引用，用于IK等需要实际操作anim的系统
    /// </summary>
    public Animator Raw => anim;

    /// <summary>
    /// Animator存在且被启用时(布娃娃/死亡时会禁用)
    /// </summary>
    public bool IsReady => anim != null && anim.isActiveAndEnabled;
    public virtual void Awake()
    {
        anim = GetComponentInChildren<Animator>();
    }

    public void SetFloat(int hash, float value, float damptime = 0f)
    {
        if(!IsReady) return;
        if(damptime > 0f)
            anim.SetFloat(hash, value, damptime, Time.deltaTime);
        else
            anim.SetFloat(hash, value);
    }

    public void SetBool(int hash, bool value)
    {
        if(!IsReady) return;
        anim.SetBool(hash, value);
    }

    public void SetTrigger(int hash)
    {
        if(!IsReady) return;
        anim.SetTrigger(hash);
    }

    public void ResetTrigger(int hash)
    {
        if (!IsReady) return;
        anim.ResetTrigger(hash);
    }

    public void SetLayerWeight(string layerName, float weight)
    {
        if(!IsReady) return;
        int index = anim.GetLayerIndex(layerName);
        if(index >= 0)anim.SetLayerWeight(index, weight);
    }

    public void SetEnable(bool value)
    {
        if(anim != null) anim.enabled = value;
    }

    /// <summary> 对象池复用：重置到默认状态并立即求值一次。 </summary>
    public void RebindAndUpdate()
    {
        if (anim == null) return;
        anim.Rebind();
        anim.Update(0f);
    }
}
