using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEvent : MonoBehaviour
{
    // 用于人物走路时动画播放走路音效
    // TODO: 在动画中未添加，需要添加动画事件
    public void FootstepSound()
    {
        EventHandler.CallPlaySoundEvent(SoundName.FootStepSoft);
    }
}
