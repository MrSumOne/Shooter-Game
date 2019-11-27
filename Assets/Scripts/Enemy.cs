using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : LivingEntity
{

    public enum State { Idle, Chasing, Attacking };
    State currentState;

    NavMeshAgent pathfinder;
    Transform target;
    Material enemyMat;
    Color originalColor;

    public ParticleSystem deathEffect;

    LivingEntity targetEntity;

    float attackDistanceThreshold = .5f;
    public float timeBetweenAttacks = 1;
    float nextAttackTime;
    public float damage = 1;

    float myCollisionRadius;
    float targetCollisionRadius;

    bool hasTarget;

    protected override void Start()
    {
        base.Start();
        
        pathfinder = GetComponent<NavMeshAgent>();
        enemyMat = GetComponent<Renderer>().material;
        originalColor = enemyMat.color;

        if (GameObject.FindGameObjectWithTag("Player") != null)
        {
            hasTarget = true;
            currentState = State.Chasing;
            target = GameObject.FindGameObjectWithTag("Player").transform;

            myCollisionRadius = GetComponent<CapsuleCollider>().radius;
            targetCollisionRadius = target.GetComponent<CapsuleCollider>().radius;



            targetEntity = target.GetComponent<LivingEntity>();
            //subscribe OnTargetDeath to the OnDead event
            targetEntity.OnDead += OnTargetDeath;

            StartCoroutine(UpdatePath());
        }
    }

    public override void TakeHit(float damage, Vector3 hitPoint, Vector3 hitDirection)
    {
        if(damage >= health)
        {
            Destroy(Instantiate(deathEffect.gameObject, hitPoint, Quaternion.FromToRotation(Vector3.forward, hitDirection)), deathEffect.main.startLifetime.constant);
        }
        base.TakeHit(damage, hitPoint, hitDirection);
    }

    void OnTargetDeath()
    {
        hasTarget = false;
        currentState = State.Idle;
    }

    private void Update()
    {
        if (hasTarget)
        {
            float radiusAttackDistanceThreshold = attackDistanceThreshold + myCollisionRadius + targetCollisionRadius;
            if (Time.time > nextAttackTime)
            {
                //we could use Vector3.Distance, but that uses the square root method. Since we don't need to know the exact distance, we'll
                //just figure it out in squares
                float sqrDistanceToTarget = (target.position - transform.position).sqrMagnitude;
                if (sqrDistanceToTarget < (radiusAttackDistanceThreshold * radiusAttackDistanceThreshold))
                {
                    StartCoroutine(Attack());
                    nextAttackTime = Time.time + timeBetweenAttacks;
                }
            }
        }
    }

    IEnumerator Attack()
    {
        currentState = State.Attacking;

        Vector3 originalPosition = transform.position;
        Vector3 directionToTarget = (target.position - transform.position).normalized;
        Vector3 targetPosition = target.position - directionToTarget * myCollisionRadius;

        float percent = 0;
        float attackSpeed = 3;

        //set off the pathfinder while attacking
        pathfinder.enabled = false;

        enemyMat.color = Color.red;
        bool hasAppliedDamage = false;

        while (percent < 1)
        {
            if(percent >=.05f && !hasAppliedDamage)
            {
                hasAppliedDamage = true;
                targetEntity.TakeDamage(damage);
            }

            //move the percent from 0 to 1 based on the time between the frames, multiplied by the attack speed
            percent += Time.deltaTime * attackSpeed;

            //interpolation that goes up to 0.5 and then back down to 1 to give a lunging effect
            // y= 4(-x^2 + x)
            float interpolation = 4 * (-percent * percent + percent);

            transform.position = Vector3.Lerp(originalPosition, targetPosition, interpolation);

            yield return null;

        }
        //return things to how they were before, like pathfinder on
        currentState = State.Chasing;
        pathfinder.enabled = true;
        enemyMat.color = originalColor;

    }

    //only do the AI calculations once every refreshRate seconds instead of 60 times a second
    IEnumerator UpdatePath()
    {
        float refreshRate = .25f;


        while (hasTarget)
        {
            if (currentState == State.Chasing)
            {
                Vector3 directionToTarget = (target.position - transform.position).normalized;
                //the target's position minus the direction to it, times the radius of both player and enemy.
                Vector3 targetPosition = target.position - directionToTarget * (myCollisionRadius + targetCollisionRadius + attackDistanceThreshold / 2);
                if (!dead)
                    pathfinder.SetDestination(targetPosition);
            }
            //wait for a fraction of a second so that we're not calculating this 60 times a second, every frame
            yield return new WaitForSeconds(refreshRate);
        }
    }

}
