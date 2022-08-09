using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : LivingEntity
{
    enum State
    {
        Idle,
        Chasing,
        Attacking
    };

    public static event System.Action OnDeathStatic;
    State currentState = State.Chasing;
    Material skin;
    Color originalColor;
    NavMeshAgent pathfinder;
    Transform target;
    public ParticleSystem deathEffect;
    float attackDistanceThreshold = 1.5f;
    float squareDistanceToTarget;
    float timeBetweenAttacks = 1f;
    float nextAttackTime;
    float myCollisionRadius;
    float targetCollisionRadius;
    float Damage = 1f;
    bool hasTarget;

    LivingEntity targetEntity;

    private void Awake()
    {
        pathfinder = GetComponent<NavMeshAgent>();

        if (GameObject.FindGameObjectWithTag("Player") != null)
        {
            hasTarget = true;

            target = GameObject.FindGameObjectWithTag("Player").transform;

            myCollisionRadius = GetComponent<CapsuleCollider>().radius;
            targetCollisionRadius = target.GetComponent<CapsuleCollider>().radius;

            targetEntity = target.GetComponent<LivingEntity>();
            targetEntity.OnDeath += OnTargetDeath;
        }
    }

    public override void Start()
    {
        base.Start();

        if (hasTarget)
        {
            currentState = State.Chasing;
            targetEntity.OnDeath += OnTargetDeath;
            StartCoroutine(UpdatePath());
        }
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
            if (Time.time > nextAttackTime)
            {
                squareDistanceToTarget = (target.position - transform.position).sqrMagnitude;

                if (
                    squareDistanceToTarget
                    <= Mathf.Pow(attackDistanceThreshold, deathEffect.startLifetime)
                )
                {
                    nextAttackTime = Time.time + timeBetweenAttacks;
                    AudioManager.instance.playSound("EnemyAttack", transform.position);
                    StartCoroutine(Attack());
                }
            }
        }
    }

    IEnumerator Attack()
    {
        skin.color = Color.red;
        currentState = State.Attacking;
        pathfinder.enabled = false;

        Vector3 attackPosition = target.position;
        Vector3 originalPosition = transform.position;

        float attackSpeed = 3f;
        float percent = 0;

        bool hasAppliedDamage = false;

        if (percent <= 1)
        {
            if (!hasAppliedDamage)
            {
                hasAppliedDamage = true;
                targetEntity.takeDamage(Damage);
            }

            percent += Time.deltaTime * attackSpeed;
            float interpolation = (-Mathf.Pow(percent, 2) + percent) * 4;
            transform.position = Vector3.Lerp(originalPosition, attackPosition, interpolation);
            yield return null;
        }

        currentState = State.Chasing;
        pathfinder.enabled = true;
    }

    IEnumerator UpdatePath()
    {
        float refreshRate = 0.25f;

        while (hasTarget)
        {
            if (currentState == State.Chasing)
            {
                skin.color = originalColor;

                Vector3 dirToTarget = (target.position - transform.position).normalized;
                Vector3 targetPosition =
                    target.position
                    - dirToTarget
                        * (myCollisionRadius + targetCollisionRadius + attackDistanceThreshold / 4);

                if (!dead)
                    pathfinder.SetDestination(targetPosition);
            }

            yield return new WaitForSeconds(refreshRate);
        }
    }

    public override void takeHit(float damage, Vector3 hitPoint, Vector3 hitDirection)
    {
        AudioManager.instance.playSound("Impact", transform.position);
        if (damage >= health)
        {
            if(OnDeathStatic!=null)
            {
                OnDeathStatic();
            }

            AudioManager.instance.playSound("EnemyDeath", transform.position);

            Destroy(
                Instantiate(
                    deathEffect.gameObject,
                    hitPoint,
                    Quaternion.FromToRotation(Vector3.forward, hitDirection)
                ) as GameObject,
                2.1f
            );
        }

        base.takeHit(damage, hitPoint, hitDirection);
    }

    public void setCharacteristics(
        float moveSpeed,
        int hitsToKill,
        float enemyHealth,
        Color skinColor
    )
    {
        pathfinder.speed = moveSpeed;

        if (hasTarget)
            Damage = Mathf.Floor(targetEntity.startingHealth / hitsToKill);

        startingHealth = enemyHealth;

        skin = GetComponent<Renderer>().sharedMaterial;
        skin.color = skinColor;
        originalColor = skin.color;
    }
}
