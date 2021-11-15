using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Ranged Weapon", menuName = "Ranged Weapon")]
public class RangedWeapon : Weapon
{
    [Header("Ranged")]
    public float gunRange;
    public GameObject projectilePrefab;
}
