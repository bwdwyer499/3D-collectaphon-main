using UnityEngine;
using System.Collections;
using System;

public class Enemy2Controller : MonoBehaviour
{
    [Header("ENEMY VARIABLES")]
    //[Tooltip("How much force the enemy will hit the player with when they collide.")]
    //public float knockBackForce = 50f;
    [Tooltip("How fast the enemy moves.")]
    public float enemySpeed = 6f;
    [Tooltip("How close the enemy needs to get before they attack.")]
    public float attackProximity = 6f;
    [Tooltip("How many seconds pass between enemy attacks.")]
    public float attackReset = 6f;
    [Tooltip("How many seconds the enemy will pause after hitting player.")]
    public float enemyPauseTime = 1f;
    [Tooltip("The maximum distance the enemy will go from its start point before stopping.")]
    public float maxDistanceFromStart = 20f;
    [Tooltip("How close the enemy will get to the player before it won't come closer.")]
    public float enemyKeepsDistance = 5f;
    [Tooltip("How close the player gets to enemy or enemy's start point before the enemy chases you.")]
    public float chaseProximity = 15f;
    [Tooltip("How far away the player gets from enemy before they return home.")]
    public float returnHomeProximity = 10f;
    [Tooltip("How fast the enemy travels when it is spawned.")]
    public float spawnEnemySpeed = 10;
    [Tooltip("The height of the helicopter above the flat ground.")]
    public float heightAboveGround = 9;

    [Header("DEBUG")]
    [Tooltip("How far from the player the enemy currently is.")]
    public float playerDistanceFromEnemy;
    [Tooltip("How far from the enemy's start point the enemy currently is.")]
    public float enemyDistanceFromStart;
    [Tooltip("How far from the enemy's start point the player currently is.")]
    public float playerDistanceFromStart;

    [Header("INPUT")]
    [SerializeField]
    private Transform bombSpawnPoint;
    [SerializeField]
    private GameObject bombPrefab;
    private GameObject instProjectile;

    private GameObject startPoint;

    private bool attackAllow = true;                                          // true when the enemy is allowed to attack - turns false for an amount of time following attack
    private bool forceEnemyReturn = false;                                    // true if the player has cheesed the enemy a distance. Enemy will return

    private Vector3 PlayerPosition;
    private Vector3 EnemyPosition;
    private GameObject Player;

    void Start()
    {
        EnemyPosition = new Vector3(transform.position.x, heightAboveGround, transform.position.z);
        transform.position = EnemyPosition;                                                                                     // This makes sure the helicopter is at the right height to start with

        startPoint = new GameObject("Enemy2StartPoint");
        startPoint.transform.position = EnemyPosition;                                                                          // This takes EnemyPosition so it isn't staring at the ground

        Player = GameObject.FindGameObjectWithTag("Player");
        PlayerPosition = new Vector3(Player.transform.position.x, transform.position.y, Player.transform.position.z);           // Defined here so it can be used to define playerDistanceFromEnemy
    }

