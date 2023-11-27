using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Audio;
using Cinemachine;

public class CombinePlayerController : MonoBehaviour
{
    //Reference Variables
    //TO DO - Animator
    public Animator playerAnimator;
    

    int isRunningHash;

    Player_Controller playerInput;              //Player input system 
    CharacterController characterController;    //Players character controller component
    Transform cameraMainTransform;

    public CinemachineFreeLook cam;
    public GameObject autoCameraText;
    public GameObject journalUI;
    //public AudioClip audioClip;
    //public AudioSource source;

    [Header("MOVEMENT CONSTRAINTS")]
    [Tooltip("How fast the player walks on the ground.")]
    public float walkSpeedMultiplier = 10.0f;
    [Tooltip("How fast the player runs on the ground.")]
    public float runSpeedMultiplier = 15.0f;

    [Header("JUMPING CONSTRAINTS")]
    [Tooltip("The maximum height of jumps - cannot be changed during play.")]
    public float maxJumpHeight = 3f;
    [Tooltip("The speed jumps will take to complete - cannot be changed during play.")]
    public float maxJumpTime = 1f;
    [Tooltip("How high the second jump is relative to the first - cannot be changed during play.")]
    public float secondJumpHeight = 1.05f;
    [Tooltip("How high the third jump is relative to the first - cannot be changed during play.")]
    public float thirdJumpHeight = 1.09f;
    [Tooltip("How many seconds pass before the player must jump again to trigger the second/third jump.")]
    public float timeBetweenJumps = 0.15f;
    [Tooltip("How high the spin jump (double jump) goes.")]
    public float spinJumpHeight = 0.9f;
    [Tooltip("How high the dash goes.")]
    public float dashHeight = 0.7f;
    [Tooltip("How far the dash goes.")]
    public float dashDistance = 40f;
    [Tooltip("How high the player spring jumps.")]
    public float springJumpHeight = 0.95f;
    [Tooltip("The window (in seconds) the spring jump is available for after a power jump.")]
    public float springJumpWindow = 0.15f;
    [Tooltip("The window (in seconds) the player will jump even if they press the button slightly too early.")]
    public float predictiveJumpWindow = 0.1f;
    [Tooltip("How often (in seconds) the player can dash. If at 0 the player can bounce everywhere.")]
    public float dashRegularity = 0.6f;
    [Tooltip("How long (in seconds) before the player starts falling after leaving a platform.")]
    public float cayoteSeconds = 0.15f;
    [Tooltip("How fast the character turns when receiving input. This is being screwy on keyboard but not on gamepad. Should be 0.1.")]
    public float turnSmoothTime = 0.001f;
    // intial jump speed calculated via (2 * maxJumpHeight) / timeToApex;
    private float initialJumpVelocity;

    private float dashReduce = 0.0001f;

    [Header("DEBUG")]
    // Variables to store player input values
    public Vector2 currentMovementInput;                // current input from player
    public Vector3 currentMovement;                     // Get initial direction on x and x axis
    public Vector3 moveDirection;                       // Final vector3 for x and z axis movemennt
    public Vector3 jumpMovement;                        // Used for calculating y axis
    public Vector3 appliedMovement;                     // Controls y axis player movement through averaging of two positions

    private Vector2 journalTarget;                      // the position the journal will reach once it finishes moving

    //Camera Variables
    private float turnSmoothVelocity;                           // smoothy camera mathsy stuff
    private float targetAngle;                                  // the current angle of the camera for fancy maths 

    //Gravity Variables
    private float groundedGrav = -0.5f;
    private float gravity = -9.8f;

    public bool hazardHit = false;                              // true when the player has just hit a hazard, suspends gravity
    private bool groundOverride = false;                        // true when the player has just left the ground, for cayote time
    private bool onlyOneOverride = false;

