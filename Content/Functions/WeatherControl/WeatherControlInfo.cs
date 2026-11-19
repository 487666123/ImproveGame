namespace ImproveGame.Content.Functions.WeatherControl;

/// <summary>
/// 普通气候控制项的具体实现，内置项与 Mod.Call 注册的项都用这个
/// </summary>
public sealed class WeatherControlInfo : IWeatherControl
{
    public string ModName { get; init; }
    public string Name { get; init; }
    public string GroupId { get; init; }
    public int Priority { get; init; }
    public Texture2D Icon { get; init; }

    public IReadOnlyList<string> Stages { get; init; } = ["Off", "On"];
    public bool SupportsLock { get; init; }

    public Func<string> DisplayNameProvider { get; init; }
    public Func<string> TooltipProvider { get; init; }
    public Func<bool> AvailableProvider { get; init; }
    public Func<int> StageProvider { get; init; }
    public Action<int> StageSetter { get; init; }
    public Func<bool> LockedProvider { get; init; }
    public Action<bool> LockedSetter { get; init; }

    public bool IsAvailable => AvailableProvider is null || AvailableProvider();

    public string GetDisplayName() => DisplayNameProvider?.Invoke() ?? Name;
    public string GetTooltip() => TooltipProvider?.Invoke();

    public int GetStage() => StageProvider?.Invoke() ?? -1;
    public void SetStage(int stage) => StageSetter?.Invoke(stage);

    public bool GetLocked() => SupportsLock && (LockedProvider?.Invoke() ?? false);

    public void SetLocked(bool locked)
    {
        if (!SupportsLock) return;
        LockedSetter?.Invoke(locked);
    }
}
