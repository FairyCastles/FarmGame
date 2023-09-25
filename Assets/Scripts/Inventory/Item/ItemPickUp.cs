using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Farm.Inventory
{
    public class ItemPickUp : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other) 
        {
            Item item = other.GetComponent<Item>();

            if(item != null)
            {
                // 不能直接拾取可以收割的物体s
                if(item.itemDetails.itemType != ItemType.ReapableScenery)
                {
                    // 拾取物体，添加到背包中
                    InventoryManager.Instance.AddItem(item, true);

                    // 播放拾取音效
                    EventHandler.CallPlaySoundEvent(SoundName.Pickup);
                }
            }
        }
    }
}


