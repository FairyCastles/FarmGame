using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Farm.CropPlant
{
    public class Crop : MonoBehaviour
    {
        [HideInInspector]
        public CropDetails cropDetails;
        private int harvestActionCount;
        [HideInInspector]
        public TileDetails tileDetails;

        private Animator animator;
        private Transform PlayerTransform => FindObjectOfType<PlayerController>().transform;

        public bool CanHarvest => tileDetails.growthDays >= cropDetails.TotalGrowthDays;

        public void InitCrop(CropDetails cropDetails, TileDetails tileDetails)
        {
            this.cropDetails = cropDetails;
            this.tileDetails = tileDetails;
        }

        /// <summary>
        /// 执行工具收获物品的操作
        /// </summary>
        /// <param name="tool"></param>
        public void ProcessToolAction(ItemDetails tool, TileDetails tile)
        {
            // 收割需要的工具使用次数
            int requireActionCount = cropDetails.GetTotalRequireCount(tool.itemID);
            if (requireActionCount == -1) return;

            animator = GetComponentInChildren<Animator>();

            // 记录工具使用次数，是否满足收割要求
            if (harvestActionCount < requireActionCount)
            {
                harvestActionCount++;

                // 判断是否有动画，一般是播放砍树的动画
                if (animator != null && cropDetails.hasAnimation)
                {
                    // 玩家在树的左边
                    if (PlayerTransform.position.x < transform.position.x)
                    {
                        animator.SetTrigger("RotateRight");
                    }
                    else
                    {
                        animator.SetTrigger("RotateLeft");
                    }
                }
                // 播放粒子特效
                if (cropDetails.hasParticalEffect)
                {
                    EventHandler.CallParticalEffectEvent(cropDetails.effectType, transform.position + cropDetails.effectPos);
                }
                // 播放声音
                if (cropDetails.soundEffect != SoundName.None)
                {
                    EventHandler.CallPlaySoundEvent(cropDetails.soundEffect);
                }
            }

            // 到达工具使用次数需求
            if (harvestActionCount >= requireActionCount)
            {
                if (cropDetails.generateAtPlayerPosition || !cropDetails.hasAnimation)
                {
                    //生成农作物
                    SpawnHarvestItems();
                }
                else if(cropDetails.hasAnimation)
                {
                    if(PlayerTransform.position.x < transform.position.x)
                    {
                        animator.SetTrigger("FallingRight");
                    }
                    else
                    {
                        animator.SetTrigger("FallingLeft");
                    }
                    EventHandler.CallPlaySoundEvent(SoundName.TreeFalling);
                    StartCoroutine(HarvestAfterAnimation());
                }
            }
        }

        /// <summary>
        /// 播放完动画，生成产物
        /// </summary>
        /// <returns></returns>
        private IEnumerator HarvestAfterAnimation()
        {
            // 等待动画播放结束
            // TODO: 这个协程完全可以在动画末尾调用函数来代替
            while (!animator.GetCurrentAnimatorStateInfo(0).IsName("End"))
            {
                yield return null;
            }

            SpawnHarvestItems();
            // 转换新物体
            if (cropDetails.transferItemID > 0)
            {
                CreateTransferCrop();
            }
        }

        /// <summary>
        /// 生成转换的物体
        /// </summary>
        private void CreateTransferCrop()
        {
            tileDetails.seedItemID = cropDetails.transferItemID;
            tileDetails.daysSinceLastHarvest = -1;
            tileDetails.growthDays = 0;

            EventHandler.CallRefreshCurrentMapEvent();
        }

        /// <summary>
        /// 生成果实
        /// </summary>
        public void SpawnHarvestItems()
        {
            for (int i = 0; i < cropDetails.producedItemID.Length; i++)
            {
                int amountToProduce;

                // 物体固定数量
                if (cropDetails.producedMinAmount[i] == cropDetails.producedMaxAmount[i])
                {
                    amountToProduce = cropDetails.producedMinAmount[i];
                }
                // 物品随机数量
                else    
                {
                    amountToProduce = Random.Range(cropDetails.producedMinAmount[i], cropDetails.producedMaxAmount[i] + 1);
                }

                // 执行生成指定数量的物品
                for (int j = 0; j < amountToProduce; j++)
                {
                    if (cropDetails.generateAtPlayerPosition)
                    {
                        EventHandler.CallHarvestAtPlayerPositionEvent(cropDetails.producedItemID[i]);
                    }
                    // 世界地图上生成物品
                    else    
                    {
                        // 判断应该生成的物品方向
                        var dirX = transform.position.x > PlayerTransform.position.x ? 1 : -1;
                        // 物体一定范围内的随机
                        var spawnPos = new Vector3(transform.position.x + Random.Range(dirX, cropDetails.spawnRadius.x * dirX),
                        transform.position.y + Random.Range(-cropDetails.spawnRadius.y, cropDetails.spawnRadius.y), 0);
                        EventHandler.CallInstantiateItemInSceneEvent(cropDetails.producedItemID[i], spawnPos);
                    }
                }
            }

            if (tileDetails != null)
            {
                tileDetails.daysSinceLastHarvest++;

                // 是否可以重复生长
                if (cropDetails.daysToRegrow > 0 && tileDetails.daysSinceLastHarvest < cropDetails.regrowTimes - 1)
                {
                    tileDetails.growthDays = cropDetails.TotalGrowthDays - cropDetails.daysToRegrow;
                    // 刷新种子
                    EventHandler.CallRefreshCurrentMapEvent();
                }
                // 不可重复生长
                else    
                {
                    tileDetails.daysSinceLastHarvest = -1;
                    tileDetails.seedItemID = -1;
                }

                Destroy(gameObject);
            }
        }
    }
}

