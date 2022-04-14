using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Main : MonoBehaviour
{
   // public static Main instance;
    // Start is called before the first frame update
    void Start()
    {
       // instance = this;
       //不受场景切换删除
        DontDestroyOnLoad(this.gameObject);
        var g = ResourcesExt.Load<GameObject>("Prefab/GameTitle");

        MonoBehaviour.Instantiate(g);
    }

   
}
