using ImproveGame.Common.Configs.FavoritedSystem;
using ImproveGame.Common.ModSystems;
using ImproveGame.Content.Functions;
using ImproveGame.UI;
using ImproveGame.UI.ExtremeStorage;
using ImproveGame.UI.ItemSearcher;
using ImproveGame.UI.MasterControl;
using ImproveGame.UI.OpenBag;
using ImproveGame.UI.PlayerStats;
using ImproveGame.UI.QuickShimmer;
using ImproveGame.UI.WorldFeature;
using ImproveGame.UIFramework;
using ImproveGame.UIFramework.Common;
using ImproveGame.UserInterfaces.BigBag;
using Newtonsoft.Json;
using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace ImproveGame.Common.Configs;

/// <summary>
/// 额外配置，用于不写进配置文件的设置选项
/// </summary>
public class ClientConfigCore : ModSystem
{
    public const string ConfigFileName = "ImproveGame_AdditionalConfig.json";
    public static readonly string ConfigFilePath = Path.Combine(ConfigManager.ModConfigPath, ConfigFileName);

    public override void PostSetupContent() => LoadConfig();
    public override void PreSaveAndQuit() => SaveConfig();

    public static void LoadConfig()
    {
        if (Main.dedServ) return;

        var config = new CommonClientConfig();

        bool jsonFileExists = File.Exists(ConfigFilePath);
        string json = jsonFileExists ? File.ReadAllText(ConfigFilePath) : "{}";

        try
        {
            JsonConvert.PopulateObject(json, config, ConfigManager.serializerSettings);
        }
        catch (Exception e) when (jsonFileExists && e is JsonReaderException or JsonSerializationException)
        {
            ImproveGame.Instance.Logger.Warn(
                $"Quality of Terraria mod additional config file located at {ConfigFilePath} failed to load. The file was likely corrupted somehow, so the defaults will be loaded and the file deleted.");
            File.Delete(ConfigFilePath);
            JsonConvert.PopulateObject("{}", config, ConfigManager.serializerSettings);
        }

        config.ApplyConfig();
    }

    public static void SaveConfig()
    {
        if (Main.dedServ) return;

        try
        {
            Directory.CreateDirectory(ConfigManager.ModConfigPath);
            File.WriteAllText(ConfigFilePath,
                JsonConvert.SerializeObject(new CommonClientConfig().UpdateConfig(), ConfigManager.serializerSettings));
        }
        catch (Exception e)
        {
            var mod = ImproveGame.Instance;

            mod.Logger.Error("保存配置文件时出错。");
            mod.Logger.Error(e);
        }
    }
}

/// <summary>
/// ImproveGame 通用 本地/客户端 配置
/// </summary>
[Serializable]
public class CommonClientConfig
{
    [Serializable]
    public class LifeformAnalyzerConfig
    {
        public List<int> VanillaBlacklist = [];
        public List<string> ModdedBlacklist = [];
    }

    [Serializable]
    public class WandModeConfig
    {
        [DefaultValue(true)] public bool BrustRangeFixed;
        [DefaultValue(true)] public bool BrustDestroyTile;
        [DefaultValue(true)] public bool BrustDestroyWall;
        [DefaultValue(false)] public bool BrustDestroyChest;
        public bool LiquidAbsorption;
        public short LiquidSelectedType;

        [DefaultValue(WandSystem.PaintMode.Tile)]
        public WandSystem.PaintMode PaintMode;
    }

    public HashSet<string> FavoritedModernConfigOptions;

    //public HashSet<string> StarBuffs;

    public LifeformAnalyzerConfig LifeformAnalyzer = new();
    public WandModeConfig WandMode = new();
    public bool UseKeybindTranslation;
    public Vector2 HugeInventoryUIPosition;
    public Vector2 BuffTrackerPosition;
    public Vector2 WorldFeaturePosition;
    public Vector2 ItemSearcherPosition;
    public Vector2 OpenBagPosition;
    public Vector2 PlayerInfoTogglePosition;
    public bool MasterControlPinned;
    public bool ExtremeStorageGUIDisplayCrafting;
    public bool QuickShimmerGUIAutoStart;
    public bool QuickShimmerGUIQuickMode;

