namespace ImproveGame.Content.Functions.WeatherControl;

/// <summary>
/// 一个气候控制项的抽象，内置项与外部模组项都实现这个接口
/// </summary>
public interface IWeatherControl
{
    /// <summary> 注册源 Mod 的内部名 </summary>
    string ModName { get; }

    /// <summary> 项目的内部名 </summary>
    string Name { get; }

    /// <summary> 唯一标识，等于 ModName 加冒号加 Name </summary>
    string Id => $"{ModName}:{Name}";

    /// <summary> 槽位图标 </summary>
    Texture2D Icon { get; }

    /// <summary>
    /// 所属内容栏的 Id，为空则归到默认的模组项网格
    /// </summary>
    string GroupId { get; }

    /// <summary> 排序优先级，越高越靠前 </summary>
    int Priority { get; }

    /// <summary>
    /// 档位名数组，至少包含两个
    /// 名字是模组内部用的英文键，不直接显示
    /// </summary>
    IReadOnlyList<string> Stages { get; }

    /// <summary> 是否支持锁定 </summary>
    bool SupportsLock { get; }

    /// <summary> 当前是否可用，不可用时 UI 灰显或隐藏 </summary>
    bool IsAvailable { get; }

    /// <summary> 显示名 </summary>
    string GetDisplayName();

    /// <summary> 悬停说明，可返回 null </summary>
    string GetTooltip();

    /// <summary> 取当前档位下标，未知返回 -1 </summary>
    int GetStage();

    /// <summary>
    /// 应用一个档位到本地
    /// 这一层不处理网络同步，网络同步在 <see cref="WeatherControlRegistry"/> 的派发层完成
    /// </summary>
    void SetStage(int stage);

    /// <summary> 取当前是否锁定 </summary>
    bool GetLocked();

    /// <summary>
    /// 应用锁定状态到本地
    /// 这一层不处理网络同步
    /// </summary>
    void SetLocked(bool locked);
}
