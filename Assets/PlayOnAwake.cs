using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayOnAwake : MonoBehaviour
{
    public Animation animation;
    // Start is called before the first frame update

    private void Awake()
    {
        animation.Play();
    }
}
