using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using DG.Tweening;

namespace Farm.Light
{
    public class LightControl : MonoBehaviour
    {
        public LightPattenList_SO lightData;

        private Light2D currentLight;

        private LightDetails currentLightDetails;

        #region Life Function

        private void Awake() 
        {
            currentLight = GetComponent<Light2D>();
        }

        #endregion

        /// <summary>
        /// 切换灯光
        /// </summary>
        /// <param name="season"></param>
        /// <param name="lightShift"></param>
        /// <param name="timeDifference"></param>
        public void ChangeLightShift(Season season, LightShift lightShift, float timeDifference)
        {
            currentLightDetails = lightData.GetLightDetails(season, lightShift);

            // 当前仍在过渡切换时间内，过渡变换灯光
            if (timeDifference < Settings.lightChangeDuration)
            {
                var colorOffset = (currentLightDetails.lightColor - currentLightDetails.lightColor) / Settings.lightChangeDuration * timeDifference;
                currentLight.color += colorOffset;
                DOTween.To(() => currentLight.color, c => currentLight.color = c, currentLightDetails.lightColor, Settings.lightChangeDuration - timeDifference);
                DOTween.To(() => currentLight.intensity, i => currentLight.intensity = i, currentLightDetails.lightAmount, Settings.lightChangeDuration - timeDifference);
            }
            // 当前已经超出过渡切换时间，直接变成下一个时间点灯光
            else
            {
                currentLight.color = currentLightDetails.lightColor;
                currentLight.intensity = currentLightDetails.lightAmount;
            }
        }
    }
}