    /// <summary>
    /// 更新数据, 从数据源获取
    /// </summary>
    public CommonClientConfig UpdateConfig()
    {
        foreach ((int id, bool blacklisted) in LifeAnalyzeCore.Blacklist)
        {
            if (!blacklisted)
                continue;
            switch (id)
            {
                case < 688:
                    LifeformAnalyzer.VanillaBlacklist.Add(id);
                    break;
                case >= 688:
                    LifeformAnalyzer.ModdedBlacklist.Add(NPCLoader.GetNPC(id).FullName);
                    break;
            }
        }

        LifeformAnalyzer.VanillaBlacklist.Sort();
        LifeformAnalyzer.ModdedBlacklist.Sort();

        WandMode.BrustRangeFixed = WandSystem.FixedMode;
        WandMode.BrustDestroyTile = WandSystem.TileMode;
        WandMode.BrustDestroyWall = WandSystem.WallMode;
        WandMode.BrustDestroyChest = WandSystem.ChestMode;
        WandMode.LiquidAbsorption = WandSystem.AbsorptionMode;
        WandMode.LiquidSelectedType = WandSystem.LiquidMode;
        WandMode.PaintMode = WandSystem.PaintWandMode;

        FavoritedModernConfigOptions = FavoritedOptionDatabase.FavoritedOptions;

        //StarBuffs = BuffTrackerGUI.FavoritedBuffs;

        UseKeybindTranslation = KeybindSystem.UseKeybindTranslation;

        var uiSystem = UISystem.Instance;

        HugeInventoryUIPosition =
            BigBagUI.Instance?.WindowPosition ?? UIPlayer.HugeInventoryUIPosition;
        UIPlayer.HugeInventoryUIPosition = HugeInventoryUIPosition; // 在这里也保存一下

        //BuffTrackerPosition =
        //    uiSystem.BuffTrackerGUI?.MainPanel?.GetDimensions().Position() ?? new Vector2(630, 160);
        //UIPlayer.BuffTrackerPosition = BuffTrackerPosition; // 在这里也保存一下

        WorldFeaturePosition =
            WorldFeatureGUI.Instance?.MainPanel?.GetDimensions().Position() ?? new Vector2(250, 280);
        UIPlayer.WorldFeaturePosition = WorldFeaturePosition; // 在这里也保存一下

        ItemSearcherPosition =
            ItemSearcherGUI.Instance?.MainPanel?.GetDimensions().Position() ?? new Vector2(620, 400);
        UIPlayer.ItemSearcherPosition = ItemSearcherPosition; // 在这里也保存一下

        OpenBagPosition =
            OpenBagGUI.Instance?.MainPanel?.GetDimensions().Position() ?? new Vector2(410, 360);
        UIPlayer.OpenBagPosition = OpenBagPosition; // 在这里也保存一下

        PlayerInfoTogglePosition =
            PlayerStatsGUI.Instance?.ControllerSwitch?.GetDimensions().Position() ?? UIPlayer.PlayerInfoToggleDefPosition;
        UIPlayer.PlayerInfoTogglePosition = PlayerInfoTogglePosition; // 在这里也保存一下

        MasterControlPinned = MasterControlGUI.Pinned;

        ExtremeStorageGUIDisplayCrafting = ExtremeStorageGUI.DisplayCrafting;
        QuickShimmerGUIAutoStart = QuickShimmerGUI.AutoStart;
        QuickShimmerGUIQuickMode = QuickShimmerGUI.QuickMode;

        return this;
    }

    /// <summary>
    /// 应用数据
    /// </summary>
    public void ApplyConfig()
    {
        LifeformAnalyzer?.VanillaBlacklist?.ForEach(i => LifeAnalyzeCore.Blacklist[i] = true);
        LifeformAnalyzer?.ModdedBlacklist?.ForEach(s =>
        {
            if (ModContent.TryFind<ModNPC>(s, out var modNpc))
                LifeAnalyzeCore.Blacklist[modNpc.Type] = true;
        });

        // 旧版兼容
        if (WandMode is not null)
        {
            WandSystem.FixedMode = WandMode.BrustRangeFixed;
            WandSystem.TileMode = WandMode.BrustDestroyTile;
            WandSystem.WallMode = WandMode.BrustDestroyWall;
            WandSystem.ChestMode = WandMode.BrustDestroyChest;
            WandSystem.AbsorptionMode = WandMode.LiquidAbsorption;
            WandSystem.LiquidMode = WandMode.LiquidSelectedType;
            WandSystem.PaintWandMode = WandMode.PaintMode;
        }

        if (FavoritedModernConfigOptions is null || FavoritedModernConfigOptions.Count is 0)
            FavoritedOptionDatabase.SetDefaultFavoritedOptions();
        else
            FavoritedOptionDatabase.FavoritedOptions = FavoritedModernConfigOptions;

        //if (StarBuffs is not null)
        //    BuffTrackerGUI.FavoritedBuffs = StarBuffs;

        KeybindSystem.UseKeybindTranslation = UseKeybindTranslation;
        UIPlayer.HugeInventoryUIPosition =
            HugeInventoryUIPosition == Vector2.Zero ? new Vector2(150, 340) : HugeInventoryUIPosition;
        //UIPlayer.BuffTrackerPosition =
        //    BuffTrackerPosition == Vector2.Zero ? new Vector2(630, 160) : BuffTrackerPosition;
        UIPlayer.WorldFeaturePosition =
            WorldFeaturePosition == Vector2.Zero ? new Vector2(250, 280) : WorldFeaturePosition;
        UIPlayer.ItemSearcherPosition =
            ItemSearcherPosition == Vector2.Zero ? new Vector2(620, 400) : ItemSearcherPosition;
        UIPlayer.OpenBagPosition =
            OpenBagPosition == Vector2.Zero ? new Vector2(410, 360) : OpenBagPosition;
        UIPlayer.PlayerInfoTogglePosition =
            PlayerInfoTogglePosition == Vector2.Zero ? UIPlayer.PlayerInfoToggleDefPosition : PlayerInfoTogglePosition;

        MasterControlGUI.Pinned = MasterControlPinned;

        ExtremeStorageGUI.DisplayCrafting = ExtremeStorageGUIDisplayCrafting;
        QuickShimmerGUI.AutoStart = QuickShimmerGUIAutoStart;
        QuickShimmerGUI.QuickMode = QuickShimmerGUIQuickMode;
    }

}
