using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthIcons : MonoBehaviour
{
    public float warningTime = 1;                 // the number of seconds between the health flashing when on 1 health
    private bool startWarn = true;          // bool to keep track of which health flash to show
    private bool changeWarning = false;
    private int currentCheck = 5;                 // int to keep track of whether or not health has changed. Script will only run when health is different to the currentCheck

    private GameObject Player;
    
    // Start is called before the first frame update
    void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        if (Player.GetComponent<CollisionController>().currentHealth == 1)                      // if health is on 1, flash warning
        {
            if (startWarn == true)
            {
                startWarn = false;
                StartCoroutine(HealthWarning());
            }
        }

        if (Player.GetComponent<CollisionController>().currentHealth != currentCheck)           // if health has changed, the correct sprites will be set active/inactive depending on currentHealth
        {
            GameObject h1 = transform.GetChild(0).gameObject;
            GameObject h2 = transform.GetChild(1).gameObject;
            GameObject h3 = transform.GetChild(2).gameObject;
            GameObject h4 = transform.GetChild(3).gameObject;
            GameObject h5 = transform.GetChild(4).gameObject;
            GameObject e2 = transform.GetChild(5).gameObject;
            GameObject e3 = transform.GetChild(6).gameObject;
            GameObject e4 = transform.GetChild(7).gameObject;
            GameObject e5 = transform.GetChild(8).gameObject;
            GameObject hw = transform.GetChild(9).gameObject;

            if (Player.GetComponent<CollisionController>().currentHealth == 5)
            {
                h1.SetActive(true);
                h2.SetActive(true);
                h3.SetActive(true);
                h4.SetActive(true);
                h5.SetActive(true);

                e2.SetActive(false);
                e3.SetActive(false);
                e4.SetActive(false);
                e5.SetActive(false);
                hw.SetActive(false);

                currentCheck = 5;
            }
            else if (Player.GetComponent<CollisionController>().currentHealth == 4)
            {
                h2.SetActive(true);
                h3.SetActive(true);
                h4.SetActive(true);
                h5.SetActive(false);

                e2.SetActive(false);
                e3.SetActive(false);
                e4.SetActive(false);
                e5.SetActive(true);

                currentCheck = 4;
            }
            else if (Player.GetComponent<CollisionController>().currentHealth == 3)
            {
                h2.SetActive(true);
                h3.SetActive(true);
                h4.SetActive(false);
                h5.SetActive(false);

                e2.SetActive(false);
                e3.SetActive(false);
                e4.SetActive(true);
                e5.SetActive(true);

                currentCheck = 3;
            }
            else if (Player.GetComponent<CollisionController>().currentHealth == 2)
            {
                h1.SetActive(true);
                h2.SetActive(true);
                h3.SetActive(false);
                h4.SetActive(false);
                h5.SetActive(false);

                e2.SetActive(false);
                e3.SetActive(true);
                e4.SetActive(true);
                e5.SetActive(true);
                hw.SetActive(false);

                currentCheck = 2;
            }
            else if (Player.GetComponent<CollisionController>().currentHealth == 1)
            {
                h2.SetActive(false);
                h3.SetActive(false);
                h4.SetActive(false);
                h5.SetActive(false);

                e2.SetActive(true);
                e3.SetActive(true);
                e4.SetActive(true);
                e5.SetActive(true);

                currentCheck = 1;
            }

        }
    }

    IEnumerator HealthWarning()                         // When health is on 1, the health will flash.
    {
        GameObject h1 = transform.GetChild(0).gameObject;
        GameObject hw = transform.GetChild(9).gameObject;

        if (changeWarning == false)
        {
            h1.SetActive(false);
            hw.SetActive(true);
            changeWarning = true;
        }
        else
        {
            h1.SetActive(true);
            hw.SetActive(false);
            changeWarning = false;
        }

        yield return new WaitForSeconds(warningTime);

        startWarn = true;
    }

}
