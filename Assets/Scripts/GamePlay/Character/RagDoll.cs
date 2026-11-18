using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RagDoll : MonoBehaviour
{
    CharacterAnimator anim;
    Rigidbody[] bones;
    Collider[] boneColliders;

    void Awake()
    {
        anim = GetComponentInChildren<CharacterAnimator>();
        Rigidbody rootBone = GetComponent<Rigidbody>();
        bones = GetComponentsInChildren<Rigidbody>()
                .Where(rb =>rb != rootBone)
                .ToArray();
        boneColliders = bones
            .Select(b => b.GetComponent<Collider>())
            .Where(c => c != null)
            .ToArray();
        SetColliders(false);
        foreach(var rb in bones)
            rb.isKinematic = true;
    }

    public void Enable()
    {
        anim?.SetEnable(false);
        SetColliders(true);
        foreach(var rb in bones)
            rb.isKinematic = false;
    }

    public void Disable()
    {
        anim?.SetEnable(true);
        foreach(var rb in bones)
            rb.isKinematic = true;
        SetColliders(false);
    }

    void SetColliders(bool enabled)
    {
        foreach (var c in boneColliders) c.enabled = enabled;
    }
}
