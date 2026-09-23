using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace ImproveGame.Common.Configs;

public class AvailableModItemConfigs : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ServerSide;
    public override void OnLoaded() => MyUtils.AvailableConfig = this;

    [DefaultValue(true)]
    public bool AvailableMagickWand = true;

    [DefaultValue(true)]
    public bool AvailableSpaceWand = true;

    [DefaultValue(true)]
    public bool AvailableStarburstWand = true;

    [DefaultValue(true)]
    public bool AvailableWallPlace = true;

    [DefaultValue(true)]
    public bool AvailableCreateWand = true;

    [DefaultValue(true)]
    public bool AvailableLiquidWand = true;

    [DefaultValue(true)]
    public bool AvailableLiquidWandAdvanced = true;

    [DefaultValue(true)]
    public bool AvailablePotionBag = true;

    [DefaultValue(true)]
    public bool AvailableBannerChest = true;

    [DefaultValue(true)]
    public bool AvailableAutofisher = true;

    [DefaultValue(true)]
    public bool AvailablePaintWand = true;

    [DefaultValue(true)]
    public bool AvailableConstructWand = true;

    [DefaultValue(true)]
    public bool AvailableMoveChest = true;

    [DefaultValue(true)]
    public bool AvailableCoinOne = true;

    [DefaultValue(true)]
    public bool AvailableExtremeStorage = true;

    [DefaultValue(true)]
    public bool AvailableDetectorDrone = true;

    [DefaultValue(true)]
    public bool AvailableBaitSupplier = true;

    [DefaultValue(true)]
    public bool AvailableActuationRodMkII = true;

    [DefaultValue(true)]
    public bool AvailableShimmerBucket = true;

    public override bool AcceptClientChanges(ModConfig pendingConfig, int whoAmI, ref NetworkText message)
    {
        return MyUtils.AcceptClientChanges(ImproveConfigs.Instance, pendingConfig, whoAmI, ref message);
    }
}
