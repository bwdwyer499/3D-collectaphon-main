using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempSpinningBlades : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.Rotate(0, 40, 0);

    }

}
