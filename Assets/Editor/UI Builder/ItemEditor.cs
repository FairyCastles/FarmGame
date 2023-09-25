using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Farm.Inventory
{
    public class ItemEditor : EditorWindow
    {
        // 原始数据信息文件
        private ItemDataList_SO dataBase;
        private List<ItemDetails> itemList = new List<ItemDetails>();
        // 左侧列表信息的模板文件
        private VisualTreeAsset itemRowTemplate;
        private ScrollView itemDetailsSection;
        // 被选中物体的详细信息
        private ItemDetails activeItem;

        //默认预览图片
        private Sprite defaultIcon;
        private VisualElement iconPreview;
        //获得VisualElement
        private ListView itemListView;

        [MenuItem("Custom/ItemEditor")]
        public static void ShowExample()
        {
            ItemEditor wnd = GetWindow<ItemEditor>();
            wnd.titleContent = new GUIContent("ItemEditor");
        }

        /// <summary>
        /// Editor 绘制的主函数
        /// </summary>
        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            // Import UXML
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UI Builder/ItemEditor.uxml");
            VisualElement labelFromUXML = visualTree.Instantiate();
            root.Add(labelFromUXML);

            // 获取左侧列表的默认模板结构数据
            itemRowTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UI Builder/ItemRow Template.uxml");

            // 获取默认的 Icon
            defaultIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/M Studio/Art/Items/Icons/icon_Game.png");

            // 获取左侧列表和右侧详细信息面板
            itemListView = root.Q<VisualElement>("ItemList").Q<ListView>("ListView");
            itemDetailsSection = root.Q<ScrollView>("ItemDetails");
            // 获取右侧详细信息面板的预览 Icon
            iconPreview = itemDetailsSection.Q<VisualElement>("Icon");

            // 获取添加信息和删除信息的按钮，并且添加事件
            root.Q<Button>("AddButton").clicked += OnAddItemClicked;
            root.Q<Button>("DeleteButton").clicked += OnDeleteClicked;
            // 加载 ScriptObject 数据
            LoadDataBase();

            // 生成 ListView
            GenerateListView();
        }

        private void OnAddItemClicked()
        {
            // 创建新的信息，并添加到 List 中
            ItemDetails newItem = new ItemDetails();
            newItem.itemName = "NEW ITEM";
            newItem.itemID = 1001 + itemList.Count;
            itemList.Add(newItem);
            itemListView.Rebuild();
        }

        private void OnDeleteClicked()
        {
            itemList.Remove(activeItem);
            // 移除当前物体后，重建左侧列表，并关闭右侧详细信息面板
            itemListView.Rebuild();
            // 重排 ID，避免删除元素后导致新添加元素 ID 与最后一个元素 ID 相同
            ReorderID();
            itemDetailsSection.visible = false;
        }

        private void ReorderID()
        {
            for(int i = 0; i < itemList.Count; i++)
            {
                itemList[i].itemID = 1001 + i;
            }
        }

        /// <summary>
        /// 加载原始数据文件
        /// </summary>
        private void LoadDataBase()
        {
            // 查找存储信息的 ScriptObject
            var dataArray = AssetDatabase.FindAssets("ItemDataList_SO");

            if (dataArray.Length > 1)
            {
                // 获取 GUID，并且加载数据
                var path = AssetDatabase.GUIDToAssetPath(dataArray[0]);
                dataBase = AssetDatabase.LoadAssetAtPath(path, typeof(ItemDataList_SO)) as ItemDataList_SO;
            }

            itemList = dataBase.itemDetailsList;
            // 让 ScriptObject 中的数据和 Editor 中显示信息同步，可以应用更改
            EditorUtility.SetDirty(dataBase);
        }

        /// <summary>
        /// 生成左侧列表信息
        /// </summary>
        private void GenerateListView()
        {
            // 根据模板获取左侧信息样式
            Func<VisualElement> makeItem = () => itemRowTemplate.CloneTree();

            // 创建生成的事件，绑定信息
            Action<VisualElement, int> bindItem = (e, i) =>
            {
                if (i < itemList.Count)
                {
                    // 设置 Icon 和 Name
                    if (itemList[i].itemIcon != null)
                        e.Q<VisualElement>("Icon").style.backgroundImage = itemList[i].itemIcon.texture;
                    e.Q<Label>("Name").text = itemList[i] == null ? "NO ITEM" : itemList[i].itemName;
                }
            };

            //根据需要高度调整数值
            itemListView.fixedItemHeight = 50;  
            itemListView.itemsSource = itemList;
            itemListView.makeItem = makeItem;
            itemListView.bindItem = bindItem;

            // 注册事件，当选择物体改变时调用
            itemListView.onSelectionChange += OnListSelectionChange;

            // 默认右侧详细信息面板不可见
            itemDetailsSection.visible = false;
        }

        /// <summary>
        /// 当选择物体被改变时调用的事件
        /// </summary>
        /// <param name="selectedItem"></param>
        private void OnListSelectionChange(IEnumerable<object> selectedItem)
        {
            activeItem = selectedItem.First() as ItemDetails;
            GetItemDetails();
            itemDetailsSection.visible = true;
        }

        /// <summary>
        /// 获取每个物体数据的详细信息
        /// </summary>
        private void GetItemDetails()
        {
            // 允许在 Editor 中对数据的修改会同步到 ScriptObject
            itemDetailsSection.MarkDirtyRepaint();

            // 获取每个物体信息在右侧面板的对应元素，并实时设置值的更新
            // 当物体每个信息被更改时，为其注册一个回调函数
            itemDetailsSection.Q<IntegerField>("ItemID").value = activeItem.itemID;
            itemDetailsSection.Q<IntegerField>("ItemID").RegisterValueChangedCallback(evt =>
            {
                activeItem.itemID = evt.newValue;
            });

            itemDetailsSection.Q<TextField>("ItemName").value = activeItem.itemName;
            itemDetailsSection.Q<TextField>("ItemName").RegisterValueChangedCallback(evt =>
            {
                activeItem.itemName = evt.newValue;
                // 物体名称改变时，要刷新左侧列表
                itemListView.Rebuild();
            });

            iconPreview.style.backgroundImage = activeItem.itemIcon == null ? defaultIcon.texture : activeItem.itemIcon.texture;
            itemDetailsSection.Q<ObjectField>("ItemIcon").value = activeItem.itemIcon;
            itemDetailsSection.Q<ObjectField>("ItemIcon").RegisterValueChangedCallback(evt =>
            {
                Sprite newIcon = evt.newValue as Sprite;
                activeItem.itemIcon = newIcon;
                iconPreview.style.backgroundImage = newIcon == null ? defaultIcon.texture : newIcon.texture;
                // 物体 Icon 改变时，要刷新左侧列表
                itemListView.Rebuild();
            });

            itemDetailsSection.Q<ObjectField>("ItemSprite").value = activeItem.itemOnWorldSprite;
            itemDetailsSection.Q<ObjectField>("ItemSprite").RegisterValueChangedCallback(evt =>
            {
                activeItem.itemOnWorldSprite = (Sprite)evt.newValue;
            });

            itemDetailsSection.Q<EnumField>("ItemType").Init(activeItem.itemType);
            itemDetailsSection.Q<EnumField>("ItemType").value = activeItem.itemType;
            itemDetailsSection.Q<EnumField>("ItemType").RegisterValueChangedCallback(evt =>
            {
                activeItem.itemType = (ItemType)evt.newValue;
            });

            itemDetailsSection.Q<TextField>("Description").value = activeItem.itemDescription;
            itemDetailsSection.Q<TextField>("Description").RegisterValueChangedCallback(evt =>
            {
                activeItem.itemDescription = evt.newValue;
            });

            itemDetailsSection.Q<IntegerField>("ItemUseRadius").value = activeItem.itemUseRadius;
            itemDetailsSection.Q<IntegerField>("ItemUseRadius").RegisterValueChangedCallback(evt =>
            {
                activeItem.itemUseRadius = evt.newValue;
            });

            itemDetailsSection.Q<Toggle>("CanPickedup").value = activeItem.canPickedup;
            itemDetailsSection.Q<Toggle>("CanPickedup").RegisterValueChangedCallback(evt =>
            {
                activeItem.canPickedup = evt.newValue;
            });

            itemDetailsSection.Q<Toggle>("CanDropped").value = activeItem.canDropped;
            itemDetailsSection.Q<Toggle>("CanDropped").RegisterValueChangedCallback(evt =>
            {
                activeItem.canDropped = evt.newValue;
            });

            itemDetailsSection.Q<Toggle>("CanCarried").value = activeItem.canCarried;
            itemDetailsSection.Q<Toggle>("CanCarried").RegisterValueChangedCallback(evt =>
            {
                activeItem.canCarried = evt.newValue;
            });

            itemDetailsSection.Q<IntegerField>("Price").value = activeItem.itemPrice;
            itemDetailsSection.Q<IntegerField>("Price").RegisterValueChangedCallback(evt =>
            {
                activeItem.itemPrice = evt.newValue;
            });

            itemDetailsSection.Q<Slider>("SellPercentage").value = activeItem.sellPercentage;
            itemDetailsSection.Q<Slider>("SellPercentage").RegisterValueChangedCallback(evt =>
            {
                activeItem.sellPercentage = evt.newValue;
            });
        }
    }
}