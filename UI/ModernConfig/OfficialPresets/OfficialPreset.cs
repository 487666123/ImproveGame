using ImproveGame.Common.Configs;
using ImproveGame.UI.ModernConfig.OptionElements.PresetElements;
using Terraria.ModLoader.Config;

namespace ImproveGame.UI.ModernConfig.OfficialPresets;

public abstract class OfficialPreset
{
    public virtual string LocalizationKey => GetType().Name;

    public string Label => MyUtils.GetText($"ModernConfig.Presets.{LocalizationKey}.Label");
    public string Tooltip => MyUtils.GetText($"ModernConfig.Presets.{LocalizationKey}.Tooltip");
    public string Link => MyUtils.GetText($"ModernConfig.Presets.{LocalizationKey}.Link");

    public abstract void ApplyPreset(ImproveConfigs modConfig, UIConfigs uiConfig,
        AvailableModItemConfigs modItemConfig);

    public void OnApply()
    {
        var mainConfig = ImproveConfigs.Instance;
        var uiConfig = UIConfigs.Instance;
        var modItemConfig = MyUtils.AvailableConfig;

        ApplyPreset(mainConfig, uiConfig, modItemConfig);

        var modConfigToLoad = ConfigManager.Configs[ImproveGame.Instance].Find(i => i.Name == mainConfig.Name);
        var uiConfigToLoad = ConfigManager.Configs[ImproveGame.Instance].Find(i => i.Name == uiConfig.Name);
        var modItemConfigToLoad = ConfigManager.Configs[ImproveGame.Instance].Find(i => i.Name == modItemConfig.Name);

        for (int n = 0; n < 3; n++)
        {
            ModConfig loadedConfig = n switch
            {
                0 => mainConfig,
                1 => uiConfig,
                2 or _ => modItemConfig,
            };
            ModConfig modConfig = n switch
            {
                0 => modConfigToLoad,
                1 => uiConfigToLoad,
                2 or _ => modItemConfigToLoad
            };
            loadedConfig.Name = modConfig.Name;
            loadedConfig.Mod = modConfig.Mod;

            PresetHandler.LoadAndApplyConfig(loadedConfig, modConfig);
        }
    }
}