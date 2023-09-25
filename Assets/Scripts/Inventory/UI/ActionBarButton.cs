using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Farm.Inventory
{
    [RequireComponent(typeof(SlotUI))]
    public class ActionBarButton : MonoBehaviour
    {
        public KeyCode key;
        private SlotUI slotUI;

        private bool canUse = true;

        #region Life Function

        private void Awake()
        {
            slotUI = GetComponent<SlotUI>();
        }

        private void OnEnable()
        {
            EventHandler.UpdateGameStateEvent += OnUpdateGameState;
        }

        private void OnDisable()
        {
            EventHandler.UpdateGameStateEvent -= OnUpdateGameState;
        }

        private void Update()
        {
            // 通过快捷键选择格子
            // 这个代码只挂载在快捷键的格子上
            if (Input.GetKeyDown(key) && canUse)
            {
                if (slotUI.itemDetails != null)
                {
                    slotUI.isSelected = !slotUI.isSelected;
                    if (slotUI.isSelected)
                        slotUI.inventoryUI.UpdateSlotHightlight(slotUI.slotIndex);
                    else
                        slotUI.inventoryUI.UpdateSlotHightlight(-1);

                    EventHandler.CallItemSelectedEvent(slotUI.itemDetails, slotUI.isSelected);
                }
            }
        }

        #endregion

        #region Event Function

        // 游戏状态暂停时，禁止快捷键使用
        private void OnUpdateGameState(GameState gameState)
        {
            canUse = gameState == GameState.Gameplay;
        }

        #endregion
    }
}