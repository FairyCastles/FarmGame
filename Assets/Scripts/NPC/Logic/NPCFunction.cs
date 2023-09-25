using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Farm.Inventory;

namespace Farm.NPC
{
    public class NPCFunction : MonoBehaviour
    {
        public InventoryBag_SO shopData;
        private bool isOpen;

        #region Life Function
        
        private void Update() 
        {
            // 按下 ESC 关闭背包
            if(isOpen && Input.GetKeyDown(KeyCode.Escape))
            {
                CloseShop();
            }
        }

        #endregion

        // 对话结束，打开背包，作为对话结束的事件被调用
        public void OpenShop()
        {
            isOpen = true;
            EventHandler.CallBaseBagOpenEvent(SlotType.Shop, shopData);
            EventHandler.CallUpdateGameStateEvent(GameState.Pause);
        }

        public void CloseShop()
        {
            isOpen = false;
            EventHandler.CallBaseBagCloseEvent(SlotType.Shop, shopData);
            EventHandler.CallUpdateGameStateEvent(GameState.Gameplay);
        }
    }
}