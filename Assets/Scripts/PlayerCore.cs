using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerStats))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerCombat))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]

public class PlayerCore : MonoBehaviour
{
    public PlayerMovement playerMovement;
    public PlayerStats playerStats;
    public PlayerCombat playerCombat;
    public new Rigidbody2D rigidbody2D;
}
