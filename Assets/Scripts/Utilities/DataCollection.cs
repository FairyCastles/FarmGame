using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
// 用于存储物体的详细信息
public class ItemDetails
{
    public int itemID;
    public string itemName;
    public ItemType itemType;

    public Sprite itemIcon;
    // 物体在世界地图上的图标
    public Sprite itemOnWorldSprite;
    public string itemDescription;

    public int itemPrice;
    // 物品售卖时相较于买入价格的折扣
    [Range(0, 1)]
    public float sellPercentage;

    public int itemUseRadius;
    public bool canPickedup;
    public bool canDropped;
    public bool canCarried;
}

[System.Serializable]
// 用于存储背包中的物体信息
// 结构体数据是值传递，会默认初始化值
// 当 itemID = 0 时，说明当前格子没有物体
public struct InventoryItem
{
    public int itemID;
    public int itemAmount;
}

[System.Serializable]
// 用于存储 Animator 类型
public class AnimatorType
{
    public PartName partName;
    public PartType partType;
    public AnimatorOverrideController overrideController;
}

// 可序列化的 Vector3，用于数据序列化
[System.Serializable]
public class SerializableVector3
{
    public float x, y, z;

    public SerializableVector3(Vector3 pos)
    {
        x = pos.x;
        y = pos.y;
        z = pos.z;
    }

    public Vector3 ToVector3()
    {
        return new Vector3(x, y, z);
    }

    public Vector2Int ToVector2Int()
    {
        return new Vector2Int((int)x, (int)y);
    }
}

// 用于存储场景中物体信息，包含 ID 和坐标
[System.Serializable]
public class SceneItem
{
    public int itemID;
    public SerializableVector3 position;
}

[System.Serializable]
public class SceneFurniture
{
    public int itemID;
    // TODO: 这个只有箱子才会用到，应该分离出来
    public int boxIndex;
    public SerializableVector3 position;
}

// 瓦片地图格子的类型信息
[System.Serializable]
public class TileProperty
{
    public Vector2Int tileCoordinate;
    public GridType gridType;
    public bool boolTypeValue;
}

// 瓦片地图格子的详细信息
[System.Serializable]
public class TileDetails
{
    public int gridX, gridY;
    public bool canDig;
    public bool canDropItem;
    public bool canPlaceFurniture;
    public bool isNPCObstacle;
    public int daysSinceDug = -1;
    public int daysSinceWatered = -1;
    public int seedItemID = -1;
    public int growthDays = -1;
    public int daysSinceLastHarvest = -1;
}

[System.Serializable]
public class NPCPosition
{
    public Transform npcTransform;
    public string startScene;
    public Vector3 position;
}

// NPC 跨场景的路径，包含多个场景的路径
[System.Serializable]
public class SceneRoute
{
    public string fromSceneName;
    public string gotoSceneName;
    public List<ScenePath> scenePathList;
}

// NPC 同场景移动的路径
[System.Serializable]
public class ScenePath
{
    public string sceneName;
    public Vector2Int fromGridCell;
    public Vector2Int gotoGridCell;
}