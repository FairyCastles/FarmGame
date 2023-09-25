using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Farm.CropPlant
{
    public class ReapItem : MonoBehaviour
    {
        private CropDetails cropDetails;

        private Transform PlayerTransform => FindObjectOfType<PlayerController>().transform;

        public void InitCropData(int ID)
        {
            cropDetails = CropManager.Instance.GetCropDetails(ID);
        }

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
        }
    }
}
