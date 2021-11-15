using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStats : CharacterStats
{
    [SerializeField] private GameObject deathEffect;

    public override void Die()
    {
        if (deathEffect != null)
        {
            if (deathEffect != null)
            {
                GameObject _deathEffect = Instantiate(deathEffect);
                _deathEffect.transform.position = transform.position;
                
                ParticleSystem.MainModule _main = _deathEffect.GetComponent<ParticleSystem>().main;
                _main.startColor = GetComponent<SpriteRenderer>().color;
            }
        }

        base.Die();
    }
}
