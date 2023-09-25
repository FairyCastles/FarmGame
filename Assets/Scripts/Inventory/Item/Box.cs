using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Farm.Inventory
{
    public class Box : MonoBehaviour
    {
        public InventoryBag_SO boxbagTemplate;
        public InventoryBag_SO boxBagData;

        public GameObject mouseIcon;

        private bool canOpen;
        private bool isOpen;
        public int index;

        #region Life Function

        private void OnEnable()
        {
            if (boxBagData == null)
            {
                boxBagData = Instantiate(boxbagTemplate);
            }
        }

        private void Update() 
        {
            // 按下右键，开启箱子
            if (!isOpen && canOpen && Input.GetMouseButtonDown(1))
            {
                EventHandler.CallBaseBagOpenEvent(SlotType.Box, boxBagData);
                isOpen = true;
            }

            if (isOpen && Input.GetKeyDown(KeyCode.Escape))
            {
                EventHandler.CallBaseBagCloseEvent(SlotType.Box, boxBagData);
                isOpen = false;
            }
            
            // 关闭箱子
            if (!canOpen && isOpen)
            {
                EventHandler.CallBaseBagCloseEvent(SlotType.Box, boxBagData);
                isOpen = false;
            }
        }

        #endregion

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                canOpen = true;
                mouseIcon.SetActive(true);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                canOpen = false;
                mouseIcon.SetActive(false);
            }
        }

        /// <summary>
        /// 初始化箱子的格子数据
        /// </summary>
        public void InitBox(int boxIndex)
        {
            index = boxIndex;
            string key = this.name + index;
            // 已经存有当前箱子的数据
            if (InventoryManager.Instance.GetBoxDataList(key) != null)
            {
                boxBagData.itemList = InventoryManager.Instance.GetBoxDataList(key);
            }
            // 新建的箱子
            else
            {
                InventoryManager.Instance.AddBoxDataDict(this);
            }
        }
    }
}
