﻿using SampleBehaviorTree;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class BehaviorCtrl : MonoBehaviour
{
    public static BehaviorCtrl instance;


    public AutomationBehavior myAutomationBehavior=new AutomationBehavior();
    public AutomationBehavior enemyAutomationBehavior = new AutomationBehavior();


    internal static void Init()
    {
        var go = new GameObject();
        instance = go.AddComponent<BehaviorCtrl>();
    }


    /// <summary>
    /// 根据AI类型获取指定的玩家
    /// </summary>
    /// <param name="behaviorType"></param>
    /// <param name="player"></param>
    /// <returns></returns>
    internal List<PlayerController> GetPlayers(BehaviorType behaviorType,PlayerController player)
    {
        if (behaviorType == BehaviorType.Attck)
            return GameCtrl.instance.GetEnemy(player.sect);
       

       return GameCtrl.instance.GetPlayers(player.sect);
       
    }


    
}
