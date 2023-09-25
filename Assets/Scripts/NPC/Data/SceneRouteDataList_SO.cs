using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Farm.NPC
{
    [CreateAssetMenu(fileName = "SceneRouteDataList_SO", menuName = "Map/SceneRouteData")]
    public class SceneRouteDataList_SO : ScriptableObject 
    {
        public List<SceneRoute> sceneRouteList;
    }
}
