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
        volume = 1;
        lastUsedTime = Time.unscaledTime + clip.length;
        if (play)
            Play();
    }
    public GameObject gameObject;
    public AudioSource audioSource;
    public AudioClip clip;
    public float volume;
    public string name => clip.name;
    public bool IsPlaying => audioSource.isPlaying;
    public float lastUsedTime;
    public float PlayingTime => audioSource.time;
    public AudioType audioType;
    public Audio Play(bool replay = true)
    {
        if (audioSource != null)
        {
            if (replay)
            {
                lastUsedTime = Time.unscaledTime + clip.length;
                audioSource.time = 0;
                audioSource.Play();
            }
            if (!audioSource.isPlaying)
            {
                lastUsedTime = Time.unscaledTime + clip.length - audioSource.time;
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
    public Audio Pause(bool active)
    {
        PauseP(active);
        return this;
    }
    void PauseP(bool active)
    {
        if (audioSource != null)
        {
            if (active)
                audioSource.Pause();
            else
            {
                audioSource.UnPause();
                lastUsedTime = Time.unscaledTime + clip.length - audioSource.time;
            }
        }
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
            gameObject.AddComponent<AudioRollOffVolume>().Set(this, minDistance, maxDistance);
        }
        return this;
    }
    public Audio RemoveRollOff(float volume = 1)
    {
        if (gameObject.TryGetComponent(out AudioRollOffVolume audioVolumeController))
        {
            Object.Destroy(audioVolumeController);
        }
        if (audioSource)
            audioSource.volume = volume;
        return this;
    }

    public Audio SetType(AudioType audioType)
    {
        if (this.audioType != AudioType.None && this.audioType != audioType)
        {
            switch (this.audioType)
            {
                case AudioType.Music:
                    AudioManager.onMusicVolumeChange -= OnVolumeChange;
                    break;
                case AudioType.Player:
                    AudioManager.onPlayerVolumeChange -= OnVolumeChange;
                    break;
                case AudioType.Effect:
                    AudioManager.onEffectVolumeChange -= OnVolumeChange;
                    break;
            }
        }
        this.audioType = audioType;
        switch (audioType)
        {
            case AudioType.Music:
                OnVolumeChange(AudioManager.MusicVolume);
                AudioManager.onMusicVolumeChange += OnVolumeChange;
                break;
            case AudioType.Player:
                OnVolumeChange(AudioManager.PlayerVolume);
                AudioManager.onPlayerVolumeChange += OnVolumeChange;
                break;
            case AudioType.Effect:
                OnVolumeChange(AudioManager.EffectVolume);
                AudioManager.onEffectVolumeChange += OnVolumeChange;
                break;
            case AudioType.None:
                OnVolumeChange(1);
                break;
        }
        return this;
    }
    public Audio SetPauseControl(bool active = true)
    {
        if (active)
            AudioManager.onPauseControl += PauseP;
        else
            AudioManager.onPauseControl -= PauseP;
        return this;
    }
    void OnVolumeChange(float volume)
    {
        this.volume = volume;
        if (audioSource)
        {
            audioSource.volume = volume;
        }
    }
    public void Destroy()
    {
        switch (audioType)
        {
            case AudioType.Music:
                AudioManager.onMusicVolumeChange -= OnVolumeChange;
                break;
            case AudioType.Player:
                AudioManager.onPlayerVolumeChange -= OnVolumeChange;
                break;
            case AudioType.Effect:
                AudioManager.onEffectVolumeChange -= OnVolumeChange;
                break;
        }
        AudioManager.onPauseControl -= PauseP;
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

    public enum AudioType
    {
        None,
        Music,
        Player,
        Effect,
    }
}
