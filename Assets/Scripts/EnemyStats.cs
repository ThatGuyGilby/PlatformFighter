using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStats : CharacterStats
{
    public GameObject deathEffect;

    public override void Die()
    {
        if (deathEffect != null)
        {
            GameObject _deathEffect = Instantiate(deathEffect);
            _deathEffect.transform.position = transform.position;

            ParticleSystem _particleSystem = _deathEffect.GetComponent<ParticleSystem>();
            ParticleSystem.MainModule _main = _particleSystem.main;
            _main.startColor = GetComponent<SpriteRenderer>().color;
        }

        base.Die();
    }
}
