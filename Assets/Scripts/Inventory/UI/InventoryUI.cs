using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Farm.Inventory
{
    public class InventoryUI : MonoBehaviour
    {
        [SerializeField]
        private SlotUI[] playerSlots;
        public ItemTooltip itemTooltip;

        [Header("Player Bag UI")]
        [SerializeField]
        private GameObject bagUI;
        private bool bagOpen;
        public Text playerMoneyText;
        
        [Header("Drag Item")]
        public Image dragItem;

        [Header("Common Bag")]
        [SerializeField]
        private GameObject baseBag;
        public GameObject shopSlotPrefab;
        public GameObject boxSlotPrefab;
        [SerializeField]
        private List<SlotUI> baseBagSlots;

        [Header("Trade UI")]
        public TradeUI tradeUI;

        #region Life Function

        private void OnEnable() 
        {
            EventHandler.UpdateInventoryUIEvent += OnUpdateInventoryUI;
            EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnload;
            EventHandler.BaseBagOpenEvent += OnBaseBagOpen;
            EventHandler.BaseBagCloseEvent += OnBaseBagClose;
            EventHandler.ShowTradeUIEvent += OnShowTradeUI;
        }

        private void OnDisable() 
        {
            EventHandler.UpdateInventoryUIEvent -= OnUpdateInventoryUI;
            EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnload;
            EventHandler.BaseBagOpenEvent -= OnBaseBagOpen;
            EventHandler.BaseBagCloseEvent -= OnBaseBagClose;
            EventHandler.ShowTradeUIEvent -= OnShowTradeUI;
        }

        private void Start() 
        {
            // 给每个格子标上序号
            for(int i = 0; i < playerSlots.Length; i++)
            {
                playerSlots[i].slotIndex = i; 
            }
            // 获取背包的初始状态，是否被打开
            bagOpen = bagUI.activeInHierarchy;
            playerMoneyText.text = InventoryManager.Instance.playerMoney.ToString();
        }

        private void Update() 
        {
            // 快捷键 B 控制背包打开关闭
            // TODO: 当快捷键多时，可以用一个类管理所有键盘输入的快捷键
            if(Input.GetKeyDown(KeyCode.B))
            {
                OpenBagUI();
            }
        }

        #endregion

        #region Event Function

        /// <summary>
        /// 发生物品获得或使用，刷新背包的 UI
        /// </summary>
        /// <param name="location">需要更新的格子所在位置</param>
        /// <param name="list">更新后的每个格子的数据信息</param>
        private void OnUpdateInventoryUI(InventoryLocation location, List<InventoryItem> list)
        {
            switch(location)
            {
                case InventoryLocation.Player:
                    // TODO: 每次都要刷新整个背包很浪费，可以传入参数，只更新变化的格子
                    for(int i = 0; i < playerSlots.Length; i++)
                    {
                        if(list[i].itemAmount > 0)
                        {
                            var item = InventoryManager.Instance.GetItemDetails(list[i].itemID);
                            playerSlots[i].UpdateSlot(item, list[i].itemAmount);
                        }
                        else
                        {
                            playerSlots[i].UpdateEmptySlot();
                        }
                    }
                    break;
                case InventoryLocation.Box:
                    for(int i = 0; i < baseBagSlots.Count; i++)
                    {
                        if(list[i].itemAmount > 0)
                        {
                            var item = InventoryManager.Instance.GetItemDetails(list[i].itemID);
                            baseBagSlots[i].UpdateSlot(item, list[i].itemAmount);
                        }
                        else
                        {
                            baseBagSlots[i].UpdateEmptySlot();
                        }
                    }
                    break;
            }
            
            playerMoneyText.text = InventoryManager.Instance.playerMoney.ToString();
        }

        /// <summary>
        /// 卸载场景前，关闭所有 UI 的高亮显示
        /// </summary>
        private void OnBeforeSceneUnload()
        {
            UpdateSlotHightlight(-1);
        }

        /// <summary>
        /// 控制背包 UI 的显示
        /// </summary>
        public void OpenBagUI()
        {
            bagOpen = !bagOpen;
            bagUI.SetActive(bagOpen);
        }

        private void OnBaseBagOpen(SlotType slotType, InventoryBag_SO bagData)
        {
            GameObject prefab = slotType switch
            {
                SlotType.Shop => shopSlotPrefab,
                SlotType.Box => boxSlotPrefab,
                _ => null,
            };

            // 生成背包 UI
            baseBag.SetActive(true);

            baseBagSlots = new List<SlotUI>();

            // 根据背包数据，生成格子
            for (int i = 0; i < bagData.itemList.Count; i++)
            {
                var slot = Instantiate(prefab, baseBag.transform.GetChild(0)).GetComponent<SlotUI>();
                slot.slotIndex = i;
                baseBagSlots.Add(slot);
            }
            // 强制刷新 UI
            LayoutRebuilder.ForceRebuildLayoutImmediate(baseBag.GetComponent<RectTransform>());

            // 若打开商店，则同时打开玩家背包
            if(slotType == SlotType.Shop)
            {
                bagUI.GetComponent<RectTransform>().pivot = new Vector2(-1, 0.5f);
                bagUI.SetActive(true);
                bagOpen = true;
            }

            // 更新 UI 显示
            OnUpdateInventoryUI(InventoryLocation.Box, bagData.itemList);
        }

        private void OnBaseBagClose(SlotType slotType, InventoryBag_SO bag)
        {
            baseBag.SetActive(false);
            itemTooltip.gameObject.SetActive(false);
            UpdateSlotHightlight(-1);

            // 关闭背包时清空所有格子
            foreach(var slot in baseBagSlots)
            {
                Destroy(slot.gameObject);
            }
            baseBagSlots.Clear();

            // 若关闭商店，则同时关闭玩家背包
            if(slotType == SlotType.Shop)
            {
                bagUI.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
                bagUI.SetActive(false);
                bagOpen = false;
            }
        }

        private void OnShowTradeUI(ItemDetails item, bool isSell)
        {
            tradeUI.gameObject.SetActive(true);
            tradeUI.SetupTradeUI(item, isSell);
        }

        #endregion

        /// <summary>
        /// 更新 Slot 高亮显示
        /// </summary>
        /// <param name="index">序号</param>
        public void UpdateSlotHightlight(int index)
        {
            foreach (var slot in playerSlots)
            {
                // 每次只有被选择的 Slot 显示高亮
                if (slot.isSelected && slot.slotIndex == index)
                {
                    slot.slotHightlight.gameObject.SetActive(true);
                }
                // 关闭其他所有格子的高亮
                else
                {
                    slot.isSelected = false;
                    slot.slotHightlight.gameObject.SetActive(false);
                }
            }
        }
    }
}
