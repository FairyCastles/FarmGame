using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Farm.Map;

namespace Farm.CropPlant
{
    public class CropGenerator : MonoBehaviour
    {
        private Grid currentGrid;

        public int seedItemID;
        public int growthDays;

        # region Lift Function

        private void Awake()
        {
            currentGrid = FindObjectOfType<Grid>();
        }

        private void OnEnable()
        {
            EventHandler.GenerateCropEvent += OnGenerateCrop;
        }

        private void OnDisable()
        {
            EventHandler.GenerateCropEvent -= OnGenerateCrop;
        }

        #endregion


        #region Event Function

        // 生成种子信息，并添加到这个 tile 上
        private void OnGenerateCrop()
        {
            Vector3Int cropGridPos = currentGrid.WorldToCell(transform.position);

            if (seedItemID != 0)
            {
                var tile = GridMapManager.Instance.GetTileDetailsOnMousePosition(cropGridPos);

                if (tile == null)
                {
                    tile = new TileDetails
                    {
                        gridX = cropGridPos.x,
                        gridY = cropGridPos.y
                    };
                }

                tile.daysSinceWatered = -1;
                tile.seedItemID = seedItemID;
                tile.growthDays = growthDays;

                GridMapManager.Instance.UpdateTileDetails(tile);
            }
        }

        #endregion
    }
}