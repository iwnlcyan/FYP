using SampleBehaviorTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UseAttack : ActionBehavior
{
    public PlayerController playerC;
     
    public override IEnumerator Execute()
    {
        //计算可以攻击的所有节点
        playerC.CalStrikingRangePath_AI();

        while (playerC.waitPath_StrikingRange) yield return null;

        var enemys = GameCtrl.instance.GetEnemy(playerC.sect);

        enemys=enemys.FindAll(e=>playerC.strikingRange.Contains(e.mapNode));

        if (enemys.Count<=0)
        {
            this.state = State.Fail;
            yield break;
        }
        //优先攻击 范围之内血量最少的敌人
        enemys.Sort(OrderBy_Hp);
        GameCtrl.instance.AttackSelect_AI(playerC, enemys[0]);
        this.state = State.Succeed;
        
    }
    /// <summary>
    /// 升序排序，血量小在前
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    private int OrderBy_Hp(PlayerController x, PlayerController y)
    {
        //throw new NotImplementedException();

        var x1 = x.attribute.hp;

        var x2 = y.attribute.hp;

        if (x1 > x2) return 1;
        if (x1 < x2) return -1;
        return 0;

    }
}
