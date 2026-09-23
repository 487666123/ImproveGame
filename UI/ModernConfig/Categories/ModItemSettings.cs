using ImproveGame.Common.Configs;

namespace ImproveGame.UI.ModernConfig.Categories;

public sealed class ModItemSettings : Category
{
    //public override int ItemIconId => ModContent.ItemType<Content.Items.Placeable.ExtremeStorage>();

    public override Texture2D GetIcon() => ModAsset.Placeable_ExtremeStorage.Value;

    public override void AddOptions(ConfigOptionsPanel panel)
    {
        panel.AddToggle(ImproveConfigs.Instance, nameof(ImproveConfigs.Instance.EmptyAutofisher));
        panel.AddValueSlider(ImproveConfigs.Instance, nameof(ImproveConfigs.Instance.ExStorageSearchDistance));
        panel.AddToggle(ImproveConfigs.Instance, nameof(ImproveConfigs.Instance.WandMaterialNoConsume));
        panel.AddToggle(MyUtils.AvailableConfig, nameof(MyUtils.AvailableConfig.AvailableMagickWand));
        panel.AddToggle(MyUtils.AvailableConfig, nameof(MyUtils.AvailableConfig.AvailableSpaceWand));
        panel.AddToggle(MyUtils.AvailableConfig, nameof(MyUtils.AvailableConfig.AvailableStarburstWand));
        panel.AddToggle(MyUtils.AvailableConfig, nameof(MyUtils.AvailableConfig.AvailableWallPlace));
        panel.AddToggle(MyUtils.AvailableConfig, nameof(MyUtils.AvailableConfig.AvailableCreateWand));
        panel.AddToggle(MyUtils.AvailableConfig, nameof(MyUtils.AvailableConfig.AvailableLiquidWand));
        panel.AddToggle(MyUtils.AvailableConfig, nameof(MyUtils.AvailableConfig.AvailableLiquidWandAdvanced));
        panel.AddToggle(MyUtils.AvailableConfig, nameof(MyUtils.AvailableConfig.AvailablePotionBag));
        panel.AddToggle(MyUtils.AvailableConfig, nameof(MyUtils.AvailableConfig.AvailableBannerChest));
        panel.AddToggle(MyUtils.AvailableConfig, nameof(MyUtils.AvailableConfig.AvailableAutofisher));
        panel.AddToggle(MyUtils.AvailableConfig, nameof(MyUtils.AvailableConfig.AvailablePaintWand));
        panel.AddToggle(MyUtils.AvailableConfig, nameof(MyUtils.AvailableConfig.AvailableConstructWand));
        panel.AddToggle(MyUtils.AvailableConfig, nameof(MyUtils.AvailableConfig.AvailableMoveChest));
        panel.AddToggle(MyUtils.AvailableConfig, nameof(MyUtils.AvailableConfig.AvailableCoinOne));
        panel.AddToggle(MyUtils.AvailableConfig, nameof(MyUtils.AvailableConfig.AvailableExtremeStorage));
        panel.AddToggle(MyUtils.AvailableConfig, nameof(MyUtils.AvailableConfig.AvailableDetectorDrone));
        panel.AddToggle(MyUtils.AvailableConfig, nameof(MyUtils.AvailableConfig.AvailableBaitSupplier));
        panel.AddToggle(MyUtils.AvailableConfig, nameof(MyUtils.AvailableConfig.AvailableActuationRodMkII));
        panel.AddToggle(MyUtils.AvailableConfig, nameof(MyUtils.AvailableConfig.AvailableShimmerBucket));
    }
}