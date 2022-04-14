using PathologicalGames;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectCtrl
{
    public static EffectCtrl instance;
    private SpawnPool spawnPool;
    internal bool playeffect;

    internal static void Init()
    {
        instance = new EffectCtrl();
        EventDispatcher.instance.Regist<Vector3>(GameEventType.showHitEffect, instance.showHitEffect);
        // EventDispatcherDemo.instance.showHitEffect += instance.showHitEffect;

        //优化后


        var i = ResourcesExt.Load<GameObject>("effect/hit-blue-1");

        var poolGo = new GameObject("hitEffect Pool");

        instance.spawnPool = poolGo.AddComponent<SpawnPool>();

        var prefabPool = new PrefabPool(i.transform);
        instance.spawnPool.CreatePrefabPool(prefabPool);



        //魔法特效
        i = ResourcesExt.Load<GameObject>("effect/MagicCircleSimpleGreen");
        prefabPool = new PrefabPool(i.transform);
        instance.spawnPool.CreatePrefabPool(prefabPool);

        i = ResourcesExt.Load<GameObject>("effect/HealBig");
        prefabPool = new PrefabPool(i.transform);
        instance.spawnPool.CreatePrefabPool(prefabPool);

        i = ResourcesExt.Load<GameObject>("effect/HealingWindZone");
        prefabPool = new PrefabPool(i.transform);
        instance.spawnPool.CreatePrefabPool(prefabPool);

        i = ResourcesExt.Load<GameObject>("effect/RocketMissileFire");
        prefabPool = new PrefabPool(i.transform);
        instance.spawnPool.CreatePrefabPool(prefabPool);

        i = ResourcesExt.Load<GameObject>("effect/MysticExplosionOrange");
        prefabPool = new PrefabPool(i.transform);
        instance.spawnPool.CreatePrefabPool(prefabPool);
        
    }


  

    internal void ShowRestoreHealthBig(PlayerController from)
    {
        // throw new NotImplementedException();
        var go = spawnPool.Spawn("HealingWindZone");

        go.transform.SetParent(from.transform, false);
        go.transform.localPosition = Vector3.zero;

        spawnPool.Despawn(go, 5F);

    }

    private void showHitEffect(Vector3 worldPos)
    {
        //throw new NotImplementedException();

        //优化前
        //var i = ResourcesExt.Load("effect/hit-blue-1");
        //var go = GameObject.Instantiate(i);
        //go.transform.position = worldPos;

        // GameObject.Destroy(go,2F);



        var go = spawnPool.Spawn("hit-blue-1");
        go.transform.position = worldPos;

        spawnPool.Despawn(go, 2F);

        //因为该特效使用很频繁，不断的创建和删除，会加大CPU的负担

        //创建过程要查询内存空间和开辟内存
        //内存接近饱满时会开辟更多的内存空间
        //为了避免开辟过多的内存，会查找是否有足够的空间和内存垃圾
        //所以会触发垃圾回收(cpu负责)

        //垃圾回收简称GC,周期内GC次数越少，游戏的性能越好

        //所以在这里我们使用对象池进行优化

        //对象池的原理，需要释放对象的时候，不是删除而是把它存放起来，需要用的时候再显示出来
        //从而避免创建和删除 过程

        //对象池的实现大家可以在百度搜索相关资料，
        //在这里项目使用了对象池插件 PoolManager


    }

    internal void ShowMagicCircleSimpleGreen(PlayerController playerController)
    {
        //  throw new NotImplementedException();
        var go = spawnPool.Spawn("MagicCircleSimpleGreen");

        go.transform.SetParent(playerController.transform, false);
        go.transform.localPosition = Vector3.zero;

        spawnPool.Despawn(go, 6F);
    }

    internal void ShowRestoreHealth(PlayerController player)
    {
        //throw new NotImplementedException();
        var go = spawnPool.Spawn("HealBig");

        go.transform.SetParent(player.transform, false);
        go.transform.localPosition = Vector3.zero;

        spawnPool.Despawn(go, 2F);
    }


    public void ShowFireFall(PlayerController player, float duration)
    {
        var p_transform = spawnPool.Spawn("RocketMissileFire");

        int rndvalue = UnityEngine.Random.Range(0,10);

        rndvalue = rndvalue < 5 ? -1 : 1;

     

        var dir =  CameraCtrl.instance.transform.position- player.transform.position  ;

        dir.Normalize();

        var qua=Quaternion.Euler(0, rndvalue * 45, 0);

      
        //在摄像机的后方 5点和7点方向出现 特效
        var startPos =Vector3.up*3+ player.transform.position+ qua * (dir * 25);

        var fireDir = player.transform.position - startPos;

        fireDir.Normalize();

        var endPos = player.transform.position+ fireDir*2;

        spawnPool.Despawn(p_transform, 4);
        player.StartCoroutine(C_FireFall(p_transform, duration, startPos, endPos));
    }

    IEnumerator C_FireFall(Transform trs,float duration, Vector3 startPos,Vector3 endPos)
    {
        var startTime = Time.time;

        while (true)
        {
           
            var t = (Time.time - startTime) / duration;
            trs.position = Vector3.Lerp(startPos, endPos,t);


            if (t>=1)
            {
                trs.position = endPos;
                break;
            }

            yield return new WaitForEndOfFrame();



        }

    }


    public void ShowMysticExplosionOrange(Vector3 worldPos)
    {

       
            var p_transform = spawnPool.Spawn("MysticExplosionOrange");
            p_transform.position = worldPos;
            spawnPool.Despawn(p_transform, 4f);
       

        
    }

    internal void ReMove()
    {
        EventDispatcher.instance.UnRegist<Vector3>(GameEventType.showHitEffect,this.showHitEffect);
    }
}
