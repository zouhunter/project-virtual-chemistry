using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;


[RequireComponent(typeof(AudioSource))]
public class AudioManager : Singleton<AudioManager>
{
    private AudioSource m_audio;
    private Hashtable m_Sounds = new Hashtable();
    private const string m_ClipFileName = "Audio/";
    private const string m_BackGround = m_ClipFileName + "backSound";

    //音量大小
    private float volume;
    public float Volume
    {
        get
        {
            return volume;
        }
        set
        {
            if (volume <= 1 && volume >= 0)
            {
                volume = value;
                m_audio.volume = volume;
            }
        }
    }

    //声音开关
    private bool isOn;
    public bool IsOn
    {
        get {
            return isOn;
        }
        set {
            isOn = value;
        }
    }


    private int backIndex = 0;
    private AudioClip[] backSound;

    void Awake(){
        m_audio = GetComponent<AudioSource>();
        backSound = Resources.LoadAll<AudioClip>(m_BackGround);
        Resources.UnloadUnusedAssets();
    }
    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="backsound"></param>
    /// <param name="volume"></param>
    public void ResetDefult(bool isOn, float volume)
    {
        m_audio.playOnAwake = false;
        m_audio.loop = false;
        m_audio.maxDistance = 4f;
        this.IsOn = isOn;
        this.Volume = volume;
    }

    /// <summary>
    /// 添加一个声音
    /// </summary>
    void Add(string key, AudioClip value)
    {
        if (m_Sounds[key] != null || value == null) return;
        m_Sounds.Add(key, value);
    }

    /// <summary>
    /// 获取一个声音
    /// </summary>
    AudioClip Get(string key)
    {
        if (m_Sounds[key] == null) return null;
        return m_Sounds[key] as AudioClip;
    }

    /// <summary>
    /// 载入一个音频
    /// </summary>
    bool LoadAudioClip(string clipName, out AudioClip clip)
    {
        AudioClip ac = Get(clipName);
        if (ac == null)
        {
            ac = (AudioClip)Resources.Load(m_ClipFileName + clipName, typeof(AudioClip));
            if (ac != null)
            {
                Add(clipName, ac);
                clip = ac;
                return true;
            }
            clip = null;
            return false;
        }
        clip = ac;
        return true;
    }

    /// <summary>
    /// 播放背景音乐
    /// </summary>
    /// <param name="canPlay"></param>
    public void PlayBacksound()
    {
        if (!IsOn|| backSound == null) return;
        AudioClip clip = backSound[backIndex];

        m_audio.loop = true;
        m_audio.clip = clip;
        m_audio.Play();
    }
    /// <summary>
    /// 切换背景音乐
    /// </summary>
    public void ExchangeBackSound()
    {
        backIndex++;
        if (backIndex >= backSound.Length)
        {
            backIndex = 0;
        }
        PlayBacksound();
    }

    /// <summary>
    /// 停止背景音乐
    /// </summary>
    public void StopBackSound()
    {
        m_audio.Stop();
        m_audio.clip = null;
    }

    /// <summary>
    /// 在指定位置播放音频剪辑
    /// </summary>
    /// <param name="clip"></param>
    /// <param name="position"></param>
    public void PlayAtPosition(string clipName, Vector3 position)
    {
        if (!IsOn) return;
        AudioClip clip;
        if (LoadAudioClip(clipName, out clip))
        {
            AudioSource.PlayClipAtPoint(clip, position);
        }
    }
    /// <summary>
    /// 界面声音
    /// </summary>
    /// <param name="clipName"></param>
    public void Play(string clipName)
    {
        if (!IsOn) return;
        AudioClip clip;
        if (LoadAudioClip(clipName, out clip))
        {
            m_audio.PlayOneShot(clip);
        }
    }
}
