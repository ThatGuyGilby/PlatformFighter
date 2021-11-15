using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerCore))]
public class PlayerCombat : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerCore playerCore;

    [Header("Combat")]
    [SerializeField] private WeaponType weaponType = WeaponType.MELEE;

    [Header("Melee")]
    [SerializeField] private MeleeWeapon meleeWeapon;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private LayerMask enemyLayers;

    [Header("Ranged")]
    [SerializeField] private RangedWeapon rangedWeapon;
    [SerializeField] private Transform gunBarrel;

    [Header("Stomper")]
    [SerializeField] private Vector2 stomperRange;
    [SerializeField] private Vector2 stomperOffset;
    [SerializeField] private float stompBounceForce;
    [SerializeField] private bool stompBounce;
    [SerializeField] private bool canStomp = false;
    [SerializeField] private float timeBetweenStomps;
    private float lastStompTime;

    private void Update()
    {
        // If stomping is enabled and the player stomp cooldown has expired check for targets.
        if (canStomp && lastStompTime <= 0)
        {
            // Use a physics overlap box to check if there are any enemies that are within the stomp hitbox.
            Collider2D[] _stompedEnemies = Physics2D.OverlapBoxAll(GetCenter(transform.position, stomperOffset), stomperRange, 0, enemyLayers);

            // If an enemy was hit apply the bounce force to the player and put the stomp on cooldown.
            if (_stompedEnemies.Length > 0 && stompBounce)
            {
                playerCore.rigidbody2D.AddForce(Vector2.up * stompBounceForce, ForceMode2D.Impulse);
                lastStompTime = timeBetweenStomps;

                // Loop through the enemies that were found and damage them.
                foreach (Collider2D _enemy in _stompedEnemies)
                {
                    if (_enemy.transform.position.y < transform.position.y)
                    {
                        _enemy.GetComponent<CharacterStats>().TakeDamage(playerCore.playerStats.damage.GetValue());
                    }
                }
            }
        }

        #region Timer
        lastStompTime -= Time.deltaTime;
        #endregion
    }

    #region Inputs
    public void Attack(InputAction.CallbackContext _context)
    {
        // If the attack input was pressed down attack based on the weapon type.
        if (_context.started)
        {
            switch(weaponType)
            {
                case WeaponType.MELEE:
                    if (meleeWeapon == null) return;

                    // Use a physics overlap box to check if there are any enemies that are within the attack hitbox.
                    Collider2D[] _hitEnemies = Physics2D.OverlapBoxAll(GetCenter(attackPoint.position, meleeWeapon.attackOffset), meleeWeapon.attackRange, 0, enemyLayers);
                    
                    if (_hitEnemies.Length > 0)
                    {
                        // If an enemy was hit and the current melee weapon has dash attacks enabled dash towards the hit enemy.
                        if (meleeWeapon.dashAttack)
                        {
                            playerCore.playerMovement.Dash(meleeWeapon.dashForce, meleeWeapon.bypassCanDash);
                        }

                        // Loop through the enemies that were found and damage them.
                        foreach (Collider2D _enemy in _hitEnemies)
                        {
                            _enemy.GetComponent<CharacterStats>().TakeDamage(playerCore.playerStats.damage.GetValue());
                        }
                    }

                    break;
                case WeaponType.RANGED:
                    break;
            }
        }
    }
    #endregion

    /// <summary>
    /// Get the center of the player acounting for the given offset.
    /// </summary>
    /// <param name="_center">The original center point.</param>
    /// <param name="_offset">The desired offset.</param>
    /// <returns></returns>
    private Vector3 GetCenter(Vector3 _center, Vector3 _offset)
    {
        if (playerCore.playerMovement.isFacingRight)
        {
            _center += new Vector3(_offset.x, _offset.y, _offset.z);
        }
        else
        {
            _center += new Vector3(-_offset.x, _offset.y, -_offset.z);
        }

        return _center;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;

        // Draw the current weapon's hitbox if all of the references are correctly linked.
        switch (weaponType)
        {
            case WeaponType.MELEE:
                if (attackPoint == null || meleeWeapon == null) return;
                
                Gizmos.DrawWireCube(GetCenter(attackPoint.position, meleeWeapon.attackOffset), meleeWeapon.attackRange);
                break;
            case WeaponType.RANGED:
                if (gunBarrel == null || rangedWeapon == null) return;
                
                Vector3 _dir = (gunBarrel.position - transform.position);
                Gizmos.DrawLine(gunBarrel.position, gunBarrel.position + _dir * rangedWeapon.gunRange);
                break;
        }
        
        // If the player can stomp draw the stomp hitbox.
        if (canStomp)
        {
            Gizmos.DrawWireCube(GetCenter(transform.position, stomperOffset), stomperRange);
        }
    }
}