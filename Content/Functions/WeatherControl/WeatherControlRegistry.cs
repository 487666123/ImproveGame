using ImproveGame.Packets.Weather;

namespace ImproveGame.Content.Functions.WeatherControl;

/// <summary>
/// 气候控制注册中心，内置项与外部 Mod 项都在这里登记
/// </summary>
public sealed class WeatherControlRegistry : ModSystem
{
    /// <summary> 内置项使用的 ModName </summary>
    public const string BuiltinModName = "ImproveGame";

    /// <summary> 内置艺术画面栏的 Name </summary>
    public const string BuiltinGroupName = "Builtin";

    /// <summary> 模组项默认网格栏的 Name </summary>
    public const string ModdedGroupName = "Modded";

    /// <summary> 内置艺术画面栏的完整 Id </summary>
    public static string BuiltinGroupId => $"{BuiltinModName}:{BuiltinGroupName}";

    /// <summary> 模组项默认网格栏的完整 Id </summary>
    public static string ModdedGroupId => $"{BuiltinModName}:{ModdedGroupName}";

    public static WeatherControlRegistry Instance => ModContent.GetInstance<WeatherControlRegistry>();

    private readonly Dictionary<string, IWeatherControl> _controls = [];
    private readonly Dictionary<string, IWeatherControlGroup> _groups = [];

    /// <summary>
    /// 注册一个控制项，重名直接覆盖
    /// </summary>
    public bool Register(IWeatherControl control)
    {
        if (control is null) return false;
        if (string.IsNullOrEmpty(control.ModName) || string.IsNullOrEmpty(control.Name)) return false;
        if (control.Stages is null || control.Stages.Count < 2) return false;
        _controls[control.Id] = control;
        return true;
    }

    public bool Unregister(string modName, string name)
    {
        if (string.IsNullOrEmpty(modName) || string.IsNullOrEmpty(name)) return false;
        return _controls.Remove($"{modName}:{name}");
    }

    public int UnregisterAllOf(string modName)
    {
        if (string.IsNullOrEmpty(modName)) return 0;
        int count = 0;
        foreach (var id in _controls.Where(p => p.Value.ModName == modName).Select(p => p.Key).ToList())
        {
            _controls.Remove(id);
            count++;
        }
        foreach (var id in _groups.Where(p => p.Value.ModName == modName).Select(p => p.Key).ToList())
        {
            _groups.Remove(id);
            count++;
        }
        return count;
    }

    public bool TryGet(string id, out IWeatherControl control) => _controls.TryGetValue(id, out control);

    public IWeatherControl Get(string id) => _controls.TryGetValue(id, out var c) ? c : null;

    public IReadOnlyCollection<IWeatherControl> AllControls => _controls.Values;

    public bool RegisterGroup(IWeatherControlGroup group)
    {
        if (group is null) return false;
        if (string.IsNullOrEmpty(group.ModName) || string.IsNullOrEmpty(group.Name)) return false;
        _groups[group.Id] = group;
        return true;
    }

    public bool UnregisterGroup(string modName, string name)
    {
        if (string.IsNullOrEmpty(modName) || string.IsNullOrEmpty(name)) return false;
        return _groups.Remove($"{modName}:{name}");
    }

    /// <summary> 已注册的所有内容栏，按优先级倒序 </summary>
    public IEnumerable<IWeatherControlGroup> SortedGroups() => _groups.Values.OrderByDescending(g => g.Priority);

    /// <summary>
    /// 默认模组项网格栏要展示的控制项
    /// 即未指定 GroupId、或显式指定到模组项网格栏的项
    /// </summary>
    public IEnumerable<IWeatherControl> ModdedGridControls()
    {
        string moddedId = ModdedGroupId;
        return _controls.Values
            .Where(c => string.IsNullOrEmpty(c.GroupId) || c.GroupId == moddedId)
            .OrderByDescending(c => c.Priority);
    }

    /// <summary> 指定栏下的控制项 </summary>
    public IEnumerable<IWeatherControl> ControlsInGroup(string groupId)
    {
        return _controls.Values
            .Where(c => c.GroupId == groupId)
            .OrderByDescending(c => c.Priority);
    }

    /// <summary>
    /// 发送档位变更，所有客户端会同步应用
    /// 单机会原地应用一次
    /// </summary>
    public void DispatchStage(string id, int stage)
    {
        if (!_controls.ContainsKey(id)) return;
        WeatherStageSyncPacket.Send(id, stage);
    }

    /// <summary>
    /// 发送锁定状态变更
    /// </summary>
    public void DispatchLocked(string id, bool locked)
    {
        if (!TryGet(id, out var c)) return;
        if (!c.SupportsLock) return;
        WeatherLockSyncPacket.Send(id, locked);
    }

    public override void Load()
    {
    }

    public override void PostSetupContent()
    {
        WeatherBuiltinControls.RegisterAll(this);
    }

    public override void Unload()
    {
        _controls.Clear();
        _groups.Clear();
    }
}
