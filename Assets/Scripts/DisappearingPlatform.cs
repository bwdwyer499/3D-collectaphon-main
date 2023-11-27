using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisappearingPlatform : MonoBehaviour
{

    bool disappearOn = false;
    public float disappearTime = 1f;
    public float reappearTime = 4f;

    Renderer rend;
    Collider coll;
    Transform innerObject;

    //[SerializeField]
    //private GameObject platformPrefab;

    // Start is called before the first frame update
    void Start()
    {
        innerObject = gameObject.transform.GetChild(0);

        rend = innerObject.GetComponent<Renderer>();
        coll = innerObject.GetComponent<Collider>();
    }

    // Update is called once per frame
    void Update()
    {
        if (disappearOn == true)
        {
            disappearOn = false;
            rend.enabled = false;
            coll.enabled = false;
            //gameObject.SetActive(false);
            //Destroy(gameObject);            //destroy game object (projectile)
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            StartCoroutine(Disappear());
        }
    }
    IEnumerator Disappear()
    {
        yield return new WaitForSeconds(disappearTime);  // very short window before landing where if player pressed button, jumping will still occur
        disappearOn = true;
        StartCoroutine(Reappear());
    }
    IEnumerator Reappear()
    {
        yield return new WaitForSeconds(reappearTime);  // very short window before landing where if player pressed button, jumping will still occur
        rend.enabled = true;
        coll.enabled = true;
        //gameObject.SetActive(true);
        //Instantiate(platformPrefab, transform.position, transform.rotation);
    }
}
