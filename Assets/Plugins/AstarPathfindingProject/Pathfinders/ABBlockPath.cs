using System.Collections.Generic;
using Pathfinding;
using UnityEngine;
using System.Collections;

public static class MyExtensions
{
    public static NNInfo GetNearest(this AstarPath asp, Vector3 position, NNConstraint constraint, GraphNode hint, List<GraphNode> 屏蔽节点)
    {

        if (asp.graphs == null) { return new NNInfo(); }

        float minDist = float.PositiveInfinity;//Math.Infinity;
        NNInfo nearestNode = new NNInfo();
        int nearestGraph = -1;

        for (int i = 0; i < asp.graphs.Length; i++)
        {

            NavGraph graph = asp.graphs[i];

            if (graph == null) continue;

            //Check if this graph should be searched
            if (!constraint.SuitableGraph(i, graph))
            {
                continue;
            }

            NNInfo nnInfo;
            
            
                nnInfo = graph.GetNearest(position, constraint, 屏蔽节点);



                //graph.GetNearest(Vector3.zero);

            GraphNode node = nnInfo.node;

            if (node == null)
            {
                continue;
            }

            float dist = ((Vector3)nnInfo.clampedPosition - position).magnitude;

            if (asp.prioritizeGraphs && dist < asp.prioritizeGraphsLimit)
            {
                //The node is close enough, choose this graph and discard all others
                minDist = dist;
                nearestNode = nnInfo;
                nearestGraph = i;
                break;
            }
            else
            {
                if (dist < minDist)
                {
                    minDist = dist;
                    nearestNode = nnInfo;
                    nearestGraph = i;
                }
            }
        }

        //No matches found
        if (nearestGraph == -1)
        {
            return nearestNode;
        }

        //Check if a constrained node has already been set
        if (nearestNode.constrainedNode != null)
        {
            nearestNode.node = nearestNode.constrainedNode;
            nearestNode.clampedPosition = nearestNode.constClampedPosition;
        }

        if (!asp.fullGetNearestSearch && nearestNode.node != null && !constraint.Suitable(nearestNode.node))
        {

            //Otherwise, perform a check to force the graphs to check for a suitable node
            NNInfo nnInfo = asp.graphs[nearestGraph].GetNearestForce(position, constraint);

            if (nnInfo.node != null)
            {
                nearestNode = nnInfo;
            }
        }

        if (!constraint.Suitable(nearestNode.node) || (constraint.constrainDistance && (nearestNode.clampedPosition - position).sqrMagnitude > asp.maxNearestNodeDistanceSqr))
        {
            return new NNInfo();
        }

        return nearestNode;
    }

    /// <summary>
    /// 获取附近合适的节点
    /// </summary>
    /// <param name="nnc"></param>
    /// <param name="node"></param>
    /// <param name="屏蔽节点"></param>
    /// <returns></returns>
    public static bool Suitable(this NNConstraint nnc, GraphNode node, List<GraphNode> 屏蔽节点)
    {
        if (屏蔽节点.Contains(node))
        {
            return false;
        }


        if (nnc.constrainArea && nnc.area >= 0 && node.Area != nnc.area) return false;





        if (nnc.constrainWalkability && node.Walkable != nnc.walkable ) return false;

        if (nnc.constrainArea && nnc.area >= 0 && node.Area != nnc.area) return false;

#if ConfigureTagsAsMultiple
			if (constrainTags && (tags & node.Tag) == 0) return false;
#else
        if (nnc.constrainTags && (nnc.tags >> (int)node.Tag & 0x1) == 0) return false;
#endif

        return true;

    }

    public static NNInfo GetNearest(this NavGraph nag, Vector3 position, NNConstraint constraint, GraphNode hint, List<GraphNode> 屏蔽节点)
    {
      
        float maxDistSqr = constraint.constrainDistance ? AstarPath.active.maxNearestNodeDistanceSqr : float.PositiveInfinity;

        float minDist = float.PositiveInfinity;
        GraphNode minNode = null;

        float minConstDist = float.PositiveInfinity;
        GraphNode minConstNode = null;

        nag.GetNodes(delegate(GraphNode node)
        {


            float dist = (position - (Vector3)node.position).sqrMagnitude;

            if (dist < minDist)
            {
                minDist = dist;
                minNode = node;
            }

            if (dist < minConstDist && dist < maxDistSqr && constraint.Suitable(node, 屏蔽节点))
            {
                minConstDist = dist;
                minConstNode = node;
            }
            return true;
        });

        NNInfo nnInfo = new NNInfo(minNode);

        nnInfo.constrainedNode = minConstNode;

        if (minConstNode != null)
        {
            nnInfo.constClampedPosition = (Vector3)minConstNode.position;
        }
        else if (minNode != null)
        {
            nnInfo.constrainedNode = minNode;
            nnInfo.constClampedPosition = (Vector3)minNode.position;
        }

        return nnInfo;
    }


    public static NNInfo GetNearest(this NavGraph nag, Vector3 position, NNConstraint constraint, List<GraphNode> 屏蔽节点)
    {

        return nag.GetNearest(position, constraint, null, 屏蔽节点);
    }

} 