    //Booleans
    private bool doubleJump;                                    // true when player has used their double jump (so the function can only be used when this boolean is false)
    private bool dashing = true;                           // true when player is currently power jumping
    private bool springJump;                                    // true when player will spring jump instead of normal jump (so they have JUST landed power jump)
    private bool springing;                                     // true when player is currently spring jumping
    private bool jumpPredict;                                   // true when player has pressed jump button just before landing, allowing them to jump (but not spam button)
    private bool dashPredict;                                  // true when player has pressed power jump button just before landing, allowing them to power jump (but not spam button)
    private bool dashAllow = true;                              // true when dash is available to player (must be restricted or player will bounce everywhere)
    private bool dashAllowOnceGrounded = false;                 // true when the dash will be made available again once the player is next grounded
    public bool playerImmunity = false;                         // true when the player has just been hit, won't allow more damage for a time. This is determined by EnemyControllers and then referenced in the CollisionController to see if the player should lose health.
    private bool isMovementPressed;                             // true if the player is moving with the left stick
    private bool isRunPressed;                                  // true if the player is running using the left bumper
    private bool isJumpPressed = false;                         // true if the player is jumping using the south button
    private bool isJumping = false;                             // true if the player is currently jumping
    private bool autoCameraSetting = true;                      // true if autocamera is on
    private bool journalSetting = false;                        // true is journal is on
    private bool journalMove = false;                           // true if the journal needs to move

    public int doubleJump360 = 0;                              // for the spin on the double jump
    private bool doubleJumpOverride = true;                     // true if the player stops spinning during their double jump. Hacky way to stop the rotation jamming after the spin, wasn't sure how else to do it

    private int jumpCount = 0;
    Dictionary<int, float> initialJumpVelocities = new Dictionary<int, float>();    //Dict to store initial jump velocity. Effected by jump combo
    Dictionary<int, float> jumpGravities = new Dictionary<int, float>();            //Dict to store jump gravity. Effected by jump combo
    Coroutine currentJumpResetRoutine = null;

    private void Awake()
    {
        // intial setting of reference variables
        playerInput = new Player_Controller();                      // access to Player_Controller Player Input system.
        characterController = GetComponent<CharacterController>();  //access to character controller.
        cameraMainTransform = Camera.main.transform;

        //player input callback setters
        //playerInput (Input System) -> Player (Action Map) -> Action (eg, Jump) -> Listen (started, cancelled, etc) -> Callback Function (eg, OnJump)
        playerInput.Player.Move.started += OnMovementInput;         //When button is first pressed out callback is run
        playerInput.Player.Move.canceled += OnMovementCancel;        //When all input has been stopped.
        playerInput.Player.Move.performed += OnMovementInput;       //When completed or updated callback is run. Movement is updated constantly so this is needed
        playerInput.Player.Look.started += OnCameraInput;
        playerInput.Player.Look.performed += OnCameraInput;
        playerInput.Player.Run.started += OnRun;                    //
        playerInput.Player.Run.canceled += OnRun;                   //  These are buttons so they
        playerInput.Player.Jump.started += OnJump;                  //  only need started and canceled
        playerInput.Player.Jump.canceled += OffJump;
        playerInput.Player.Dash.started += OnPowerJump;
        playerInput.Player.Dash.canceled += OffPowerJump;
        playerInput.Player.AutoCamera.started += OnOffAutoCamera;
        playerInput.Player.Journal.started += OnOffJournal;

        ////Store the values of the current movement if it is performed and convert to a bool for if the player is moving.
        ////The most round about fucking way to store this shit I swear but this is what I have found to use the new input system
        ////There must be an easier way.
        //playerInput.Player.Move.performed += ctx =>
        //{
        //    currentMovementInput = ctx.ReadValue<Vector2>();
        //    isMovementPressed = currentMovementInput.x != 0 || currentMovementInput.y != 0;
        //};

        setJumpVariables();
    }

