using UnityEngine;

namespace Farm.CropPlant
{
    public class CropManager : Singleton<CropManager>
    {
        public CropDataList_SO cropData;
        private Transform cropParent;
        private Grid currentGrid;
        private Season currentSeason;

        #region Lift Function

        private void OnEnable()
        {
            EventHandler.PlantSeedEvent += OnPlantSeed;
            EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoaded;
            EventHandler.GameDayEvent += OnGameDay;
        }

        private void OnDisable()
        {
            EventHandler.PlantSeedEvent -= OnPlantSeed;
            EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoaded;
            EventHandler.GameDayEvent -= OnGameDay;
        }

        #endregion

        #region Event Function

        private void OnPlantSeed(int ID, TileDetails tileDetails)
        {
            CropDetails currentCrop = GetCropDetails(ID);
            // 用于第一次种植
            if (currentCrop != null && SeasonAvailable(currentCrop) && tileDetails.seedItemID == -1)
            {
                tileDetails.seedItemID = ID;
                tileDetails.growthDays = 0;
                // 显示农作物
                DisplayCropPlant(tileDetails, currentCrop);
            }
            // 用于刷新地图
            else if (tileDetails.seedItemID != -1)
            {
                // 显示农作物
                DisplayCropPlant(tileDetails, currentCrop);
            }
        }

        private void OnAfterSceneLoaded()
        {
            currentGrid = FindObjectOfType<Grid>();
            cropParent = GameObject.FindWithTag("CropParent").transform;
        }

        private void OnGameDay(int day, Season season)
        {
            currentSeason = season;
        }

        #endregion

        /// <summary>
        /// 显示农作物
        /// </summary>
        /// <param name="tileDetails">瓦片地图信息</param>
        /// <param name="cropDetails">种子信息</param>
        private void DisplayCropPlant(TileDetails tileDetails, CropDetails cropDetails)
        {
            // 成长阶段
            int growthStages = cropDetails.growthDays.Length;
            int currentStage = 0;
            int dayCounter = cropDetails.TotalGrowthDays;

            // 倒序计算当前的成长阶段
            for (int i = growthStages - 1; i >= 0; i--)
            {
                if (tileDetails.growthDays >= dayCounter)
                {
                    currentStage = i;
                    break;
                }
                dayCounter -= cropDetails.growthDays[i];
            }

            // 获取当前阶段的 Prefab
            GameObject cropPrefab = cropDetails.growthPrefabs[currentStage];
            Sprite cropSprite = cropDetails.growthSprites[currentStage];

            Vector3 pos = new Vector3(tileDetails.gridX + 0.5f, tileDetails.gridY + 0.5f, 0);
            GameObject cropInstance = Instantiate(cropPrefab, pos, Quaternion.identity, cropParent);
            cropInstance.GetComponentInChildren<SpriteRenderer>().sprite = cropSprite;
            cropInstance.GetComponent<Crop>().InitCrop(cropDetails, tileDetails);
        }

        /// <summary>
        /// 通过物品 ID 查找种子信息
        /// </summary>
        /// <param name="ID">物品 ID</param>
        /// <returns></returns>
        public CropDetails GetCropDetails(int ID)
        {
            return cropData.cropDetailsList.Find(c => c.seedItemID == ID);
        }

        /// <summary>
        /// 判断当前季节是否可以种植
        /// </summary>
        /// <param name="crop">种子信息</param>
        /// <returns></returns>
        private bool SeasonAvailable(CropDetails crop)
        {
            for (int i = 0; i < crop.seasons.Length; i++)
            {
                if (crop.seasons[i] == currentSeason)
                    return true;
            }
            return false;
        }
    }
}