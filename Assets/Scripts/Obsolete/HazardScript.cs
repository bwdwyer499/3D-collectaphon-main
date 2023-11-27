using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HazardScript : MonoBehaviour
{
    [Tooltip("How much force the enemy will hit the player with when they collide.")]
    public float knockBackForce = 50f;
    [Tooltip("How many seconds the player has immunity after being hit.")]
    public float playerImmuneTime = 1f;

    private Vector3 knockBackDirection;                                       // the direction the player will be knocked back when hit
    private bool knockBackTime = false;                                       // true when the player has been hit and the motion is being applied
    private GameObject Player;
    private CharacterController PlayerCC;

    // Start is called before the first frame update
    void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
        PlayerCC = Player.GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (knockBackTime)
        {
            PlayerCC.Move(knockBackDirection.normalized * knockBackForce * Time.deltaTime);
        }
    }

    void OnTriggerEnter(Collider other)                 // If the enemy hits the player they are knocked back. Note: only the nose is a trigger.
    {
        Debug.Log("tfewf");

        if (other.gameObject == Player)
        {
            knockBackDirection = Player.transform.position - transform.position;        // Finds the direction from the hit point for the player to be knocked back
            knockBackDirection.y += 0.5f;           // How high the player should go after being hit - make public variable for this if people want to change
            knockBackTime = true;
            StartCoroutine(PlayerHit());            // Applies knockback force

            if (Player.GetComponent<CombinePlayerController>().playerImmunity == false)     // Player immunity is only applied if it's not already on.
            {
                Player.GetComponent<CombinePlayerController>().playerImmunity = true;       // Change variable in CombinePlayerController
                StartCoroutine(PlayerImmunity());
            }
        }
    }

    IEnumerator PlayerHit()                         // Waits a certain amount of time before player can get hit again
    {
        yield return new WaitForSeconds(0.2f);
        knockBackTime = false;
    }

    IEnumerator PlayerImmunity()                         // Waits a certain amount of time before player can get hit again
    {
        yield return new WaitForSeconds(playerImmuneTime);
        Player.GetComponent<CombinePlayerController>().playerImmunity = false;          // Change variable in CombinePlayerController
    }
}
