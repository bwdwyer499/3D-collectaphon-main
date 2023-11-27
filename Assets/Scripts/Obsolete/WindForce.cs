using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindForce : MonoBehaviour
{
    public int windForce = 7;
    public CharacterController cc;

    private void Update()
    {
        cc.SimpleMove(speed: Vector3.right * windForce);
    }
}