using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class DialogueTrigger : MonoBehaviour
{
   
    public Dialogue dialogue; //takes a dialogue script

    //Will start the dialogue vial the DialogueManager StartDialouge function passing in the dialogue.

    public void TriggerDialogue()
    {
        FindObjectOfType<DialogueManager>().StartDialogue(dialogue);
    }

    //When the player runs in the Dialogue Trigger object attached to whatever you want. Dialouge will start. 
    //TODO: Switch player controls to that of the ui controls.
    //TODO: Stop the game world, or the enemies, or something while the player talks or reads dialogue.
    void OnTriggerEnter(Collider col)
    {
        if(col.gameObject.tag == "Player")
        {
            if (transform.parent.gameObject.name != "AnimalDialogue")
            {
                transform.parent.gameObject.transform.parent.GetComponent<DialogueChange>().NPCPop = true;
            }
            TriggerDialogue();
            //Debug.Log("Converstation Trigger Detected");
        }
    }
}
