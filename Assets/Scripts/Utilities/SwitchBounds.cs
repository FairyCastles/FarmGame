using Cinemachine;
using UnityEngine;

public class SwitchBounds : MonoBehaviour
{
    private void OnEnable() 
    {
        EventHandler.AfterSceneLoadedEvent += SwitchConfinerShape;
    }

    private void OnDisable() 
    {
        EventHandler.AfterSceneLoadedEvent -= SwitchConfinerShape;
    }

    /// <summary>
    /// 找到每个场景摄像机的 Confiner，并设置到对应的 Cinemachine 上
    /// </summary>
    private void SwitchConfinerShape()
    {
        PolygonCollider2D confinerShape = GameObject.FindGameObjectWithTag("BoundsConfiner").GetComponent<PolygonCollider2D>();

        CinemachineConfiner confiner = GetComponent<CinemachineConfiner>();

        confiner.m_BoundingShape2D = confinerShape;

        // Call this if the bounding shape's points change at runtime
        confiner.InvalidatePathCache();
    }
}
