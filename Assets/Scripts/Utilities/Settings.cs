using System;
using UnityEngine;

/// <summary>
/// 管理各种可调整数据
/// </summary>
public class Settings
{
    // 物体透明度渐变持续时间
    public const float itemFadeDuration = 0.35f;

    // 加载场景遮挡的画布透明度渐变时间
    public const float loadingCanvasFadeDuration = 1f;

    // 物体半透明的透明度
    public const float targetAlpha = 0.45f;

    // 游戏时间系统的时间流速，值越小时间越快
    public const float secondThreshold = 0.01f;

    // 游戏中秒，分，时，天，季，年的临界值
    public const int secondHold = 59;
    public const int minuteHold = 59;
    public const int hourHold = 23;
    public const int dayHold = 29;
    public const int seasonHold = 3;
    public const int yearHold = 12;

    // 割草的上限个数
    public const int reapAmount = 2;

    // NPC 网格移动
    public const float gridCellSize = 1;
    public const float gridCellDiagonalSize = 1.41f;
    // 网格像素大小 1 / (20 * 20)
    public const float pixelSize = 0.05f;
    // 动画间隔时间
    public const float animationBreakTime = 5f;

    public const int maxGridSize = 9999;

    // 灯光设置
    public const float lightChangeDuration = 25f;
    public static TimeSpan morningTime = new TimeSpan(5, 0, 0);
    public static TimeSpan nightTime = new TimeSpan(19, 0, 0);

    // 玩家初始设置
    public static Vector3 playerStartPos = new Vector3(-5, -4, 0);
    public const int playerStartMoney = 100;
}
