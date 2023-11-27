using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirebombScript : MonoBehaviour
{
    [SerializeField]
    private GameObject enemyPrefab;
    [SerializeField]
    private GameObject startPointPrefab;

    GameObject objToSpawn;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Player")) // || collision.gameObject.CompareTag("Enemy") this last compare tag will cause lag if you keep spawning them in same spot
        {
            Destroy(gameObject);
            Instantiate(enemyPrefab, transform.position, transform.rotation);
        }
    }
}
