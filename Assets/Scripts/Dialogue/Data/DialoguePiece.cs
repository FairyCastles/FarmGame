using UnityEngine;

namespace Farm.Dialogue
{
    [System.Serializable]
    public class DialoguePiece
    {
        [Header("Dialogue Info")]
        public Sprite faceImage;
        public bool onLeft;
        public string name;
        [TextArea]
        public string dialogueText;
        public bool hasToPause;
        public bool isDone;
    }
}
