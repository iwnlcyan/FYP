using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioCtrl : MonoBehaviour
{
    public static AudioCtrl instance;
    AudioSource audioSourceMusic;
    AudioSource audioSourceSound;
    public bool soundMute=false;
    public bool musicMute;
    //不能够删除
    static GameObject gob;
    private void Awake()
    {
   
        this.gameObject.AddComponent<AudioListener>();
        AudioCtrl.instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        //背景音乐的播放
        //audioSourceMusic = GameObject.FindObjectOfType<AudioSource>();
        audioSourceMusic = gob.AddComponent<AudioSource>();
        audioSourceMusic.clip = GetAudio("bgm");
        audioSourceMusic.Play();

        audioSourceSound = audioSourceMusic.gameObject.AddComponent<AudioSource>();

        EventDispatcher.instance.Regist(GameEventType.playButtonUiSound,this.playButtonUiSound);
        //hitbody
        //人物被打击的音效

        EventDispatcher.instance.Regist(GameEventType.playHitBodySound, this.playHitBodySound);

        //被动技能触发先攻音效
        EventDispatcher.instance.Regist(GameEventType.playXianGongSound, this.playXianGongSound);

        LoadApplyCfg();
    }

   
    internal void SetSoundOpen(bool arg0)
    {
       
        soundMute = !arg0;
        this.audioSourceSound.mute = soundMute;
    }

    internal void SetMusicOpen(bool arg0)
    {
        //throw new NotImplementedException();
        this.musicMute= !arg0;
        audioSourceMusic.mute = this.musicMute;
    }

    void LoadApplyCfg()
    {
       this.soundMute= PlayerPrefs.GetInt("soundMute")==1;
        this.musicMute = PlayerPrefs.GetInt("musicMute") == 1;
        this.audioSourceSound.mute = soundMute;
        audioSourceMusic.mute = this.musicMute;
    }

    internal void SaveCfg()
    {
        //  throw new NotImplementedException();
        PlayerPrefs.SetInt("soundMute",this.soundMute?1:0);
        PlayerPrefs.SetInt("musicMute", this.musicMute ? 1 : 0);
    }

    private void playXianGongSound()
    {
        playSound("xiangong");
    }

    private void playHitBodySound()
    {
        playSound("hitbody");
    }

    private void playButtonUiSound()
    {
        playSound("button");
    }

    void playSound(string path)
    {
        if (soundMute==false)
            audioSourceSound.PlayOneShot(GetAudio(path));
    }

    private AudioClip GetAudio(string path)
    {
        return ResourcesExt.Load<AudioClip>("Audio/"+path);

        
    }





    // Update is called once per frame
    void Update()
    {
        
    }

    
    internal static void Init()
    {
       
        gob = new GameObject("audioCtrl");
        gob.AddComponent<AudioCtrl>();

        //场景切换时不被删除
        DontDestroyOnLoad(gob);
    }
}
