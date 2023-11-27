using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class FireProjectiles : MonoBehaviour
{
    Player_Controller playerInput;

    [SerializeField]
    private Transform projectileSpawnPoint;
    [SerializeField]
    private GameObject projectilePrefab;
    [SerializeField]
    private GameObject instProjectile;
    public float projectileSpeed = 10;

    public Slider waterSlider;

    [Tooltip("Total water charge or brake charge limit")]
    public float waterCharge = 100;
    [Tooltip("How long it takes for the water to start recharging without firing")]
    public float waterTimer = 0.5f;
    [Tooltip("The speed at which the water depletes")]
    public float waterChargeDepletion = 40;
    [Tooltip("The speed at which the water recharges")]
    public float waterChargeAddition = 40;

    private bool isFirePressed;              // true when firing water
    private bool rechargeWater = false;      // true when not firing, otherwise recharge will start when you're still firing
    private int justFired = 0;               // Determines when to start the recharge. Weird 3-way boolean, could be better coded by being triggered when the player STOPS pressing fire. Must only trigger ONCE when you stop firing. 0 = not firing, 1 = firing right now, 2 = just stopped firing
    private float currentWaterCharge;        // the current amount of charge left in the slider

    private void Awake()
    {
        playerInput = new Player_Controller();
        playerInput.Player.Fire.started += ProjectileShoot;
        playerInput.Player.Fire.canceled += ProjectileShoot;

        currentWaterCharge = waterCharge;
    }

    private void OnEnable()
    {
        playerInput.Player.Enable();
    }
    private void OnDisable()
    {
        playerInput.Player.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        RechargeWater();

        if (isFirePressed)
        {
            FireWater();
            justFired = 1;              // when firing, change to 1
        }
        else if (justFired == 1)        // if not firing and the value is 1 (meaning it's just been firing), change to 2
        {
            justFired = 2;
        }

        if (justFired == 2)             // if 2, start the recharge and change back to 0
        {
            SetWaterRecharge();
            justFired = 0;
        }
    }

    private void ProjectileShoot(InputAction.CallbackContext context)
    {
        isFirePressed = context.ReadValueAsButton();
    }

    private void FireWater()
    {
        if (currentWaterCharge > 0)
        {
            instProjectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, projectileSpawnPoint.rotation);
            instProjectile.GetComponent<Rigidbody>().velocity = projectileSpawnPoint.forward * projectileSpeed;

            rechargeWater = false;                                  //dont let it charge if you're firing
            currentWaterCharge = currentWaterCharge - (waterChargeDepletion * Time.deltaTime);
            if (currentWaterCharge < 0)
            {
                currentWaterCharge = 0;     //dont let it go below zero
            }
        }
    }

    private void RechargeWater()
    {
        if (rechargeWater && currentWaterCharge < waterCharge)
        {
            if (currentWaterCharge == 0) currentWaterCharge = 1;
            currentWaterCharge = currentWaterCharge + (waterChargeAddition * Time.deltaTime);
            if (currentWaterCharge > waterCharge) currentWaterCharge = waterCharge;     //dont let it go above maximum waterCharge
        }
        waterSlider.value = currentWaterCharge;
    }

    private void SetWaterRecharge()
    {
        StopCoroutine(TimerWaterRecharge(waterTimer));
        StartCoroutine(TimerWaterRecharge(waterTimer));
    }

    private IEnumerator TimerWaterRecharge(float time)
    {
        while (time > 0)
        {
            yield return new WaitForSeconds(1);
            time = time - 1;
        }
        rechargeWater = true;
    }
}
