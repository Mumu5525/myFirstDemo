using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AnimParams 
{
    public static class Player
    {
        public static readonly int VerticalSpeed    = Animator.StringToHash("VerticalSpeed");
        public static readonly int HorizontalSpeed  = Animator.StringToHash("HorizontalSpeed");
        public static readonly int MoveSpeedScale   = Animator.StringToHash("MoveSpeedScale");
        public static readonly int Hit              = Animator.StringToHash("Hit");
        public static readonly int Fire             = Animator.StringToHash("Fire");

        public const string HitLayer = "Hit";
        public const string AttackLayer = "Attack";
    }

    public static class Enemy
    {
        public static readonly int Speed        = Animator.StringToHash("Speed");
        public static readonly int Attack       = Animator.StringToHash("Attack");
        public static readonly int AttackSpeed  = Animator.StringToHash("AttackSpeed");

        public const string AttackLayer = "Attack";
    }
}
