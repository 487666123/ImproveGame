using ImproveGame.Common.Configs;
using Terraria.GameContent.Creative;
using Terraria.GameContent.Events;
using Terraria.ModLoader.IO;

namespace ImproveGame.Content.Functions;

public sealed class WeatherController : ModSystem
{
    /// <summary>
    /// 是否能够使用天气控制功能
    /// </summary>
    public static bool Enabled => ImproveConfigs.Instance.WeatherControl && Unlocked;

    public static bool Unlocked;

    // 为了适配别的作弊功能Mod和原版，这里的锁都是软锁
    // 在PreUpdateEntities记录状态，再在PostUpdateWorld将状态赋值到原版的字段上
    // 在这俩方法调用期间的值改也没用，而在这期间之外的值改了就有用
    // 这样可以适配其他Mod的UI的作弊功能，但也可能锁不住其他Mod伪造的“自然改变”
    public static bool RainLocked;
    public static bool SandstormLocked;
    public static bool MoonPhaseLocked;
    public static bool WindLocked;

    private double _rainTime;
    private float _maxRaining;
    private bool _raining;

    private bool _happening;
    private double _timeLeft;

    private int _moonPhase;

    private float _windSpeedCurrent;
    private float _windSpeedTarget;
    private int _windCounter;
    private int _extremeWindCounter;

    public override void Load()
    {
        // 这β玩意在PreUpdateEntities之前调用，而它之前就没有Hook了，所以必须搁这特殊On了
        On_Main.UpdateWeather += (orig, self, time, iteration) =>
        {
            var thePower = CreativePowerManager.Instance.GetPower<CreativePowers.FreezeWindDirectionAndStrength>();
            bool creativePower = thePower.Enabled;
            thePower.Enabled = WindLocked;

            orig.Invoke(self, time, iteration);

            thePower.Enabled = creativePower;
        };
    }

    public override void PreUpdateEntities()
    {


        _rainTime = Main.rainTime;
        _maxRaining = Main.maxRaining;
        _raining = Main.raining;

        _happening = Sandstorm.Happening;
        _timeLeft = Sandstorm.TimeLeft;

        _moonPhase = Main.moonPhase;

        _windSpeedCurrent = Main.windSpeedCurrent;
        _windSpeedTarget = Main.windSpeedTarget;
        _windCounter = Main.windCounter;
        _extremeWindCounter = Main.extremeWindCounter;
    }

    public override void PostUpdateEverything()
    {
        if (RainLocked)
        {
            Main.rainTime = _rainTime;
            Main.maxRaining = _maxRaining;
            Main.raining = _raining;
        }

        if (SandstormLocked)
        {
            Sandstorm.Happening = _happening;
            Sandstorm.TimeLeft = _timeLeft;
        }

        if (MoonPhaseLocked)
        {
            Main.moonPhase = _moonPhase;
        }

        if (WindLocked)
        {
            Main.windSpeedCurrent = _windSpeedCurrent;
            Main.windSpeedTarget = _windSpeedTarget;
            Main.windCounter = _windCounter;
            Main.extremeWindCounter = _extremeWindCounter;
        }
    }

    public override void NetSend(BinaryWriter writer)
    {
        var states = new BitsByte(RainLocked, SandstormLocked, MoonPhaseLocked, WindLocked, Unlocked);
        writer.Write(states);
    }

    public override void NetReceive(BinaryReader reader)
    {
        var states = (BitsByte)reader.ReadByte();
        RainLocked = states[0];
        SandstormLocked = states[1];
        MoonPhaseLocked = states[2];
        WindLocked = states[3];
        Unlocked = states[4];
    }

    public override void ClearWorld()
    {
        RainLocked = false;
        SandstormLocked = false;
        MoonPhaseLocked = false;
        WindLocked = false;
        Unlocked = false;
    }

    public override void SaveWorldData(TagCompound tag)
    {
        if (Unlocked) tag.Add("unlocked", true);
        if (RainLocked) tag.Add("rainLocked", true);
        if (SandstormLocked) tag.Add("sandstormLocked", true);
        if (MoonPhaseLocked) tag.Add("moonPhaseLocked", true);
        if (WindLocked) tag.Add("windLocked", true);
    }

    public override void LoadWorldData(TagCompound tag)
    {
        if (tag.ContainsKey("unlocked")) Unlocked = tag.GetBool("unlocked");
        if (tag.ContainsKey("rainLocked")) RainLocked = tag.GetBool("rainLocked");
        // 旧版本写入的键名是 sandstorm，这里兼容一下
        if (tag.ContainsKey("sandstormLocked")) SandstormLocked = tag.GetBool("sandstormLocked");
        else if (tag.ContainsKey("sandstorm")) SandstormLocked = tag.GetBool("sandstorm");
        if (tag.ContainsKey("moonPhaseLocked")) MoonPhaseLocked = tag.GetBool("moonPhaseLocked");
        if (tag.ContainsKey("windLocked")) WindLocked = tag.GetBool("windLocked");
    }
}