using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKController : MonoBehaviour
{
    Animator anim;
    public Transform rightHandPosition;
    public Transform leftHandPosition;

    void Start()
    {
        anim = GetComponentInChildren<Animator>();
    }

    private void OnAnimatorIK(int layerIndex) 
    {
        anim.SetIKPosition(AvatarIKGoal.RightHand, rightHandPosition.position);
        anim.SetIKRotation(AvatarIKGoal.RightHand, rightHandPosition.rotation);
        anim.SetIKPosition(AvatarIKGoal.LeftHand, leftHandPosition.position);
        anim.SetIKRotation(AvatarIKGoal.LeftHand, leftHandPosition.rotation);


        anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1f);
        anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1f);
        anim.SetIKPositionWeight(AvatarIKGoal.RightHand, 1f);
        anim.SetIKRotationWeight(AvatarIKGoal.RightHand, 1f);

    }
}
