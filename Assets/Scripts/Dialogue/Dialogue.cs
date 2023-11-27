using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Just holds a name and dialogue strings.
//Accessed via the DialogueTrigger Script.
//This is what is displayed int the text boxes in the DialogueTrigger in the Inspector in Unity.
[System.Serializable]
public class Dialogue
{
    public string name;

    [TextArea(3,5)] //this makes the sentence area bigger so it is easier to write dialoge.
                    //Min 3 line space. Max 5 lines. Change if needed
    public string[] sentences;

    public Sprite portrait;
    
}
