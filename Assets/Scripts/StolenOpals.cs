using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StolenOpals : MonoBehaviour
{
    private GameObject Player;

    int currentCheck = 0;

    // Start is called before the first frame update
    void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        if (Player.GetComponent<CollisionController>().currentOpals != currentCheck)
        {
            GameObject o1 = transform.GetChild(0).gameObject;
            GameObject o2 = transform.GetChild(1).gameObject;
            GameObject o3 = transform.GetChild(2).gameObject;
            GameObject o4 = transform.GetChild(3).gameObject;
            GameObject o5 = transform.GetChild(4).gameObject;
            GameObject o6 = transform.GetChild(5).gameObject;
            GameObject o7 = transform.GetChild(6).gameObject;
            GameObject o8 = transform.GetChild(7).gameObject;
            GameObject o9 = transform.GetChild(8).gameObject;
            GameObject o10 = transform.GetChild(9).gameObject;
            GameObject o11 = transform.GetChild(10).gameObject;
            GameObject o12 = transform.GetChild(11).gameObject;

            if (Player.GetComponent<CollisionController>().currentOpals == 1)
            {
                o1.SetActive(true);
                currentCheck = 1;
            }
            else if (Player.GetComponent<CollisionController>().currentOpals == 2)
            {
                o2.SetActive(true);
                currentCheck = 2;
            }
            else if (Player.GetComponent<CollisionController>().currentOpals == 3)
            {
                o3.SetActive(true);
                currentCheck = 3;
            }
            else if (Player.GetComponent<CollisionController>().currentOpals == 4)
            {
                o4.SetActive(true);
                currentCheck = 4;
            }
            else if (Player.GetComponent<CollisionController>().currentOpals == 5)
            {
                o5.SetActive(true);
                currentCheck = 5;
            }
            else if (Player.GetComponent<CollisionController>().currentOpals == 6)
            {
                o6.SetActive(true);
                currentCheck = 6;
            }
            else if (Player.GetComponent<CollisionController>().currentOpals == 7)
            {
                o7.SetActive(true);
                currentCheck = 7;
            }
            else if (Player.GetComponent<CollisionController>().currentOpals == 8)
            {
                o8.SetActive(true);
                currentCheck = 8;
            }
            else if (Player.GetComponent<CollisionController>().currentOpals == 9)
            {
                o9.SetActive(true);
                currentCheck = 9;
            }
            else if (Player.GetComponent<CollisionController>().currentOpals == 10)
            {
                o10.SetActive(true);
                currentCheck = 10;
            }
            else if (Player.GetComponent<CollisionController>().currentOpals == 11)
            {
                o11.SetActive(true);
                currentCheck = 11;
            }
            else if (Player.GetComponent<CollisionController>().currentOpals == 12)
            {
                o12.SetActive(true);
                currentCheck = 12;
            }
        }
    }
}
