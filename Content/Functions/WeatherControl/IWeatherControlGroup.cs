namespace ImproveGame.Content.Functions.WeatherControl;

/// <summary>
/// 气候控制面板里的一整栏内容
/// 内置艺术画面与模组项网格各是一栏，外部 Mod 可以注册自己的栏
/// </summary>
public interface IWeatherControlGroup
{
    /// <summary> 注册源 Mod 的内部名 </summary>
    string ModName { get; }

    /// <summary> 内容栏的内部名 </summary>
    string Name { get; }

    /// <summary> 唯一标识 </summary>
    string Id => $"{ModName}:{Name}";

    /// <summary> 排序优先级，越高越靠前 </summary>
    int Priority { get; }

    /// <summary> 该栏当前是否可用 </summary>
    bool IsAvailable { get; }

    /// <summary> 显示名 </summary>
    string GetDisplayName();

    /// <summary>
    /// 创建该栏的视图，每次 WeatherGUI 打开时调用一次
    /// 返回的 UIElement 会作为子元素挂到主面板下
    /// </summary>
    UIElement CreateView(IReadOnlyList<IWeatherControl> controls);
}
