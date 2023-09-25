using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using Farm.Save;
using Farm.CropPlant;

namespace Farm.Map
{
    [RequireComponent(typeof(DataGUID))]
    public class GridMapManager : Singleton<GridMapManager>, ISaveable
    {
        [Header("Tile Info")]
        public RuleTile digTile;
        public RuleTile waterTile;
        private Tilemap digTilemap;
        private Tilemap waterTilemap;

        [Header("Map Info")]
        public List<MapData_SO> mapDataList;

        // 场景名字 + 坐标和对应的瓦片信息
        private Dictionary<string, TileDetails> tileDetailsDict = new Dictionary<string, TileDetails>();

        // 记录场景是否第一次加载
        private Dictionary<string, bool> firstLoadDict = new Dictionary<string, bool>();

        private Grid currentGrid;
        private Season currentSeason;

        private List<ReapItem> itemsInRadius;

        public string GUID => GetComponent<DataGUID>().guid;

        #region Life Function

        private void OnEnable()
        {
            EventHandler.ExecuteActionAfterAnimationEvent += OnExecuteActionAfterAnimation;
            EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoaded;
            EventHandler.GameDayEvent += OnGameDay;
            EventHandler.RefreshCurrentMapEvent += OnRefreshMap;
        }

        private void OnDisable()
        {
            EventHandler.ExecuteActionAfterAnimationEvent -= OnExecuteActionAfterAnimation;
            EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoaded;
            EventHandler.GameDayEvent -= OnGameDay;
            EventHandler.RefreshCurrentMapEvent -= OnRefreshMap;
        }

        private void Start()
        {
            ISaveable saveable = this;
            saveable.RegisterSaveable();

            foreach (MapData_SO mapData in mapDataList)
            {
                firstLoadDict.Add(mapData.sceneName, true);
                InitTileDetailsDict(mapData);
            }
        }

        #endregion

        #region Event Function

        /// <summary>
        /// 执行实际工具或物品功能
        /// </summary>
        /// <param name="mouseWorldPos">鼠标坐标</param>
        /// <param name="itemDetails">物品信息</param>
        private void OnExecuteActionAfterAnimation(Vector3 mouseWorldPos, ItemDetails itemDetails)
        {
            Vector3Int mouseGridPos = currentGrid.WorldToCell(mouseWorldPos);
            TileDetails currentTile = GetTileDetailsOnMousePosition(mouseGridPos);

            if (currentTile != null)
            {
                switch (itemDetails.itemType)
                {
                    case ItemType.Seed:
                        EventHandler.CallPlantSeedEvent(itemDetails.itemID, currentTile);
                        EventHandler.CallDropItemEvent(itemDetails.itemID, mouseGridPos, itemDetails.itemType);
                        EventHandler.CallPlaySoundEvent(SoundName.Plant);
                        break;
                    case ItemType.Commodity:
                        EventHandler.CallDropItemEvent(itemDetails.itemID, mouseWorldPos, itemDetails.itemType);
                        break;
                    case ItemType.HoeTool:
                        SetDigGround(currentTile);
                        // 修改挖坑相关数值
                        currentTile.daysSinceDug = 0;
                        currentTile.canDig = false;
                        currentTile.canDropItem = false;
                        // 添加音效
                        EventHandler.CallPlaySoundEvent(SoundName.Hoe);
                        break;
                    case ItemType.WaterTool:
                        SetWaterGround(currentTile);
                        // 修改浇水相关数值
                        currentTile.daysSinceWatered = 0;
                        // 添加音效
                        EventHandler.CallPlaySoundEvent(SoundName.Water);
                        break;
                    case ItemType.ChopTool:
                    case ItemType.BreakTool:
                        Crop chopCrop = GetCropObject(mouseWorldPos);
                        chopCrop?.ProcessToolAction(itemDetails, chopCrop.tileDetails);
                        break;
                    case ItemType.CollectTool:
                        Crop collectCrop = GetCropObject(mouseWorldPos);
                        collectCrop?.ProcessToolAction(itemDetails, currentTile);
                        break;
                    case ItemType.ReapTool:
                        int count = 0;
                        for (int i = 0; i < itemsInRadius.Count && count < Settings.reapAmount; i++, count++)
                        {
                            ReapItem item = itemsInRadius[i];
                            EventHandler.CallParticalEffectEvent(ParticalEffectType.ReapableScenery, item.transform.position + Vector3.up);
                            item.SpawnHarvestItems();
                            Destroy(item.gameObject);
                        }
                        EventHandler.CallPlaySoundEvent(SoundName.Reap);
                        break;
                    case ItemType.Furniture:
                        // 建造物体，调用事件
                        // 在地图上生成物品
                        // 移除当前物品(图纸)
                        // 移除建造消耗的资源物品
                        EventHandler.CallBuildFurnitureEvent(itemDetails.itemID, mouseWorldPos);
                        break;
                }
                UpdateTileDetails(currentTile);
            }
        }