    void FixedUpdate()
    {
        playerDistanceFromEnemy = Vector3.Distance(PlayerPosition, transform.position);
        enemyDistanceFromStart = Vector3.Distance(startPoint.transform.position, transform.position);
        playerDistanceFromStart = Vector3.Distance(startPoint.transform.position, Player.transform.position);

        Enemy2Algorithm();
    }
    private void Enemy2Algorithm()
    {
        PlayerPosition = new Vector3(Player.transform.position.x, heightAboveGround, Player.transform.position.z);       //uses the transform's y position since the hellchopper's y position never changes, if we add hills this could change
        EnemyPosition = new Vector3(transform.position.x, heightAboveGround, transform.position.z);

        //transform.position = Vector3.MoveTowards(EnemyPosition, startPoint.transform.position, enemySpeed * Time.deltaTime);
        //transform.LookAt(PlayerPosition);

        if (forceEnemyReturn && enemyDistanceFromStart < maxDistanceFromStart)
        {
            // Top priority - if the player has cheesed the enemy away from start, it must return before the forceEnemyReturn turns off
            forceEnemyReturn = false;
        }
        else if (forceEnemyReturn)
        {
            // If the enemy has been cheesed away, 
            transform.LookAt(PlayerPosition);
            transform.position = Vector3.MoveTowards(EnemyPosition, startPoint.transform.position, (enemySpeed * 3) * Time.fixedDeltaTime);
        }
        else if (enemyDistanceFromStart > maxDistanceFromStart + 10)            //the extra +10 is the maximum range player's could cheese enemy
        {
            // This is only in case the player tries to cheese the enemy far away from its start point.
            forceEnemyReturn = true;
        }
        else if (playerDistanceFromEnemy - enemyKeepsDistance < 0)
        {
            // If the player gets too close to enemy, it will back away
            transform.LookAt(PlayerPosition);
            transform.position = Vector3.MoveTowards(transform.position, PlayerPosition, -1 * (enemySpeed / 2.2f) * Time.fixedDeltaTime);     // the enemy moves back slower due to the division
        }
        else if (playerDistanceFromEnemy < attackProximity && attackAllow)
        {
            // The enemy will attack if it's within attacking range and it hasn't already attacked recently 
            attackAllow = false;
            instProjectile = Instantiate(bombPrefab, bombSpawnPoint.position, bombSpawnPoint.rotation);
            instProjectile.GetComponent<Rigidbody>().velocity = bombSpawnPoint.forward * spawnEnemySpeed;
            StartCoroutine(AttackResetRoutine());
        }
        else if (playerDistanceFromStart < maxDistanceFromStart + 3)       // the extra +3 is so it chases when you go beside it
        {
            // If player is within the enemy's range, it will always chase player. Top priority.
            transform.LookAt(PlayerPosition);
            transform.position = Vector3.MoveTowards(EnemyPosition, PlayerPosition, enemySpeed * Time.fixedDeltaTime);
        }
        else if ((enemyDistanceFromStart > maxDistanceFromStart) && (playerDistanceFromEnemy < chaseProximity))
        {
            // If enemy hits max distance but player is still close, for now it will do nothing.
            transform.LookAt(PlayerPosition);
        }
        else if (enemyDistanceFromStart > maxDistanceFromStart)
        {
            // If enemy goes too far, move back. Very important, will get stuck otherwise.
            transform.LookAt(startPoint.transform);
            transform.position = Vector3.MoveTowards(EnemyPosition, startPoint.transform.position, enemySpeed * Time.fixedDeltaTime);
        }
        else if ((playerDistanceFromStart < chaseProximity) || (playerDistanceFromEnemy < chaseProximity))
        {
            // If player gets too close to enemy or its start point, it will chase player.
            transform.LookAt(PlayerPosition);
            transform.position = Vector3.MoveTowards(EnemyPosition, PlayerPosition, enemySpeed * Time.fixedDeltaTime);
        }
        else if ((playerDistanceFromEnemy > returnHomeProximity) && enemyDistanceFromStart > 1)
        {
            // If after hitting range limit the player moves far away, enemy will return home.
            transform.LookAt(startPoint.transform);
            transform.position = Vector3.MoveTowards(EnemyPosition, startPoint.transform.position, enemySpeed * Time.fixedDeltaTime);
        }
        else if (enemyDistanceFromStart < 1)
        {
            transform.LookAt(PlayerPosition);
            // If we ever make an idle animation, chuck it in here!
        }
    }

    IEnumerator AttackResetRoutine()                    // Waits a certain amount of time between the enemy being allowed to attack
    {
        yield return new WaitForSeconds(attackReset);
        attackAllow = true; //reset the jump counter if the player takes too long to jump
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            Destroy(gameObject);
        }
    }
}