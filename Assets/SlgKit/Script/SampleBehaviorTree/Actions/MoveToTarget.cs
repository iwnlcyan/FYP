using System;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using SampleBehaviorTree;
using UnityEngine;

public class MoveToTarget : ActionBehavior
{
    public PlayerController playerC;
    private Path moveRangePath;

    public override IEnumerator Start()
    {
        moveRangePath = null;
        return base.Start();
    }

    public override IEnumerator Execute()
    {
        //查找任意敌人，并移动到他身边
        var enemy = GameCtrl.instance.GetEnemy(playerC.sect);

        //路径查找 思想 求出移动范围和 最短路径，求出交集部分

        playerC.GetMovePath(this.OnMovePathOk);
        while (moveRangePath == null) yield return new WaitForSeconds(0.5f);

        var abpath=GetMove2EnemyPath(playerC.transform.position, enemy[0].transform.position, moveRangePath);

        yield return playerC.StartCoroutine(abpath.WaitForPath());

        var endNode = abpath.path[abpath.path.Count-1];
        playerC.Move_AI(endNode, abpath.path);

        //playerC.moving 异步赋值原因需要等待
        yield return new WaitForSeconds(0.5f);

        while (playerC.moving) yield return new WaitForSeconds(0.5f);
      
        this.state = SampleBehaviorTree.State.Succeed;
    }

    private void OnMovePathOk(Path path)
    {
        moveRangePath = path;
    }

  
    public ABPathExt GetMove2EnemyPath(Vector3 p_start, Vector3 end, Path p_moveRangePath)
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

        ABPathExt mPath = ABPathExt.ConstructRange(p_start, p_endpos,null);

        mPath.canTraverseWater = playerC.canTraverseWater;

        AstarPath.StartPath(mPath, true);

        return mPath;
    }

}
