using ImproveGame.Common.Configs.Elements;
using Newtonsoft.Json;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;

namespace ImproveGame.Common.Configs.FavoritedSystem;

/// <summary>
/// 一个Config空壳，用于显示被收藏的Config项
/// </summary>
public class FavoritedConfigs : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ServerSide;

    [JsonIgnore] private ImproveConfigs _pendingConfig;

    [CustomModConfigItem(typeof(LargerPanelElement))]
    public object FavoriteIntroduction;

    public void PopulateElements(UIModConfig configUI)
    {
        _pendingConfig = ConfigManager.GeneratePopulatedClone(ImproveConfigs.Instance) as ImproveConfigs;
        if (_pendingConfig == null)
            return;

        _pendingConfig.IsRealImproveConfigs = false;

        int top = 0;
        int order = 0;

        AddHeader(configUI, "Favorited", order++);
        foreach (PropertyFieldWrapper variable in ConfigManager.GetFieldsAndProperties(_pendingConfig))
        {
            if (!FavoritedOptionDatabase.FavoritedOptions.Contains($"ImproveConfigs.{variable.Name}"))
                continue;

            UIModConfig.WrapIt(configUI.mainConfigList, ref top, variable, _pendingConfig, order++);
        }
    }

    private static void AddHeader(UIModConfig configUI, string localizationKey, int order)
    {
        var e = new HeaderElement(MyUtils.GetText($"Configs.FavoritedConfigs.Header.{localizationKey}"));
        e.Recalculate();
        var elementHeight = (int)e.GetOuterDimensions().Height;
        var container = UIModConfig.GetContainer(e, order);
        container.Height.Pixels = elementHeight;
        configUI.mainConfigList.Add(container);
        configUI.mainConfigList.GetTotalHeight();
        var tuple = new Tuple<UIElement, UIElement>(container, e);
        Terraria.ModLoader.UI.Interface.modConfig.mainConfigItems.Add(tuple);
    }

    public void SaveConfig(Action<UIModConfig, UIMouseEvent, UIElement> orig, UIModConfig configUI, UIMouseEvent evt,
        UIElement listeningElement)
    {
        configUI.modConfig = ConfigManager.Configs[Mod].Find(i => i.Name == "ImproveConfigs");
        configUI.pendingConfig = _pendingConfig;

        orig.Invoke(configUI, evt, listeningElement);

        configUI.modConfig = ConfigManager.Configs[Mod].Find(i => i.Name == "FavoritedConfigs");
        configUI.pendingConfig = this;

        if (Main.netMode is not NetmodeID.MultiplayerClient)
            configUI.DoMenuModeState();
    }
}