        private void OnAfterSceneLoaded()
        {
            currentGrid = FindObjectOfType<Grid>();
            digTilemap = GameObject.FindWithTag("Dig").GetComponent<Tilemap>();
            waterTilemap = GameObject.FindWithTag("Water").GetComponent<Tilemap>();

            string sceneName = SceneManager.GetActiveScene().name;
            // 第一次加载当前场景
            if(firstLoadDict[sceneName])
            {
                // 预先生成农作物
                EventHandler.CallGenerateCropEvent();
                firstLoadDict[sceneName] = false;
            }

            OnRefreshMap();
        }

        /// <summary>
        /// 每天刷新地图状态的事件
        /// </summary>
        /// <param name="day"></param>
        /// <param name="season"></param>
        private void OnGameDay(int day, Season season)
        {
            currentSeason = season;

            foreach (var tile in tileDetailsDict)
            {
                if (tile.Value.daysSinceWatered > -1)
                {
                    tile.Value.daysSinceWatered = -1;
                }
                if (tile.Value.daysSinceDug > -1)
                {
                    tile.Value.daysSinceDug++;
                }
                // 挖过的坑超过 5 天没种子，消除坑状态
                if (tile.Value.daysSinceDug > 5 && tile.Value.seedItemID == -1)
                {
                    tile.Value.daysSinceDug = -1;
                    tile.Value.canDig = true;
                    tile.Value.growthDays = -1;
                }
                // 有种子种下，种植时间增加
                if(tile.Value.seedItemID > -1)
                {
                    tile.Value.growthDays++;
                }
            }

            OnRefreshMap();
        }

        /// <summary>
        /// 刷新当前地图
        /// </summary>
        private void OnRefreshMap()
        {
            digTilemap?.ClearAllTiles();
            waterTilemap?.ClearAllTiles();

            foreach(var crop in FindObjectsOfType<Crop>())
            {
                Destroy(crop.gameObject);
            }

            DisplayMap(SceneManager.GetActiveScene().name);
        }

        #endregion

        /// <summary>
        /// 根据地图信息生成字典
        /// </summary>
        /// <param name="mapData">地图信息</param>
        private void InitTileDetailsDict(MapData_SO mapData)
        {
            foreach (TileProperty tileProperty in mapData.tileProperties)
            {
                TileDetails tileDetails = new TileDetails
                {
                    gridX = tileProperty.tileCoordinate.x,
                    gridY = tileProperty.tileCoordinate.y
                };

                //字典的 Key
                string key = tileDetails.gridX + "x" + tileDetails.gridY + "y" + mapData.sceneName;

                if (GetTileDetails(key) != null)
                {
                    tileDetails = GetTileDetails(key);
                }

                switch (tileProperty.gridType)
                {
                    case GridType.Diggable:
                        tileDetails.canDig = tileProperty.boolTypeValue;
                        break;
                    case GridType.DropItem:
                        tileDetails.canDropItem = tileProperty.boolTypeValue;
                        break;
                    case GridType.PlaceFurniture:
                        tileDetails.canPlaceFurniture = tileProperty.boolTypeValue;
                        break;
                    case GridType.NPCObstacle:
                        tileDetails.isNPCObstacle = tileProperty.boolTypeValue;
                        break;
                }

                // 判断字典中是否已经有这个格子信息
                if (GetTileDetails(key) != null)
                {
                    tileDetailsDict[key] = tileDetails;
                }
                else
                {
                    tileDetailsDict.Add(key, tileDetails);
                }
            }
        }

        /// <summary>
        /// 根据 key 返回瓦片信息
        /// </summary>
        /// <param name="key">x + y + 地图名字</param>
        /// <returns></returns>
        public TileDetails GetTileDetails(string key)
        {
            if (tileDetailsDict.ContainsKey(key))
            {
                return tileDetailsDict[key];
            }
            return null;
        }

        /// <summary>
        /// 根据网格坐标获取瓦片详细信息
        /// </summary>
        /// <param name="mouseGridPos"></param>
        /// <returns></returns>
        public TileDetails GetTileDetailsOnMousePosition(Vector3Int mouseGridPos)
        {
            string key = mouseGridPos.x + "x" + mouseGridPos.y + "y" + SceneManager.GetActiveScene().name;
            return GetTileDetails(key);
        }

