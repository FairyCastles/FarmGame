using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Farm.Inventory
{
    public class ItemTooltip : MonoBehaviour
    {
        [Header("Tips")]
        [SerializeField]
        private TextMeshProUGUI nameText;
        [SerializeField]
        private TextMeshProUGUI typeText;
        [SerializeField]
        private TextMeshProUGUI descriptionText;
        [SerializeField]
        private Text valueText;
        [SerializeField]
        private GameObject bottomPart;

        [Header("Build")]
        public GameObject resourcePannel;
        [SerializeField]
        private Image[] resourceItem;

        /// <summary>
        /// 设置 ItemTooltip 的信息
        /// </summary>
        /// <param name="itemDetails">物品的详细信息</param>
        /// <param name="slotType">格子的类型</param>
        public void SetupTooltip(ItemDetails itemDetails, SlotType slotType)
        {
            nameText.text = itemDetails.itemName;

            typeText.text = GetItemType(itemDetails.itemType);

            descriptionText.text = itemDetails.itemDescription;

            // 只有当前格子是可以售卖的物体，才显示 ItemTooltip 最下面的金额栏
            if (itemDetails.itemType == ItemType.Seed || itemDetails.itemType == ItemType.Commodity || itemDetails.itemType == ItemType.Furniture)
            {
                bottomPart.SetActive(true);

                int price = itemDetails.itemPrice;
                if (slotType == SlotType.Bag)
                {
                    price = (int)(price * itemDetails.sellPercentage);
                }

                valueText.text = price.ToString();
            }
            else
            {
                bottomPart.SetActive(false);
            }
            // 强制立刻刷新 UI，使得多行的 Description 可以立刻延展
            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        }

        /// <summary>
        /// 根据 ItemType 返回对应中文名
        /// </summary>
        /// <param name="itemType"></param>
        /// <returns></returns>
        private string GetItemType(ItemType itemType)
        {
            return itemType switch
            {
                ItemType.Seed => "种子",
                ItemType.Commodity => "商品",
                ItemType.Furniture => "家具",
                ItemType.BreakTool => "工具",
                ItemType.ChopTool => "工具",
                ItemType.CollectTool => "工具",
                ItemType.HoeTool => "工具",
                ItemType.ReapTool => "工具",
                ItemType.WaterTool => "工具",
                // 默认值 Default
                _ => "无"
            };
        }

        /// <summary>
        /// 设置蓝图的面板信息
        /// </summary>
        /// <param name="bluePrintDetails"></param>
        public void SetupResourcePannel(int ID)
        {
            var bluePrintDetails = InventoryManager.Instance.bluePrintData.GetBluePrintDetails(ID);
            // 目前至多显示 3 个物品
            for (int i = 0; i < resourceItem.Length; i++)
            {
                if(i < bluePrintDetails.resourceItem.Length)
                {
                    var item = bluePrintDetails.resourceItem[i];
                    resourceItem[i].gameObject.SetActive(true);
                    resourceItem[i].sprite = InventoryManager.Instance.GetItemDetails(item.itemID).itemIcon;
                    resourceItem[i].transform.GetChild(0).GetComponent<Text>().text = item.itemAmount.ToString();
                }
                else
                {
                    resourceItem[i].gameObject.SetActive(false);
                }
            }
        }
    }
}
