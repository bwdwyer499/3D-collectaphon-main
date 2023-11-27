using UnityEngine;
using System.Collections;
using System;

public class FireSpawnerScript : MonoBehaviour
{
    [Header("SETTINGS")]
    public int spawnTime = 5;
    private bool spawnPause = false;

    [Header("INPUT")]
    [SerializeField]
    private Transform bombSpawnPoint;
    [SerializeField]
    private GameObject bombPrefab;
    private GameObject instProjectile;

    void Start()
    {
        
    }

    void FixedUpdate()
    {
        if (spawnPause == false)
        {
            instProjectile = Instantiate(bombPrefab, bombSpawnPoint.position, bombSpawnPoint.rotation);
            instProjectile.GetComponent<Rigidbody>().velocity = bombSpawnPoint.forward * 10;
            spawnPause = true;

            StartCoroutine(SpawnRecharge());
        }
    }
    IEnumerator SpawnRecharge()
    {
        yield return new WaitForSeconds(spawnTime);  // very short window before landing where if player pressed button, jumping will still occur
        spawnPause = false;
    }
}