        /// <summary>
        /// 设置当前瓦片为挖坑状态
        /// </summary>
        /// <param name="tile"></param>
        private void SetDigGround(TileDetails tile)
        {
            Vector3Int pos = new Vector3Int(tile.gridX, tile.gridY, 0);
            digTilemap?.SetTile(pos, digTile);
        }

        /// <summary>
        /// 设置当前瓦片为浇水状态
        /// </summary>
        /// <param name="tile"></param>
        private void SetWaterGround(TileDetails tile)
        {
            Vector3Int pos = new Vector3Int(tile.gridX, tile.gridY, 0);
            waterTilemap?.SetTile(pos, waterTile);
        }

        /// <summary>
        /// 更新瓦片信息
        /// </summary>
        /// <param name="tileDetails"></param>
        public void UpdateTileDetails(TileDetails tileDetails)
        {
            string key = tileDetails.gridX + "x" + tileDetails.gridY + "y" + SceneManager.GetActiveScene().name;
            if (tileDetailsDict.ContainsKey(key))
            {
                tileDetailsDict[key] = tileDetails;
            }
            else
            {
                tileDetailsDict.Add(key, tileDetails);
            }
        }

        /// <summary>
        /// 显示地图瓦片
        /// </summary>
        /// <param name="sceneName">场景名字</param>
        private void DisplayMap(string sceneName)
        {
            foreach (var tile in tileDetailsDict)
            {
                var key = tile.Key;
                var tileDetails = tile.Value;

                if (key.Contains(sceneName))
                {
                    if (tileDetails.daysSinceDug > -1)
                        SetDigGround(tileDetails);
                    if (tileDetails.daysSinceWatered > -1)
                        SetWaterGround(tileDetails);
                    if(tileDetails.seedItemID > -1)
                        EventHandler.CallPlantSeedEvent(tileDetails.seedItemID, tileDetails);
                }
            }
        }

        /// <summary>
        /// 通过物理方法判断鼠标点击位置的农作物
        /// </summary>
        /// <param name="mouseWorldPos">鼠标坐标</param>
        /// <returns></returns>
        public Crop GetCropObject(Vector3 mouseWorldPos)
        {
            // 获取鼠标位置周围的碰撞体
            Collider2D[] colliders = Physics2D.OverlapPointAll(mouseWorldPos);

            Crop currentCrop = null;

            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i].GetComponent<Crop>())
                    currentCrop = colliders[i].GetComponent<Crop>();
            }
            return currentCrop;
        }

        /// <summary>
        /// 判断鼠标周围是否有 Reap 类型物体
        /// </summary>
        /// <param name="tool"></param>
        /// <returns></returns>
        public bool HaveReapableItemsInRadius(Vector3 mouseWorldPos, ItemDetails tool)
        {
            itemsInRadius = new List<ReapItem>();
            Collider2D[] colliders = new Collider2D[20];

            Physics2D.OverlapCircleNonAlloc(mouseWorldPos, tool.itemUseRadius, colliders);

            if(colliders.Length > 0)
            {
                foreach(Collider2D collider in colliders)
                {
                    if(collider != null && collider.GetComponent<ReapItem>())
                    {
                        ReapItem reapItem = collider.GetComponent<ReapItem>();
                        itemsInRadius.Add(reapItem);
                    }
                }
            }

            return itemsInRadius.Count > 0;
        }


        /// <summary>
        /// 根据场景名字构建网格范围，输出范围和原点
        /// </summary>
        /// <param name="sceneName">场景名字</param>
        /// <param name="gridDimensions">网格范围</param>
        /// <param name="gridOrigin">网格原点</param>
        /// <returns>是否有当前场景的信息</returns>
        public bool GetGridDimensions(string sceneName, out Vector2Int gridDimensions, out Vector2Int gridOrigin)
        {
            gridDimensions = Vector2Int.zero;
            gridOrigin = Vector2Int.zero;

            foreach (var mapData in mapDataList)
            {
                if (mapData.sceneName == sceneName)
                {
                    gridDimensions.x = mapData.gridWidth;
                    gridDimensions.y = mapData.gridHeight;

                    gridOrigin.x = mapData.originX;
                    gridOrigin.y = mapData.originY;

                    return true;
                }
            }
            return false;
        }

        public GameSaveData GenerateSaveData()
        {
            GameSaveData saveData = new GameSaveData();
            saveData.tileDetailsDict = this.tileDetailsDict;
            saveData.firstLoadDict = this.firstLoadDict;

            return saveData;
        }

        public void RestoreData(GameSaveData saveData)
        {
            this.tileDetailsDict = saveData.tileDetailsDict;
            this.firstLoadDict = saveData.firstLoadDict;
        }
    }
}