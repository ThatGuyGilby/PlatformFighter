using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CharacterStats : MonoBehaviour
{
    public float currentHealth;
    public Stat health, damage;

    public void Start()
    {
        currentHealth = health.GetValue();
    }

    /// <summary>
    /// Take the desired amoount of damage.
    /// </summary>
    /// <param name="_damage">The amount of damage to be taken.</param>
    public void TakeDamage(float _damage)
    {
        currentHealth -= _damage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Cause the character to die.
    /// </summary>
    public virtual void Die()
    {
        Debug.Log($"{gameObject.name} died.");
        Destroy(gameObject);
    }
}
