using UnityEngine;

public class Projectile : MonoBehaviour {

    public float speed = 10;
    float damage = 1;
    float lifetime = 3;
    float skinWidth = .1f;

    public LayerMask collisionMask;

    private void Start()
    {
        //make sure the projectile deletes after lifetime
        Destroy(gameObject, lifetime);

        //if the projectile is already in the enemy when it gets instantiated
        Collider[] initialCollisions = Physics.OverlapSphere(transform.position, .1f, collisionMask);
        if (initialCollisions.Length > 0)
        {
            //will use a collider instead of a ray, so we have to use the OnHitObject with a collider below
            OnHitObject(initialCollisions[0]);
        }
    }

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

	void Update () {
        //shoot the bullet forward and check for collisions
        float moveDistance = speed * Time.deltaTime;
        CheckCollisions(moveDistance);
        transform.Translate(Vector3.forward * moveDistance);
	}

    void CheckCollisions(float moveDistance)
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if(Physics.Raycast(ray, out hit, moveDistance + skinWidth, collisionMask))
        {
            OnHitObject(hit);
        }
    }

    void OnHitObject(RaycastHit hit)
    {
        //since we don't know what we're hitting, we use the interface for a damageable object
        IDamageable damageableObject = hit.collider.gameObject.GetComponent<IDamageable>();
        if(damageableObject != null)
        {
            damageableObject.TakeHit(damage, hit);
            
        }
        Destroy(gameObject);

    }

    //same as above but to deal with that initial collider issue, since the ray won't see the enemy, the
    //overlap method returns a collider, which we use here
    void OnHitObject(Collider c)
    {
        IDamageable damageableObject = c.gameObject.GetComponent<IDamageable>();
        if (damageableObject != null)
        {
            damageableObject.TakeDamage(damage);
            
        }
        Destroy(gameObject);
    }
}