/// <summary>
/// ABPath的改版，加入临时屏蔽节点的功能
/// </summary>
public class ABBlockPath : ABPath
{


    /// <summary>
    /// 阻挡的节点数据
    /// </summary>
    private List<GraphNode> blockNodes = new List<GraphNode>();

    /// <summary>
    /// 静态构造函数，用于创建一个从起点到达终点的路径
    /// </summary>
    /// <param name="start">起点</param>
    /// <param name="end">终点</param>
    /// <param name="callback">寻路完成的回调</param>
    /// <returns></returns>
    public   static ABBlockPath Construct(Vector3 start, Vector3 end, List<GraphNode> CloseNodes,OnPathDelegate callback = null)
    {


        //从路径池中获取路径
        ABBlockPath p = PathPool<ABBlockPath>.GetPath();
        //初始化


      

        //主要处理endpos
      //  for (int i = 0; i < CloseNodes.Count; i++)
      //  {
      //      if ((Vector3)CloseNodes[i].position == end)
      //      {
      //          CloseNodes.RemoveAt(i);
      //          Debug.Log("移除了end点");
      //          break;
                
      //      }
      //  }

      //  for (int i = 0; i < CloseNodes.Count; i++)
      //  {
      //      if ((Vector3)CloseNodes[i].position == start)
      //      {
      //          CloseNodes.RemoveAt(i);
      //          Debug.Log("移除了start点");
      //          break;

      //      }
      //  }




       

      p.blockNodes = CloseNodes;


        p.Setup(start, end, callback);
        return p;
    }




    /// <summary>
    /// 添加一个需要屏蔽的节点
    /// </summary>
    /// <param name="node"></param>
    public void AddBlockNode(GraphNode node)
    {
        if (!blockNodes.Contains(node))
        {
            blockNodes.Add(node);
        }
    }

    /// <summary>
    /// 删除需要屏蔽的节点
    /// </summary>
    /// <param name="node"></param>
    public void RemoveBlockNode(GraphNode node)
    {
        if (blockNodes.Contains(node))
        {
            blockNodes.Remove(node);
        }
    }

    /// <summary>
    /// 清空所有屏蔽的节点
    /// </summary>
    public void ClearAllBlockNode()
    {
        blockNodes.Clear();
    }
    /// <summary>
    /// 这个方法相当于Close节点
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public override bool CanTraverse(GraphNode node)
    {
        if (blockNodes.Contains(node)) return false;
        return base.CanTraverse(node);
    }

    public override void Prepare()
    {

        AstarProfiler.StartProfile("Get Nearest");

        //Initialize the NNConstraint
        nnConstraint.tags = enabledTags;
        NNInfo startNNInfo = AstarPath.active.GetNearest(startPoint, nnConstraint, startHint);

        //Tell the NNConstraint which node was found as the start node if it is a PathNNConstraint and not a normal NNConstraint
        PathNNConstraint pathNNConstraint = nnConstraint as PathNNConstraint;
        if (pathNNConstraint != null)
        {
            pathNNConstraint.SetStart(startNNInfo.node);
        }

        startPoint = startNNInfo.clampedPosition;

        startIntPoint = (Int3)startPoint;
        startNode = startNNInfo.node;

        //If it is declared that this path type has an end point
        //Some path types might want to use most of the ABPath code, but will not have an explicit end point at this stage
        if (hasEndPoint)
        {
            NNInfo endNNInfo = AstarPath.active.GetNearest(endPoint, nnConstraint, endHint, blockNodes);
            endPoint = endNNInfo.clampedPosition;

            // Note, other methods assume hTarget is (Int3)endPoint
            hTarget = (Int3)endPoint;
            endNode = endNNInfo.node;
        }

        AstarProfiler.EndProfile();

#if ASTARDEBUG
			if (startNode != null)
				Debug.DrawLine ((Vector3)startNode.position,startPoint,Color.blue);
			if (endNode != null)
				Debug.DrawLine ((Vector3)endNode.position,endPoint,Color.blue);
#endif

        if (startNode == null && (hasEndPoint && endNode == null))
        {
            Error();
            LogError("Couldn't find close nodes to the start point or the end point");
            return;
        }

        if (startNode == null)
        {
            Error();
            LogError("Couldn't find a close node to the start point");
            return;
        }

        if (endNode == null && hasEndPoint)
        {
            Error();
            LogError("Couldn't find a close node to the end point");
            return;
        }

        if (!startNode.Walkable)
        {
#if ASTARDEBUG
				Debug.DrawRay (startPoint,Vector3.up,Color.red);
				Debug.DrawLine (startPoint,(Vector3)startNode.position,Color.red);
#endif
            Error();
            LogError("The node closest to the start point is not walkable");
            return;
        }

        if (hasEndPoint && !endNode.Walkable)
        {
            Error();
            LogError("The node closest to the end point is not walkable");
            return;
        }

        if (hasEndPoint && startNode.Area != endNode.Area)
        {
            Error();
            LogError("There is no valid path to the target (start area: " + startNode.Area + ", target area: " + endNode.Area + ")");
            return;
        }
    }
  

}