    void Start()
    {
        //set anim ref
        //playerAnimator.GetComponent<Animator>(); //will find animator componet the object this script is attached to.

        //set bool transition IDs
        //isIdleHash = Animator.StringToHash("IsIdle");
        isRunningHash = Animator.StringToHash("IsRunning");
    }
    void FixedUpdate()
    {
        SpinningDoubleJump();
        DashReduce();

        if (journalMove)
        {
            journalUI.transform.position = Vector2.MoveTowards(journalUI.transform.position, journalTarget, 2000 * Time.fixedDeltaTime);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (PauseManager.paused) return;

        if (hazardHit == false)     // Gravity is suspended if the Player hits a hazard
        {
            if (dashing)  // If player is power jumping they will continue to move in that direction without control
            {
                characterController.Move(moveDirection.normalized * dashDistance * Time.fixedDeltaTime);
            }
            else if (isMovementPressed)     // How the player actually moves via the character controller - x and z axis
            {
                if (isRunPressed)
                {
                    characterController.Move(moveDirection.normalized * runSpeedMultiplier * Time.fixedDeltaTime);       // Running
                    playerAnimator.SetBool(isRunningHash, true); //Set animation to true
                    playerAnimator.speed = 1.5f;
                }
                else
                {
                    characterController.Move(moveDirection.normalized * walkSpeedMultiplier * Time.fixedDeltaTime);      // Walking            
                    playerAnimator.SetBool(isRunningHash, true); //Set animation to true
                    playerAnimator.speed = 1f;
                }
                //idle animation.
            }
            else if (!isMovementPressed)
            {
                playerAnimator.SetBool(isRunningHash, false); //Set is running to false and restart the idle animation
            }

            characterController.Move(appliedMovement * Time.fixedDeltaTime);     // All y axis movement

            if (characterController.isGrounded)
            {
                if (dashAllowOnceGrounded)
                {
                    dashAllow = true;
                    dashAllowOnceGrounded = false;
                }

                onlyOneOverride = false;

                SpringJumpCheck();                                  // Checks if player has just landed power jump
                doubleJump = false;                                 // Resets double jump permission
            }

            if (!groundOverride)
            {
                HandleGravity();
            }

            HandleJump();

            if (!characterController.isGrounded && !isJumping && !onlyOneOverride)
            {
                StartCoroutine(CayoteTime());
            }
        }
        else
        {
            dashing = false;   // So you don't get stuck in a powerjumping loop if you hit a hazard
        }
    }

    IEnumerator CayoteTime()                    // Waits a certain amount of time between the enemy being allowed to attack
    {
        groundOverride = true;
        onlyOneOverride = true;
        yield return new WaitForSeconds(cayoteSeconds);
        groundOverride = false; //reset the jump counter if the player takes too long to jump
    }

    void setJumpVariables()
    {
        float timeToApex = maxJumpTime / 2;
        gravity = (-2 * maxJumpHeight) / Mathf.Pow(timeToApex, 2);
        initialJumpVelocity = (2 * maxJumpHeight) / timeToApex;
        float secondJumpGravity = (-2 * (maxJumpHeight + 1)) / Mathf.Pow((timeToApex * secondJumpHeight), 2);
        float secondJumpInitialVelocity = (2 * (maxJumpHeight + 1)) / timeToApex * secondJumpHeight;
        float thirdJumpGravity = (-2 * (maxJumpHeight + 2)) / Mathf.Pow((timeToApex * thirdJumpHeight), 2);
        float thirdJumpInitialVelocity = (2 * (maxJumpHeight + 2)) / timeToApex * thirdJumpHeight;
        float doubleJumpGravity = (2 * maxJumpHeight) / Mathf.Pow((timeToApex * spinJumpHeight), 2);
        float doubleJumpInitialVelocity = (2 * maxJumpHeight) / timeToApex * spinJumpHeight;
        float springJumpGravity = (-2 * (maxJumpHeight + 2)) / Mathf.Pow((timeToApex * springJumpHeight), 2);
        float springJumpInitialVelocity = (2 * (maxJumpHeight + 2)) / timeToApex * springJumpHeight;
        float dashGravity = (2 * maxJumpHeight) / Mathf.Pow((timeToApex * dashHeight), 2);
        float dashInitialVelocity = (2 * maxJumpHeight) / timeToApex * dashHeight;


        //add to the dictonary
        initialJumpVelocities.Add(1, initialJumpVelocity);
        initialJumpVelocities.Add(2, secondJumpInitialVelocity);
        initialJumpVelocities.Add(3, thirdJumpInitialVelocity);
        initialJumpVelocities.Add(4, doubleJumpInitialVelocity);
        initialJumpVelocities.Add(5, springJumpInitialVelocity);
        initialJumpVelocities.Add(6, dashInitialVelocity);

        jumpGravities.Add(0, gravity); //for when jumping count resets
        jumpGravities.Add(1, gravity);
        jumpGravities.Add(2, secondJumpGravity);
        jumpGravities.Add(3, thirdJumpGravity);
        jumpGravities.Add(4, doubleJumpGravity);
        jumpGravities.Add(5, springJumpGravity);
        jumpGravities.Add(6, dashGravity);
    }

    void HandleJump()
    {
        //you have not jumped yet
        if (!isJumping && (characterController.isGrounded || groundOverride) && (isJumpPressed || jumpPredict))
        {
            if (springJump == true)                                                    // spring jump
            {
                jumpPredict = false;
                isJumping = true;
                springing = true;
                jumpMovement.y = initialJumpVelocities[5];
                appliedMovement.y = initialJumpVelocities[5];
                if (playerAnimator.GetBool("NotSpinning") == true)
                {
                    playerAnimator.SetBool("IsSpinJumping", true); //animation bool
                    StartCoroutine(DelayOnSpinBool());
                }
            }
            else
            {
                if (jumpCount < 3 && StartCoroutine("JumpResetRoutine") != null)                  // if the jump count gets too high, reset it
                //if (jumpCount < 3)
                {
                    StopCoroutine("JumpResetRoutine");
                    //Debug.Log("Coroutine Stopped");
                }
                jumpPredict = false;
                isJumping = true;
                jumpCount += 1;
                //Debug.Log("Jump Count " + jumpCount);
                jumpMovement.y = initialJumpVelocities[jumpCount];                  // normal jump routine
                appliedMovement.y = initialJumpVelocities[jumpCount];
                if (jumpCount == 1)
                {
                    playerAnimator.SetBool("IsJumping", true);                  
                    //source.Play();
                }
                else if (jumpCount == 2)
                {
                    playerAnimator.SetBool("IsJumping2", true);
                }
                else if (jumpCount == 3)
                {
                    if (playerAnimator.GetBool("NotSpinning") == true)
                    {
                        playerAnimator.SetBool("IsSpinJumping", true); //animation bool
                        StartCoroutine(DelayOnSpinBool());
                    }
                }
            }
        }
        else if (!isJumpPressed && isJumping && characterController.isGrounded)     // reset permission to jump once you've landed
        {
            isJumping = false;
        }
    }

    void HandleGravity()
    {
        bool isFalling = jumpMovement.y <= 0.0f || !isJumpPressed; // if the player is falling or if the jump button is not pressed
        float fallMultiplier = 3.0f;

        //apply the proper gravity to the player if they are grounded or in the air
        if (characterController.isGrounded)
        {
            springing = false;
            playerAnimator.SetBool("NotSpinning", true);
            playerAnimator.SetBool("IsJumping", false);
            playerAnimator.SetBool("IsJumping2", false);
            playerAnimator.SetBool("IsJumping3", false);
            playerAnimator.SetBool("IsSpinJumping", false);

            //start the jump combo coroutine
            StartCoroutine("JumpResetRoutine");
            if (jumpCount == 3)
            {
                jumpCount = 0;
            }
            //Debug.Log("Is player grounded "+characterController.isGrounded);
            jumpMovement.y = groundedGrav;
            appliedMovement.y = groundedGrav;
        }

        //apply additional gravity after the player reaches the apex of the jump
        else if (isFalling)
        {

            float previousYVelocity = jumpMovement.y;
            jumpMovement.y = jumpMovement.y + (jumpGravities[jumpCount] * fallMultiplier * Time.fixedDeltaTime);
            appliedMovement.y = Mathf.Max((previousYVelocity + jumpMovement.y) * 0.5f, -20.0f); // Mathf.max - Cap the max fall

        }
        //Velocity Verlet - Jumps will be the same regardless of the framerate. 
        //Uses the average of two points to adjust the player characters trajectory.
        else
        {
            //Velocity Verlet - Jumps will be the same regardless of the framerate.
            float previousYVelocity = jumpMovement.y;
            jumpMovement.y = jumpMovement.y + (jumpGravities[jumpCount] * Time.fixedDeltaTime);
            appliedMovement.y = (previousYVelocity + jumpMovement.y) * 0.5f;

        }
    }

    void updateMovement()
    {
        targetAngle = Mathf.Atan2(currentMovement.x, currentMovement.z) * Mathf.Rad2Deg + cameraMainTransform.eulerAngles.y;
        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
        if (doubleJumpOverride == true)
        {
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
        }

        moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;   // moveDirection updated here
    }

    void OnMovementInput(InputAction.CallbackContext context)
    {
        if (!dashing)                                                  // movement direction cannot be changed during power jump
        {
            currentMovementInput = context.ReadValue<Vector2>();
            currentMovement = new Vector3(currentMovementInput.x, 0f, currentMovementInput.y).normalized;

            updateMovement();
        }

        isMovementPressed = currentMovementInput.x != 0 || currentMovementInput.y != 0;
    }

    void OnMovementCancel(InputAction.CallbackContext context)
    {
        if (!dashing)
        {
            moveDirection = Vector3.zero;
            currentMovement = Vector3.zero;
            isMovementPressed = false;
        }
    }

    void OnCameraInput(InputAction.CallbackContext context)
    {
        if (!dashing)                                                          // movement direction cannot be changed during power jump
        {
            if (isMovementPressed)
            {
                updateMovement();
            }
            else                                                                    // if the player isn't inputting movement, don't turn the character
            {
                targetAngle = Mathf.Atan2(currentMovement.x, currentMovement.z) * Mathf.Rad2Deg + cameraMainTransform.eulerAngles.y;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);

                moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            }
        }
        //else
        //{       // trying to get the player to be able to turn in midair without moving during power jump
        //targetAngle = Mathf.Atan2(currentMovement.x, currentMovement.z) * Mathf.Rad2Deg + cameraMainTransform.eulerAngles.y;
        //float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
        //transform.rotation = Quaternion.Euler(0f, angle, 0f);
        //}
    }

