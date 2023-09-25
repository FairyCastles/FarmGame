using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Farm.NPC;

namespace Farm.Dialogue
{
    [RequireComponent(typeof(NPCMovement))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class DialogueController : MonoBehaviour
    {
        private NPCMovement npc => GetComponent<NPCMovement>();
        public UnityEvent OnFinishEvent;
        public List<DialoguePiece> dialogueList = new List<DialoguePiece>();

        private Queue<DialoguePiece> dailogueQueue;

        private bool canTalk;
        private bool isTalking;
        private GameObject uiSign;

        #region Lift Function

        private void Awake()
        {
            uiSign = transform.GetChild(1).gameObject;
            FillDialogueQueue();
        }

        private void Update()
        {
            uiSign.SetActive(canTalk);

            // 按下空格对话
            if (canTalk & Input.GetKeyDown(KeyCode.Space) && !isTalking)
            {
                StartCoroutine(DailogueRoutine());
            }
        }

        #endregion

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                canTalk = !npc.isMoving && npc.interactable;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                canTalk = false;
            }
        }

        /// <summary>
        /// 构建对话队列
        /// </summary>
        private void FillDialogueQueue()
        {
            dailogueQueue = new Queue<DialoguePiece>();
            foreach(DialoguePiece dialogue in dialogueList)
            {
                dialogue.isDone = false;
                dailogueQueue.Enqueue(dialogue);
            }
        }

        private IEnumerator DailogueRoutine()
        {
            isTalking = true;
            if (dailogueQueue.TryDequeue(out DialoguePiece result))
            {
                // 传到 UI 显示对话
                EventHandler.CallShowDialogueEvent(result);
                EventHandler.CallUpdateGameStateEvent(GameState.Pause);
                yield return new WaitUntil(() => result.isDone);
                isTalking = false;
            }
            // 对话结束
            else
            {
                EventHandler.CallShowDialogueEvent(null);
                EventHandler.CallUpdateGameStateEvent(GameState.Gameplay);
                FillDialogueQueue();
                isTalking = false;

                // 在触发对话结束时间后，禁止再次交互进行对话
                if (OnFinishEvent != null)
                {
                    OnFinishEvent.Invoke();
                    canTalk = false;
                }
            }
        }
    }
}

