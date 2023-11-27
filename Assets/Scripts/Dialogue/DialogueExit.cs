using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueExit : MonoBehaviour
{
    //Will start the dialogue vial the DialogueManager StartDialouge function passing in the dialogue.

    public void StopDialogue()
    {
        FindObjectOfType<DialogueManager>().EndDialogue();
    }

    //When the player runs in the Dialogue Trigger object attached to whatever you want. Dialouge will start. 
    //TODO: Switch player controls to that of the ui controls.
    //TODO: Stop the game world, or the enemies, or something while the player talks or reads dialogue.
    void OnTriggerExit(Collider col)
    {
        if (col.gameObject.tag == "Player")
        {
            if (transform.parent.gameObject.name != "AnimalDialogue")
            {
                transform.parent.gameObject.transform.parent.GetComponent<DialogueChange>().NPCPop = true;
            }
            StopDialogue();
            //Debug.Log("Converstation Trigger Detected");
        }
    }
}