    void SpinningDoubleJump()
    {
        if (doubleJump == true)
        {
            if (playerAnimator.GetBool("NotSpinning") == true)
            {
                playerAnimator.SetBool("IsSpinJumping", true); //animation bool
                StartCoroutine(DelayOnSpinBool());
            }

            if (springing || jumpCount == 3)            // front sault if double jumping after a spring jump or a third boost jump
            {
                if (doubleJump360 < 719)
                {
                    transform.Rotate(36, 0, 0);
                    doubleJump360 += 36;
                }
                else
                {
                    doubleJumpOverride = true;
                }
            }
            else if (doubleJump360 < 719)               // spin jump if double jumping at any other time
            {
                transform.Rotate(0, 36, 0);
                doubleJump360 += 36;
            }
            else
            {
                doubleJumpOverride = true;
            }
        }
        else
        {
            //playerAnimator.SetBool("IsSpinJumping", false); //animation bool
            doubleJump360 = 0;
            doubleJumpOverride = true;              // This is crucial since if the player doesn't finish the 720 before landing the override won't change and rotation will be stuck.
        }
    }

    void OnJump(InputAction.CallbackContext context)
    {
        isJumpPressed = context.ReadValueAsButton();

        if (doubleJump || dashing == true)
        {
            jumpPredict = true;
            StartCoroutine(PredictiveJump());
        }

        DoubleJump();       // call doubleJump. Must be in OnJump so it only calls once. Must be after the previous if statement or that will always be called.
    }

