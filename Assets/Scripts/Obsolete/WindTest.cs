using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindTest : MonoBehaviour
{

    public float windForce = 2f;
    Rigidbody windTestObject;
   


 // Start is called before the first frame update
    void Start()
    {
        windTestObject = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        windTestObject.AddForce(transform.right * windForce);
        
    }
}
