namespace ImproveGame.Content.Functions.WeatherControl;

/// <summary>
/// 把 Mod.Call 收到的弱引用参数转成 <see cref="WeatherControlInfo"/> 注册进 Registry
/// </summary>
internal static class WeatherControlCrossMod
{
    public static bool RegisterControl(object[] args)
    {
        int len = args.Length;
        if (len < 9) return false;

        var sourceMod = args[1] as Mod;
        if (sourceMod is null) return false;

        var name = args[2] as string;
        if (string.IsNullOrEmpty(name)) return false;

        var icon = args[3] as Texture2D;
        var displayName = ToStringFunc(args[4]);
        var tooltip = ToStringFunc(args[5]);

        var stages = args[6] as IReadOnlyList<string> ?? (args[6] is string[] arr ? arr : null);
        if (stages is null || stages.Count < 2) return false;

        var stageProvider = args[7] as Func<int>;
        var stageSetter = args[8] as Action<int>;

        bool supportsLock = len > 9 && args[9] is bool b9 && b9;
        var lockedProvider = len > 10 ? args[10] as Func<bool> : null;
        var lockedSetter = len > 11 ? args[11] as Action<bool> : null;
        var availableProvider = len > 12 ? args[12] as Func<bool> : null;
        int priority = len > 13 && args[13] is int p ? p : 0;
        var groupId = len > 14 ? args[14] as string : null;

        var info = new WeatherControlInfo
        {
            ModName = sourceMod.Name,
            Name = name,
            GroupId = groupId,
            Priority = priority,
            Icon = icon,
            Stages = stages,
            SupportsLock = supportsLock,
            DisplayNameProvider = displayName,
            TooltipProvider = tooltip,
            AvailableProvider = availableProvider,
            StageProvider = stageProvider,
            StageSetter = stageSetter,
            LockedProvider = lockedProvider,
            LockedSetter = lockedSetter,
        };

        return WeatherControlRegistry.Instance.Register(info);
    }

    public static bool UnregisterControl(object[] args)
    {
        if (args.Length < 3) return false;
        var sourceMod = args[1] as Mod;
        var name = args[2] as string;
        if (sourceMod is null || string.IsNullOrEmpty(name)) return false;
        return WeatherControlRegistry.Instance.Unregister(sourceMod.Name, name);
    }

    public static bool RegisterGroup(object[] args)
    {
        if (args.Length < 6) return false;
        var sourceMod = args[1] as Mod;
        var name = args[2] as string;
        var displayName = ToStringFunc(args[3]);
        if (sourceMod is null || string.IsNullOrEmpty(name)) return false;
        int priority = args[4] is int p ? p : 0;
        var createContent = args[5] as Func<UIElement>;
        if (createContent is null) return false;
        var availableProvider = args.Length > 6 ? args[6] as Func<bool> : null;

        var group = new CrossModWeatherControlGroup
        {
            ModName = sourceMod.Name,
            Name = name,
            Priority = priority,
            DisplayNameProvider = displayName,
            AvailableProvider = availableProvider,
            ContentProvider = createContent,
        };

        return WeatherControlRegistry.Instance.RegisterGroup(group);
    }

    public static bool UnregisterGroup(object[] args)
    {
        if (args.Length < 3) return false;
        var sourceMod = args[1] as Mod;
        var name = args[2] as string;
        if (sourceMod is null || string.IsNullOrEmpty(name)) return false;
        return WeatherControlRegistry.Instance.UnregisterGroup(sourceMod.Name, name);
    }

    public static int QueryStage(string id)
        => WeatherControlRegistry.Instance.TryGet(id, out var c) ? c.GetStage() : -1;

    public static bool DispatchStage(string id, int stage)
    {
        if (string.IsNullOrEmpty(id)) return false;
        if (!WeatherControlRegistry.Instance.TryGet(id, out _)) return false;
        WeatherControlRegistry.Instance.DispatchStage(id, stage);
        return true;
    }

    public static bool QueryLocked(string id)
        => WeatherControlRegistry.Instance.TryGet(id, out var c) && c.GetLocked();

    public static bool DispatchLocked(string id, bool locked)
    {
        if (string.IsNullOrEmpty(id)) return false;
        if (!WeatherControlRegistry.Instance.TryGet(id, out var c)) return false;
        if (!c.SupportsLock) return false;
        WeatherControlRegistry.Instance.DispatchLocked(id, locked);
        return true;
    }

    private static Func<string> ToStringFunc(object arg)
    {
        return arg switch
        {
            null => null,
            Func<string> f => f,
            LocalizedText lt => () => lt.Value,
            string s => () => s,
            _ => null
        };
    }
}

/// <summary>
/// Mod.Call 注册自定义内容栏用的实现
/// </summary>
internal sealed class CrossModWeatherControlGroup : IWeatherControlGroup
{
    public string ModName { get; init; }
    public string Name { get; init; }
    public int Priority { get; init; }
    public Func<string> DisplayNameProvider { get; init; }
    public Func<bool> AvailableProvider { get; init; }
    public Func<UIElement> ContentProvider { get; init; }

    public bool IsAvailable => AvailableProvider is null || AvailableProvider();

    public string GetDisplayName() => DisplayNameProvider?.Invoke() ?? Name;

    public UIElement CreateView(IReadOnlyList<IWeatherControl> controls)
        => ContentProvider?.Invoke();
}
