
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class CollisionController : MonoBehaviour
{
    //To use this script
    //- Create a new UI Text field (Create -> UI -> Text)
    //- this will create a Canvas object (read about the UI system here: https://docs.unity3d.com/Manual/UISystem.html)
    //- link up the text object to the Health Text variable in the inspector

    public Text UIText;                            //if you want to use Text Mesh Pro, comment out the variable above, and uncomment this one
    //public TextMeshProUGUI healthText;
    public int startingHealth = 5;
    public int startingOpals = 0;
    public int startingAnimals = 0;
    public int currentHealth;                     // player health - this is only public so it can be accessed by EnemyControllers
    public int currentOpals = 0;                      // collected opals
    public int currentAnimals = 0;                      // collected animals

    private GameObject SpawnPoint;
    private GameObject checkPoint1;
    private GameObject checkPoint2;
    private GameObject checkPoint3;
    private GameObject Player;

    public float fireForce = 30f;
    public float fireHeight = 0.5f;
    public int fireDamage = 1;
    public float chopperForce = 40f;
    public float chopperHeight = 3f;
    public int chopperDamage = 1;
    public float hazardForce = 40f;
    public int hazardDamage = 1;
    public float hazardHeight = 5f;
    public float playerImmuneTime = 1f;
    public float suspendTime = 0.5f;
    public float RespawnHealthKitTime = 30f;

    private float knockBackForce;
    private float lengthOfKnockback = 0.2f;                                   // 0.2 seems best
    private bool knockBackTime = false;
    private Vector3 knockBackDirection;                                       // the direction the player will be knocked back when hit

    private bool playerImmunity = false;

    public GameObject blackscreen;
    public CharacterController PlayerCC;

    bool checkCP1 = false;
    bool checkCP2 = false;
    bool checkCP3 = false;

    private void Awake()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
        //PlayerCC = Player.GetComponent<CharacterController>();
        SpawnPoint = GameObject.Find("PlayerSpawn");
        checkPoint1 = GameObject.Find("CP1");
        checkPoint2 = GameObject.Find("CP2");
        checkPoint3 = GameObject.Find("CP3");

        currentHealth = startingHealth;
        currentOpals = startingOpals;
        currentAnimals = startingAnimals;
        SetUIValues();
    }

    public void FixedUpdate()
    {
        if (knockBackTime)
        {
            PlayerCC.Move(knockBackDirection.normalized * knockBackForce * Time.deltaTime);
        }
    }

    private void SetUIValues()
    {
        UIText.text = "OPALS - " + currentOpals + "/12 \nANIMALS - " + currentAnimals + "/3";
    }

    private void IncreaseHealth(int amount)
    {
        currentHealth += amount;
        SetUIValues();
    }

    private void DecreaseHealth(int amount)
    {
        if (playerImmunity == false)
        {
            currentHealth -= amount;
            SetUIValues();
        }
    }

    private void IncreaseOpals(int amount)
    {
        currentOpals += amount;
        SetUIValues();
    }
    private void IncreaseAnimals(int amount)
    {
        currentAnimals += amount;
        SetUIValues();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "EnemyDamagePoint")
        {
            if (other.gameObject.name == "FireDamage")
            {
                knockBackForce = fireForce;
                DamageCalculator(other, fireDamage, fireHeight);
            }

            if (other.gameObject.name == "BladeDamage")
            {
                knockBackForce = chopperForce;
                DamageCalculator(other, chopperDamage, chopperHeight);
            }

            if (other.gameObject.name == "HazardDamage")
            {
                knockBackForce = hazardForce;
                DamageCalculator(other, hazardDamage, hazardHeight);
                StartCoroutine(SuspendGravity());
            }
        }

        if (other.gameObject.tag == "HP")     
        {
            if (currentHealth < 5)  // only increase health if it's currently less than max
            {
                IncreaseHealth(1);
            }
            GameObject Healthkit = other.gameObject.transform.parent.gameObject;
            StartCoroutine(RespawnHealthkit(Healthkit));
        }

        if (other.gameObject.tag == "Opal")
        {
            IncreaseOpals(1);
            Destroy(other.gameObject);
        }

        if (other.gameObject.tag == "Animal")
        {
            IncreaseAnimals(1);
            FindObjectOfType<DialogueManager>().EndDialogue();
            Destroy(other.gameObject);
        }

        if (other.gameObject.tag == "CP1")
        {
            checkCP1 = true;
            checkCP2 = false;
            checkCP3 = false;
        }

        if (other.gameObject.tag == "CP2")
        {
            checkCP1 = false;
            checkCP2 = true;
            checkCP3 = false;
        }

        if (other.gameObject.tag == "CP3")
        {
            checkCP1 = false;
            checkCP2 = false;
            checkCP3 = true;
        }
    }

    IEnumerator SuspendGravity()                         // Waits a certain amount of time before player can get hit again
    {
        Player.GetComponent<CombinePlayerController>().hazardHit = true;
        yield return new WaitForSeconds(suspendTime);
        Player.GetComponent<CombinePlayerController>().hazardHit = false;
    }

    IEnumerator PlayerHit()                         // Waits a certain amount of time before player can get hit again
    {
        yield return new WaitForSeconds(lengthOfKnockback);
        knockBackTime = false;
    }

    IEnumerator PlayerImmunity()                         // Waits a certain amount of time before player can get hit again
    {
        yield return new WaitForSeconds(playerImmuneTime);
        playerImmunity = false;          // Change variable in CombinePlayerController
    }

    private void DamageCalculator(Collider other, int healthDamage, float height)
    {
        if (playerImmunity == false)
        {
            DecreaseHealth(healthDamage);
        }

        if (checkCP1 == true)
        {
            if (currentHealth <= 0)
            {
                knockBackTime = false;
                GetComponent<CharacterController>().enabled = false;
                transform.localPosition = checkPoint1.transform.position;  // respawns the player at the position of CP1.
                currentHealth = 5;
                SetUIValues();
                GetComponent<CharacterController>().enabled = true;
                Respawn();
            }
        }

        if (checkCP2 == true)
        {
            if (currentHealth <= 0)
            {
                knockBackTime = false;
                GetComponent<CharacterController>().enabled = false;
                transform.localPosition = checkPoint2.transform.position;  // respawns the player at the position of CP1.
                currentHealth = 5;
                SetUIValues();
                GetComponent<CharacterController>().enabled = true;
                Respawn();
            }
        }

        if (checkCP3 == true)
        {
            if (currentHealth <= 0)
            {
                knockBackTime = false;
                GetComponent<CharacterController>().enabled = false;
                transform.localPosition = checkPoint3.transform.position;  // respawns the player at the position of CP1.
                currentHealth = 5;
                SetUIValues();
                GetComponent<CharacterController>().enabled = true;
                Respawn();
            }
        }

        if (currentHealth <= 0)
        {
            knockBackTime = false;
            GetComponent<CharacterController>().enabled = false;
            transform.localPosition = SpawnPoint.transform.position;  // respawns the player at the position of the SpawnPoint.
            currentHealth = 5;
            SetUIValues();
            GetComponent<CharacterController>().enabled = true;
            Respawn();
        }
        else
        {
            if (other.gameObject.name == "HazardDamage")
            {
                GameObject target = other.transform.GetChild(0).gameObject;
                knockBackDirection = target.transform.position - Player.transform.position;        // Compares the player's position to the damage point's position, then knocks the player away
            }
            else
            {
                knockBackDirection = Player.transform.position - other.gameObject.transform.position;        // Compares the player's position to the damage point's position, then knocks the player away
            }
            knockBackDirection.y += height;           // How high the player should go after being hit - make public variable for this if people want to change
            knockBackTime = true;
            StartCoroutine(PlayerHit());            // Applies knockback force

            if (playerImmunity == false)     // Player immunity is only applied if it's not already on.
            {
                playerImmunity = true;       // Change variable in CombinePlayerController
                StartCoroutine(PlayerImmunity());
            }
        }
    }

    void Respawn()
    {
        blackscreen.gameObject.SetActive(true);
        StartCoroutine(RespawnTime());
    }

    IEnumerator RespawnTime ()
    {
        yield return new WaitForSeconds(1);
        blackscreen.gameObject.SetActive(false);
    }

    IEnumerator RespawnHealthkit(GameObject Healthkit)      // healthkits respawn after a certain amount of time, determined by RespawnHealthKitTime (default 30 seconds)
    {
        Healthkit.SetActive(false);
        yield return new WaitForSeconds(RespawnHealthKitTime);
        Healthkit.SetActive(true);
    }
}