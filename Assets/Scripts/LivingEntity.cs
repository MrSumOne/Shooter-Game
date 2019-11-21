using UnityEngine;

public class LivingEntity : MonoBehaviour, IDamageable
{
    public event System.Action OnDead;

    public float startingHealth;
    protected float health;
    protected bool dead = false;

    protected virtual void Start()
    {
        health = startingHealth;
    }

    public void TakeHit(float damage, RaycastHit hit)
    {
        //do some stuff with hit
        TakeDamage(damage);
    }

    public void TakeDamage(float damage)
    {
        health -= damage;

        if (health <= 0 && !dead)
        {
            Die();
        }
    }

    protected void Die()
    {
        dead = true;
        if(OnDead != null)
            //call all the methods that have subscribed to OnDead()
            OnDead();
        Destroy(gameObject);
    }
}
