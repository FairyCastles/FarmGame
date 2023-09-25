using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Farm.Inventory
{
    public class TradeUI : MonoBehaviour
    {
        public Image itemIcon;
        public Text itemName;
        public InputField tradeAmount;
        public Button submitButton;
        public Button cancelButton;

        private ItemDetails item;
        private bool isSellTrade;

        #region Life Function

        private void Awake() 
        {
            submitButton.onClick.AddListener(TradeItem);
            cancelButton.onClick.AddListener(CancelTrade);   
        }

        #endregion

        /// <summary>
        /// 根据传入的物品信息，设置交易 UI
        /// </summary>
        /// <param name="item"></param>
        /// <param name="isSell"></param>
        public void SetupTradeUI(ItemDetails item, bool isSell)
        {
            this.item = item;
            itemIcon.sprite = item.itemIcon;
            itemName.text = item.itemName;
            isSellTrade = isSell;
            tradeAmount.text = string.Empty;
        }
        
        private void TradeItem()
        {
            int amount = Convert.ToInt32(tradeAmount.text);
            InventoryManager.Instance.TradeItem(item, amount, isSellTrade);
            CancelTrade();
        }

        private void CancelTrade()
        {
            gameObject.SetActive(false);
        }
    }
}
    
