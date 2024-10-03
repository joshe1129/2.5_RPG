using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class EnemySimpleAI : MonoBehaviour
{
    [SerializeField] enum EnemyState { Idle, Patrolling, Chasing, Attacking, Death }
    [SerializeField] EnemyState currentState = EnemyState.Idle;
    [SerializeField] float idleTime = 1.7f;
    [SerializeField] float chaseRange = 7f;
    [SerializeField] float attackRange = 3f;
    [SerializeField] float patrolSpeed = 1f;
    [SerializeField] float chaseSpeed = 3f;    

    private Transform player;
    private Animator enemyAnimator;
    private List<Transform> waypoints = new List<Transform>();
    private Transform currentWaypoint;
    private EnemyManager enemyManager;

    private SpriteRenderer enemySprite;
    private NavMeshAgent navMeshAgent;

    public int prefabIndex; // Índice del prefab en la lista de prefabs
    private float timer = 0f;

    private const string BATTLE_SCENE = "BattleScene";

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        enemySprite = GetComponent<SpriteRenderer>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        enemyAnimator = GetComponent<Animator>();
        enemyManager = GameObject.FindFirstObjectByType<EnemyManager>();
        FindWaypoints();
        SetNextWaypoint();
    }

    void Update()
    {
        enemyAnimator.SetFloat("speed", navMeshAgent.speed);
        switch (currentState)
        {
            case EnemyState.Idle:
                Idle();
                break;
            case EnemyState.Patrolling:
                Patrol();
                break;
            case EnemyState.Chasing:
                Chase();
                break;
            case EnemyState.Attacking:
                Attack();
                break;
            case EnemyState.Death:
                Die();
                break;
        }
    }

    private void Idle()
    {
        navMeshAgent.speed = 0;
        timer += Time.deltaTime;
        if (timer > idleTime)
        {
            timer = 0;
            SetNextWaypoint();
            currentState = EnemyState.Patrolling;
        }
        CheckForPlayer();
    }

    private void Patrol()
    {
        navMeshAgent.speed = patrolSpeed;
        if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
        {
            currentState = EnemyState.Idle;
        }
        CheckForPlayer();
        FlipCharacter();
    }

    private void Chase()
    {
        navMeshAgent.speed = chaseSpeed;
        navMeshAgent.SetDestination(player.position);

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer > chaseRange)
        {
            currentState = EnemyState.Idle;
        }
        else if (distanceToPlayer < attackRange)
        {
            currentState = EnemyState.Attacking;
        }

        FlipCharacter();
    }

    private void Attack()
    {
        enemyAnimator.SetTrigger("attack");
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer > attackRange)
        {
            currentState = EnemyState.Chasing;
        }
    }

    private void Die()
    {
        navMeshAgent.isStopped = true;
        enemyAnimator.SetTrigger("isDying");
        Destroy(gameObject, 1.7f);
    }


    // This method will be called at the end of the attack animation via an animation event
    private void HitPlayer()
    {
        player.GetComponent<PlayerController>().ProcessHit();
        enemyManager.SaveDGEnemiesData(transform);
    }

    private void CheckForPlayer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer < chaseRange)
        {
            currentState = EnemyState.Chasing;
        }
    }

    private void FindWaypoints()
    {
        waypoints.Clear();
        GameObject[] waypointObjects = GameObject.FindGameObjectsWithTag("Waypoint");
        foreach (GameObject waypoint in waypointObjects)
        {
            waypoints.Add(waypoint.transform);
        }
    }

    private void SetNextWaypoint()
    {
        if (waypoints.Count == 0) return;

        currentWaypoint = waypoints[Random.Range(0, waypoints.Count)];
        navMeshAgent.SetDestination(currentWaypoint.position);
    }

    private void FlipCharacter()
    {
        Vector3 direction = navMeshAgent.destination - transform.position;
        if (direction.x != 0)
        {
            enemySprite.flipX = direction.x < 0;
        }
    }

    public void TriggerDeath()
    {
        currentState = EnemyState.Death;
    }
}
