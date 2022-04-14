//#define ASTARDEBUG //Draws a ray for each node visited

using UnityEngine;
using System;
using System.Collections.Generic;
using Pathfinding;

namespace Pathfinding
{

    /// <summary>
    /// 恒定消耗寻路类的改进版，加入屏蔽节点的功能
    /// </summary>
    public class ConstantBlockPath : Path
    {


        public List<GraphNode> blockNodes;

        public GraphNode startNode;
        public Vector3 startPoint;
        public Vector3 originalStartPoint;

        /** Contains all nodes the path found.
          * \note Due to the nature of the search, there might be duplicates of some nodes in the array.
          * This list will be sorted by G score (cost/distance to reach the node), however only the last duplicate of a node in the list is guaranteed to be sorted in this way.
          */
        /// <summary>
        /// 包含所有节点的路径。
        ///注意由于搜索的性质,可能有一些节点数组中的重复。
        ///这个列表将由G值排序(成本/距离达到节点),然而只有最后复制列表中的一个节点是保证以这种方式排序。
        /// </summary>
        public List<GraphNode> allNodes;

        /** Controls when the path should terminate.
         * This is set up automatically in the constructor to an instance of the Pathfinding.EndingConditionDistance class with a \a maxGScore is specified in the constructor.
         * If you want to use another ending condition.
         * \see Pathfinding.PathEndingCondition for examples
         */
        ///控制路径应该终止 maxGScore是在构造函数中指定。*如果你想使用另一个结束条件。
        ///* \看到寻路。PathEndingCondition为例子
        public PathEndingCondition endingCondition;

        public ConstantBlockPath() : base() { }





        /** Constructs a ConstantPath starting from the specified point.
         * 构造一个ConstantPath从指定的点
         * 
         * \param start 			From where the path will be started from (the closest node to that point will be used)
         * 路径的参数从将从(最接近的节点将使用这一点)

         * \param maxGScore			Searching will be stopped when a node has a G score greater than this
         * param maxgscore 搜索时将停止一个节点有一个G值比这个更大的

         * \param callback			Will be called when the path has completed, leave this to null if you use a Seeker to handle calls
         * 参数完成回调时将调用路径,离开这个为null如果使用导引头处理调用

         * Searching will be stopped when a node has a G score (cost to reach it) greater than \a maxGScore */
        //搜索时将停止一个节点有一个G值(成本达到)大于\ maxGScore * 




        public static ConstantBlockPath Construct(Vector3 start, int maxGScore, OnPathDelegate callback = null)
        {
            
            ConstantBlockPath p = PathPool<ConstantBlockPath>.GetPath();
            p.Setup(start, maxGScore, callback);
            return p;
        }






        /** Sets up a ConstantPath starting from the specified point 
         设置一个ConstantPath从指定点*/
        protected void Setup(Vector3 start, int maxGScore, OnPathDelegate callback)
        {
            this.callback = callback;
            startPoint = start;
            originalStartPoint = startPoint;

            endingCondition = new EndingConditionDistance(this, maxGScore);
        }

        public override void OnEnterPool()
        {
            base.OnEnterPool();
            if (allNodes != null) Util.ListPool<GraphNode>.Release(allNodes);
        }

        protected override void Recycle()
        {
            PathPool<ConstantBlockPath>.Recycle(this);
        }

        /** Reset the path to default values.
         * Clears the #allNodes list.
         * \note This does not reset the #endingCondition.
         * 
         * Also sets #heuristic to Heuristic.None as it is the default value for this path type
         */
        public override void Reset()
        {
            base.Reset();
            allNodes = Util.ListPool<GraphNode>.Claim();
            endingCondition = null;
            originalStartPoint = Vector3.zero;
            startPoint = Vector3.zero;
            startNode = null;
            heuristic = Heuristic.None;
        }

