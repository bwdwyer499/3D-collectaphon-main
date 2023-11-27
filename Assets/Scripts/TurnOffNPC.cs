using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnOffNPC : MonoBehaviour
{
    private GameObject Player;
    private GameObject NPC;
    // Start is called before the first frame update
    void Awake()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
        NPC = transform.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (Player.GetComponent<CollisionController>().currentOpals > 0)
        {
            NPC.SetActive(false);
        }
    }
}
