using SampleBehaviorTree;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 数据模型
/// </summary>
public class UseSkillResult
{
    public PlayerController target;
    public Skill skill;
    public List<PlayerController> insidePlayers;
    public StrikingRangePath resultPath;
}


public class UseSkill_Advanced:ActionBehavior
{
    public InActiveSkillRange_Advanced inActiveSkillRange_Advanced;
    public BehaviorType behaviorType;
    public override IEnumerator Execute()
    {
        var from = inActiveSkillRange_Advanced.playerC;
        from.actionRangePath = null;

        List<UseSkillResult> data_UseSkill = new List<UseSkillResult>();
        //穷举出节能射程范围内所有敌人，得到所需的数据模型
        foreach (var item in inActiveSkillRange_Advanced.data)
        {
            var p_skill = item.resultSkill;
            // var players = GameCtrl.instance.GetEnemy(from.sect);
            var players = new List<PlayerController>();
            if (p_skill.activeSkillConfig.skillTarget == SkillTarget.Enemy)
                players = GameCtrl.instance.GetEnemy(from.sect);
            else
                players = GameCtrl.instance.GetPlayers(from.sect);


            foreach (var p_target in item.insidePlayers)
            {
                var path = from.StartPath_ActiveSkill_Range(p_target.mapNode, p_skill.activeSkillConfig.actionRange);
                yield return from.StartCoroutine(path.WaitForPath());
                var filterPlayer = players.FindAll(s => path.allNodes.Contains(s.mapNode));

                //假如是治疗类型的技能的话,人物血量大于95%则不使用技能
                //假如不做判断很会出现满血也会使用技能
                if (p_skill.activeSkillConfig.skillType == SkillType.RestoreHealth)
                {
                    if (p_target.hp_percentage > 0.95f)
                    {
                        continue;
                    }
                }

                

                data_UseSkill.Add(new UseSkillResult()
                {
                    target = p_target,
                    insidePlayers = filterPlayer,
                    resultPath = path,
                    skill = p_skill,
                });
            }
        }


        


        if (data_UseSkill.Count == 0)
        {
            state = State.Fail;
            yield break;
        }
        else
        {
            data_UseSkill.Sort(sortSkillResult);
            var data = data_UseSkill[0];
            var to = data_UseSkill[0].target;
            //为了看清楚 节点的 运行过程
            GridMeshManager.Instance.ShowPathRed(data.resultPath.allNodes);
            yield return new WaitForSeconds(1f);


            GameCtrl.instance.SkillSelectionTarget(from, to, data.skill);

            state = State.Succeed;
            //等待路径计算完成
            while (from.actionRangePath == null) yield return null;

            //为了看清楚 节点的 运行过程
            yield return new WaitForSeconds(1f);

            GameCtrl.instance.confirmUseSkill_AI(from, to);
        }
    }

    private int sortSkillResult(UseSkillResult x, UseSkillResult y)
    {
        //throw new NotImplementedException();

        var x1 = getWeight(x);

        var x2 = getWeight(y);

        if (x1 > x2) return -1;
        if (x1 < x2) return 1;
        return 0;

    }
 
    private int getWeight(UseSkillResult x)
    {
      

        //技能能打到的人数越多，权重越高
        var weight1 = x.insidePlayers.Count * 10;
        // return weight1;

        
        var hp_weight = 0;
        foreach (var player in x.insidePlayers)
        {
            //血量越少,权重越高
            var p = 1f - player.hp_percentage;
            var h = Mathf.FloorToInt(p * 10);
            h *= h;
            hp_weight += h;
        }


        var auxiliaryWeight = 0;
        var attackWeight = 0;
        
        //辅助型 优先适用技能进行治疗
        if (behaviorType == BehaviorType.Auxiliary)
        {
            if (x.skill.activeSkillConfig.skillTarget == SkillTarget.Friendly)
                auxiliaryWeight += 100;
           else if (x.skill.activeSkillConfig.skillTarget == SkillTarget.Enemy)
                attackWeight += 50;

        }
        else if (behaviorType == BehaviorType.Attck)
        {
            //进攻型
            if (x.skill.activeSkillConfig.skillTarget == SkillTarget.Enemy)
            attackWeight += 1000;
        }

        return weight1 + hp_weight+ auxiliaryWeight+ attackWeight;
    }
}




