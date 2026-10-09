using Microsoft.Xna.Framework.Graphics;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace ImproveGame;
// 复制到你的项目中之后记得右键解决方案资源管理器中的项目然后同步命名空间
// Copy it to your project and sync the namespace

// 这个文件提供 ImproveGame 气候控制面板的跨模组注册支持
// This file provides cross-mod assistance for registering items in the Weather Control panel
public static class ImproveGame_WeatherControlCrossModHelper
{
    // 内置项的 Id 速查，便于跨模组调用时操作内置控制
    // Quick reference for the built-in control ids
    public const string BuiltinModName = "ImproveGame";
    public const string TimeId = "ImproveGame:Time";
    public const string MoonPhaseId = "ImproveGame:MoonPhase";
    public const string RainId = "ImproveGame:Rain";
    public const string SandstormId = "ImproveGame:Sandstorm";
    public const string WindId = "ImproveGame:Wind";

    /// <summary>
    /// 注册一个气候控制项 | Register a weather control item
    /// <br>stages 至少两个，索引顺序就是档位顺序</br>
    /// <br>stages must contain at least 2 entries</br>
    /// </summary>
    public static bool RegisterWeatherControl(Mod qot, Mod source, string name, Texture2D icon,
        Func<string> displayName, Func<string> tooltip,
        string[] stages, Func<int> getStage, Action<int> setStage,
        bool supportsLock = false, Func<bool> getLocked = null, Action<bool> setLocked = null,
        Func<bool> isAvailable = null, int priority = 0, string groupId = null)
        => (bool)qot.Call(nameof(RegisterWeatherControl), source, name, icon,
            displayName, tooltip, stages, getStage, setStage,
            supportsLock, getLocked, setLocked, isAvailable, priority, groupId);

    /// <summary>
    /// 移除一个气候控制项 | Unregister a weather control item
    /// </summary>
    public static bool UnregisterWeatherControl(Mod qot, Mod source, string name)
        => (bool)qot.Call(nameof(UnregisterWeatherControl), source, name);

    /// <summary>
    /// 注册一个自定义内容栏 | Register a custom content section
    /// </summary>
    public static bool RegisterWeatherControlGroup(Mod qot, Mod source, string name,
        Func<string> displayName, int priority, Func<UIElement> createContent, Func<bool> isAvailable = null)
        => (bool)qot.Call(nameof(RegisterWeatherControlGroup), source, name, displayName, priority, createContent, isAvailable);

    /// <summary>
    /// 移除一个自定义内容栏 | Unregister a custom content section
    /// </summary>
    public static bool UnregisterWeatherControlGroup(Mod qot, Mod source, string name)
        => (bool)qot.Call(nameof(UnregisterWeatherControlGroup), source, name);

    /// <summary>
    /// 查询某控制项的当前档位，未知返回 -1
    /// Query the current stage of a control, returns -1 if unknown
    /// </summary>
    public static int QueryWeatherControlStage(Mod qot, string id)
        => (int)qot.Call(nameof(QueryWeatherControlStage), id);

    /// <summary>
    /// 派发档位变更，所有客户端会同步应用
    /// Dispatch a stage change, all clients apply the update
    /// </summary>
    public static bool SetWeatherControlStage(Mod qot, string id, int stage)
        => (bool)qot.Call(nameof(SetWeatherControlStage), id, stage);

    /// <summary>
    /// 查询某控制项是否锁定 | Query whether a control is currently locked
    /// </summary>
    public static bool IsWeatherControlLocked(Mod qot, string id)
        => (bool)qot.Call(nameof(IsWeatherControlLocked), id);

    /// <summary>
    /// 派发锁定状态变更 | Dispatch a locked state change
    /// </summary>
    public static bool SetWeatherControlLocked(Mod qot, string id, bool locked)
        => (bool)qot.Call(nameof(SetWeatherControlLocked), id, locked);
}
