using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Farm.CropPlant;

namespace Farm.Inventory
{
    public class Item : MonoBehaviour
    {
        public int itemID;
        private SpriteRenderer spriteRenderer;
        private BoxCollider2D coll;
        [HideInInspector]
        public ItemDetails itemDetails;

        #region Life Function

        private void Awake()
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            coll = GetComponent<BoxCollider2D>();
        }

        private void Start()
        {
            if (itemID != 0)
            {
                Init(itemID);
            }
        }

        #endregion

        /// <summary>
        /// 根据物体 ID，初始化当前物体
        /// </summary>
        /// <param name="ID"></param>
        private void Init(int ID)
        {
            itemID = ID;

            //Inventory获得当前数据
            itemDetails = InventoryManager.Instance.GetItemDetails(itemID);

            // 避免 ID 无效导致获得空物体
            if (itemDetails != null)
            {
                spriteRenderer.sprite = itemDetails.itemOnWorldSprite != null ? itemDetails.itemOnWorldSprite : itemDetails.itemIcon;

                // 由于锚点问题可能导致碰撞盒与物体图片不匹配，对碰撞体大小和偏移进行修改
                Vector2 newSize = new Vector2(spriteRenderer.sprite.bounds.size.x, spriteRenderer.sprite.bounds.size.y);
                coll.size = newSize;
                coll.offset = new Vector2(0, spriteRenderer.sprite.bounds.center.y);
            }

            // 当前物体是可收割的杂草类型，添加一个代码组件用于实现收割逻辑
            if(itemDetails.itemType == ItemType.ReapableScenery)
            {
                gameObject.AddComponent<ReapItem>();
                gameObject.GetComponent<ReapItem>().InitCropData(ID);
            }
        }
    }
}
