using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Farm.Light
{
    public class LightManager : MonoBehaviour
    {
        private LightControl[] sceneLights;
        private LightShift currentLightShift;
        private Season currentSeason;
        private float timeDifference = Settings.lightChangeDuration;

        #region Life Function

        private void OnEnable()
        {
            EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoaded;
            EventHandler.LightShiftChangeEvent += OnLightShiftChange;
            EventHandler.StartNewGameEvent += OnStartNewGame;
        }

        private void OnDisable()
        {
            EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoaded;
            EventHandler.LightShiftChangeEvent -= OnLightShiftChange;
            EventHandler.StartNewGameEvent -= OnStartNewGame;
        }

        #endregion

        #region Event Function

        private void OnAfterSceneLoaded()
        {
            sceneLights = FindObjectsOfType<LightControl>();

            foreach (LightControl light in sceneLights)
            {
                // 切换灯光
                light.ChangeLightShift(currentSeason, currentLightShift, timeDifference);
            }
        }

        private void OnLightShiftChange(Season season, LightShift lightShift, float timeDifference)
        {
            currentSeason = season;
            this.timeDifference = timeDifference;
            if (currentLightShift != lightShift)
            {
                currentLightShift = lightShift;

                foreach (LightControl light in sceneLights)
                {
                    // 切换灯光
                    light.ChangeLightShift(currentSeason, currentLightShift, timeDifference);
                }
            }
        }

        private void OnStartNewGame(int index)
        {
            currentLightShift = LightShift.Morning;
        }

        #endregion
    }
}