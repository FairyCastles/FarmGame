using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Farm.Inventory;

public class AnimatorOverride : MonoBehaviour
{
    private Animator[] animators;

    public SpriteRenderer holdItem;

    [Header("Animator List")]
    public List<AnimatorType> animatorTypes;

    private Dictionary<string, Animator> animatorNameDict = new Dictionary<string, Animator>();

    #region Life Function

    private void Awake()
    {
        animators = GetComponentsInChildren<Animator>();

        foreach (var animator in animators)
        {
            animatorNameDict.Add(animator.name, animator);
        }
    }

    private void OnEnable()
    {
        EventHandler.ItemSelectedEvent += OnItemSelectedEvent;
        EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnload;
        EventHandler.HarvestAtPlayerPositionEvent += OnHarvestAtPlayerPosition;
    }

    private void OnDisable()
    {
        EventHandler.ItemSelectedEvent -= OnItemSelectedEvent;
        EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnload;
        EventHandler.HarvestAtPlayerPositionEvent -= OnHarvestAtPlayerPosition;
    }

    #endregion

    #region Event Function

    /// <summary>
    /// 选择物品后，修改身体各个 Animator，切换不同的动画 
    /// </summary>
    /// <param name="itemDetails"></param>
    /// <param name="isSelected"></param>
    private void OnItemSelectedEvent(ItemDetails itemDetails, bool isSelected)
    {
        // TODO: 后续添加不同物品类型的动画切换
        PartType currentType = itemDetails.itemType switch
        {
            ItemType.Seed => PartType.Carry,
            ItemType.Commodity => PartType.Carry,
            ItemType.HoeTool => PartType.Hoe,
            ItemType.WaterTool => PartType.Water,
            ItemType.CollectTool => PartType.Collect,
            ItemType.ChopTool => PartType.Chop,
            ItemType.BreakTool => PartType.Break,
            ItemType.ReapTool => PartType.Reap,
            // 可以举起家具，感觉没啥必要
            // ItemType.Furniture => PartType.Carry,
            _ => PartType.None
        };

        if (isSelected == false)
        {
            currentType = PartType.None;
            holdItem.enabled = false;
        }
        else
        {
            // 可以被举起的物品才切换动画
            if (currentType == PartType.Carry)
            {
                holdItem.sprite = itemDetails.itemOnWorldSprite;
                holdItem.enabled = true;
            }
            // 不是举起物体状态，要关闭举起的物体显示，并切换回初始状态
            // TODO: 不确定这种切换回初始状态是否会导致工具动画异常
            else
            {
                holdItem.enabled = false;
                SwitchAnimator(PartType.None);
            }
        }

        SwitchAnimator(currentType);
    }

    /// <summary>
    /// 切换场景前，将玩家动画切换成原始状态
    /// </summary>
    private void OnBeforeSceneUnload()
    {
        holdItem.enabled = false;
        SwitchAnimator(PartType.None);
    }

    /// <summary>
    /// 收获植物时，在玩家头顶显示收获的植物图片
    /// </summary>
    /// <param name="obj"></param>
    private void OnHarvestAtPlayerPosition(int ID)
    {
        Sprite itemSprite = InventoryManager.Instance.GetItemDetails(ID).itemOnWorldSprite;
        if(!holdItem.enabled)
        {
            StartCoroutine(ShowItem(itemSprite));
        }
    }

    #endregion

    /// <summary>
    /// 切换 AnimatorController
    /// </summary>
    /// <param name="partType"></param>
    private void SwitchAnimator(PartType partType)
    {
        foreach (var animatorType in animatorTypes)
        {
            if (animatorType.partType == partType)
            {
                // 根据名称，切换到对应的 Animator Controller
                animatorNameDict[animatorType.partName.ToString()].runtimeAnimatorController = animatorType.overrideController;
            }
        }
    }

    /// <summary>
    /// 用协程短暂显示收获的物品
    /// </summary>
    /// <param name="itemSprite"></param>
    /// <returns></returns>
    private IEnumerator ShowItem(Sprite itemSprite)
    {
        holdItem.sprite = itemSprite;
        holdItem.enabled = true;
        yield return new WaitForSeconds(1f);
        holdItem.enabled = false;
    }
}