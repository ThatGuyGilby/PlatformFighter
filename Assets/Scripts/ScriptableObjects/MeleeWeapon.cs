using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Melee Weapon", menuName = "Melee Weapon")]
public class MeleeWeapon : Weapon
{
    [Header("Melee")]
    public Vector2 attackRange;
    public Vector2 attackOffset;
    public AttackFrameData frameData;
    public bool dashAttack = false;
    public float dashForce;
    public bool bypassCanDash;
}
