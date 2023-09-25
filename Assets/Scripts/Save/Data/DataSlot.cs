using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Farm.Transition;
using Farm.GameTime;

namespace Farm.Save
{
    public class DataSlot
    {
        // 每一个存档所包含的所有数据
        public Dictionary<string, GameSaveData> dataDict = new Dictionary<string, GameSaveData>();

        /// <summary>
        /// 获取存档数据中的时间
        /// </summary>
        /// <value></value>
        public string DataTime
        {
            get
            {
                var key = TimeManager.Instance.GUID;

                if(dataDict.ContainsKey(key))
                {
                    var timeData = dataDict[key];
                    return timeData.timeDict["gameYear"] + "." + timeData.timeDict["gameMonth"] + "." + timeData.timeDict["gameDay"];
                }
                else return String.Empty;
            }
        }

        /// <summary>
        /// 获取存档数据中的场景名字
        /// </summary>
        /// <value></value>
        public string DataScene
        {
            get
            {
                var key = TransitionManager.Instance.GUID;
                if(dataDict.ContainsKey(key))
                {
                    var transitionData = dataDict[key];
                    return transitionData.dataSceneName switch
                    {
                        "01.Field" => "Farm",
                        "02.Home" => "Home",
                        _ => string.Empty
                    };
                }
                else return String.Empty;
            }
        }
    }
}
