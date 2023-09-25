using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Farm.Inventory
{
    public class SlotUI : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("Component")]
        [SerializeField] private Image slotImage;
        [SerializeField] private TextMeshProUGUI amountText;
        [SerializeField] public Image slotHightlight;
        [SerializeField] private Button button;

        [Header("Slot Info")]
        public SlotType slotType;
        public int slotIndex;
        public bool isSelected;

        [Header("Item Info")]
        public ItemDetails itemDetails;
        public int itemAmount;

        public InventoryLocation Location
        {
            get
            {
                return slotType switch
                {
                    SlotType.Bag => InventoryLocation.Player,
                    SlotType.Box => InventoryLocation.Box,
                    _ => InventoryLocation.Player
                };
            }
        }

        public InventoryUI inventoryUI => GetComponentInParent<InventoryUI>();

        private void Start()
        {
            isSelected = false;
            // 当前格子没有物品信息
            if (itemDetails == null)
            {
                UpdateEmptySlot();
            }
        }

        /// <summary>
        /// 更新格子 UI 和信息
        /// </summary>
        /// <param name="item">物品的详细信息</param>
        /// <param name="amount">物品持有数量</param>
        public void UpdateSlot(ItemDetails item, int amount)
        {
            itemDetails = item;
            slotImage.sprite = item.itemIcon;
            itemAmount = amount;
            amountText.text = amount.ToString();
            // 打开格子物品的图片
            slotImage.enabled = true;
            // 格子按钮可以交互
            button.interactable = true;
        }

        /// <summary>
        /// 更新为空格子
        /// </summary>
        public void UpdateEmptySlot()
        {
            if (isSelected) 
            { 
                isSelected = false;
                // 关闭高亮
                inventoryUI.UpdateSlotHightlight(-1);
                EventHandler.CallItemSelectedEvent(itemDetails, isSelected);
            }

            itemDetails = null;
            slotImage.enabled = false;
            amountText.text = string.Empty;
            button.interactable = false;
        }


        /// <summary>
        /// 当格子被点击后，该函数会被自动调用
        /// 当格子被点击后，显示高亮
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerClick(PointerEventData eventData)
        {
            if(itemDetails == null) return;

            isSelected = !isSelected;
            inventoryUI.UpdateSlotHightlight(slotIndex);

            // 点击背包中的物体时，调用事件，通知物体被选中
            if(slotType == SlotType.Bag)
            {
                EventHandler.CallItemSelectedEvent(itemDetails, isSelected);
            }
        }

        /// <summary>
        /// 格子被拖拽时自动调用
        /// </summary>
        /// <param name="eventData"></param>
        public void OnBeginDrag(PointerEventData eventData)
        {
            // 有物品时才能拖拽
            if(itemAmount != 0)
            {
                // 将格子中的图标赋予专门用于显示拖拽的图标上
                inventoryUI.dragItem.enabled = true;
                inventoryUI.dragItem.sprite = slotImage.sprite;
                // 将大小设置为图标原始大小，以防图标不是 20x20 尺寸
                inventoryUI.dragItem.SetNativeSize();

                // 显示高亮表示当前格子的图标被拖拽
                isSelected = true;
                inventoryUI.UpdateSlotHightlight(slotIndex);
            }
        }

        /// <summary>
        /// 在拖拽过程中自动调用
        /// </summary>
        /// <param name="eventData"></param>
        public void OnDrag(PointerEventData eventData)
        {
            // 跟随鼠标位置移动
            inventoryUI.dragItem.transform.position = Input.mousePosition;
        }

        /// <summary>
        /// 结束拖拽时调用
        /// </summary>
        /// <param name="eventData"></param>
        public void OnEndDrag(PointerEventData eventData)
        {
            inventoryUI.dragItem.enabled = false;

            GameObject obj = eventData.pointerCurrentRaycast.gameObject;

            // 结束点在 UI 上
            if (obj != null)
            {
                // 不在格子上，直接跳过
                if (obj.GetComponent<SlotUI>() == null) return;

                var targetSlot = obj.GetComponent<SlotUI>();
                int targetIndex = targetSlot.slotIndex;

                // 在玩家自身背包范围内交换
                if (slotType == SlotType.Bag && targetSlot.slotType == SlotType.Bag)
                {
                    InventoryManager.Instance.SwapItem(slotIndex, targetIndex);
                }
                // 从商店购买物品
                else if (slotType == SlotType.Shop && targetSlot.slotType == SlotType.Bag)
                {
                    EventHandler.CallShowTradeUIEvent(itemDetails, false);
                }
                // 向商店卖物品
                else if (slotType == SlotType.Bag && targetSlot.slotType == SlotType.Shop)
                {
                    EventHandler.CallShowTradeUIEvent(itemDetails, true);
                }
                // 跨背包交换物品
                else if (slotType != SlotType.Shop && targetSlot.slotType != SlotType.Shop && slotType != targetSlot.slotType)
                {
                    InventoryManager.Instance.SwapItem(Location, slotIndex, targetSlot.Location, targetSlot.slotIndex);
                }

                // 清空所有高亮显示
                inventoryUI.UpdateSlotHightlight(-1);
            }
            // 结束点在 UI 以外，即世界地图中，则将物体扔到地图中
            else
            {
                if (itemDetails.canDropped)
                {
                    // 获取鼠标对应世界地图坐标
                    var pos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -Camera.main.transform.position.z));

                    // 调用事件，在世界地图中生成对应物品
                    EventHandler.CallInstantiateItemInSceneEvent(itemDetails.itemID, pos);
                }
            }
        }
    }
}