using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileScript : MonoBehaviour
{
    [SerializeField]
    private float lifeTime = 3.0f;

    private void Awake()
    {
        Destroy(gameObject, lifeTime);  //destroy game object (projectile) after lifetime;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("EnemyDamagePoint"))
        {
            Destroy(collision.gameObject.transform.parent.gameObject);  //destroy what it hits
            Destroy(gameObject);
        }

        if (collision.gameObject.CompareTag("Enemy"))
        {
            Destroy(collision.gameObject);  //destroy what it hits
            Destroy(gameObject);
        }

        //if (!collision.gameObject.CompareTag("Enemy")) return;
        //Destroy(collision.gameObject);  //destroy what it hits
        //Destroy(gameObject);            //destroy game object (projectile)
    }
}
