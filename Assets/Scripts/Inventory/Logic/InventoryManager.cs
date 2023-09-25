using System.Collections;
using System.Collections.Generic;
using Farm.Save;
using UnityEngine;

namespace Farm.Inventory
{
    [RequireComponent(typeof(DataGUID))]
    public class InventoryManager : Singleton<InventoryManager>, ISaveable
    {
        [Header("Item Info")]
        public ItemDataList_SO itemDataList;

        [Header("Bag Info")]
        private InventoryBag_SO playerBag;
        public InventoryBag_SO playerBagTemplate;
        private InventoryBag_SO currentBoxBag;

        [Header("Trade")]
        public int playerMoney;

        [Header("Build")]
        public BluePrintDataList_SO bluePrintData;

        private Dictionary<string, List<InventoryItem>> boxDataDict = new Dictionary<string, List<InventoryItem>>();

        public int BoxDataAmount => boxDataDict.Count;

        public string GUID => GetComponent<DataGUID>().guid;

        #region Life Function

        private void OnEnable()
        {
            EventHandler.DropItemEvent += OnDropItem;
            EventHandler.HarvestAtPlayerPositionEvent += OnHarvestAtPlayerPosition;
            EventHandler.BuildFurnitureEvent += OnBuildFurniture;
            EventHandler.BaseBagOpenEvent += OnBaseBagOpen;
            EventHandler.StartNewGameEvent += OnStartNewGame;
        }

        private void OnDisable()
        {
            EventHandler.DropItemEvent -= OnDropItem;
            EventHandler.HarvestAtPlayerPositionEvent -= OnHarvestAtPlayerPosition;
            EventHandler.BuildFurnitureEvent -= OnBuildFurniture;
            EventHandler.BaseBagOpenEvent -= OnBaseBagOpen;
            EventHandler.StartNewGameEvent -= OnStartNewGame;
        }

        private void Start() 
        {
            ISaveable saveable = this;
            saveable.RegisterSaveable();
        }

        #endregion

        #region Event Function

        private void OnDropItem(int ID, Vector3 pos, ItemType itemType)
        {
            RemoveItem(ID, 1);
        }

        private void OnHarvestAtPlayerPosition(int ID)
        {
            int index = GetItemIndexInBag(ID);
            AddItemAtIndex(ID, index, 1);
            EventHandler.CallUpdateInventoryUIEvent(InventoryLocation.Player, playerBag.itemList);
        }

        private void OnBuildFurniture(int ID, Vector3 mousePos)
        {
            RemoveItem(ID, 1);
            BluePrintDetails bluePrint = bluePrintData.GetBluePrintDetails(ID);
            foreach(var item in bluePrint.resourceItem)
            {
                RemoveItem(item.itemID, item.itemAmount);
            }
        }

        private void OnBaseBagOpen(SlotType slotType, InventoryBag_SO bag_SO)
        {
            currentBoxBag = bag_SO;
        }

        private void OnStartNewGame(int index)
        {
            // 把背包恢复到默认状态
            playerBag = Instantiate(playerBagTemplate);
            playerMoney = Settings.playerStartMoney;
            boxDataDict.Clear();
            EventHandler.CallUpdateInventoryUIEvent(InventoryLocation.Player, playerBag.itemList);
        }

        #endregion

        /// <summary>
        /// 根据 ID 获取对应物品的详细信息
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public ItemDetails GetItemDetails(int ID)
        {
            return itemDataList.itemDetailsList.Find(i => i.itemID == ID);
        }

        /// <summary>
        /// 添加物品到 Player 背包中
        /// </summary>
        /// <param name="item"></param>
        /// <param name="toDestory">是否要销毁物品</param>
        public void AddItem(Item item, bool toDestory)
        {
            int index = GetItemIndexInBag(item.itemID);
            AddItemAtIndex(item.itemID, index, 1);

            if (toDestory)
            {
                Destroy(item.gameObject);
            }

            // 调用事件更新玩家背包的 UI
            EventHandler.CallUpdateInventoryUIEvent(InventoryLocation.Player, playerBag.itemList);
        }

        /// <summary>
        /// 检查背包是否有空位
        /// </summary>
        /// <returns></returns>
        private bool CheckBagCapacity()
        {
            for (int i = 0; i < playerBag.itemList.Count; i++)
            {
                if (playerBag.itemList[i].itemID == 0) return true;
            }
            return false;
        }

