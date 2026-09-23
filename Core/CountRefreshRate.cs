using MonoMod.Cil;
using System.Diagnostics;

namespace ImproveGame.Core;

public class CountRefreshRate : ModSystem
{
    public static Stopwatch RefreshRateStopwatch = new();

    /// <summary>
    /// 刷新率因子，可以乘在AnimationTimer的缓动上，来根据帧率实时调节动画速度，实现高帧率下的丝滑动画。
    /// 在绘制中调用AnimationTimer.Update()方法时传入此参数，即可实现高帧缓动。
    /// </summary>
    public static float CurrentRefreshRateFactor = 1f;

    public override void Load()
    {
        // 使用IL而不是On插入代码，这样的话如果哪个Mod把DoDraw炸了，报错里不会显示我们的信息，不会有人找上门来
        // 原先的实现是在ModifyInterfaceLayers里执行下面的代码，但是那个方法在游戏主界面不会运行，配置中心的界面使用UpdateHighFps就会出问题
        IL_Main.DoDraw += IL_EmitRefreshRate;
    }

    static void IL_EmitRefreshRate(ILContext il)
    {
        var c = new ILCursor(il);

        c.EmitDelegate(() => { CurrentRefreshRateFactor = MyUtils.GetRefreshRateFactor(RefreshRateStopwatch); });
    }
}