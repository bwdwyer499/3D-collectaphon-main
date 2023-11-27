using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    private GameObject Player;
    private Vector3 DoorHeightAfterRise;

    public float DoorHeight = 20;
    public float doorRiseSpeed = 5;

    // Start is called before the first frame update
    void Awake()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
        //DoorHeightAfterRise = new Vector3(transform.position.x, transform.position.y+20, transform.position.z);
    }

    // Update is called once per frame
    void Update()
    {
        DoorHeightAfterRise = new Vector3(transform.position.x, DoorHeight, transform.position.z);

        if (Player.GetComponent<CollisionController>().currentAnimals == 3)
        {
            transform.position = Vector3.MoveTowards(transform.position, DoorHeightAfterRise, (doorRiseSpeed * 2) * Time.deltaTime);
        }

    }
}
