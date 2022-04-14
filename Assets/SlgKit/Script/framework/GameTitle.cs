using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameTitle : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (AudioCtrl.instance == null)
            AudioCtrl.Init();

       var d= GameObject.Find("starttButton").GetComponent<Button>();

        d.onClick.AddListener(OnStartButtonClick);
    }

    void OnStartButtonClick()
    {
        Destroy(this.gameObject);

        SceneManagerExt.instance.LoadSceneShowProgress(GameDefine.SceneType.GameLevel1);


    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [ContextMenu("TestDispatchEvent")]
    void TestDispatchEvent()
    {
        EventDispatcher.instance.DispatchEvent<Vector3>(GameEventType.showHitEffect,Vector3.zero);
    }


}