        /// <summary>
        /// 通过物品 ID 找到背包已有物品位置
        /// </summary>
        /// <param name="ID">物品 ID</param>
        /// <returns>返回背包中该物体的序号，-1 代表没有该物体</returns>
        private int GetItemIndexInBag(int ID)
        {
            for (int i = 0; i < playerBag.itemList.Count; i++)
            {
                if (playerBag.itemList[i].itemID == ID) return i;
            }
            return -1;
        }

        /// <summary>
        /// 在背包指定序号位置添加物品
        /// </summary>
        /// <param name="ID">物品 ID</param>
        /// <param name="index">序号</param>
        /// <param name="amount">数量</param>
        private void AddItemAtIndex(int ID, int index, int amount)
        {
            //背包没有这个物品，尝试添加物体数据
            if (index == -1)
            {
                for (int i = 0; i < playerBag.itemList.Count; i++)
                {
                    if (playerBag.itemList[i].itemID == 0)
                    {
                        playerBag.itemList[i] = new InventoryItem { itemID = ID, itemAmount = amount };
                        return;
                    }
                }
                // 若找不到空位，将不会添加物体
                Debug.LogWarning("The bag has no capacity!");
            }
            // 背包有这个物品
            else    
            {
                int currentAmount = playerBag.itemList[index].itemAmount + amount;
                var item = new InventoryItem { itemID = ID, itemAmount = currentAmount };
                // 结构体是值类型，不能直接修改结构体中某个成员值，只能对整体进行重新赋值
                playerBag.itemList[index] = item;
            }
        }

        /// <summary>
        /// 玩家背包范围内交换物品
        /// </summary>
        /// <param name="fromIndex">起始序号</param>
        /// <param name="toIndex">目标序号</param>
        public void SwapItem(int fromIndex, int toIndex)
        {
            InventoryItem currentItem = playerBag.itemList[fromIndex];
            InventoryItem targetItem = playerBag.itemList[toIndex];

            // 目标格子有物品，交换信息
            if (targetItem.itemID != 0)
            {
                playerBag.itemList[fromIndex] = targetItem;
                playerBag.itemList[toIndex] = currentItem;
            }
            // 目标格子没物品，要将当前格子信息置空
            else
            {
                playerBag.itemList[toIndex] = currentItem;
                playerBag.itemList[fromIndex] = new InventoryItem();
            }
            // 更新背包 UI
            EventHandler.CallUpdateInventoryUIEvent(InventoryLocation.Player, playerBag.itemList);
        }

        /// <summary>
        /// 移除指定数量的背包物品
        /// </summary>
        /// <param name="ID">物品 ID</param>
        /// <param name="removeAmount">数量</param>
        private void RemoveItem(int ID, int removeAmount)
        {
            var index = GetItemIndexInBag(ID);

            if (playerBag.itemList[index].itemAmount > removeAmount)
            {
                var amount = playerBag.itemList[index].itemAmount - removeAmount;
                var item = new InventoryItem { itemID = ID, itemAmount = amount };
                playerBag.itemList[index] = item;
            }
            // 物品数量归零
            else if (playerBag.itemList[index].itemAmount == removeAmount)
            {
                var item = new InventoryItem();
                playerBag.itemList[index] = item;
            }

            EventHandler.CallUpdateInventoryUIEvent(InventoryLocation.Player, playerBag.itemList);
        }

        /// <summary>
        /// 交易物品
        /// </summary>
        /// <param name="itemDetails">物品信息</param>
        /// <param name="amount">交易数量</param>
        /// <param name="isSellTrade">是否卖东西</param>
        public void TradeItem(ItemDetails itemDetails, int amount, bool isSellTrade)
        {
            int cost = itemDetails.itemPrice * amount;
            // 获得物品背包位置
            int index = GetItemIndexInBag(itemDetails.itemID);

            // 卖东西
            if (isSellTrade)
            {
                if (playerBag.itemList[index].itemAmount >= amount)
                {
                    RemoveItem(itemDetails.itemID, amount);
                    //卖出总价
                    cost = (int)(cost * itemDetails.sellPercentage);
                    playerMoney += cost;
                }
            }
            // 买东西
            else if (playerMoney >= cost)
            {
                if (CheckBagCapacity())
                {
                    AddItemAtIndex(itemDetails.itemID, index, amount);
                }
                playerMoney -= cost;
            }
            // 刷新 UI
            EventHandler.CallUpdateInventoryUIEvent(InventoryLocation.Player, playerBag.itemList);
        }

