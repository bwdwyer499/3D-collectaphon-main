using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{

    public TextMeshProUGUI nameText;        //for the name to be displayed in the dialogue box
    public TextMeshProUGUI dialogueText;    //for the dialogue to be displayed in the dialogue box
    public GameObject portraitImage;

    public Animator animator;               //animator to close the dialogue box
    public Queue<string> sentences;         //a Queue to hold sentences to display in the dialogue box

    // Start is called before the first frame update
    void Start()
    {
        sentences = new Queue<string>(); //make a queue (fifo) to hold sentences
    }

    //Opens the dialogue box calling an animation to do so
    //Displays the name for the dialogue box in the top left corner. eg "Jane"
    //Queue the sentences from the StartDialogue parameters passed in
    //Call DisplayNextSentence.
    public void StartDialogue(Dialogue dialogue)
    {
        //Debug.Log("Starting Coversation with " + dialogue.name);
        animator.SetBool("IsOpen",true);
        nameText.text = dialogue.name;
        portraitImage.GetComponent<Image>().sprite = dialogue.portrait;

        sentences.Clear();

        foreach (string sentence in dialogue.sentences)
        {
            sentences.Enqueue(sentence);
        }

        DisplayNextSentence();
    }

    //Will call Dequeue sentences from the queue
    //Will stop any dialogue if for some reason it happens to already be running
    //Calls the typeoutsentence enumerator
    public void DisplayNextSentence()
    {
        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }
        string sentence = sentences.Dequeue();
        StopAllCoroutines();
        StartCoroutine(TypeOutSentence(sentence));

        //dialogueText.text = sentence;
        //Debug.Log(sentence);

    }


    //Will type out a sentence in the form of an array of Char[] for a given string.
    IEnumerator TypeOutSentence(string sentence)
    {
        dialogueText.text = "";
        foreach(char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return null;
        }
    }

    //End the dialogue. Will "close" the dialogue box by calling an
    //animation to move it down below the canvas.
    public void EndDialogue()
    {
        animator.SetBool("IsOpen", false);
        //Debug.Log("End Convo");
    }
}
