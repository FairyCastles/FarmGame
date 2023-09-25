using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Farm.Save;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Farm.Inventory
{
    [RequireComponent(typeof(DataGUID))]
    public class ItemManager : MonoBehaviour, ISaveable
    {
        public Item itemPrefab;

        public Item bounceItemPrefab;

        private Transform itemParent;

        private Transform PlayerTransform => FindObjectOfType<PlayerController>().transform;

        public string GUID => GetComponent<DataGUID>().guid;

        private Dictionary<string, List<SceneItem>> sceneItemDict = new Dictionary<string, List<SceneItem>>();

        private Dictionary<string, List<SceneFurniture>> sceneFurnitureDict = new Dictionary<string, List<SceneFurniture>>();

        #region Life Function

        private void OnEnable() 
        {
            EventHandler.InstantiateItemInSceneEvent += OnInstantiateItemInScene;
            EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnload;
            EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoad;
            EventHandler.DropItemEvent += OnDropItem;
            EventHandler.BuildFurnitureEvent += OnBuildFurniture;
            EventHandler.StartNewGameEvent += OnStartNewGame;
        }

        private void OnDisable() 
        {
            EventHandler.InstantiateItemInSceneEvent -= OnInstantiateItemInScene;
            EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnload;
            EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoad;
            EventHandler.DropItemEvent -= OnDropItem;
            EventHandler.BuildFurnitureEvent -= OnBuildFurniture;
            EventHandler.StartNewGameEvent -= OnStartNewGame;
        }

        private void Start()
        {
            ISaveable saveable = this;
            saveable.RegisterSaveable();
        }

        #endregion

        #region Event Function

        private void OnInstantiateItemInScene(int ID, Vector3 pos)
        {
            var item = Instantiate(bounceItemPrefab, pos, Quaternion.identity, itemParent);
            item.itemID = ID;
            item.GetComponent<ItemBounce>().InitBounceItem(pos, Vector3.up);
        }

        private void OnBeforeSceneUnload()
        {
            GetAllSceneItems();
            GetAllSceneFurniture();
        }

        private void OnAfterSceneLoad()
        {
            itemParent = GameObject.FindWithTag("ItemParent").transform;
            RecreateAllItems();
            RebuildFurniture();
        }

        // 从玩家头上生成一个东西丢下来
        private void OnDropItem(int ID, Vector3 mousePos, ItemType itemType)
        {
            // 种下种子，不执行生成物体
            if(itemType == ItemType.Seed)   return;
            var item = Instantiate(bounceItemPrefab, PlayerTransform.position, Quaternion.identity, itemParent);
            item.itemID = ID;
            // 飞行方向是从玩家位置指向鼠标位置
            Vector3 dir = (mousePos - PlayerTransform.position).normalized;
            item.GetComponent<ItemBounce>().InitBounceItem(mousePos, dir);
        }

        private void OnBuildFurniture(int ID, Vector3 mousePos)
        {
            BluePrintDetails bluePrint = InventoryManager.Instance.bluePrintData.GetBluePrintDetails(ID);
            var buildItem = Instantiate(bluePrint.buildPrefab, mousePos, Quaternion.identity, itemParent);
            buildItem.GetComponent<Furniture>().itemID = ID;

            // 当前物品是箱子，初始化箱子信息
            if (buildItem.GetComponent<Box>())
            {
                buildItem.GetComponent<Box>().index = InventoryManager.Instance.BoxDataAmount;
                buildItem.GetComponent<Box>().InitBox(buildItem.GetComponent<Box>().index);
            }
        }

        private void OnStartNewGame(int index)
        {
            sceneItemDict.Clear();
            sceneFurnitureDict.Clear();
        }

        #endregion

        /// <summary>
        /// 获得当前场景所有 Item
        /// </summary>
        private void GetAllSceneItems()
        {
            List<SceneItem> currentSceneItems = new List<SceneItem>();

            foreach (var item in FindObjectsOfType<Item>())
            {
                SceneItem sceneItem = new SceneItem
                {
                    itemID = item.itemID,
                    position = new SerializableVector3(item.transform.position)
                };

                currentSceneItems.Add(sceneItem);
            }

            string sceneName = SceneManager.GetActiveScene().name;

            // 字典中已经有当前场景的数据
            if (sceneItemDict.ContainsKey(sceneName))
            {
                sceneItemDict[sceneName] = currentSceneItems;
            }
            else
            {
                sceneItemDict.Add(sceneName, currentSceneItems);
            }
        }

        /// <summary>
        /// 刷新重建当前场景物品
        /// </summary>
        private void RecreateAllItems()
        {
            List<SceneItem> currentSceneItems = new List<SceneItem>();

            if (sceneItemDict.TryGetValue(SceneManager.GetActiveScene().name, out currentSceneItems))
            {
                if (currentSceneItems != null)
                {
                    // 先清空场景中所有物品
                    foreach (var item in FindObjectsOfType<Item>())
                    {
                        Destroy(item.gameObject);
                    }

                    // 根据字典中存储的数据重新生成
                    foreach (var item in currentSceneItems)
                    {
                        Item newItem = Instantiate(itemPrefab, item.position.ToVector3(), Quaternion.identity, itemParent);
                        newItem.itemID = item.itemID;
                    }
                }
            }
        }

        /// <summary>
        /// 获得场景所有家具
        /// </summary>
        private void GetAllSceneFurniture()
        {
            List<SceneFurniture> currentSceneFurniture = new List<SceneFurniture>();

            foreach (var item in FindObjectsOfType<Furniture>())
            {
                SceneFurniture sceneFurniture = new SceneFurniture
                {
                    itemID = item.itemID,
                    position = new SerializableVector3(item.transform.position)
                };

                // 给箱子赋予编号
                if (item.GetComponent<Box>())
                {
                    sceneFurniture.boxIndex = item.GetComponent<Box>().index;
                }

                currentSceneFurniture.Add(sceneFurniture);
            }
            // 已经保存过当前场景的家具信息
            if (sceneFurnitureDict.ContainsKey(SceneManager.GetActiveScene().name))
            {
                sceneFurnitureDict[SceneManager.GetActiveScene().name] = currentSceneFurniture;
            }
            // 如果是新场景
            else
            {
                sceneFurnitureDict.Add(SceneManager.GetActiveScene().name, currentSceneFurniture);
            }
        }

        /// <summary>
        /// 重建当前场景家具
        /// </summary>
        private void RebuildFurniture()
        {
            List<SceneFurniture> currentSceneFurniture = new List<SceneFurniture>();

            if (sceneFurnitureDict.TryGetValue(SceneManager.GetActiveScene().name, out currentSceneFurniture))
            {
                if (currentSceneFurniture != null)
                {
                    foreach (SceneFurniture sceneFurniture in currentSceneFurniture)
                    {
                        BluePrintDetails bluePrint = InventoryManager.Instance.bluePrintData.GetBluePrintDetails(sceneFurniture.itemID);
                        var buildItem = Instantiate(bluePrint.buildPrefab, sceneFurniture.position.ToVector3(), Quaternion.identity, itemParent);

                        // 当前物品是箱子，初始化箱子信息
                        if (buildItem.GetComponent<Box>())
                        {
                            buildItem.GetComponent<Box>().InitBox(sceneFurniture.boxIndex);
                        }
                    }
                }
            }
        }

        public GameSaveData GenerateSaveData()
        {
            GetAllSceneItems();
            GetAllSceneFurniture();

            GameSaveData saveData = new GameSaveData();
            saveData.sceneItemDict = this.sceneItemDict;
            saveData.sceneFurnitureDict = this.sceneFurnitureDict;

            return saveData;
        }

        public void RestoreData(GameSaveData saveData)
        {
            this.sceneItemDict = saveData.sceneItemDict;
            this.sceneFurnitureDict = saveData.sceneFurnitureDict;

            RecreateAllItems();
            RebuildFurniture();
        }
    }
}
