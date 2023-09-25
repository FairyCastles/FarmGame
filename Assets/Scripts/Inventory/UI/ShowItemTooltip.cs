using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Farm.Inventory
{
    [RequireComponent(typeof(SlotUI))]
    public class ShowItemTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private SlotUI slotUI => GetComponent<SlotUI>();

        private InventoryUI inventoryUI => GetComponentInParent<InventoryUI>();

        public void OnPointerEnter(PointerEventData eventData)
        {
            // 当前格子有物品，鼠标进入格子时显示 ItemTooltip
            if(slotUI.itemDetails != null)
            {
                inventoryUI.itemTooltip.gameObject.SetActive(true);
                inventoryUI.itemTooltip.SetupTooltip(slotUI.itemDetails, slotUI.slotType);

                // 将 ItemTooltip 的锚点设置为底部中间
                inventoryUI.itemTooltip.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0);
                // TODO: 在不同分辨率下，相同的位移在不同分辨率表现不同，ItemTooltip 显示的位置要根据分辨率进行更改
                inventoryUI.itemTooltip.transform.position = transform.position + Vector3.up * 60;

                // 图纸类物品，显示图纸栏的信息
                if(slotUI.itemDetails.itemType == ItemType.Furniture)
                {
                    inventoryUI.itemTooltip.resourcePannel.SetActive(true);
                    inventoryUI.itemTooltip.SetupResourcePannel(slotUI.itemDetails.itemID);
                }
                else
                {
                    inventoryUI.itemTooltip.resourcePannel.SetActive(false);
                }
            }
            else
            {
                inventoryUI.itemTooltip.gameObject.SetActive(false);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            inventoryUI.itemTooltip.gameObject.SetActive(false);
        }
    }
}

