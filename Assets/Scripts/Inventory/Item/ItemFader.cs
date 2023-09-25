using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace Farm.Inventory
{
    public class ItemFader : MonoBehaviour
    {
        private SpriteRenderer spriteRenderer;

        #region Lift Function

        private void Awake() 
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        #endregion

        /// <summary>
        /// 物体透明度渐变恢复正常
        /// </summary>
        public void FadeIn()
        {
            Color targetColor = new Color(1, 1, 1, 1);
            spriteRenderer.DOColor(targetColor, Settings.itemFadeDuration);
        }

        /// <summary>
        /// 物体透明度变成半透明
        /// </summary>
        public void FadeOut()
        {
            Color targetColor = new Color(1, 1, 1, Settings.targetAlpha);
            spriteRenderer.DOColor(targetColor, Settings.itemFadeDuration);
        }
    }
}
