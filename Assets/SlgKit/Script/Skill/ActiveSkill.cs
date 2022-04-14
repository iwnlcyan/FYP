using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//public class ActiveSkill : MonoBehaviour
//{
//    // Start is called before the first frame update
//    void Start()
//    {

//    }

//    // Update is called once per frame
//    void Update()
//    {

//    }
//}




public enum SkillTarget
{
    //盟军
    Friendly = 0,
    Enemy

}

public abstract class ActiveSkillAction
{
    public abstract void Releaseskill(PlayerController from, List<PlayerController> to);


    public abstract void BeforReleaseskill(PlayerController from, List<PlayerController> to);
}

public class RestoreHealth : ActiveSkillAction
{
    public override void BeforReleaseskill(PlayerController from, List<PlayerController> to)
    {
        EffectCtrl.instance.ShowMagicCircleSimpleGreen(from);
    }

    public override void Releaseskill(PlayerController from, List<PlayerController> to)
    {
        var addHp = Mathf.FloorToInt((float)from.attribute.atk * 1.5f);
        foreach (var player in to)
        {
            player.RestoreHealth(addHp);
            EffectCtrl.instance.ShowRestoreHealth(player);
        }
    }
}


public class RestoreHealthEffectBig : RestoreHealth
{
    public override void Releaseskill(PlayerController from, List<PlayerController> to)
    {
        base.Releaseskill(from, to);

        EffectCtrl.instance.ShowRestoreHealthBig(from);

    }
}


public enum SkillType
{
    Attack,
    RestoreHealth
}


public class ActiveSkillConfig
{
    //以人物为中心的方范围
    public uint releaseRange = 1;

    //作用范围
    public uint actionRange = 1;

    public SkillTarget skillTarget;

    public ActiveSkillAction activeSkillAction;
    internal int cd_config;
    internal int cd=0;

    public SkillType skillType;


    public bool canSelect(PlayerController from, PlayerController to)
    {
        if (skillTarget == SkillTarget.Friendly)
        {
            return from.sect == to.sect;
        }
        else
        {
            return from.sect != to.sect;
        }
    }
}


public class DamageSkill : ActiveSkillAction
{
    public override void BeforReleaseskill(PlayerController from, List<PlayerController> to)
    {
      
        EffectCtrl.instance.playeffect = true;
        EffectCtrl.instance.ShowMagicCircleSimpleGreen(from);
    }

    public override void Releaseskill(PlayerController from, List<PlayerController> to)
    {
        var damage = Mathf.FloorToInt((float)from.attribute.atk * 1.5f);
        GameCtrl.instance.StartCoroutine(C_ShowTime(damage, to));
    }


    IEnumerator C_ShowTime(int damage, List<PlayerController> to)
    {
        //镜头震动
        CameraCtrl.instance.Shake(0.5f, 0.3f);

        foreach (PlayerController player in to)
        {
            GameCtrl.instance.StartCoroutine(C_showTime_1(damage, player));
            yield return new WaitForSeconds(0.3f);
        }

        yield return new WaitForSeconds(1.3f);
        //特效播放完才可以操作
        EffectCtrl.instance.playeffect = false;
    }


    IEnumerator C_showTime_1(int damage,PlayerController  to)
    {
        EffectCtrl.instance.ShowFireFall(to, 1.5f);

        yield return new WaitForSeconds(1.3f);
        EffectCtrl.instance.ShowMysticExplosionOrange(to.transform.position);
        to.DamgeBySkill(damage);
        CameraCtrl.instance.Shake(1, 1);
    }
}
