using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioGroup
{
    Audio[] audios;
    AudioClip audioClip;
    int maxCount;
    int oldestNumber;
    int priority;
    GameObject gameObject;
    internal float lastUsedTime;
    public AudioGroup(AudioClip audioClip, int maxCount, int priority)
    {
        this.audioClip = audioClip;
        this.maxCount = maxCount;
        this.priority = priority;
        Init();
    }
    public void Init()
    {
        lastUsedTime = Time.unscaledTime + audioClip.length;
        if (!gameObject)
            gameObject = new GameObject(audioClip.name + "_Group");
        gameObject.transform.SetParent(AudioManager.Instance.SceneAudioParent.transform);
        audios = new Audio[maxCount];
        for (int i = 0; i < maxCount; i++)
        {
            audios[i] = AudioManager.Create(audioClip, gameObject.transform);
            audios[i].audioSource.priority = priority;
        }
        oldestNumber = 0;
    }
    public Audio Play()
    {
        lastUsedTime = Time.unscaledTime + audioClip.length;
        Audio audio = audios[oldestNumber].Play();
        audio.audioSource.loop = false;
        oldestNumber++;
        if (oldestNumber >= maxCount)
            oldestNumber = 0;
        return audio;
    }
    public void Destroy()
    {
        if (gameObject)
            Object.Destroy(gameObject);
    }
    ~AudioGroup()
    {
        Destroy();
    }

    public static implicit operator bool(AudioGroup audioGroup)
    {
        return audioGroup != null && audioGroup.gameObject != null && audioGroup.audios != default && audioGroup.audios.Length > 0 && audioGroup.audios[0];
    }
}
