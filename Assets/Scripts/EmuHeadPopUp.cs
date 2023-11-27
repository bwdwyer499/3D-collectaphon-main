using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmuHeadPopUp : MonoBehaviour
{
    private bool timeToMove = false;            // true when the boolean on the parent NPC GameObject changes, indicating it is time to move
    private bool poppedUp = false;              // true when the emu's head is popped up
    private bool goingUp = false;               // true when the emu is still going up

    Vector3 downPosition;
    Vector3 upPosition;

    private void Awake()
    {
        downPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);           // downPosition is the defeault position of emu
        upPosition = new Vector3(transform.position.x, transform.position.y+3, transform.position.z);           // upPosition has a y value increased by 3
    }

    void FixedUpdate()
    {
        if (transform.parent.GetComponent<DialogueChange>().NPCPop != poppedUp)
        {
            timeToMove = true;

            if (poppedUp == true)
            {
                goingUp = false;
            }
            else
            {
                goingUp = true;
            }

            poppedUp = transform.parent.GetComponent<DialogueChange>().NPCPop;
        }

        if (timeToMove == true)     // triggers when the emu needs to move
        {
            popUp();
        }
    }

    void popUp()
    {
        if (goingUp)
        {
            transform.position = Vector3.MoveTowards(transform.position, upPosition, 8f * Time.fixedDeltaTime);

            if (transform.position == upPosition)     // if the emu has reached its position, stop calling 
            {
                timeToMove = false;
            }
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, downPosition, 5f * Time.fixedDeltaTime);

            if (transform.position == downPosition)
            {
                timeToMove = false;
            }
        }
    }
}
