using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Farm.Map
{
    [ExecuteInEditMode]
    public class GridMap : MonoBehaviour
    {
        public MapData_SO mapData;
        public GridType gridType;
        private Tilemap currentTilemap;

        #region Life Function

        private void OnEnable()
        {
            if (!Application.IsPlaying(this))
            {
                currentTilemap = GetComponent<Tilemap>();

                if (mapData != null)
                {
                    mapData.tileProperties.Clear();
                }
            }
        }

        // 关闭 Grid Property 时，存储当前 TileMap 中所有瓦片信息
        private void OnDisable()
        {
            if (!Application.IsPlaying(this))
            {
                currentTilemap = GetComponent<Tilemap>();

                UpdateTileProperties();

                #if UNITY_EDITOR
                if (mapData != null)
                {
                    EditorUtility.SetDirty(mapData);
                }
                #endif
            }
        }

        #endregion

        private void UpdateTileProperties()
        {
            currentTilemap.CompressBounds();

            if (!Application.IsPlaying(this))
            {
                if (mapData != null)
                {
                    // 已绘制范围的左下角坐标
                    Vector3Int startPos = currentTilemap.cellBounds.min;
                    // 已绘制范围的右上角坐标
                    Vector3Int endPos = currentTilemap.cellBounds.max;

                    for (int x = startPos.x; x < endPos.x; x++)
                    {
                        for (int y = startPos.y; y < endPos.y; y++)
                        {
                            TileBase tile = currentTilemap.GetTile(new Vector3Int(x, y, 0));

                            if (tile != null)
                            {
                                TileProperty newTile = new TileProperty
                                {
                                    tileCoordinate = new Vector2Int(x, y),
                                    gridType = gridType,
                                    boolTypeValue = true
                                };

                                mapData.tileProperties.Add(newTile);
                            }
                        }
                    }
                }
            }
        }
    }
}
