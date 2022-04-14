using Pathfinding;
using SampleBehaviorTree;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveResult
{
    internal List<GraphNode> abPath;

    public PlayerController moveToPlayer;
    internal List<GraphNode> fullabPath;
}


public class MoveToTarget_Advanced : ActionBehavior
{
    public PlayerController playerC;
    private Path moveRangePath;
    public BehaviorType behaviorType;
    public List<MoveResult> moveResult = new List<MoveResult>();
    public override IEnumerator Start()
    {
        moveRangePath = null;
        moveResult.Clear();
        return base.Start();
    }



    public override IEnumerator Execute()
    {
        //查找任意玩家，并移动到他身边
       // var players = GameCtrl.instance.players;
        
        //路径查找 思想 求出移动范围和 最短路径，求出交集部分

        playerC.GetMovePath(this.OnMovePathOk);
        while (moveRangePath == null) yield return new WaitForSeconds(0.5f);

        var players = new List<PlayerController>();
        
        
        players=BehaviorCtrl.instance.GetPlayers(this.behaviorType, playerC);


        if (players.Count==0)
        {
            state = State.Fail;
            yield break;
          
        }
            foreach (PlayerController p_player in players)
        {
            var abpath = GetMove2TargetPath(playerC.transform.position, p_player.transform.position, moveRangePath);

            yield return playerC.StartCoroutine(abpath.WaitForPath());

            var p_fullabPath = GetMove2TargetFullPath(playerC.transform.position, p_player.transform.position);

            yield return playerC.StartCoroutine(abpath.WaitForPath());

            var m= new MoveResult() { abPath=abpath.path, moveToPlayer= p_player , fullabPath= p_fullabPath.path };

            moveResult.Add(m);
        }

        

        moveResult.Sort(SortResult);

        var abPath = moveResult[0].abPath;
        var endNode = abPath[abPath.Count - 1];
        playerC.Move_AI(endNode, abPath);

        //playerC.moving 异步赋值原因需要等待
        yield return new WaitForSeconds(0.5f);

        while (playerC.moving) yield return new WaitForSeconds(0.5f);

        this.state = SampleBehaviorTree.State.Succeed;
    }

    private int SortResult(MoveResult x, MoveResult y)
    {
        var x1=GetWeight(x);
        var x2 = GetWeight(y);


        if (x1 > x2) return -1;
        if (x1 < x2) return 1;
        return 0;
    }

    /// <summary>
    /// 思考维度 目标的 距离,血量,辅助技能
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    int GetWeight(MoveResult t)
    {
        //血量越少,权重越高
        var p = 1f - ((float)t.moveToPlayer.attribute.hp / t.moveToPlayer.attribute.maxHp);
        var hp_weight = Mathf.FloorToInt(p * 10);
        int auxiliaryWeight = 0;
        int attackWeight = 0;
        //距离越远权重越低
         var distanceWeight = -((3*t.fullabPath.Count));

        //辅助决策权重
        if (behaviorType == BehaviorType.Auxiliary)
        {
            if (t.moveToPlayer.sect == playerC.sect)
            {
                auxiliaryWeight = 15;
                //友军血量高于95%辅助权重变低
                if (t.moveToPlayer.hp_percentage > 0.95f)
                    auxiliaryWeight = -1000;

            }
        }
        else if (behaviorType == BehaviorType.Attck && t.moveToPlayer.sect != playerC.sect)
        { ////如果是进攻类型则优先移动到敌人身边
            attackWeight = 10;
        }



        return hp_weight+distanceWeight+ attackWeight+ auxiliaryWeight;
    }

    private void OnMovePathOk(Path path)
    {
        moveRangePath = path;
    }


    public ABPathExt GetMove2TargetPath(Vector3 p_start, Vector3 end, Path p_moveRangePath)
    {

        var otherPlayersMapNode = GameCtrl.instance.GetPlayersMapNode();
        otherPlayersMapNode.Remove(playerC.mapNode);
        var serchNode = p_moveRangePath.path.FindAll(s => !otherPlayersMapNode.Contains(s));

        //约束规则
        var nnc = new NNCPlayerMove();
        //符合条件的节点必须满足
        //1 在移动范围之内
        // 2 不能有玩家
        nnc.moveRangePath = serchNode;
        nnc.playersMapNode = otherPlayersMapNode;
        //返回的结果是在移动路径上距离敌人的坐标
        var p_Node = AstarPath.active.GetNearest(end, nnc).node;

        Vector3 p_endpos = (Vector3)p_Node.position;

        ABPathExt mPath = ABPathExt.ConstructRange(p_start, p_endpos, null);

        mPath.canTraverseWater = playerC.canTraverseWater;

        AstarPath.StartPath(mPath, true);

        return mPath;
    }

    public ABPathExt GetMove2TargetFullPath(Vector3 p_start, Vector3 end)
    {

        var otherPlayersMapNode = GameCtrl.instance.GetPlayersMapNode();
        otherPlayersMapNode.Remove(playerC.mapNode);
        //var serchNode = p_moveRangePath.path.FindAll(s => !otherPlayersMapNode.Contains(s));

        //约束规则
        var nnc = new NNCPlayerMove();
        
        nnc.playersMapNode = otherPlayersMapNode;
        //返回的结果是在移动路径上距离敌人的坐标
        var p_Node = AstarPath.active.GetNearest(end, nnc).node;

        Vector3 p_endpos = (Vector3)p_Node.position;

        ABPathExt mPath = ABPathExt.ConstructRange(p_start, p_endpos, null);

        mPath.canTraverseWater = playerC.canTraverseWater;

        AstarPath.StartPath(mPath, true);

        return mPath;
    }
}
