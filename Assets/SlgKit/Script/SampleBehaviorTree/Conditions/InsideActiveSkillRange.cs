using Pathfinding;
using SampleBehaviorTree;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;





public class InActiveSkillRange : Condition_Behavior
{
    public PlayerController playerC;
    public List<PlayerController> insidePlayers;
    public Skill resultSkill;
    public StrikingRangePath resultPath;

    public override IEnumerator Start()
    {
        insidePlayers = null;
        resultSkill = null;
        resultPath = null;
        return base.Start();
    }

    public override IEnumerator Execute()
    {

        GridMeshManager.Instance.DespawnAllPath();

        foreach (Skill skill in playerC.skill)
        {
            if (skill != null && skill.activeSkillConfig != null && skill.activeSkillConfig.cd == 0)
            {
               StrikingRangePath path= playerC.StartPath_ActiveSkill_Range(playerC.mapNode, skill.activeSkillConfig.releaseRange);

                yield return playerC.StartCoroutine(path.WaitForPath());


                var enemys=GameCtrl.instance.GetEnemy(playerC.sect);
                //查找技能射程范围内的玩家
                insidePlayers = enemys.FindAll(enemy => path.allNodes.Contains(enemy.mapNode));

                if (insidePlayers.Count>0)
                {
                    state = State.Succeed;
                    resultSkill = skill;
                    resultPath = path;
                }
                else
                {
                    state = State.Fail;
                }

              
            }

        }
        //没有技能符合条件
        if (resultSkill == null)
        {
            state = State.Fail;
            yield break;
        }
    

        //yield return base.Execute();
    }

  
}

