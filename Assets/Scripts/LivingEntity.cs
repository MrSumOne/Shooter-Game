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

    public virtual void TakeHit(float damage, Vector3 hitPoint, Vector3 hitDirection)
    {
        //do some stuff with hit
        TakeDamage(damage);
    }

    public virtual void TakeDamage(float damage)
    {
        health -= damage;

        if (health <= 0 && !dead)
        {
            Die();
        }
    }

    [ContextMenu("Self Destruct")]
    public virtual void Die()
    {
        dead = true;
        if(OnDead != null)
            //call all the methods that have subscribed to OnDead()
            OnDead();
        Destroy(gameObject);
    }


}