        public override void Prepare()
        {
            nnConstraint.tags = enabledTags;
            NNInfo startNNInfo = AstarPath.active.GetNearest(startPoint, nnConstraint);

            startNode = startNNInfo.node;
            if (startNode == null)
            {
                Error();
                LogError("Could not find close node to the start point");
                return;
            }
        }

        /** Initializes the path.
         * Sets up the open list and adds the first node to it */
        public override void Initialize()
        {

            PathNode startRNode = pathHandler.GetPathNode(startNode);
            startRNode.node = startNode;
            startRNode.pathID = pathHandler.PathID;
            startRNode.parent = null;
            startRNode.cost = 0;
            startRNode.G = GetTraversalCost(startNode);
            startRNode.H = CalculateHScore(startNode);

            startNode.Open(this, startRNode, pathHandler);

            searchedNodes++;

            startRNode.flag1 = true;
            allNodes.Add(startNode);

            //any nodes left to search?
            if (pathHandler.HeapEmpty())
            {
                CompleteState = PathCompleteState.Complete;
                return;
            }

            currentR = pathHandler.PopNode();
        }

        public override void Cleanup()
        {
            int c = allNodes.Count;
            for (int i = 0; i < c; i++) pathHandler.GetPathNode(allNodes[i]).flag1 = false;
        }

        public override void CalculateStep(long targetTick)
        {

            int counter = 0;

            //Continue to search while there hasn't ocurred an error and the end hasn't been found
            while (CompleteState == PathCompleteState.NotCalculated)
            {

                searchedNodes++;

                //--- Here's the important stuff				
                //Close the current node, if the current node satisfies the ending condition, the path is finished
                if (endingCondition.TargetFound(currentR))
                {
                    CompleteState = PathCompleteState.Complete;
                    break;
                }

                if (!currentR.flag1)
                {
                    //Add Node to allNodes
                    allNodes.Add(currentR.node);
                    currentR.flag1 = true;
                }

#if ASTARDEBUG
				Debug.DrawRay ((Vector3)currentR.node.position,Vector3.up*5,Color.cyan);
#endif

                //--- Here the important stuff ends

                AstarProfiler.StartFastProfile(4);
                //Debug.DrawRay ((Vector3)currentR.node.Position, Vector3.up*2,Color.red);

                //Loop through all walkable neighbours of the node and add them to the open list.
                currentR.node.Open(this, currentR, pathHandler);

                AstarProfiler.EndFastProfile(4);

                //any nodes left to search?
                if (pathHandler.HeapEmpty())
                {
                    CompleteState = PathCompleteState.Complete;
                    break;
                }


                //Select the node with the lowest F score and remove it from the open list
                AstarProfiler.StartFastProfile(7);
                if (blockNodes != null)
                {
                    while (true)
                    {
                        if (pathHandler.HeapEmpty())
                        {
                            CompleteState = PathCompleteState.Complete;
                            return;
                        }
                        currentR = pathHandler.PopNode();
                        if (blockNodes.Contains(currentR.node))
                        {
                            continue;
                        }
                        break;
                    }
                }
                else
                {
                    currentR = pathHandler.PopNode();
                }
                AstarProfiler.EndFastProfile(7);

                //Check for time every 500 nodes, roughly every 0.5 ms usually
                if (counter > 500)
                {

                    //Have we exceded the maxFrameTime, if so we should wait one frame before continuing the search since we don't want the game to lag
                    if (System.DateTime.UtcNow.Ticks >= targetTick)
                    {
                        //Return instead of yield'ing, a separate function handles the yield (CalculatePaths)
                        return;
                    }
                    counter = 0;

                    if (searchedNodes > 1000000)
                    {
                        throw new System.Exception("Probable infinite loop. Over 1,000,000 nodes searched");
                    }
                }

                counter++;
            }
        }

        internal static ConstantPath Construct(GraphNode position, int mobility, OnPathDelegate onPathDelegate)
        {
            throw new NotImplementedException();
        }
    }
}