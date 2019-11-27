using UnityEngine;


//interface for anything that is damageable. If it uses the interface it has to use the methods
public interface IDamageable
{
    void TakeHit(float damage, Vector3 hitPoint, Vector3 hitDirection);

    void TakeDamage(float damage);
}