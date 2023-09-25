using UnityEngine;

namespace Farm.CropPlant
{
    [System.Serializable]
    public class CropDetails
    {
        public int seedItemID;

        [Header("Growth")]
        public int[] growthDays;
        public int TotalGrowthDays
        {
            get
            {
                int amount = 0;
                foreach (var days in growthDays)
                {
                    amount += days;
                }
                return amount;
            }
        }
        public GameObject[] growthPrefabs;
        public Sprite[] growthSprites;
        public Season[] seasons;

        [Space]
        [Header("Tool")]
        public int[] harvestToolItemID;
        public int[] requireActionCount;
        public int transferItemID;

        [Space]
        [Header("Produce Item")]
        public int[] producedItemID;
        public int[] producedMinAmount;
        public int[] producedMaxAmount;
        public Vector2 spawnRadius;

        public int daysToRegrow;
        public int regrowTimes;

        [Header("Options")]
        public bool generateAtPlayerPosition;
        public bool hasAnimation;
        public bool hasParticalEffect;

        [Header("Effect")]
        public ParticalEffectType effectType;
        public Vector3 effectPos;
        public SoundName soundEffect;

        /// <summary>
        /// 检查当前工具是否可以收割该植物
        /// </summary>
        /// <param name="toolID"></param>
        /// <returns></returns>
        public bool CheckToolAvailable(int toolID)
        {
            foreach(var tool in harvestToolItemID)
            {
                if(tool == toolID)  return true;
            }
            return false;
        }

        /// <summary>
        /// 判断当前工具需要使用的次数
        /// </summary>
        /// <param name="toolID"></param>
        /// <returns></returns>
        public int GetTotalRequireCount(int toolID)
        {
            for(int i = 0; i < harvestToolItemID.Length; i++)
            {
                if(harvestToolItemID[i] == toolID)
                    return requireActionCount[i];
            }
            return -1;
        }

    }
}