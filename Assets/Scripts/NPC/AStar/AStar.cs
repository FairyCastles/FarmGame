using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Farm.CropPlant;
using Farm.Map;

namespace Farm.NPC
{
    public class AStar : Singleton<AStar>
    {
        private GridNodes gridNodes;
        private Node startNode;
        private Node targetNode;
        private int gridWidth;
        private int gridHeight;
        private int originX;
        private int originY;

        // 开放集，代表所有待探索的节点，按照优先度排序
        private PriorityQueue<Node> openNodeQueue;
        // 保存所有开放集的节点
        private HashSet<Node> openNodeSet;
        // 闭合集，代表所有已经探索过的节点
        private HashSet<Node> closeNodeSet;

        private bool pathFound;

        public void BuildPath(string sceneName, Vector2Int startPos, Vector2Int endPos, Stack<MovementStep> movementStepsStack)
        {
            pathFound = false;

            if (GenerateGridNodes(sceneName, startPos, endPos))
            {
                // 查找最短路径
                if (FindShortestPath())
                {
                    // 构建NPC移动路径
                    UpdatePathOnMovmentStepStack(sceneName, movementStepsStack);
                }
            }
        }

        /// <summary>
        /// 构建网格节点信息，初始化两个列表
        /// </summary>
        /// <param name="sceneName">场景名字</param>
        /// <param name="startPos">起点</param>
        /// <param name="endPos">终点</param>
        /// <returns></returns>
        private bool GenerateGridNodes(string sceneName, Vector2Int startPos, Vector2Int endPos)
        {
            if (GridMapManager.Instance.GetGridDimensions(sceneName, out Vector2Int gridDimensions, out Vector2Int gridOrigin))
            {
                // 根据瓦片地图范围构建网格移动节点范围数组
                gridNodes = new GridNodes(gridDimensions.x, gridDimensions.y);
                gridWidth = gridDimensions.x;
                gridHeight = gridDimensions.y;
                originX = gridOrigin.x;
                originY = gridOrigin.y;

                openNodeQueue = new PriorityQueue<Node>();
                openNodeSet = new HashSet<Node>();
                closeNodeSet = new HashSet<Node>();
            }
            else return false;

            // gridNodes 的范围是从 0,0 开始所以需要减去原点坐标得到实际位置
            startNode = gridNodes.GetGridNode(startPos.x - originX, startPos.y - originY);
            targetNode = gridNodes.GetGridNode(endPos.x - originX, endPos.y - originY);

            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    Vector3Int tilePos = new Vector3Int(x + originX, y + originY, 0);

                    string key = tilePos.x + "x" + tilePos.y + "y" + sceneName;
                    TileDetails tile = GridMapManager.Instance.GetTileDetails(key);

                    if (tile != null)
                    {
                        Node node = gridNodes.GetGridNode(x, y);

                        if (tile.isNPCObstacle)
                            node.isObstacle = true;
                    }
                }
            }

            return true;
        }


        /// <summary>
        /// 寻找到最短的路径
        /// </summary>
        /// <returns></returns>
        private bool FindShortestPath()
        {
            // 添加起点
            openNodeQueue.Enqueue(startNode);
            openNodeSet.Add(startNode);

            while (openNodeSet.Count > 0)
            {
                Node currentNode = openNodeQueue.Dequeue();
                // 当开放集重新计算相同点的权重时，会把相同点的信息放入队列中
                // 所以当这个点已经在闭合集时，说明一个权重更小的相同点已经被遍历过了
                // 则跳过这个节点
                if(closeNodeSet.Contains(currentNode))  continue;
                openNodeSet.Remove(currentNode);
                closeNodeSet.Add(currentNode);

                if (currentNode == targetNode)
                {
                    pathFound = true;
                    break;
                }

                //计算周围 8 个 Node 补充到开放集中
                EvaluateNeighbourNodes(currentNode);
            }

            return pathFound;
        }


        /// <summary>
        /// 评估周围 8 个点，并生成对应消耗值
        /// </summary>
        /// <param name="currentNode"></param>
        private void EvaluateNeighbourNodes(Node currentNode)
        {
            Vector2Int currentNodePos = currentNode.gridPosition;
            Node validNeighbourNode;

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0) continue;

                    validNeighbourNode = GetValidNeighbourNode(currentNodePos.x + x, currentNodePos.y + y);

                    if (validNeighbourNode != null)
                    {
                        // 不在开放集中，将其加入开放集
                        if (!openNodeSet.Contains(validNeighbourNode))
                        {
                            validNeighbourNode.gCost = currentNode.gCost + GetDistance(currentNode, validNeighbourNode);
                            validNeighbourNode.hCost = GetDistance(validNeighbourNode, targetNode);
                            // 链接父节点
                            validNeighbourNode.parentNode = currentNode;
                            openNodeQueue.Enqueue(validNeighbourNode);
                            openNodeSet.Add(validNeighbourNode);
                        }
                        // 在开放集中，重新计算
                        else
                        {
                            int gCost = currentNode.gCost + GetDistance(currentNode, validNeighbourNode);
                            // 更新父节点为当前节点，重新计算权重，放入队列中
                            // 并不从队列中删去这个点，而是在取出点时判断这个点是否已经在闭合集中
                            if(gCost < validNeighbourNode.gCost)
                            {
                                validNeighbourNode.gCost = gCost;
                                validNeighbourNode.hCost = GetDistance(validNeighbourNode, targetNode);
                                validNeighbourNode.parentNode = currentNode;
                                openNodeQueue.Enqueue(validNeighbourNode);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 找到有效的 Node
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private Node GetValidNeighbourNode(int x, int y)
        {
            if (x >= gridWidth || y >= gridHeight || x < 0 || y < 0) return null;

            Node neighbourNode = gridNodes.GetGridNode(x, y);

            if (neighbourNode.isObstacle || closeNodeSet.Contains(neighbourNode)) return null;
            
            return neighbourNode;
        }

        /// <summary>
        /// 返回两点距离值
        /// </summary>
        /// <param name="nodeA"></param>
        /// <param name="nodeB"></param>
        /// <returns></returns>
        private int GetDistance(Node nodeA, Node nodeB)
        {
            int xDistance = Mathf.Abs(nodeA.gridPosition.x - nodeB.gridPosition.x);
            int yDistance = Mathf.Abs(nodeA.gridPosition.y - nodeB.gridPosition.y);

            // 默认先执行 X 移动，设置为 10 倍数
            // 存在 X，Y的位移，则要进行斜方向移动，设置为 14 倍数
            if (xDistance > yDistance)
            {
                return 14 * yDistance + 10 * (xDistance - yDistance);
            }
            return 14 * xDistance + 10 * (yDistance - xDistance);
        }
        
        private void UpdatePathOnMovmentStepStack(string sceneName, Stack<MovementStep> movementStepsStack)
        {
            Node nextNode = targetNode;

            while(nextNode != null)
            {
                MovementStep newStep = new MovementStep
                {
                    sceneName = sceneName,
                    gridCoordinate = new Vector2Int(nextNode.gridPosition.x + originX, nextNode.gridPosition.y + originY)
                };

                movementStepsStack.Push(newStep);
                nextNode = nextNode.parentNode;
            }
        }
    }
}