using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Farm.NPC
{
    public class NPCManager : Singleton<NPCManager>
    {
        public List<NPCPosition> npcPositionList;

        public SceneRouteDataList_SO sceneRouteData;

        private Dictionary<string, SceneRoute> sceneRouteDict = new Dictionary<string, SceneRoute>();


        #region Life Function

        protected override void Awake() 
        {
            base.Awake();
            InitSceneRouteDict();
        }

        private void OnEnable()
        {
            EventHandler.StartNewGameEvent += OnStartNewGame;
        }

        private void OnDisable()
        {
            EventHandler.StartNewGameEvent -= OnStartNewGame;
        }

        #endregion

        #region Event Function

        private void OnStartNewGame(int index)
        {
            foreach (var character in npcPositionList)
            {
                character.npcTransform.position = character.position;
                character.npcTransform.GetComponent<NPCMovement>().StartScene = character.startScene;
            }
        }

        #endregion

        /// <summary>
        /// 初始化路径字典
        /// </summary>
        private void InitSceneRouteDict()
        {
            if (sceneRouteData.sceneRouteList.Count > 0)
            {
                foreach (SceneRoute route in sceneRouteData.sceneRouteList)
                {
                    var key = route.fromSceneName + route.gotoSceneName;
                    if (sceneRouteDict.ContainsKey(key)) continue;
                    else sceneRouteDict.Add(key, route);
                }
            }
        }

        /// <summary>
        /// 根据起点和终点场景，获得路径
        /// </summary>
        /// <param name="fromSceneName"></param>
        /// <param name="gotoSceneName"></param>
        /// <returns></returns>
        public SceneRoute GetSceneRoute(string fromSceneName, string gotoSceneName)
        {
            return sceneRouteDict[fromSceneName + gotoSceneName];
        }
    }
}
