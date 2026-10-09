using Terraria.GameContent.Events;

namespace ImproveGame.Content.Functions.WeatherControl;

/// <summary>
/// 把内置 5 项注册进 <see cref="WeatherControlRegistry"/>，让外部 Mod 也能查询和操作它们
/// </summary>
internal static class WeatherBuiltinControls
{
    public const string TimeId = "Time";
    public const string MoonPhaseId = "MoonPhase";
    public const string RainId = "Rain";
    public const string SandstormId = "Sandstorm";
    public const string WindId = "Wind";

    public static string FullId(string name) => $"{WeatherControlRegistry.BuiltinModName}:{name}";

    public static void RegisterAll(WeatherControlRegistry registry)
    {
        string groupId = WeatherControlRegistry.BuiltinGroupId;

        registry.Register(new WeatherControlInfo
        {
            ModName = WeatherControlRegistry.BuiltinModName,
            Name = TimeId,
            GroupId = groupId,
            Priority = 50,
            Icon = ModAsset.ClockHighlight.Value,
            Stages = ["Dawn", "Noon", "Dusk", "Midnight"],
            SupportsLock = false,
            DisplayNameProvider = () => GetText("UI.WeatherGUI.Time"),
            AvailableProvider = () => Main.hardMode,
            StageProvider = () => -1,
            StageSetter = ApplyTime,
        });

        registry.Register(new WeatherControlInfo
        {
            ModName = WeatherControlRegistry.BuiltinModName,
            Name = MoonPhaseId,
            GroupId = groupId,
            Priority = 40,
            Icon = ModAsset.MoonPhaseHighlight.Value,
            Stages = ["Phase0", "Phase1", "Phase2", "Phase3", "Phase4", "Phase5", "Phase6", "Phase7"],
            SupportsLock = true,
            DisplayNameProvider = () => GetText("UI.WeatherGUI.MoonPhase"),
            StageProvider = () => Main.moonPhase,
            StageSetter = ApplyMoonPhase,
            LockedProvider = () => WeatherController.MoonPhaseLocked,
            LockedSetter = v => WeatherController.MoonPhaseLocked = v,
        });

        registry.Register(new WeatherControlInfo
        {
            ModName = WeatherControlRegistry.BuiltinModName,
            Name = RainId,
            GroupId = groupId,
            Priority = 30,
            Icon = ModAsset.RainActive.Value,
            Stages = ["Off", "On"],
            SupportsLock = true,
            DisplayNameProvider = () => GetText("UI.WeatherGUI." + (Main.raining ? "RainInactive" : "RainActive")),
            StageProvider = () => Main.raining ? 1 : 0,
            StageSetter = ApplyRain,
            LockedProvider = () => WeatherController.RainLocked,
            LockedSetter = v => WeatherController.RainLocked = v,
        });

        registry.Register(new WeatherControlInfo
        {
            ModName = WeatherControlRegistry.BuiltinModName,
            Name = SandstormId,
            GroupId = groupId,
            Priority = 20,
            Icon = ModAsset.SandstormActive.Value,
            Stages = ["Off", "On"],
            SupportsLock = true,
            DisplayNameProvider = () => GetText("UI.WeatherGUI." + (Sandstorm.Happening ? "SandstormInactive" : "SandstormActive")),
            StageProvider = () => Sandstorm.Happening ? 1 : 0,
            StageSetter = ApplySandstorm,
            LockedProvider = () => WeatherController.SandstormLocked,
            LockedSetter = v => WeatherController.SandstormLocked = v,
        });

        registry.Register(new WeatherControlInfo
        {
            ModName = WeatherControlRegistry.BuiltinModName,
            Name = WindId,
            GroupId = groupId,
            Priority = 10,
            Icon = ModAsset.Wheel.Value,
            Stages = ["West", "No", "East"],
            SupportsLock = true,
            DisplayNameProvider = () => GetText("UI.WeatherGUI.Wind"),
            StageProvider = GetWindStage,
            StageSetter = ApplyWind,
            LockedProvider = () => WeatherController.WindLocked,
            LockedSetter = v => WeatherController.WindLocked = v,
        });
    }

    private static void ApplyTime(int stage)
    {
        switch (stage)
        {
            case 0: Main.SkipToTime(0, setIsDayTime: true); break;
            case 1: Main.SkipToTime(27000, setIsDayTime: true); break;
            case 2: Main.SkipToTime(0, setIsDayTime: false); break;
            case 3: Main.SkipToTime(16200, setIsDayTime: false); break;
        }
    }

    private static void ApplyMoonPhase(int stage)
    {
        if (stage < 0) stage = 0;
        Main.moonPhase = stage % 8;
    }

    private static void ApplyRain(int stage)
    {
        bool target = stage != 0;
        if (target == Main.raining) return;

        if (target)
        {
            Main.StartRain();
            Main.cloudAlpha = 0.7f;
            Main.maxRaining = 0.7f;
        }
        else
        {
            Main.StopRain();
            Main.cloudAlpha = 0f;
            Main.maxRaining = 0f;
        }
    }

    private static void ApplySandstorm(int stage)
    {
        bool target = stage != 0;
        if (target == Sandstorm.Happening) return;

        if (target)
            Sandstorm.StartSandstorm();
        else
            Sandstorm.StopSandstorm();
    }

    private static int GetWindStage()
    {
        // 与原版 SetWindPacket 的 stage 数值对齐
        // 0=West, 1=No, 2=East 三档，无风段按绝对值阈值划分
        float w = Main.windSpeedCurrent;
        if (w >= 0.4f) return 0;
        if (w <= -0.4f) return 2;
        return 1;
    }

    private static void ApplyWind(int stage)
    {
        float v = stage switch
        {
            0 => 0.61f,
            2 => -0.61f,
            _ => Main.rand.NextFloat(-0.04f, 0.04f)
        };
        Main.windSpeedCurrent = Main.windSpeedTarget = v;
    }
}