        /// <summary>
        /// 检查建造资源物品库存
        /// </summary>
        /// <param name="ID">图纸ID</param>
        /// <returns></returns>
        public bool CheckStock(int ID)
        {
            var bluePrintDetails = bluePrintData.GetBluePrintDetails(ID);

            foreach (var resourceItem in bluePrintDetails.resourceItem)
            {
                var itemStock = playerBag.GetInventoryItem(resourceItem.itemID);
                if (itemStock.itemAmount >= resourceItem.itemAmount) continue;
                else return false;
            }
            return true;
        }

        /// <summary>
        /// 跨背包交换数据
        /// </summary>
        /// <param name="locationFrom"></param>
        /// <param name="fromIndex"></param>
        /// <param name="locationTarget"></param>
        /// <param name="targetIndex"></param>
        public void SwapItem(InventoryLocation locationFrom, int fromIndex, InventoryLocation locationTarget, int targetIndex)
        {
            var currentList = GetItemList(locationFrom);
            var targetList = GetItemList(locationTarget);

            InventoryItem currentItem = currentList[fromIndex];

            if (targetIndex < targetList.Count)
            {
                InventoryItem targetItem = targetList[targetIndex];

                // 不相同的两个物品，交换数据
                if (targetItem.itemID != 0 && currentItem.itemID != targetItem.itemID)
                {
                    currentList[fromIndex] = targetItem;
                    targetList[targetIndex] = currentItem;
                }
                // 相同的两个物品，叠加数量，清空原格子
                else if (currentItem.itemID == targetItem.itemID)
                {
                    targetItem.itemAmount += currentItem.itemAmount;
                    targetList[targetIndex] = targetItem;
                    currentList[fromIndex] = new InventoryItem();
                }
                // 目标为空格子
                else
                {
                    targetList[targetIndex] = currentItem;
                    currentList[fromIndex] = new InventoryItem();
                }
                // 调用事件，刷新 UI
                EventHandler.CallUpdateInventoryUIEvent(locationFrom, currentList);
                EventHandler.CallUpdateInventoryUIEvent(locationTarget, targetList);
            }
        }

        /// <summary>
        /// 根据位置返回背包数据列表
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        private List<InventoryItem> GetItemList(InventoryLocation location)
        {
            return location switch
            {
                InventoryLocation.Player => playerBag.itemList,
                InventoryLocation.Box => currentBoxBag.itemList,
                _ => null
            };
        }
        
        /// <summary>
        /// 查找箱子数据
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public List<InventoryItem> GetBoxDataList(string key)
        {
            if (boxDataDict.ContainsKey(key))
                return boxDataDict[key];
            return null;
        }

        /// <summary>
        /// 加入箱子数据字典
        /// </summary>
        /// <param name="box"></param>
        public void AddBoxDataDict(Box box)
        {
            var key = box.name + box.index;
            if (!boxDataDict.ContainsKey(key))
                boxDataDict.Add(key, box.boxBagData.itemList);
        }

        public GameSaveData GenerateSaveData()
        {
            GameSaveData saveData = new GameSaveData();
            saveData.playerMoney = this.playerMoney;

            saveData.inventoryDict = new Dictionary<string, List<InventoryItem>>();
            saveData.inventoryDict.Add(playerBag.name, playerBag.itemList);

            foreach (var item in boxDataDict)
            {
                saveData.inventoryDict.Add(item.Key, item.Value);
            }

            return saveData;
        }

        public void RestoreData(GameSaveData saveData)
        {
            this.playerMoney = saveData.playerMoney;
            playerBag = Instantiate(playerBagTemplate);
            playerBag.itemList = saveData.inventoryDict[playerBag.name];

            foreach (var item in saveData.inventoryDict)
            {
                if (boxDataDict.ContainsKey(item.Key))
                {
                    boxDataDict[item.Key] = item.Value;
                }
            }

            EventHandler.CallUpdateInventoryUIEvent(InventoryLocation.Player, playerBag.itemList);
        }
    }
}