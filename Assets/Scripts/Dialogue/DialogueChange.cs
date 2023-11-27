using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueChange : MonoBehaviour
{
    private GameObject Player;
    public bool NPCPop = false;
    // Start is called before the first frame update
    void Awake()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        if (NPCPop)     // only call the code when an NPC pops up
        {
            if (transform.gameObject.name == "NPC1")
            {
                GameObject first = transform.GetChild(0).gameObject.transform.GetChild(0).gameObject;
                GameObject second = transform.GetChild(0).gameObject.transform.GetChild(1).gameObject;
                GameObject third = transform.GetChild(0).gameObject.transform.GetChild(2).gameObject;

                if (Player.GetComponent<CollisionController>().currentOpals > 0 && Player.GetComponent<CollisionController>().currentAnimals < 3)
                {
                    first.SetActive(false);
                    second.SetActive(true);

                    //transform.Find("DialogueTriggerRadius1").gameObject.SetActive(false);
                    //transform.Find("DialogueTriggerRadius2").gameObject.SetActive(true);
                }

                if (Player.GetComponent<CollisionController>().currentAnimals == 3)
                {
                    first.SetActive(false);
                    second.SetActive(false);
                    third.SetActive(true);
                }
            }

            if (transform.gameObject.name == "NPC2" || transform.gameObject.name == "NPC10" || transform.gameObject.name == "NPC14")
            {
                GameObject first = transform.GetChild(0).gameObject.transform.GetChild(0).gameObject;
                GameObject second = transform.GetChild(0).gameObject.transform.GetChild(1).gameObject;

                if (Player.GetComponent<CollisionController>().currentAnimals == 3)
                {
                    first.SetActive(false);
                    second.SetActive(true);
                }
            }

            if (transform.gameObject.name == "NPC3")
            {
                GameObject first = transform.GetChild(0).gameObject.transform.GetChild(0).gameObject;
                GameObject second = transform.GetChild(0).gameObject.transform.GetChild(1).gameObject;

                if (Player.GetComponent<CollisionController>().currentOpals > 1)
                {
                    first.SetActive(false);
                    second.SetActive(true);
                }
            }

            if (transform.gameObject.name == "NPC7")
            {
                GameObject first = transform.GetChild(0).gameObject.transform.GetChild(0).gameObject;
                GameObject second = transform.GetChild(0).gameObject.transform.GetChild(1).gameObject;

                if (Player.GetComponent<CollisionController>().currentAnimals > 1)
                {
                    first.SetActive(false);
                    second.SetActive(true);
                }
            }

            if (transform.gameObject.name == "NPC15")
            {
                GameObject first = transform.GetChild(0).gameObject.transform.GetChild(0).gameObject;
                GameObject second = transform.GetChild(0).gameObject.transform.GetChild(1).gameObject;

                if (Player.GetComponent<CollisionController>().currentOpals == 12)
                {
                    first.SetActive(false);
                    second.SetActive(true);
                }
            }

            if (transform.gameObject.name == "NPC16")
            {
                GameObject first = transform.GetChild(0).gameObject.transform.GetChild(0).gameObject;
                GameObject second = transform.GetChild(0).gameObject.transform.GetChild(1).gameObject;

                if (Player.GetComponent<CollisionController>().currentOpals > 0)
                {
                    first.SetActive(false);
                    second.SetActive(true);
                }
            }
        }
    }
}
