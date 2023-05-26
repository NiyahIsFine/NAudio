using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioVolumeController : MonoBehaviour
{
    Audio _audio;
    AudioSource audioSource;
    public float minDistance, maxDistance;
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }
    public void Set(Audio audio, float minDistance, float maxDistance)
    {
        _audio = audio;
        this.minDistance = minDistance;
        this.maxDistance = maxDistance;
    }
    private void Update()
    {
        if (!Camera.main || !audioSource)
            return;
        float d = Vector2.Distance(Camera.main.transform.position, transform.position);
        float volume;
        if (d < minDistance)
        {
            volume = 1;
        }
        else if (d > maxDistance)
        {
            volume = 0;
        }
        else
        {
            volume = 1 - ((d - minDistance) / (maxDistance - minDistance));
        }
        audioSource.volume = _audio.volume * volume;
    }
}
