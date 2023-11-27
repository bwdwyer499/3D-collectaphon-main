using UnityEngine;
using System.Collections;

public class FollowPlayer : MonoBehaviour
{
    private GameObject Player;
    public float speed = 5f;
    public float followDistance = 5f;
    void Start()
    {
        Player = GameObject.Find("Koala");
    }

    void Update()
    {
        //Debug.Log("transform: " + transform.position + " & player: " + Player.transform.position);

        float distance = Vector3.Distance(Player.transform.position, transform.position);

       
            transform.position = Vector3.MoveTowards(transform.position, Player.transform.position, speed * Time.deltaTime);
        
    }
}



