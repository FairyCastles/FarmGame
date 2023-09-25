using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Farm.NPC
{
    public class Node : IComparable<Node>
    {
        // 网格坐标
        public Vector2Int gridPosition;
        // 距离 Start 格子的距离
        public int gCost = 0;
        // 距离 Target 格子的距离
        public int hCost = 0;
        // 当前格子的代价值
        public int FCost => gCost + hCost;
        // 当前格子是否是障碍 
        public bool isObstacle = false;
        // 父节点
        public Node parentNode;

        public Node(Vector2Int pos)
        {
            gridPosition = pos;
            parentNode = null;
        }

        public int CompareTo(Node other)
        {
            // 比较选出最低的 F 值，返回 -1，0，1
            int result = FCost.CompareTo(other.FCost);
            if (result == 0)
            {
                result = hCost.CompareTo(other.hCost);
            }
            return result;
        }
    }
}