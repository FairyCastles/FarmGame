using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Farm.Inventory
{
    [CreateAssetMenu(fileName = "ItemDataList_SO", menuName = "Inventory/ItemDataList")]
    public class ItemDataList_SO : ScriptableObject 
    {
        public List<ItemDetails> itemDetailsList;
    }
}

