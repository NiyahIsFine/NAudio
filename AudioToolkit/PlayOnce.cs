using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayOnce : MonoBehaviour
{
    AudioSource Audio;
    private void Awake()
    {
        Audio = GetComponent<AudioSource>();
    }
    void Update()
    {
        if (!Audio.isPlaying)
            Destroy(gameObject);
    }
}
