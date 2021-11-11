using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{

    public WeaponType weaponType = WeaponType.Melee;

    [Header("Melee")]
    public Transform attackPoint;
    public float attackRange;
    public LayerMask enemyLayers;

    [Header("Ranged")]
    public Transform gunBarrel;
    public float gunRange;
    public GameObject projectilePrefab;

    [Header("Dash")]
    public float dashForce;

    private CharacterStats stats;
    private PlayerMovement playerMovement;
    private Rigidbody2D rb;

    private void Start()
    {
        stats = GetComponent<CharacterStats>();
        playerMovement = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody2D>();
    }

    #region Inputs
    public void Attack(InputAction.CallbackContext _context)
    {
        if (_context.started)
        {
            switch(weaponType)
            {
                case WeaponType.Melee:
                    // Detect enemies
                    Collider2D[] _hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

                    // If an enemy was hit dash towards them.
                    if (_hitEnemies.Length > 0)
                    {
                        Dash();
                    }

                    // Damage enemies
                    foreach (Collider2D _enemy in _hitEnemies)
                    {
                        _enemy.GetComponent<CharacterStats>().TakeDamage(stats.damage.GetValue());
                    }
                    break;
                case WeaponType.Ranged:
                    break;
            }
        }
    }

    public void Dash(InputAction.CallbackContext _context)
    {
        if (_context.started)
        {
            Dash();
        }
    }
    #endregion

    #region Dash
    private void Dash()
    {
        // Get the direction to dash in
        Vector3 pos = transform.position;
        Vector3 dir = (gunBarrel.position - transform.position).normalized;

        // Add the dash force
        rb.AddForce(dir * dashForce, ForceMode2D.Impulse);
    }
    #endregion

    private void OnDrawGizmos()
    {
        switch (weaponType)
        {
            case WeaponType.Melee:
                if (attackPoint == null) return;

                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(attackPoint.position, attackRange);
                break;
            case WeaponType.Ranged:
                if (gunBarrel == null) return;

                Gizmos.color = Color.green;
                Vector3 pos = transform.position;
                Vector3 dir = (gunBarrel.position - transform.position);
                Gizmos.DrawLine(gunBarrel.position, gunBarrel.position + dir * gunRange);
                break;
        }
    }
}

public enum WeaponType
{
    Melee,
    Ranged
}