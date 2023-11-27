using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StepsTrigger : MonoBehaviour
{
    private GameObject Player;
    private Vector3 DoorHeightAfterRise;

    public float stairsHeight = 17.85f;
    public float stairRiseSpeed = 100;

    // Start is called before the first frame update
    void Awake()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
        //DoorHeightAfterRise = new Vector3(transform.position.x, transform.position.y+20, transform.position.z);
    }

    // Update is called once per frame
    void Update()
    {
        DoorHeightAfterRise = new Vector3(transform.position.x, stairsHeight, transform.position.z);

        if (Player.GetComponent<CollisionController>().currentOpals > 0)
        {
            transform.position = Vector3.MoveTowards(transform.position, DoorHeightAfterRise, (stairRiseSpeed * 10) * Time.deltaTime);
        }

    }
}

