using SampleBehaviorTree;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 数据模型
/// </summary>
public class InActiveSkillRangeResult
{
    public List<PlayerController> insidePlayers;
    public Skill resultSkill;
    public StrikingRangePath resultPath;
}

public class InActiveSkillRange_Advanced:Condition_Behavior
{
    public PlayerController playerC;
    public List<InActiveSkillRangeResult> data;
    

    protected virtual List<PlayerController> GetTargetPlayers()
    {
        return null;
    }

   
    public override IEnumerator Start()
    {
        data = new List<InActiveSkillRangeResult>();
        return base.Start();
    }

    public override IEnumerator Execute()
    {
        GridMeshManager.Instance.DespawnAllPath();

       

        foreach (Skill skill in playerC.skill)
        {
            if (skill != null && skill.activeSkillConfig != null && skill.activeSkillConfig.cd == 0)
            {
                if (!CanSelectSkill(skill)) continue;

                StrikingRangePath path = playerC.StartPath_ActiveSkill_Range(playerC.mapNode, skill.activeSkillConfig.releaseRange);

                yield return playerC.StartCoroutine(path.WaitForPath());
                //根据技能的配置获取作用对象

                var players = GetTargetPlayers();
                //派生伤害技能和辅助的技能的原因是为了减少if esle 提高代码可描述性
                //if (behaviorType== BehaviorType.Attck&&skill.activeSkillConfig.skillTarget== SkillTarget.Enemy)
                //   players = GameCtrl.instance.GetEnemy(playerC.sect);
                //else if (behaviorType == BehaviorType.Auxiliary && skill.activeSkillConfig.skillTarget == SkillTarget.Friendly)
                //    players = GameCtrl.instance.GetPlayers(playerC.sect);

                //查找技能射程范围内的玩家
                var insidePlayers = players.FindAll(f_plyaer => path.allNodes.Contains(f_plyaer.mapNode));

                if (insidePlayers.Count > 0)
                {
                    data.Add(new InActiveSkillRangeResult()
                    {
                        insidePlayers = insidePlayers,
                        resultPath = path,
                        resultSkill = skill
                    });
                }
            }
        }
        //没有技能符合条件
        if (data.Count > 0)
        {
            state = State.Succeed;
        }
        else
        {
            state = State.Fail;
            yield break;
        }

        //return base.Execute();
    }

    protected virtual bool CanSelectSkill(Skill skill)
    {
        return false;
    }
}

public class InDamageSkillRange : InActiveSkillRange_Advanced
{
    protected override bool CanSelectSkill(Skill skill)
    {
        return skill.activeSkillConfig.skillTarget==  SkillTarget.Enemy;
    }

    protected override List<PlayerController> GetTargetPlayers()
    {
        return GameCtrl.instance.GetEnemy(playerC.sect);
    }
}

public class InAuxiliarySkillRange : InActiveSkillRange_Advanced
{
    protected override bool CanSelectSkill(Skill skill)
    {
        return skill.activeSkillConfig.skillTarget == SkillTarget.Friendly;
    }
    protected override List<PlayerController> GetTargetPlayers()
    {
        return GameCtrl.instance.GetPlayers(playerC.sect);
    }
}