    void OffJump(InputAction.CallbackContext context)           // This is necessary since otherwise unclicking the Jump button calls OnJump again
    {
        isJumpPressed = context.ReadValueAsButton();
    }

    void OnRun(InputAction.CallbackContext context)
    {
        isRunPressed = context.ReadValueAsButton();
        //Debug.Log("is run pressed: " + isRunPressed);
    }

    void OnPowerJump(InputAction.CallbackContext context)
    {
        Dash();
    }

    void OffPowerJump(InputAction.CallbackContext context)
    {
        // this fixes it for some reason
    }

    private void DoubleJump()
    {
        if (groundOverride == false && !characterController.isGrounded && doubleJump == false) //if you're in air and jump, double jump
        {
            isJumping = true;
            jumpMovement.y = initialJumpVelocities[4];
            appliedMovement.y = initialJumpVelocities[4];
            doubleJump = true;
            doubleJumpOverride = false;
        }
    }

    private void Dash()
    {

        dashDistance = 40f;
        dashReduce = 0.0001f;

        if (dashAllow)
        {
            if (isMovementPressed)   // jumps only if player is moving
            {
                jumpPredict = false;
                isJumping = true;
                jumpMovement.y = initialJumpVelocities[6];                  // dash jump height
                appliedMovement.y = initialJumpVelocities[6];

                dashing = true;                                // player is currently dashing
                dashAllow = false;                                 // player cannot dash again for certain window of time (determined by dashRegularity)
                StartCoroutine(PowerAvailable());
            }
        }
        else                                                    // when in air priority 2: turns on predictive jump. Will jump if the player lands in the next few frames (controlled by predictiveJumpWindow)
        {
            dashPredict = true;
            StartCoroutine(PredictiveJump());
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
    // Coroutine. Handles jump timer for multiple jump combos.
    // Resets jumpCount if they take too long between jumps
    IEnumerator JumpResetRoutine()
    {
        yield return new WaitForSeconds(timeBetweenJumps);
        jumpCount = 0; //reset the jump counter if the player takes too long to jump
    }

    IEnumerator PredictiveJump()                                // timer for player being able to jump if they press button slightly too early
    {
        yield return new WaitForSeconds(predictiveJumpWindow);  // very short window before landing where if player pressed button, jumping will still occur
        jumpPredict = false;                                    // after very short window of time, the predictive jump will turn off
        dashPredict = false;                                   // after very short window of time, the predictive power jump will turn off
    }

    private void SpringJumpCheck()                              // checks if player has just landed power jump
    {
        if (dashing == true)
        {
            dashing = false;                                    // resets power jump permission
            springJump = true;                                  // spring jump is now available
            StartCoroutine(SpringAvailable());
        }
    }

    IEnumerator SpringAvailable()                               // waits certain amount of time before spring stops being available
    {
        yield return new WaitForSeconds(springJumpWindow);      // amount of time in seconds before spring jump is no longer available
        springJump = false;                                     // spring jump now unavailable
    }

    IEnumerator PowerAvailable()                                // waits certain amount of time before power jump is available again
    {
        yield return new WaitForSeconds(dashRegularity);   // amount of time in seconds before power jump is available again
        dashAllowOnceGrounded = true;                                      // power jump now available again
    }

    void OnOffAutoCamera(InputAction.CallbackContext context)
    {
        if (autoCameraSetting)
        {
            cam.GetComponent<CinemachineFreeLook>().m_YAxisRecentering.m_enabled = false;
            cam.GetComponent<CinemachineFreeLook>().m_RecenterToTargetHeading.m_enabled = false;
            autoCameraSetting = false;
            StopCoroutine("ACText");
            StartCoroutine(ACText(0, 1, 2, 3));
        }
        else
        {
            cam.GetComponent<CinemachineFreeLook>().m_YAxisRecentering.m_enabled = true;
            cam.GetComponent<CinemachineFreeLook>().m_RecenterToTargetHeading.m_enabled = true;
            autoCameraSetting = true;
            StopCoroutine("ACText");
            StartCoroutine(ACText(2, 3, 0, 1));
        }
    }

    IEnumerator ACText(int txt, int img, int oldtxt, int oldimg)            // turns autocamera text/image on for a short time
    {
        autoCameraText.transform.GetChild(oldtxt).gameObject.SetActive(false);
        autoCameraText.transform.GetChild(oldimg).gameObject.SetActive(false);
        autoCameraText.transform.GetChild(txt).gameObject.SetActive(true);
        autoCameraText.transform.GetChild(img).gameObject.SetActive(true);
        yield return new WaitForSeconds(1);
        autoCameraText.transform.GetChild(txt).gameObject.SetActive(false);
        autoCameraText.transform.GetChild(img).gameObject.SetActive(false);
    }

    void OnOffJournal(InputAction.CallbackContext context)
    {
        if (journalSetting)
        {
            journalTarget = new Vector2(-403.5f, journalUI.transform.position.y);

            journalMove = true;
            journalSetting = false;
        }
        else
        {
            journalTarget = new Vector2(196.5f, journalUI.transform.position.y);

            journalMove = true;
            journalSetting = true;
        }
    }

    void DashReduce()
    {
        if (dashDistance > 0)       // reduces dash over time, otherwise you will keep flying forward
        {
            dashDistance = dashDistance - dashReduce;
            dashReduce = dashReduce + dashReduce;
        }
        else
        {
            dashing = false;
        }
    }

    IEnumerator DelayOnSpinBool()            // just a very small timer so code won't overwrite something it's just done
    {
        yield return new WaitForSeconds(0.3f);
        playerAnimator.SetBool("NotSpinning", false);
    }
}