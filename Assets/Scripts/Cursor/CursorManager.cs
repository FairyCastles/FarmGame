using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Farm.CropPlant;
using Farm.Inventory;
using Farm.Map;

public class CursorManager : MonoBehaviour
{
    public Sprite normal, tool, seed, item;

    private Sprite currentSprite;
    private Image cursorImage;
    private Image buildImage;
    private RectTransform cursorCanvas;

    private Camera mainCamera;
    private Grid currGrid;

    private Vector3 mouseWorldPos;
    private Vector3Int mouseGridPos;

    private bool cursorEnable;
    private bool cursorPositionValid;

    private ItemDetails currentItem;

    private Transform PlayerTransform => FindObjectOfType<PlayerController>().transform;

    #region Life Function
    private void OnEnable()
    {
        EventHandler.ItemSelectedEvent += OnItemSelected;
        EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnloaded;
        EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoaded;
    }

    private void OnDisable()
    {
        EventHandler.ItemSelectedEvent -= OnItemSelected;
        EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnloaded;
        EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoaded;
    }

    private void Start()
    {
        cursorCanvas = GameObject.FindGameObjectWithTag("CursorCanvas").GetComponent<RectTransform>();
        cursorImage = cursorCanvas.GetChild(0).GetComponent<Image>();
        buildImage = cursorCanvas.GetChild(1).GetComponent<Image>();
        buildImage.gameObject.SetActive(false);
        currentSprite = normal;
        SetCursorImage(normal);

        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (cursorCanvas == null) return;

        // 图片跟随鼠标移动
        cursorImage.transform.position = Input.mousePosition;

        if (!InteractWithUI() && cursorEnable)
        {
            SetCursorImage(currentSprite);
            CheckCursorValid();
            CheckPlayerInput(); 
        }
        // 鼠标指向 UI，变回普通的鼠标状态
        else
        {
            SetCursorImage(normal);
            buildImage.gameObject.SetActive(false);
        }
    }

    #endregion

    #region Event Function

    /// <summary>
    /// 选择物品时，改变鼠标图片
    /// </summary>
    /// <param name="itemDetails"></param>
    /// <param name="isSelected"></param>
    private void OnItemSelected(ItemDetails itemDetails, bool isSelected)
    {
        if (!isSelected)
        {
            currentItem = null;
            cursorEnable = false;
            currentSprite = normal;
            buildImage.gameObject.SetActive(false);
        }
        // 物品被选中，切换图片
        else    
        {
            currentItem = itemDetails;
            currentSprite = itemDetails.itemType switch
            {
                ItemType.Seed => seed,
                ItemType.Commodity => item,
                ItemType.ChopTool => tool,
                ItemType.HoeTool => tool,
                ItemType.WaterTool => tool,
                ItemType.BreakTool => tool,
                ItemType.ReapTool => tool,
                ItemType.Furniture => tool,
                ItemType.CollectTool => tool,
                _ => normal,
            };
            cursorEnable = true;

            // 选择了图纸类物品，将建造的物体跟随鼠标显示
            if(itemDetails.itemType == ItemType.Furniture)
            {
                buildImage.gameObject.SetActive(true);
                buildImage.sprite = itemDetails.itemOnWorldSprite;
                // 不知道为啥 SetNativeSize 图片变得很大，现在默认图片大小不改变
                // buildImage.SetNativeSize();
            }
        }
    }

    private void OnBeforeSceneUnloaded()
    {
        cursorEnable = false;
    }

    private void OnAfterSceneLoaded()
    {
        currGrid = FindObjectOfType<Grid>();
    }

    #endregion

    /// <summary>
    /// 设置鼠标图片
    /// </summary>
    /// <param name="sprite"></param>
    private void SetCursorImage(Sprite sprite)
    {
        cursorImage.sprite = sprite;
        cursorImage.color = new Color(1, 1, 1, 1);
    }

    private void SetCursorValid()
    {
        cursorPositionValid = true;
        cursorImage.color = new Color(1, 1, 1, 1);
        buildImage.color = new Color(1, 1, 1, 0.5f);
    }

    private void SetCursorInValid()
    {
        cursorPositionValid = false;
        cursorImage.color = new Color(1, 0, 0, 0.4f);
        buildImage.color = new Color(1, 0, 0, 0.5f);
    }

