using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    Player_Controller playerInput;              //Player input system 
    CharacterController CC;                     //Players character controller component (changed to CC)
    Transform cam;                              //changed to cam

    [Header("PLAYER")]
    [Tooltip("How fast the player walks on the ground.")]
    public float walkSpeedMultiplier = 10f;
    [Tooltip("How fast the player runs on the ground.")]
    public float runSpeedMultiplier = 15f;
    //[Tooltip("How fast the player moves in the air.")]
    //public float airSpeedMultiplier;                          // if we want to change the speed in the air
    [Tooltip("How high the player jumps.")]
    public float jumpHeight;
    //[Tooltip("How far the player jumps.")]                    // if we want to add initial momentum to normal jumps
    //public float jumpDistance;
    [Tooltip("How high the player double jumps.")]
    public float doubleJumpHeight;
    [Tooltip("How far the player power jumps.")]
    public float powerJumpDistance;
    [Tooltip("How high the player spring jumps.")]
    public float springJumpHeight;
    [Tooltip("The window (in seconds) the spring jump is available for after a power jump.")]
    public float springJumpWindow;
    [Tooltip("The window (in seconds) the player will jump even if they press the button slightly too early.")]
    public float predictiveJumpWindow;
    [Tooltip("How often (in seconds) the player can power jump. If at 0 the player can bounce everywhere.")]
    public float powerJumpRegularity;
    [Tooltip("How strong gravity pulls on the player.")]
    public float playerGravity;
    [Tooltip("The speed the player will GAIN each update they're falling.")]
    public float fallSpeed = -0.4f;

    [Header("DEBUG")]
    //[Tooltip("Example")]
    //[SerializeField]

    [Tooltip("How fast the player is moving down.")]
    public Vector3 velocityDown;

    public Vector3 direction;                                  // the initial direction the player has input
    private Vector3 moveDirection;                              // the direction relative to the camera the player will move, calculated with fancy maths
    private Vector2 currentMovementInput;                       // the forward/back or left/right direction input by player
    private float targetAngle;                                  // the current angle of the camera for fancy maths
    private float fallingMomentum;                              // keeps track of the player's y position relative to the previous update

    private bool doubleJump;                                    // true when player has used their double jump (so the function can only be used when this boolean is false)
    private bool powerJumping = true;                           // true when player is currently power jumping
    private bool springJump;                                    // true when player will spring jump instead of normal jump (so they have JUST landed power jump)
    private bool springStop;                                    // true when player is currently spring jumping (to prevent double jumping from a spring jump)
    private bool jumpPredict;                                   // true when player has pressed jump button just before landing, allowing them to jump (but not spam button)
    private bool powerPredict;                                  // true when player has pressed power jump button just before landing, allowing them to power jump (but not spam button)
    private bool powerAllow = true;                             // true when power jump is available to player (must be restricted or player will bounce everywhere)
    private bool isGrounded;                                    // true when the player is touching the ground
    //private bool isMovementPressed;                             // true when the player is pressing movement
    private bool isJumpPressed = false;                         // true when the player is pressing jump
    private bool isRunPressed;                                  // true when the player is pressing run

    // these 3 lines of code are to fix the issue of the game reading 'isGrounded' as true for an update or 2 after leaving the ground. They override it. 
    private int groundCount = 2;
    private int gCount = 0;
    private bool groundOverride;

    public float turnSmoothTime = 0.1f;                         // turn camera smooth
    private float turnSmoothVelocity;                           // more smoothy stuff I dunno

    public Transform groundCheck;                               // the empty object at the bottom of the player which assesses if the player is grounded
    public float groundDistance;                                // the size of the empty object at the bottom of the player
    public LayerMask groundMask;

    // Start is called before the first frame update

    private void Awake()
    {
        // intial setting of reference variables
        playerInput = new Player_Controller();                      // access to Player_Controller Player Input system.
        CC = GetComponent<CharacterController>();                   //access to character controller.
        cam = Camera.main.transform;

        //player input callback setters
        //playerInput (Input System) -> Player (Action Map) -> Action (eg, Jump) -> Listen (started, cancelled, etc) -> Callback Function (eg, OnJump)
        playerInput.Player.Move.started += OnMovementInput;         //When button is first pressed out callback is run
        playerInput.Player.Move.canceled += OnMovementCancel;        //When all input has been stopped.
        playerInput.Player.Move.performed += OnMovementInput;       //When completed or updated callback is run. Movement is updated constantly so this is needed
        playerInput.Player.Look.started += OnCameraInput;
        playerInput.Player.Look.performed += OnCameraInput;
        //playerInput.Player.Look.canceled += OnMovementCancel;
        playerInput.Player.Run.started += OnRun;                    //
        playerInput.Player.Run.canceled += OnRun;                   //  These are buttons so they
        playerInput.Player.Jump.started += OnJump;                  // when player presses jump
        playerInput.Player.Jump.canceled += OffJump;                // when player stops pressing jump
        playerInput.Player.Dash.started += OnPowerJump;
        playerInput.Player.Dash.canceled += OffPowerJump;

        fallingMomentum = transform.position.y;                     // sets the initial value to judge when the player is falling

    }

    // Update is called once per frame
    void FixedUpdate()
    {

        // if the player is falling relative to the previous update, they will gain momentum based on the fallSpeed variable.
        if (transform.position.y < fallingMomentum)
        {
            velocityDown.y += fallSpeed;
            if (velocityDown.y > 50)
            {
                velocityDown.y = 50;                // not working?
            }
        }

        fallingMomentum = transform.position.y;

        // isGrounded override. If the player has just jumped, the groundOverride will be on and so functions which require being grounded won't work.
        if (gCount > 0)
        {
            groundOverride = true;
            gCount--;
        }
        else
        {
            groundOverride = false;
        }

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask); // determine if player is grounded

        if (isGrounded && !groundOverride && velocityDown.y < 0)
        {
            velocityDown.y = -2f;                       // Ensures player sticks to ground
        }

        //gravity
        velocityDown.y += playerGravity * Time.deltaTime;
        CC.Move(velocityDown * Time.deltaTime);

        if (powerJumping)  // If player is power jumping they will continue to move in that direction without control
        {
            CC.Move(moveDirection.normalized * (walkSpeedMultiplier * powerJumpDistance) * Time.deltaTime);
        }
        else if (direction.magnitude >= 0.1f) // Otherwise player determines direction
        {
            if (isRunPressed)
            {
                CC.Move(moveDirection.normalized * runSpeedMultiplier * Time.deltaTime);
            }
            else
            {
                CC.Move(moveDirection.normalized * walkSpeedMultiplier * Time.deltaTime);
            }
        }

        if (isGrounded && !groundOverride)                      // while the player is on the ground
        {
            //Debug.Log("Player grounded");

            SpringJumpCheck();                                  // checks if player has just landed power jump
            PredictiveJumpCheck();                              // checks if player has pressed jump button slightly too early, and if so, will still jump
            doubleJump = false;                                 // resets double jump permission

            // if we want to separate ground and air speed, ground speed here
        }
        else                                                    // while the player is in the air
        {
            //Debug.Log("Player airtime");

            // if we want to separate ground and air speed, air speed here
        }
    }

    void updateMovement()
    {
        targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
        transform.rotation = Quaternion.Euler(0f, angle, 0f);

        moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;   // moveDirection updated here
    }

    void OnCameraInput(InputAction.CallbackContext context)
    {
        if (!powerJumping)                                                  // movement direction cannot be changed during power jump
        {
            if (direction.magnitude >= 0.1f)
            {
                updateMovement();
            }
            else                                                                    // if the player isn't inputting movement, don't turn the character
            {
                targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);

                moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            }
        }
        else 
        {
            //targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            //float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            //transform.rotation = Quaternion.Euler(0f, angle, 0f);
        }
    }

    void OnMovementInput(InputAction.CallbackContext context)
    {
        if (!powerJumping)                                                  // movement direction cannot be changed during power jump
        {
            currentMovementInput = context.ReadValue<Vector2>();
            direction = new Vector3(currentMovementInput.x, 0f, currentMovementInput.y).normalized;
            updateMovement();
        }
    }

    void OnMovementCancel(InputAction.CallbackContext context)
    {
        if (!powerJumping)
        {
            moveDirection = Vector3.zero;
            direction = Vector3.zero;
        }
    }

    void OnRun(InputAction.CallbackContext context)
    {
        isRunPressed = context.ReadValueAsButton();
    }

    void OnJump(InputAction.CallbackContext context)
    {
        isJumpPressed = context.ReadValueAsButton();
        Jump();
    }

    void OffJump(InputAction.CallbackContext context)
    {
        if (context.ReadValueAsButton())
        {
            isJumpPressed = false;
        }
    }

    void OnPowerJump(InputAction.CallbackContext context)
    {
        //isPowerJumpPressed = context.ReadValueAsButton();
        PowerJump();
    }

    void OffPowerJump(InputAction.CallbackContext context)
    {
        if (context.ReadValueAsButton())
        {
            //isPowerJumpPressed = false;
        }
    }

    private void OnEnable()
    {
        playerInput.Player.Enable();
    }
    private void OnDisable()
    {
        playerInput.Player.Disable();
    }

    private void Jump()
    {
        if (isGrounded && !groundOverride)
        {
            if (springJump == true)                             // when grounded priority 1: spring jumps if it's available
            {
                velocityDown.y = Mathf.Sqrt(springJumpHeight * -2 * playerGravity);
                gCount = groundCount;
            }
            else                                                // when grounded priority 2: normal jump
            {
                velocityDown.y = Mathf.Sqrt(jumpHeight * -2 * playerGravity);
                gCount = groundCount;
            }
        }
        else if (doubleJump == false && powerJumping == false && springStop == false)       // when in air priority 1: double jump. Note: can't double jump when powerjumping or springjumping
        {
            velocityDown.y = Mathf.Sqrt(doubleJumpHeight * -2 * playerGravity);
            gCount = groundCount;
            doubleJump = true;
        }
        else                                                    // when in air priority 2: turns on predictive jump. Will jump if the player lands in the next few frames (controlled by predictiveJumpWindow)
        {
            jumpPredict = true;
            StartCoroutine(PredictiveJump());
        }
    }

    private void PredictiveJumpCheck()                          // checks if player pressed the jump button slightly before landing
    {
        if (jumpPredict == true)
        {
            Debug.Log("You used the predictive jump!");
            Jump();
        }

        if (powerPredict == true && powerAllow == true)
        {
            Debug.Log("You used the predictive power jump!");
            PowerJump();
        }
    }

    IEnumerator PredictiveJump()                                // timer for player being able to jump if they press button slightly too early
    {
        yield return new WaitForSeconds(predictiveJumpWindow);  // very short window before landing where if player pressed button, jumping will still occur
        jumpPredict = false;                                    // after very short window of time, the predictive jump will turn off
        powerPredict = false;                                   // after very short window of time, the predictive power jump will turn off
    }

    private void PowerJump()
    {
        if (!powerAllow)
        {
            Debug.Log("You can't power jump too often.");
        }
        else if (isGrounded && !groundOverride)
        {
            if (direction.magnitude >= 0.1f)   // jumps only if player is moving
            {
                velocityDown.y = Mathf.Sqrt(jumpHeight * -2 * playerGravity);
                gCount = groundCount;

                powerJumping = true;                                // player is currently power jumping
                powerAllow = false;                                 // player cannot power jump again for certain window of time (determined by dashRegularity)
                StartCoroutine(PowerAvailable());
            }
        }
        else                                                    // when in air priority 2: turns on predictive jump. Will jump if the player lands in the next few frames (controlled by predictiveJumpWindow)
        {
            powerPredict = true;
            StartCoroutine(PredictiveJump());
        }
    }

    private void SpringJumpCheck()                              // checks if player has just landed power jump
    {
        if (powerJumping == true)
        {
            powerJumping = false;                               // resets power jump permission
            springJump = true;                                  // spring jump is now available
            StartCoroutine(SpringAvailable());
        }

        if (springJump == false)
        {
            springStop = false;                                 // allows you to use double jump only once spring jump has landed
        }
    }

    IEnumerator SpringAvailable()                               // waits certain amount of time before spring stops being available
    {
        yield return new WaitForSeconds(springJumpWindow);      // amount of time in seconds before spring jump is no longer available
        springJump = false;                                     // spring jump now unavailable
        springStop = true;                                      // sorry this is messy I wasn't sure how else to do it. This ensures you can't double jump from a spring jump (you must land first).
    }

    IEnumerator PowerAvailable()                                // waits certain amount of time before power jump is available again
    {
        yield return new WaitForSeconds(powerJumpRegularity);   // amount of time in seconds before power jump is available again
        powerAllow = true;                                      // power jump now available again
    }
}