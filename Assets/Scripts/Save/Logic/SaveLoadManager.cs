using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace Farm.Save
{
    public class SaveLoadManager : Singleton<SaveLoadManager>
    {
        private List<ISaveable> saveableList = new List<ISaveable>();

        public List<DataSlot> dataSlots = new List<DataSlot>(new DataSlot[3]);

        private string jsonFolder;
        private int currentDataIndex;

        #region Life Function

        protected override void Awake()
        {
            base.Awake();
            jsonFolder = Application.persistentDataPath + "/SAVE DATA/";
            ReadSaveData();
        }

        private void OnEnable()
        {
            EventHandler.StartNewGameEvent += OnStartNewGame;
            EventHandler.EndGameEvent += OnEndGame;
        }

        private void OnDisable()
        {
            EventHandler.StartNewGameEvent -= OnStartNewGame;
            EventHandler.EndGameEvent -= OnEndGame;
        }


        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.I))
                Save(currentDataIndex);
            if (Input.GetKeyDown(KeyCode.O))
                Load(currentDataIndex);
        }

        #endregion

        #region Event Function

        private void OnStartNewGame(int index)
        {
            currentDataIndex = index;
        }

        private void OnEndGame()
        {
            Save(currentDataIndex);
        }

        #endregion

        /// <summary>
        /// 将需要数据存档的类进行注册
        /// </summary>
        /// <param name="saveable"></param>
        public void RegisterSaveable(ISaveable saveable)
        {
            if (!saveableList.Contains(saveable))
                saveableList.Add(saveable);
        }

        /// <summary>
        /// 读取存档数据，设置到存档格子中
        /// </summary>
        private void ReadSaveData()
        {
            if (Directory.Exists(jsonFolder))
            {
                for (int i = 0; i < dataSlots.Count; i++)
                {
                    var resultPath = jsonFolder + "data" + i + ".json";
                    if (File.Exists(resultPath))
                    {
                        var stringData = File.ReadAllText(resultPath);
                        var jsonData = JsonConvert.DeserializeObject<DataSlot>(stringData);
                        dataSlots[i] = jsonData;
                    }
                }
            }
        }

        private void Save(int index)
        {
            DataSlot data = new DataSlot();
            // 获取所有需要存储的数据，添加到字典中
            foreach (var saveable in saveableList)
            {
                data.dataDict.Add(saveable.GUID, saveable.GenerateSaveData());
            }
            
            dataSlots[index] = data;
            var resultPath = jsonFolder + "data" + index + ".json";
            // 序列化数据变成 json
            var jsonData = JsonConvert.SerializeObject(dataSlots[index], Formatting.Indented);

            if (!File.Exists(resultPath))
            {
                Directory.CreateDirectory(jsonFolder);
            }

            // 写入 json 到指定路径
            File.WriteAllText(resultPath, jsonData);
        }

        public void Load(int index)
        {
            currentDataIndex = index;

            var resultPath = jsonFolder + "data" + index + ".json";

            var stringData = File.ReadAllText(resultPath);

            // 反序列化 json 变成数据
            var jsonData = JsonConvert.DeserializeObject<DataSlot>(stringData);

            // 从存档数据，恢复游戏内所有信息
            foreach (var saveable in saveableList)
            {
                saveable.RestoreData(jsonData.dataDict[saveable.GUID]);
            }
        }
    }
}