    /// <summary>
    /// 判断鼠标指向位置是否能执行操作
    /// </summary>
    private void CheckCursorValid()
    {
        Vector3 pos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, -Camera.main.transform.position.z);
        mouseWorldPos = mainCamera.ScreenToWorldPoint(pos);
        mouseGridPos = currGrid.WorldToCell(mouseWorldPos);

        Vector3Int playerGridPos = currGrid.WorldToCell(PlayerTransform.position);

        // 建造图片跟随鼠标移动
        buildImage.rectTransform.position = Input.mousePosition;
        
        if (Mathf.Abs(mouseGridPos.x - playerGridPos.x) > currentItem.itemUseRadius || Mathf.Abs(mouseGridPos.y - playerGridPos.y) > currentItem.itemUseRadius)
        {
            SetCursorInValid();
            return;
        }

        TileDetails currentTile = GridMapManager.Instance.GetTileDetailsOnMousePosition(mouseGridPos);

        if(currentTile != null)
        {
            switch(currentItem.itemType)
            {
                case ItemType.Seed:
                    if (currentTile.daysSinceDug > -1 && currentTile.seedItemID == -1) SetCursorValid(); else SetCursorInValid();
                    break;
                case ItemType.Commodity:
                    if (currentTile.canDropItem && currentItem.canDropped) SetCursorValid(); else SetCursorInValid();
                    break;
                case ItemType.HoeTool:
                    if (currentTile.canDig) SetCursorValid(); else SetCursorInValid();
                    break;
                case ItemType.WaterTool:
                    // 挖过坑且未浇水，才能进行浇水
                    if(currentTile.daysSinceDug > -1 && currentTile.daysSinceWatered == -1) SetCursorValid(); else SetCursorInValid();
                    break;
                case ItemType.ChopTool:
                case ItemType.BreakTool:
                    Crop crop = GridMapManager.Instance.GetCropObject(mouseWorldPos);
                    if (crop != null)
                    {
                        if(crop.CanHarvest && crop.cropDetails.CheckToolAvailable(currentItem.itemID)) SetCursorValid();
                        else SetCursorInValid();
                    }
                    else SetCursorInValid();
                    break;
                case ItemType.CollectTool:
                    CropDetails currCrop = CropManager.Instance.GetCropDetails(currentTile.seedItemID);
                    if (currCrop != null)
                    {
                        // 判断当前工具是否可以用于收割该植物
                        if (currCrop.CheckToolAvailable(currentItem.itemID))
                            if (currentTile.growthDays >= currCrop.TotalGrowthDays) SetCursorValid(); 
                            else SetCursorInValid();
                    }
                    else SetCursorInValid();
                    break;
                case ItemType.ReapTool:
                    if (GridMapManager.Instance.HaveReapableItemsInRadius(mouseWorldPos, currentItem)) SetCursorValid();
                    else SetCursorInValid();
                    break;
                case ItemType.Furniture:
                    buildImage.gameObject.SetActive(true);
                    var bluePrintDetails = InventoryManager.Instance.bluePrintData.GetBluePrintDetails(currentItem.itemID);
                    if (currentTile.canPlaceFurniture && InventoryManager.Instance.CheckStock(currentItem.itemID) && !HaveFurnitureInRadius(bluePrintDetails))
                        SetCursorValid();
                    else SetCursorInValid();
                    break;
            }
        }
        else
        {
            SetCursorInValid();
        }
    }

    private void CheckPlayerInput()
    {
        if (Input.GetMouseButtonDown(0) && cursorPositionValid)
        {
            EventHandler.CallMouseClickedEvent(mouseWorldPos, currentItem);
        }
    }

    /// <summary>
    /// 判断是否与 UI 互动
    /// </summary>
    /// <returns></returns>
    private bool InteractWithUI()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// 判断范围内是否有其他家具
    /// </summary>
    /// <param name="bluePrintDetails"></param>
    /// <returns></returns>
    private bool HaveFurnitureInRadius(BluePrintDetails bluePrintDetails)
    {
        var buildItem = bluePrintDetails.buildPrefab;
        Vector2 point = mouseWorldPos;
        var size = buildItem.GetComponent<BoxCollider2D>().size;

        var otherColl = Physics2D.OverlapBox(point, size, 0);
        if (otherColl != null)
            return otherColl.GetComponent<Furniture>();
        else return false;
    }
}