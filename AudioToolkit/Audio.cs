using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Audio
{
    public Audio(string clipPath, Transform parent, bool play, int priority)
        => new Audio(AssetManager.GetResource<AudioClip>(clipPath), parent, play, priority);
    public Audio(AudioClip _clip, Transform parent, bool play, int priority)
    {
        clip = _clip;
        gameObject = new GameObject(clip.name, typeof(AudioSource));
        gameObject.transform.SetParent(parent);
        audioSource = gameObject.GetComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.priority = priority;

        lastUsedTime = Time.unscaledTime + clip.length;
        if (play)
            Play();
    }
    public GameObject gameObject;
    public AudioSource audioSource;
    public AudioClip clip;
    public string name => clip.name;
    public bool IsPlaying => audioSource.isPlaying;
    public float lastUsedTime;
    public Audio Play(bool replay = true)
    {
        if (audioSource != null)
        {
            if (!audioSource.isPlaying || replay)
            {
                lastUsedTime = Time.unscaledTime + clip.length;
                audioSource.Play();
            }
        }
        return this;
    }
    public Audio PlayOneShot()
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
        return this;
    }
    public Audio Stop()
    {
        if (audioSource != null)
        {
            lastUsedTime = Time.unscaledTime;
            audioSource.Stop();
        }
        return this;
    }
    public Audio SetPlayOnAwake(bool active)
    {
        if (audioSource != null)
            audioSource.playOnAwake = active;
        return this;
    }
    public Audio SetRollOff(float minDistance, float maxDistance)
    {
        if (audioSource != null)
        {
            gameObject.AddComponent<AudioVolumeController>().Set(minDistance, maxDistance);
        }
        return this;
    }
    public Audio RemoveRollOff(float volume = 1)
    {
        if (gameObject.TryGetComponent(out AudioVolumeController audioVolumeController))
        {
            Object.Destroy(audioVolumeController);
        }
        if (audioSource)
            audioSource.volume = volume;
        return this;
    }

    public void Destroy()
    {
        if (gameObject)
            Object.Destroy(gameObject);
    }
    ~Audio()
    {
        Destroy();
    }
    public static implicit operator bool(Audio audio)
    {
        return audio != null && audio.gameObject != null;
    }
}
