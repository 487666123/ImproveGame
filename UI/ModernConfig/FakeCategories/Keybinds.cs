using ImproveGame.Common.Configs;
using ImproveGame.Common.ModSystems;
using ImproveGame.UI.ModernConfig.OptionElements;

namespace ImproveGame.UI.ModernConfig.FakeCategories;

public class Keybinds(string ModName) : Category()
{
    public Keybinds() : this("ImproveGame") { }

    public override int ItemIconId => ItemID.RainbowCursor;

    public string ModName { get; } = ModName;

    public override void AddOptions(ConfigOptionsPanel panel)
    {
        panel.ShouldHideSearchBar = true;

        var uiConfig = UIConfigs.Instance;
        bool hasKeybind = MyUtils.TryGetKeybindString(KeybindSystem.MasterControlKeybind, out _);
        if (!hasKeybind)
            panel.AddToOptionsDirect<OptionToggle>(uiConfig, nameof(uiConfig.FckKeybindPopup));

        if (Language.ActiveCulture.Name == "zh-Hans")
        {
            panel.AddToOptionsDirect(new KeybindChineseToggle());
        }

        foreach (var modKeybind in KeybindLoader.Keybinds)
        {
            if (modKeybind.Mod.Name == ModName)
            {
                panel.AddToOptionsDirect(new OptionKeybind(modKeybind.FullName));
            }
        }
    }
}