using UnityEngine;
using System.Collections;
using System;
using Random = UnityEngine.Random;

public class Enemy1Controller : MonoBehaviour
{
    [Header("ENEMY VARIABLES")]
    [Tooltip("How much force the enemy will hit the player with when they collide.")]
    public float knockBackForce = 50f;
    [Tooltip("How fast the enemy moves.")]
    public float enemySpeed = 6f;
    [Tooltip("How close the enemy needs to get before they attack.")]
    public float attackProximity = 6f;
    [Tooltip("How far the enemy jumps when attacking.")]
    public float attackDistance = 800f;
    [Tooltip("How high the enemy jumps during its attack.")]
    public float attackJumpHeight = 1000f;
    [Tooltip("How many seconds pass between enemy attacks.")]
    public float attackReset = 2f;
    [Tooltip("How many seconds the enemy will pause after hitting player.")]
    public float enemyPauseTime = 1f;
    [Tooltip("The maximum distance the enemy will go from its start point before stopping.")]
    public float maxDistanceFromStart = 20f;
    [Tooltip("How close the player gets to enemy or enemy's start point before the enemy chases you.")]
    public float chaseProximity = 15f;
    [Tooltip("How far away the player gets from enemy before they return home.")]
    public float returnHomeProximity = 10f;
    [Tooltip("Gravity force applied to enemy.")]
    public float enemyGravity = -5000f;

    [Header("DEBUG")]
    [Tooltip("How far from the player the enemy currently is.")]
    public float playerDistanceFromEnemy;
    [Tooltip("How far from the enemy's start point the enemy currently is.")]
    public float enemyDistanceFromStart;
    [Tooltip("How far from the enemy's start point the player currently is.")]
    public float playerDistanceFromStart;



    [Header("INPUT")]
    // nothing

    private GameObject startPoint;

    private bool attackAllow = true;                                          // true when the enemy is allowed to attack - turns false for an amount of time following attack
    private bool forceEnemyReturn = false;                                    // true if the player has cheesed the enemy a distance. Enemy will return
    private bool enemySuccessHit = false;                                     // true if the enemy has just hit the player (for small period enemy will pause and player can't get hit again)
    private bool knockBackTime = false;                                       // true when the player has been hit and the motion is being applied
    private bool idleJump = true;                                             // true when the enemy can jump while idle (it will go false for a small frame of time)
    private float defaultYPosition;                                           // the Y position of the enemy at the start of each update
    private Vector3 knockBackDirection;                                       // the direction the player will be knocked back when hit
    private Vector3 velocityDown;                                             // the velocity the enemy moves downwards, determined by enemyGravity
    private Vector3 attackJump;                                               // the height the enemy will jump during attacks, determined by attackJumpHeight

    private GameObject Player;
    private CharacterController PlayerCC;
    private Rigidbody EnemyRB;
    public Animator animator;

    void Start()
    {
        startPoint = new GameObject("Enemy1StartPoint");
        startPoint.transform.position = transform.position;

        Player = GameObject.FindGameObjectWithTag("Player");
        PlayerCC = Player.GetComponent<CharacterController>();
        EnemyRB = transform.GetComponent<Rigidbody>();

        velocityDown.y += enemyGravity;                                     // getting the value from the public variable and putting it into a Vector3
        attackJump.y += attackJumpHeight;                                   // getting the value from the public variable and putting it into a Vector3
        defaultYPosition = transform.position.y;                            // setting the initial y value
    }

