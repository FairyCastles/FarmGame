using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Farm.Map
{
    [CreateAssetMenu(fileName = "MapData_SO", menuName = "Map/MapData")]
    public class MapData_SO : ScriptableObject 
    {
        [SceneName]
        public string sceneName;

        [Header("Map Info")]
        public int gridWidth;
        public int gridHeight;

        [Header("Origin")]
        public int originX;
        public int originY;

        public List<TileProperty> tileProperties;    
    }
}