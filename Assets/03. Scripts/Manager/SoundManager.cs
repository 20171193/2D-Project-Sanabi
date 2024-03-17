using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;

public enum SoundType
{
    BGM,
    AMB,
    SFX
}

public class SoundManager : Singleton<SoundManager>
{
    [SerializeField]
    private AudioMixerGroup bgmGroup;
    [SerializeField]
    private AudioMixerGroup ambGroup;
    [SerializeField]
    private AudioMixerGroup sfxGroup;

    [SerializeField]
    private AudioClip[] bgm;
    public AudioClip[] BGM { get { return bgm; } }
    private Dictionary<string, AudioClip> bgmDic;

    [SerializeField]
    private AudioClip[] amb;
    public AudioClip[] AMB { get { return bgm; } }
    private Dictionary<string, AudioClip> ambDic;

    [SerializeField]
    private AudioClip[] sfx;
    public AudioClip[] SFX { get { return bgm; } }
    private Dictionary<string, AudioClip> sfxDic;

    private AudioSource bgmSource;
    private AudioSource[] ambSource;
    private AudioSource sfxSource;

    public void Awake()
    {
        bgmDic = new Dictionary<string, AudioClip>();
        ambDic = new Dictionary<string, AudioClip>();
        sfxDic = new Dictionary<string, AudioClip>();

        foreach(AudioClip sound in bgm)
        {
            bgmDic.Add(sound.name, sound);
        }
        foreach (AudioClip sound in amb)
        {
            bgmDic.Add(sound.name, sound);
        }
        foreach (AudioClip sound in sfx)
        {
            bgmDic.Add(sound.name, sound);
        }
    }


    public void PlaySound(SoundType type, string name)
    {
        switch(type)
        {
            case SoundType.BGM:
                if (!bgmDic.ContainsKey(name)) return;
                bgmSource.clip = bgmDic[name];
                bgmSource.Play();
                break;

            case SoundType.AMB:
                if (!ambDic.ContainsKey(name)) return;
                int index = GetAMBChanelIndex();
                ambSource[index].clip = bgmDic[name];
                ambSource[index].Play();
                break;

            case SoundType.SFX:
                if (!sfxDic.ContainsKey(name)) return;
                bgmSource.clip = bgmDic[name];
                bgmSource.Play();
                break;

            default:
                return;
        }
    }
    public void UnPlaySound(SoundType type)
    {
        switch (type)
        {
            case SoundType.BGM:
                bgmSource.Stop();
                bgmSource.clip = null;
                break;

            case SoundType.AMB:
                foreach (AudioSource source in ambSource)
                {
                    source.Stop();
                    source.clip = null;
                }
                break;

            case SoundType.SFX:
                sfxSource.Stop();
                sfxSource.clip = null;
                break;

            default:
                return;
        }
    }

    // 사운드 출력할 AMB채널의 인덱스 Get
    private int GetAMBChanelIndex()
    {
        for(int i =0; i<ambSource.Length; i++)
        {
            // 빈 채널이 있다면 해당 위치
            if (ambSource[i].clip == null) 
                return i;
        }

        // 빈 채널이 없다면 새로운 채널 생성
        GameObject inst = Instantiate(gameObject);
        inst.name = $"AMB{ambSource.Length + 1}";
        AudioSource audioSource = inst.AddComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.outputAudioMixerGroup = ambGroup;
        inst.transform.parent = this.transform;

        return ambSource.Length;
    }
}