    void FixedUpdate()
    {
        defaultYPosition = transform.position.y;                            // updates the y, is this necessary or only in Start()?

        EnemyRB.AddForce(velocityDown * Time.fixedDeltaTime);                    // adds gravity

        playerDistanceFromEnemy = Vector3.Distance(Player.transform.position, transform.position);
        enemyDistanceFromStart = Vector3.Distance(startPoint.transform.position, transform.position);
        playerDistanceFromStart = Vector3.Distance(startPoint.transform.position, Player.transform.position);

        Enemy1Algorithm();
    }
    private void Enemy1Algorithm()
    {
        Vector3 PlayerPosition = new Vector3(Player.transform.position.x, defaultYPosition, Player.transform.position.z);
        Vector3 EnemyPosition = new Vector3(transform.position.x, defaultYPosition, transform.position.z);
        Vector3 StartPosition = new Vector3(startPoint.transform.position.x, defaultYPosition, startPoint.transform.position.z);

        if (enemySuccessHit)
        {
            // Player has just been hit, enemy will do nothing but look at player.
            transform.LookAt(PlayerPosition);
        }
        else if (forceEnemyReturn && enemyDistanceFromStart < maxDistanceFromStart)
        {
            // Top priority - if the player has cheesed the enemy away from start, it must return before the forceEnemyReturn turns off
            forceEnemyReturn = false;
        }
        else if (forceEnemyReturn)
        {
            // If the enemy has been cheesed away, 
            transform.LookAt(StartPosition);
            animator.SetBool("IsWalking", true);
            transform.position = Vector3.MoveTowards(EnemyPosition, StartPosition, (enemySpeed * 3) * Time.fixedDeltaTime);
        }
        else if (enemyDistanceFromStart > maxDistanceFromStart + 10)            //the extra +10 is the maximum range player's could cheese enemy
        {
            // This is only in case the player tries to cheese the enemy far away from its start point.
            forceEnemyReturn = true;
        }
        else if (playerDistanceFromEnemy < attackProximity && attackAllow)
        {
            // The enemy will attack if it's within attacking range and it hasn't already attacked recently 
            attackAllow = false;
            StopCoroutine("idleJumpPause");
            Vector3 attackTarget = Player.transform.position - transform.position;
            EnemyRB.AddForce(attackTarget.normalized * attackDistance);
            EnemyRB.AddForce(attackJump);
            StartCoroutine(AttackResetRoutine());
        }
        else if (playerDistanceFromStart < maxDistanceFromStart + 3)       // the extra +3 is so it chases when you go beside it
        {
            // If player is within the enemy's range, it will always chase player. Top priority.
            transform.LookAt(PlayerPosition);
            animator.SetBool("IsWalking", true);
            transform.position = Vector3.MoveTowards(EnemyPosition, PlayerPosition, enemySpeed * Time.fixedDeltaTime);
        }
        else if ((enemyDistanceFromStart > maxDistanceFromStart) && (playerDistanceFromEnemy < chaseProximity))
        {
            // If enemy hits max distance but player is still close, for now it will do nothing.
            transform.LookAt(PlayerPosition);
            animator.SetBool("IsWalking", false);
        }
        else if (enemyDistanceFromStart > maxDistanceFromStart)
        {
            // If enemy goes too far, move back. Very important, will get stuck otherwise.
            transform.LookAt(StartPosition);
            animator.SetBool("IsWalking", true);
            transform.position = Vector3.MoveTowards(EnemyPosition, StartPosition, enemySpeed * Time.fixedDeltaTime);
        }
        else if ((playerDistanceFromStart < chaseProximity) || (playerDistanceFromEnemy < chaseProximity))
        {
            // If player gets too close to enemy or its start point, it will chase player.
            transform.LookAt(PlayerPosition);
            animator.SetBool("IsWalking", true);
            transform.position = Vector3.MoveTowards(EnemyPosition, PlayerPosition, enemySpeed * Time.fixedDeltaTime);
        }
        else if ((playerDistanceFromEnemy > returnHomeProximity) && enemyDistanceFromStart > 1)
        {
            // If after hitting range limit the player moves far away, enemy will return home.
            transform.LookAt(StartPosition);
            animator.SetBool("IsWalking", true);
            transform.position = Vector3.MoveTowards(EnemyPosition, StartPosition, enemySpeed * Time.fixedDeltaTime);
        }
        else if (enemyDistanceFromStart < 1)
        {
            animator.SetBool("IsWalking", false);
            transform.LookAt(PlayerPosition);

            if (idleJump)
            {
                float secondsToNextJump = Random.Range(1.0f, 3.0f);
                idleJump = false;
                StartCoroutine(idleJumpPause(secondsToNextJump));
            }

            // If we ever make an idle animation, chuck it in here!

        }
    }

    IEnumerator AttackResetRoutine()                    // Waits a certain amount of time between the enemy being allowed to attack
    {
        yield return new WaitForSeconds(attackReset);
        attackAllow = true;
    }

    void OnTriggerEnter(Collider other)                 // If the enemy hits the player they are knocked back. Note: only the nose is a trigger.
    {
        if (other.gameObject == Player)
        {
            StartCoroutine(EnemyPause());           // Enemy will pause for a time before continuing to chase
        }
    }

    IEnumerator EnemyPause()                         // How long the enemy will pause after hitting character
    {
        yield return new WaitForSeconds(enemyPauseTime);
        enemySuccessHit = false;
    }

    IEnumerator idleJumpPause(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        attackAllow = false;
        StartCoroutine(AttackResetRoutine());
        EnemyRB.AddForce(attackJump * 1.5f);
        idleJump = true;
    }

}