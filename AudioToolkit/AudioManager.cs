using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class AudioManager : MonoBehaviour
{
    const float RemoveNullInterval = 30f;
    const float RemoveIdleInterval = 10f;
    float removeNullTimer = 0;
    float removeIdleTimer = 0;

    public static AudioManager Instance;
    private void Awake()
    {
        if (Instance != null)
        {
            DestroyImmediate(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public Audio BGM;

    [SerializeField]
    GameObject sceneAudioParent;
    public GameObject SceneAudioParent
    {
        get
        {
            if (sceneAudioParent == null)
            {
                sceneAudioParent = new GameObject("Temp Audio Parent");
            }
            return sceneAudioParent;
        }
    }


    Dictionary<string, AudioGroup> audioGroup = new();
    Dictionary<AudioClip, Audio> intervalAudio = new();
    List<AudioClip> intervalAudioTmp = new();

    private void Update()
    {

        if (removeNullTimer > RemoveNullInterval)
        {
            if (audioGroup.Count > 0)
            {
                string keyNames = "";
                foreach (var dic in audioGroup)
                {
                    if (!dic.Value)
                    {
                        keyNames += dic.Key + ",";
                        if (dic.Value != null)
                            dic.Value.Destroy();
                    }
                }
                if (keyNames != "")
                {
                    string[] str_keyNames = keyNames.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string key in str_keyNames)
                    {
#if UNITY_EDITOR
                        Debug.Log("从audioGroup中移除无效项，key：" + key);
#endif
                        audioGroup.Remove(key);
                    }
                }
            }
            if (intervalAudio.Count > 0)
            {
                intervalAudioTmp.Clear();
                foreach (var dic in intervalAudio)
                {
                    if (!dic.Value)
                    {
                        intervalAudioTmp.Add(dic.Key);
                        if (dic.Value != null)
                            dic.Value.Destroy();
                    }
                }
                if (intervalAudioTmp.Count > 0)
                {
                    foreach (var key in intervalAudioTmp)
                    {
#if UNITY_EDITOR
                        Debug.Log("从intervalAudio中移除无效项，key：" + key);
#endif
                        intervalAudio.Remove(key);
                    }
                }
            }
            removeNullTimer = 0;
        }
        if (removeIdleTimer > RemoveIdleInterval)
        {
            if (audioGroup.Count > 0)
            {
                string keyNames = "";
                foreach (var dic in audioGroup)
                {
                    if (dic.Value != null && (Time.unscaledTime - dic.Value.lastUsedTime) > RemoveIdleInterval)
                    {
                        keyNames += dic.Key + ",";
                        if (dic.Value != null)
                            dic.Value.Destroy();
                    }
                }
                if (keyNames != "")
                {
                    string[] str_keyNames = keyNames.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string key in str_keyNames)
                    {
#if UNITY_EDITOR
                        Debug.Log("从audioGroup中移除闲置项：" + key);
#endif
                        audioGroup.Remove(key);
                    }
                }
            }
            if (intervalAudio.Count > 0)
            {
                intervalAudioTmp.Clear();
                foreach (var dic in intervalAudio)
                {
                    if (dic.Value && (Time.unscaledTime - dic.Value.lastUsedTime) > RemoveIdleInterval)
                    {
                        intervalAudioTmp.Add(dic.Key);
                        if (dic.Value != null)
                            dic.Value.Destroy();
                    }
                }
                if (intervalAudioTmp.Count > 0)
                {
                    foreach (var key in intervalAudioTmp)
                    {
#if UNITY_EDITOR
                        Debug.Log("从intervalAudio中移除闲置项，key：" + key);
#endif
                        intervalAudio.Remove(key);
                    }
                }
            }
            removeIdleTimer = 0;
        }

        removeNullTimer += Time.unscaledDeltaTime;
        removeIdleTimer += Time.unscaledDeltaTime;
    }


    public static Audio PlayBGM(string clipPath) => PlayBGM(AssetManager.GetResource<AudioClip>(clipPath));
    public static Audio PlayBGM(AudioClip clip)
    {
        if (Instance.BGM != null)
        {
            if (clip == Instance.BGM.clip)
                return Instance.BGM;
            Instance.BGM.Stop();
            Destroy(Instance.BGM.gameObject);
        }
        Instance.BGM = new(clip, Instance.transform, true, 999);
        Instance.BGM.audioSource.loop = true;
        return Instance.BGM;
    }


    public static Audio Play(string clipPath, Transform parent = null, int priority = 0) =>
        Play(AssetManager.GetResource<AudioClip>(clipPath), parent, priority);
    public static Audio Play(AudioClip clip, Transform parent = null, int priority = 0)
    {
        return new(clip, parent == null ? Instance.SceneAudioParent.transform : parent, true, priority);
    }


    public static Audio PlayOnce(string clipPath, int priority = 0) =>
        PlayOnce(AssetManager.GetResource<AudioClip>(clipPath), priority);
    public static Audio PlayOnce(AudioClip clip, int priority = 0)
    {

        Audio au = new(clip, Instance.SceneAudioParent.transform, true, priority);
        au.gameObject.AddComponent(typeof(PlayOnce));
        return au;
    }
    public static Audio PlayOnce(string clipPath, string groupName, int maxCount, int priority = 0)
        => PlayOnce(AssetManager.GetResource<AudioClip>(clipPath), groupName, maxCount, priority);
    public static Audio PlayOnce(AudioClip clip, string groupName, int maxCount, int priority = 0)
    {
        if (Instance.audioGroup.TryGetValue(groupName, out AudioGroup audioGroup))
        {
            if (!audioGroup)
            {
                if (audioGroup == null)
                {
                    Instance.audioGroup.Remove(groupName);
                    audioGroup = new(clip, maxCount, priority);
                    Instance.audioGroup.Add(groupName, audioGroup);
                }
                else
                    audioGroup.Init();
            }
        }
        else
        {
            audioGroup = new(clip, maxCount, priority);
            Instance.audioGroup.Add(groupName, audioGroup);
        }
        return audioGroup.Play();
    }

    public static Audio PlayOnce(string clipPath, float minInterval, int priority = 0)
        => PlayOnce(AssetManager.GetResource<AudioClip>(clipPath), minInterval, priority);
    public static Audio PlayOnce(AudioClip clip, float minInterval, int priority = 0)
    {
        if (Instance.intervalAudio.TryGetValue(clip, out Audio audio))
        {
            if (!audio)
            {
                Instance.intervalAudio.Remove(clip);
                audio = Create(clip, Instance.SceneAudioParent.transform, priority);
                Instance.intervalAudio.Add(clip, audio);
            }
        }
        else
        {
            audio = Create(clip, Instance.SceneAudioParent.transform, priority);
            Instance.intervalAudio.Add(clip, audio);
        }
        if (audio.IsPlaying && audio.audioSource.time < minInterval)
        {

        }
        else
        {
            audio.Play();
        }
        return audio;
    }


    //先加聲音組件后賦Clip,PlayOnAwake執行但無clip播放
    public static Audio Create(string clipPath, Transform parent = null, int priority = 0) => Create(AssetManager.GetResource<AudioClip>(clipPath), parent, priority);
    public static Audio Create(AudioClip clip, Transform parent = null, int priority = 0)
    {
        return new(clip, parent == null ? Instance.SceneAudioParent.transform : parent, false, priority);
    }



}
