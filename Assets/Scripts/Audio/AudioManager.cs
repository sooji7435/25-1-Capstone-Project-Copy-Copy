using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;


public class AudioManager : Singleton<AudioManager> 
{

    [SerializeField] private GameObject sfxParent;
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private SoundLibrary soundLibrary;
    [SerializeField] private AudioMixer audioMixer;

    private AudioSource[] sfxSources;
    private int currentSfxIndex = 0;

    private Dictionary<string, string> sceneBgmMap = new Dictionary<string, string>()
    {
        { "MainMenu",  "MainMenu" },
        { "HubScene",   "HubScene" },
        { "BlueDragon1F", "BlueDragon1F"},
        { "BlueDragonBoss", "BlueDragonBoss"},
        {"BossBattle", "BossBattle"}
    };

    protected override void Awake()
    {
        base.Awake(); 
        sfxSources = sfxParent.GetComponents<AudioSource>();

    }

    public void PlaySFX(string key)
    {
        var clip = soundLibrary.GetClip(key);
        if (clip != null)
        {
            sfxSources[currentSfxIndex].PlayOneShot(clip);
            currentSfxIndex = (currentSfxIndex + 1) % sfxSources.Length;
        }
    }


    public void PlayBGM(AudioClip bgmClip, bool loop = true)
    {
        if (bgmSource.clip == bgmClip) return;
        bgmSource.clip = bgmClip;
        bgmSource.loop = loop;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    public void PauseBGM()
    {
        if (bgmSource != null && bgmSource.isPlaying)
            bgmSource.Pause();
    }
    public void ResumeBGM()
    {
        if (bgmSource != null)
            bgmSource.UnPause();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[AudioManager] Scene Loaded : {scene.name}");
        if (sceneBgmMap.TryGetValue(scene.name, out string bgmKey))
        {
            if (string.IsNullOrEmpty(bgmKey))
            {
                StopBGM();   // null이면 BGM 끄기
            }
            else
            {
                var clip = soundLibrary.GetClip(bgmKey);
                if (clip != null)
                    PlayBGM(clip, true);
            }
        }
    }

    public void ChangeBGM(string bgmKey)
    {
        var clip = soundLibrary.GetClip(bgmKey);
        if (clip != null)
            PlayBGM(clip, true);
    }

    public void SetBGMVolume(float volume)
    {
        audioMixer.SetFloat("BGM", volume);
    }
    public void SetSFXVolume(float volume)
    {
        audioMixer.SetFloat("SFX", volume);
    }
    public void SetMasterVolume(float volume)
    {
        audioMixer.SetFloat("Master", volume);
    }



}