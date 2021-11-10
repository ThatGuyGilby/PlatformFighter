using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    public PlayerMovement playerMovement;
    public Transform attackPoint;
    public float attackRange;
    public LayerMask enemyLayers;

    private CharacterStats stats;

    private void Start()
    {
        stats = GetComponent<CharacterStats>();
    }

    #region Inputs
    public void Attack(InputAction.CallbackContext _context)
    {
        if (_context.started)
        {
            Collider2D[] _hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

            foreach (Collider2D _enemy in _hitEnemies)
            {
                Debug.Log($"Hit: {_enemy.name}");
                _enemy.GetComponent<CharacterStats>().TakeDamage(stats.damage.GetValue());
            }
        }
    }
    #endregion

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
