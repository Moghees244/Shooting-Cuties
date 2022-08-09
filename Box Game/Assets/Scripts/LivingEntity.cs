using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LivingEntity : MonoBehaviour, IDamagable
{
    public float startingHealth;
    public float health{protected set;get;}
    protected bool dead;

    public event System.Action OnDeath;

    public virtual void Start()
    {
        health = startingHealth;
        dead = false;
    }

    public virtual void takeHit(float damage, Vector3 hitPoint,Vector3 hitDirection)
    {
        takeDamage(damage);
    }

    public virtual void takeDamage(float damage)
    {
        health -= damage;

        if (health <= 0 && !dead)
            Die();
    }

    public void heightDamage()
    {
        AudioManager.instance.playSound("EnemyDeath", transform.position);
        health-=2;

        if (health <= 0 && !dead)
            Die();
    }
    public virtual void Die()
    {
        dead = true;

        if (OnDeath != null)
            OnDeath();

        GameObject.Destroy(gameObject);
    }

    public void resetHealth()
    {
        health=100;
    }
}
