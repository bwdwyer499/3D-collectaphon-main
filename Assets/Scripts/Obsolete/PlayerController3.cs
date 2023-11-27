using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController3 : MonoBehaviour
{
    // Reference Variables
    //TO DO - Animator
    Player_Controller playerInput;              //Player input system 
    CharacterController characterController;    //Players character controller component
    Transform cameraMainTransform;

    // Variables to store player input values
    Vector2 currentMovementInput;
    public Vector3 currentMovement;                
    public Vector3 currentRunMovement;
    public Vector3 appliedMovement;             //Controls player movement through averaging of two positions
    bool isMovementPressed;                     //bool for if the player is moving with the left stick
    bool isRunPressed;                          //bool for if the player is running using the left bumper

    //Constraints 
    float rotationFactorPreFrame = 0.1f;        //Speed/Smoothness the player rotates at
    public float walkSpeedMultiplier = 10.0f;   //Speed the player walks at
    public float runSpeedMultiplier = 15.0f;    //Speed the player runs at

    //Jumping Variables
    bool isJumpPressed = false;
    public float initialJumpVelocity;
    public float maxJumpHeight = 8.0f;
    public float maxJumpTime = 2f;
    bool isJumping = false;
    int jumpCount = 0;
    Dictionary<int, float> initialJumpVelocities = new Dictionary<int, float>(); //Dict to store initial jump velocity. Effected by jump combo
    Dictionary<int, float> jumpGravities = new Dictionary<int, float>();        //Dict to store jump gravity. Effected by jump combo
    Coroutine currentJumpResetRoutine = null;

    //Gravity Variables
    public float groundedGrav = -0.5f;
    public float gravity = -9.8f;

    private void Awake()
    {
        // intial setting of reference variables
        playerInput = new Player_Controller();                      // access to Player_Controller Player Input system.
        characterController = GetComponent<CharacterController>();  //access to character controller.
        cameraMainTransform = Camera.main.transform;

        //player input callback setters
        //playerInput (Input System) -> Player (Action Map) -> Action (eg, Jump) -> Listen (started, cancelled, etc) -> Callback Function (eg, OnJump)
        playerInput.Player.Move.started += OnMovementInput;         //When button is first pressed out callback is run
        playerInput.Player.Move.canceled += OnMovementInput;        //When all input has been stopped.
        playerInput.Player.Move.performed += OnMovementInput;       //When completed or updated callback is run. Movement is updated constantly so this is needed
        playerInput.Player.Run.started += OnRun;                    //
        playerInput.Player.Run.canceled += OnRun;                   //  These are buttons so they
        playerInput.Player.Jump.started += OnJump;                  //  only need started and canceled
        playerInput.Player.Jump.canceled += OnJump;                 //

        setJumpVariables();
    }

    // Update is called once per frame
    void Update()
    {
        HandleRotation();
        if (isRunPressed)
        {
            appliedMovement.x = currentRunMovement.x;
            appliedMovement.z = currentRunMovement.z;
        }
        else
        {
            appliedMovement.x =  currentMovement.x;
            appliedMovement.z =  currentMovement.z;
        }
        
        characterController.Move(((appliedMovement.x * cameraMainTransform.right.normalized) + (appliedMovement.y * cameraMainTransform.up.normalized) + (appliedMovement.z * cameraMainTransform.forward.normalized)) * Time.deltaTime);
        HandleGravity();
        HandleJump();
    }
    void setJumpVariables()
    {
        float timeToApex = maxJumpTime / 2; 
        gravity = (-2 * maxJumpHeight) / Mathf.Pow(timeToApex, 2);
        initialJumpVelocity = (2 * maxJumpHeight) / timeToApex;
        float secondJumpGravity = (-2 * (maxJumpHeight + 1)) / Mathf.Pow((timeToApex * 1.1f), 2);
        float secondJumpInitialVelocity = (2 * (maxJumpHeight + 1)) / timeToApex * 1.1f;
        float thirdJumpGravity = (-2 * (maxJumpHeight + 2)) / Mathf.Pow((timeToApex * 1.25f), 2);
        float thirdJumpInitialVelocity = (2 * (maxJumpHeight + 2)) / timeToApex * 1.25f;

        //add to the dictonary
        initialJumpVelocities.Add(1, initialJumpVelocity);
        initialJumpVelocities.Add(2, secondJumpInitialVelocity);
        initialJumpVelocities.Add(3, thirdJumpInitialVelocity);

        jumpGravities.Add(0, gravity); //for when jumping count resets
        jumpGravities.Add(1, gravity);
        jumpGravities.Add(2, secondJumpGravity);
        jumpGravities.Add(3, thirdJumpGravity);
    }

    void HandleJump()
    {
        if (!isJumping && characterController.isGrounded && isJumpPressed)
        {
            if (jumpCount < 3 && currentJumpResetRoutine != null)
            {
                StopAllCoroutines();
                //StopCoroutine(currentJumpResetRoutine);
                //StopCoroutine(JumpResetRoutine());
                Debug.Log("Coroutine Stopped");
            }
            isJumping = true;
            jumpCount += 1;
            Debug.Log("Jump Count " + jumpCount);
            currentMovement.y = initialJumpVelocities[jumpCount];
            appliedMovement.y = initialJumpVelocities[jumpCount];
        }
        else if(!isJumpPressed && isJumping && characterController.isGrounded)
        {
            isJumping = false;
        }
    }
    void HandleGravity()
    {
        bool isFalling = currentMovement.y <= 0.0f || !isJumpPressed; // if the player is falling or if the jump button is not pressed
        float fallMultiplier = 3.0f;

        //apply the proper gravity to the player if they are grounded or in the air
        if (characterController.isGrounded)
        {
            //start the jump combo coroutine
            currentJumpResetRoutine = StartCoroutine(JumpResetRoutine());
            if (jumpCount == 3)
            {
                jumpCount = 0;
            }
            //Debug.Log("Is player grounded "+characterController.isGrounded);
            currentMovement.y = groundedGrav;       
            appliedMovement.y = groundedGrav;
        }

        //apply additional gravity after the player reaches the apex of the jump
        else if(isFalling)
        {
            float previousYVelocity = currentMovement.y;
            currentMovement.y = currentMovement.y + (jumpGravities[jumpCount] * fallMultiplier * Time.deltaTime);
            appliedMovement.y = Mathf.Max((previousYVelocity + currentMovement.y) * 0.5f, -20.0f); // Mathf.max - Cap the max fall
            
        }
        //Velocity Verlet - Jumps will be the same regardless of the framerate. 
        //Uses the average of two points to adjust the player characters trajectory.
        else
        {
            //Velocity Verlet - Jumps will be the same regardless of the framerate.
            float previousYVelocity = currentMovement.y;
            currentMovement.y = currentMovement.y + (jumpGravities[jumpCount] * Time.deltaTime);
            appliedMovement.y = (previousYVelocity + currentMovement.y) * 0.5f;
            
        }
    }

    void HandleRotation()
    {
        Vector3 positionToLookAt;
        positionToLookAt.x = currentMovement.x;
        positionToLookAt.y = 0.0f;
        positionToLookAt.z = currentMovement.z;
        Quaternion currentRotation = transform.rotation;
        Quaternion camRotation = cameraMainTransform.rotation.normalized;

        if (isMovementPressed)
        {
            // this needs the camera!
            Quaternion targetRotation = Quaternion.LookRotation(positionToLookAt); 
            transform.rotation = Quaternion.Slerp(currentRotation, targetRotation * camRotation, rotationFactorPreFrame);
        }
    }

    void OnJump(InputAction.CallbackContext context)
    {
        isJumpPressed = context.ReadValueAsButton();
        Debug.Log("is jump pressed: "+isJumpPressed);
    }

    void OnRun(InputAction.CallbackContext context)
    {
        isRunPressed = context.ReadValueAsButton();
        Debug.Log("is run pressed: " + isRunPressed);
    }

    void OnMovementInput(InputAction.CallbackContext context)
    {
        currentMovementInput = context.ReadValue<Vector2>();
        currentMovement.x = currentMovementInput.x * walkSpeedMultiplier;
        currentMovement.z = currentMovementInput.y * walkSpeedMultiplier;
        currentRunMovement.x = currentMovementInput.x * runSpeedMultiplier;
        currentRunMovement.z = currentMovementInput.y * runSpeedMultiplier;
        isMovementPressed = currentMovementInput.x != 0 || currentMovementInput.y != 0;
    }
    private void OnEnable()
    {
        playerInput.Player.Enable();
    }
    private void OnDisable()
    {
        playerInput.Player.Disable();
    }
    // Coroutine. Handles jump timer for multiple jump combos.
    // Resets jumpCount if they take too long between jumps
    IEnumerator JumpResetRoutine()
    {
        yield return new WaitForSeconds(0.5f);
        jumpCount = 0; //reset the jump counter if the player takes too long to jump
    }
}
