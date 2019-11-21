using UnityEngine;


//interface for anything that is damageable. If it uses the interface it has to use the methods
public interface IDamageable
{
    void TakeHit(float damage, RaycastHit ray);

    void TakeDamage(float damage);
}