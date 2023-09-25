using System.Collections;
using System.Collections.Generic;
using Farm.Save;
using UnityEngine;
using UnityEngine.UI;

namespace Farm.Menu
{
    // TODO: 可以考虑添加删除存档功能
    public class SaveSlotUI : MonoBehaviour
    {
        public Text dataTime, dataScene;

        private Button currentButton;

        private DataSlot currData;

        private int Index => transform.GetSiblingIndex();

        #region Life Function

        private void Awake() 
        {
            currentButton = GetComponent<Button>();
            currentButton.onClick.AddListener(LoadGameData);
        }

        private void OnEnable() 
        {
            SetupSlotUI();
        }

        #endregion

        /// <summary>
        /// 读取游戏数据
        /// </summary>
        private void LoadGameData()
        {
            if (currData != null)
            {
                SaveLoadManager.Instance.Load(Index);
            }
            // 没有数据，则开启新游戏
            else
            {
                Debug.Log("New Game!");
                EventHandler.CallStartNewGameEvent(Index);
            }
        }

        /// <summary>
        /// 设置存档格子的文本显示
        /// </summary>
        private void SetupSlotUI()
        {
            currData = SaveLoadManager.Instance.dataSlots[Index];

            if (currData != null)
            {
                dataTime.text = currData.DataTime;
                dataScene.text = currData.DataScene;
            }
            // 当前格子不存在存档数据
            else
            {
                dataTime.text = "Empty";
                dataScene.text = "Empty";
            }
        }
    }
}
