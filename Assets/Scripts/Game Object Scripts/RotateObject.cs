using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateObject : MonoBehaviour
{
    public float rotateSpeed = 1;
    public bool constantRotation = true;
    
    // Update is called once per frame
    void Update()
    {
        if (constantRotation)
        {
            transform.Rotate(Vector3.up * rotateSpeed);
        }
    }
}
