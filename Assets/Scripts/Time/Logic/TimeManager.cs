using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Farm.Save;

namespace Farm.GameTime
{
    [RequireComponent(typeof(DataGUID))]
    public class TimeManager : Singleton<TimeManager>, ISaveable
    {
        private int gameSecond, gameMinute, gameHour, gameDay, gameMonth, gameYear;
        private Season gameSeason;
        private int monthInSeason = 3;

        public bool gameClockPause;
        private float tikTime;

        // 灯光时间差
        private float timeDifference;

        public TimeSpan GameTime => new TimeSpan(gameHour, gameMinute, gameSecond);

        public string GUID => GetComponent<DataGUID>().guid;

        #region Life Function

        private void OnEnable()
        {
            EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnload;
            EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoaded;
            EventHandler.UpdateGameStateEvent += OnUpdateGameState;
            EventHandler.StartNewGameEvent += OnStartNewGame;
            EventHandler.EndGameEvent += OnEndGame;
        }

        private void OnDisable()
        {
            EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnload;
            EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoaded;
            EventHandler.UpdateGameStateEvent -= OnUpdateGameState;
            EventHandler.StartNewGameEvent -= OnStartNewGame;
            EventHandler.EndGameEvent -= OnEndGame;
        }

        private void Start() 
        {
            ISaveable saveable = this;
            saveable.RegisterSaveable();

            gameClockPause = true;
        }

        private void Update()
        {
            // 时间未停止情况下，改变时间
            if (!gameClockPause)
            {
                tikTime += Time.deltaTime;

                if (tikTime >= Settings.secondThreshold)
                {
                    tikTime -= Settings.secondThreshold;
                    UpdateGameTime();
                }
            }

            // 调试使用，按下 T 快速流逝时间
            if(Input.GetKey(KeyCode.T))
            {
                for(int i = 0; i < 120; i++)
                {
                    UpdateGameTime();
                }
            }
        }

        #endregion

        #region Event Function

        private void OnBeforeSceneUnload()
        {
            gameClockPause = true;
        }

        private void OnAfterSceneLoaded()
        {
            gameClockPause = false;

            EventHandler.CallGameMinuteEvent(gameMinute, gameHour, gameDay, gameSeason);
            EventHandler.CallGameDateEvent(gameHour, gameDay, gameMonth, gameYear, gameSeason);
            EventHandler.CallLightShiftChangeEvent(gameSeason, GetCurrentLightShift(), timeDifference);
        }

        private void OnUpdateGameState(GameState gameState)
        {
            gameClockPause = gameState == GameState.Pause;
        }

        private void OnStartNewGame(int index)
        {
            NewGameTime();
            gameClockPause = false;
        }

        private void OnEndGame()
        {
            gameClockPause = true;
        }

        #endregion

        /// <summary>
        /// 初始化游戏的时间状态
        /// </summary>
        private void NewGameTime()
        {
            gameSecond = 0;
            gameMinute = 0;
            gameHour = 7;
            gameDay = 1;
            gameMonth = 3;
            gameYear = 2023;
            gameSeason = Season.Spring;
        }

        /// <summary>
        /// 更新时间，增加秒数，判断时间的推进
        /// </summary>
        private void UpdateGameTime()
        {
            gameSecond++;
            if (gameSecond > Settings.secondHold)
            {
                gameMinute++;
                gameSecond = 0;

                if (gameMinute > Settings.minuteHold)
                {
                    gameHour++;
                    gameMinute = 0;

                    if (gameHour > Settings.hourHold)
                    {
                        gameDay++;
                        gameHour = 0;

                        if (gameDay > Settings.dayHold)
                        {
                            gameDay = 1;
                            gameMonth++;

                            if (gameMonth > 12)
                                gameMonth = 1;

                            // 该季节的月份过去了一个月
                            monthInSeason--;
                            if (monthInSeason == 0)
                            {
                                monthInSeason = 3;

                                int seasonNumber = (int)gameSeason;
                                seasonNumber++;

                                if (seasonNumber > Settings.seasonHold)
                                {
                                    seasonNumber = 0;
                                    gameYear++;
                                }

                                gameSeason = (Season)seasonNumber;

                                // 年份超过上限，重置
                                if (gameYear > 9999)
                                {
                                    gameYear = 2023;
                                }
                            }
                        }
                        // 过了一天，刷新地图和农作物生长
                        EventHandler.CallGameDayEvent(gameDay, gameSeason);
                    }
                    // 调用日期变化事件
                    EventHandler.CallGameDateEvent(gameHour, gameDay, gameMonth, gameYear, gameSeason);
                }
                // 调用分钟变化事件
                EventHandler.CallGameMinuteEvent(gameMinute, gameHour, gameDay, gameSeason);
                // 调用灯光切换事件
                EventHandler.CallLightShiftChangeEvent(gameSeason, GetCurrentLightShift(), timeDifference);
            }
        }

        /// <summary>
        /// 返回lightshift同时计算时间差
        /// </summary>
        /// <returns></returns>
        private LightShift GetCurrentLightShift()
        {
            if (GameTime >= Settings.morningTime && GameTime < Settings.nightTime)
            {
                timeDifference = (float)(GameTime - Settings.morningTime).TotalMinutes;
                return LightShift.Morning;
            }

            if (GameTime < Settings.morningTime || GameTime >= Settings.nightTime)
            {
                timeDifference = Mathf.Abs((float)(GameTime - Settings.nightTime).TotalMinutes);
                return LightShift.Night;
            }

            return LightShift.Morning;
        }

        public GameSaveData GenerateSaveData()
        {
            GameSaveData saveData = new GameSaveData();
            saveData.timeDict = new Dictionary<string, int>
            {
                { "gameYear", gameYear },
                { "gameSeason", (int)gameSeason },
                { "gameMonth", gameMonth },
                { "gameDay", gameDay },
                { "gameHour", gameHour },
                { "gameMinute", gameMinute },
                { "gameSecond", gameSecond }
            };

            return saveData;
        }

        public void RestoreData(GameSaveData saveData)
        {
            gameYear = saveData.timeDict["gameYear"];
            gameSeason = (Season)saveData.timeDict["gameSeason"];
            gameMonth = saveData.timeDict["gameMonth"];
            gameDay = saveData.timeDict["gameDay"];
            gameHour = saveData.timeDict["gameHour"];
            gameMinute = saveData.timeDict["gameMinute"];
            gameSecond = saveData.timeDict["gameSecond"];
        }
    }
}