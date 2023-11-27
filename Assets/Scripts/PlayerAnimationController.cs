using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;

public class PlayerAnimationController : MonoBehaviour
{
    Animator playerAnimator;
    int isIdleHash;
    int isRunningHash;
    PlayerInput Player_Controller;
   
    void Start()
    {
        //set anim ref
        playerAnimator.GetComponent<Animator>(); //will find animator componet the object this script is attached to.

        //set bool transition IDs
        isIdleHash = Animator.StringToHash("isIdle");
        isRunningHash = Animator.StringToHash("isRunning");
    }

    void Update()
    {
        
    }
}
