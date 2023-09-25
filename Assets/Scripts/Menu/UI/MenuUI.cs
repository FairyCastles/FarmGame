using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Farm.Menu
{
    public class MenuUI : MonoBehaviour
    {
        public GameObject[] panels;

        public void SwitchPanel(int index)
        {
            for (int i = 0; i < panels.Length; i++)
            {
                if (i == index)
                {
                    // 将当前 pannel 放到最下面
                    // 最先渲染遮挡其他 pannel
                    panels[i].transform.SetAsLastSibling();
                }
            }
        }

        // TODO: 不清楚这个函数在游戏内有没有按钮使用过
        public void ExitGame()
        {
            Application.Quit();
            Debug.Log("EXIT GAME");
        }
    }
}