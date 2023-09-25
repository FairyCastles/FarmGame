using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Farm.Dialogue
{
    public class DialogueUI : MonoBehaviour
    {
        public GameObject dialogueBox;
        public GameObject continueBox;
        public Text dialogueText;
        public Image faceRight, faceLeft;
        public Text nameRight, nameLeft;

        #region Life Function

        private void Awake()
        {   
            continueBox.SetActive(false);
        }

            private void OnEnable()
        {
            EventHandler.ShowDialogueEvent += OnShowDailogueEvent;
        }

        private void OnDisable()
        {
            EventHandler.ShowDialogueEvent -= OnShowDailogueEvent;
        }

        #endregion

        #region Event Function

        private void OnShowDailogueEvent(DialoguePiece piece)
        {
            StartCoroutine(ShowDialogue(piece));
        }

        #endregion

        /// <summary>
        /// 显示对话片段
        /// </summary>
        /// <param name="piece"></param>
        /// <returns></returns>
        private IEnumerator ShowDialogue(DialoguePiece piece)
        {
            if (piece != null)
            {
                piece.isDone = false;

                dialogueBox.SetActive(true);
                continueBox.SetActive(false);

                dialogueText.text = string.Empty;

                // 设置画像和名称框
                if (piece.name != string.Empty)
                {
                    if (piece.onLeft)
                    {
                        faceRight.gameObject.SetActive(false);
                        faceLeft.gameObject.SetActive(true);
                        faceLeft.sprite = piece.faceImage;
                        nameLeft.text = piece.name;
                    }
                    else
                    {
                        faceRight.gameObject.SetActive(true);
                        faceLeft.gameObject.SetActive(false);
                        faceRight.sprite = piece.faceImage;
                        nameRight.text = piece.name;
                    }
                }
                else
                {
                    faceLeft.gameObject.SetActive(false);
                    faceRight.gameObject.SetActive(false);
                    nameLeft.gameObject.SetActive(false);
                    nameRight.gameObject.SetActive(false);
                }
                // 在 1s 内显示对话文本信息
                yield return dialogueText.DOText(piece.dialogueText, 1f).WaitForCompletion();

                piece.isDone = true;

                if (piece.hasToPause && piece.isDone)
                    continueBox.SetActive(true);
            }
            else
            {
                dialogueBox.SetActive(false);
                yield break;
            }
        }
    }
}
