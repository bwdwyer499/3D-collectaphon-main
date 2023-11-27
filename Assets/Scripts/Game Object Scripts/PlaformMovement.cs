using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaformMovement : MonoBehaviour
{
    [Header("Movement Points")]
    [SerializeField]
    private Transform pointA, pointB;   //Point to move between
    [Header("Movement Speed")]
    [SerializeField]
    private float moveSpeed = 5.0f;     //Movement speed of object     
    private bool switching = false;     //bool to idenify if the object is at pointA or pointB;
    private void FixedUpdate()
    {
        PlatformMovement();
    }

    //Controls the movement of the platform by moving it towards pointA or pointB.
    private void PlatformMovement()
    {
        if (!switching)
        {
            transform.position = Vector3.MoveTowards(transform.position, pointB.position, moveSpeed * Time.fixedDeltaTime);
        }
        else if (switching)
        {
            transform.position = Vector3.MoveTowards(transform.position, pointA.position, moveSpeed * Time.fixedDeltaTime);
        }
        if (transform.position == pointB.position)
        {
            switching = true;       // if the platform is at pointB;
        }
        else if (transform.position == pointA.position)
        {
            switching = false;      // if the platform is at pointA;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Plaform Trigger detected");
        if (!other.CompareTag("Player")) return; //if the player is not detected get out;
        other.transform.parent = transform;      //make the player move the same as the platform so they don't slide off
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("Plaform Trigger exited");
        if (!other.CompareTag("Player")) return; //if the player is not detected get out;
        other.transform.parent = null;           //return the player to normal movement when no longer touching the platform
    }
}
