using SampleBehaviorTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UseSkill : ActionBehavior
{
   
    internal InActiveSkillRange inActiveSkillRange;

    public override IEnumerator Execute()
    {
        //为了看清楚 节点的 运行过程
        GridMeshManager.Instance.ShowPathRed(inActiveSkillRange.resultPath.allNodes);
        yield return new WaitForSeconds(1f);
      


        var data = inActiveSkillRange;

        data.playerC.actionRangePath = null;

        var from = data.playerC;
        var to = data.insidePlayers[0];
       GameCtrl.instance.SkillSelectionTarget(from, to, data.resultSkill);
       
        state = State.Succeed;
        //等待路径计算完成
        while (data.playerC.actionRangePath==null) yield return null;

        //为了看清楚 节点的 运行过程
        yield return new WaitForSeconds(1f);

        GameCtrl.instance.confirmUseSkill_AI(from,to);

        

        // return base.Execute();
    }

}
