using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerCore))]
public class PlayerStats : CharacterStats
{
    public override void Die()
    {
        Debug.Log($"Fucky Wucky the player died!");
    }
}
