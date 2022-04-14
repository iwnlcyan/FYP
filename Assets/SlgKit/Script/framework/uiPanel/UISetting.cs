using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UISetting : MonoBehaviour,IPointerDownHandler
{
    Toggle musicToggle;
    Toggle soundToggle;
    //点击空白处关闭
    Button gotoHome;

    private void Awake()
    {

    }


    // Start is called before the first frame update
    void Start()
    {
       // image = GetComponent<Image>();
        musicToggle = transform.Find("item/musicToggle").GetComponent<Toggle>();
        soundToggle = transform.Find("item/soundToggle").GetComponent<Toggle>();
        gotoHome = transform.Find("item/gotoHome").GetComponent<Button>();
        //先更新UI再订阅回调
        musicToggle.isOn = !AudioCtrl.instance.musicMute;
        soundToggle.isOn = !AudioCtrl.instance.soundMute;

        musicToggle.onValueChanged.AddListener(OnMusicToggle);
        soundToggle.onValueChanged.AddListener(OnSoundToggle);


        gotoHome.onClick.AddListener(OnGotoHomeClICK);


    }

    void OnGotoHomeClICK()
    {
        EventDispatcher.instance.DispatchEvent(GameEventType.GotoHomeClICK);
    }




    private void OnSoundToggle(bool arg0)
    {
        AudioCtrl.instance.SetSoundOpen(arg0);
    }

    private void OnMusicToggle(bool arg0)
    {
        AudioCtrl.instance.SetMusicOpen(arg0);
    }

  

    public void OnPointerDown(PointerEventData eventData)
    {
        GameObject.Destroy(gameObject);
        AudioCtrl.instance.SaveCfg();
    }
}
