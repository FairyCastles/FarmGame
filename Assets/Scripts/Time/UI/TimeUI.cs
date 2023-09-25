using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

namespace Farm.GameTime
{
    public class TimeUI : MonoBehaviour
    {
        public RectTransform dayNightImage;
        public RectTransform clockParent;
        public Image seasonImage;
        public TextMeshProUGUI dateText;
        public TextMeshProUGUI timeText;

        public Sprite[] seasonSprites;

        private List<GameObject> clockBlocks = new List<GameObject>();

        #region Life Function

        private void Awake()
        {
            for (int i = 0; i < clockParent.childCount; i++)
            {
                clockBlocks.Add(clockParent.GetChild(i).gameObject);
                clockParent.GetChild(i).gameObject.SetActive(false);
            }
        }

        private void OnEnable()
        {
            EventHandler.GameMinuteEvent += OnGameMinute;
            EventHandler.GameDateEvent += OnGameDate;
        }

        private void OnDisable()
        {
            EventHandler.GameMinuteEvent -= OnGameMinute;
            EventHandler.GameDateEvent -= OnGameDate;
        }

        #endregion

        #region Event Function

        /// <summary>
        /// 时间变化时调用
        /// </summary>
        /// <param name="minute"></param>
        /// <param name="hour"></param>
        private void OnGameMinute(int minute, int hour, int day, Season season)
        {
            // 显示两位
            timeText.text = hour.ToString("00") + ":" + minute.ToString("00");
        }

        /// <summary>
        /// 日期变化时调用
        /// </summary>
        /// <param name="hour"></param>
        /// <param name="day"></param>
        /// <param name="month"></param>
        /// <param name="year"></param>
        /// <param name="season"></param>
        private void OnGameDate(int hour, int day, int month, int year, Season season)
        {
            dateText.text = year + "." + month.ToString("00") + "." + day.ToString("00");
            seasonImage.sprite = seasonSprites[(int)season];

            SwitchHourImage(hour);
            DayNightImageRotate(hour);
        }

        #endregion

        /// <summary>
        /// 根据小时切换时间块显示
        /// </summary>
        /// <param name="hour"></param>
        private void SwitchHourImage(int hour)
        {
            // index = [0, 5]
            int index = hour / 4;

            for(int i = 0; i < clockBlocks.Count; i++)
            {
                if(i < index + 1)
                {
                    clockBlocks[i].SetActive(true);
                }
                else
                {
                    clockBlocks[i].SetActive(false);
                }
            }
        }

        /// <summary>
        /// 根据小时切换时间图片的旋转
        /// </summary>
        /// <param name="hour"></param>
        private void DayNightImageRotate(int hour)
        {
            var target = new Vector3(0, 0, hour * 15 - 90);
            dayNightImage.DORotate(target, 1f, RotateMode.Fast);
        }
    }
}