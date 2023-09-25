using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Farm.Inventory
{
    public class ItemInteractive : MonoBehaviour
    {
        private bool isAnimation;

        private WaitForSeconds pause = new WaitForSeconds(0.05f);

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!isAnimation)
            {
                if (other.transform.position.x < transform.position.x)
                {
                    StartCoroutine(RotateRight());
                }
                else
                {
                    StartCoroutine(RotateLeft());
                }
                // 播放穿梭草地音效
                EventHandler.CallPlaySoundEvent(SoundName.Restle);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!isAnimation)
            {
                if (other.transform.position.x > transform.position.x)
                {
                    StartCoroutine(RotateRight());
                }
                else
                {
                    StartCoroutine(RotateLeft());
                }
                // 播放穿梭草地音效
                EventHandler.CallPlaySoundEvent(SoundName.Restle);
            }
        }

        private IEnumerator RotateLeft()
        {
            isAnimation = true;

            for(int i = 0; i < 5; i++)
            {
                transform.GetChild(0).Rotate(0, 0, 2);
                yield return pause;
            }
            for(int i = 0; i < 5; i++)
            {
                transform.GetChild(0).Rotate(0, 0, -2);
                yield return pause;
            }

            isAnimation = false;
        }

            private IEnumerator RotateRight()
        {
            isAnimation = true;

            for(int i = 0; i < 5; i++)
            {
                transform.GetChild(0).Rotate(0, 0, -2);
                yield return pause;
            }
            for(int i = 0; i < 5; i++)
            {
                transform.GetChild(0).Rotate(0, 0, 2);
                yield return pause;
            }

            isAnimation = false;
        }
    }
}