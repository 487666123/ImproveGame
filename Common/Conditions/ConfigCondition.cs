using ImproveGame.Common.Configs;

namespace ImproveGame.Common.Conditions
{
    public static class ConfigCondition
    {
        public static Condition AvailableAutofisherC { get; } = new Condition("Mods.ImproveGame.Conditions.AvailableAutofisher", () => MyUtils.AvailableConfig.AvailableAutofisher);
        public static Condition AvailableActuationRodMkIIC { get; } = new Condition("Mods.ImproveGame.Conditions.AvailableActuationRodMkII", () => MyUtils.AvailableConfig.AvailableActuationRodMkII);
        public static Condition AvailableBaitSupplierC { get; } = new Condition("Mods.ImproveGame.Conditions.AvailableBaitSupplier", () => MyUtils.AvailableConfig.AvailableBaitSupplier);
        public static Condition AvailableExtremeStorageC { get; } = new Condition("Mods.ImproveGame.Conditions.AvailableExtremeStorage", () => MyUtils.AvailableConfig.AvailableExtremeStorage);
        public static Condition AvailableLiquidWandC { get; } = new Condition("Mods.ImproveGame.Conditions.AvailableLiquidWand", () => MyUtils.AvailableConfig.AvailableLiquidWand);
        public static Condition NotAvailableLiquidWandC { get; } = new Condition("Mods.ImproveGame.Conditions.NotAvailableLiquidWand", () => !MyUtils.AvailableConfig.AvailableLiquidWand);
        public static Condition AvailableLiquidWandAdvancedC { get; } = new Condition("Mods.ImproveGame.Conditions.AvailableLiquidWandAdvanced", () => MyUtils.AvailableConfig.AvailableLiquidWandAdvanced);
        public static Condition AvailableMagickWandC { get; } = new Condition("Mods.ImproveGame.Conditions.AvailableMagickWand", () => MyUtils.AvailableConfig.AvailableMagickWand);
        public static Condition NotAvailableMagickWandC { get; } = new Condition("Mods.ImproveGame.Conditions.NotAvailableMagickWand", () => !MyUtils.AvailableConfig.AvailableMagickWand);
        public static Condition AvailableStarburstWandC { get; } = new Condition("Mods.ImproveGame.Conditions.AvailableStarburstWand", () => MyUtils.AvailableConfig.AvailableStarburstWand);
        public static Condition AvailablePotionBagC { get; } = new Condition("Mods.ImproveGame.Conditions.AvailablePotionBag", () => MyUtils.AvailableConfig.AvailablePotionBag);
        public static Condition AvailableSpaceWandC { get; } = new Condition("Mods.ImproveGame.Conditions.AvailableSpaceWand", () => MyUtils.AvailableConfig.AvailableSpaceWand);
        public static Condition AvailableWallPlaceC { get; } = new Condition("Mods.ImproveGame.Conditions.AvailableWallPlace", () => MyUtils.AvailableConfig.AvailableWallPlace);
        public static Condition AvailablePaintWandC { get; } = new Condition("Mods.ImproveGame.Conditions.AvailablePaintWand", () => MyUtils.AvailableConfig.AvailablePaintWand);
        public static Condition AvailableBannerChestC { get; } = new Condition("Mods.ImproveGame.Conditions.AvailableBannerChest", () => MyUtils.AvailableConfig.AvailableBannerChest);
        public static Condition AvailableConstructWandC { get; } = new Condition("Mods.ImproveGame.Conditions.AvailableConstructWand", () => MyUtils.AvailableConfig.AvailableConstructWand);
        public static Condition AvailableCreateWandC { get; } = new Condition("Mods.ImproveGame.Conditions.AvailableCreateWand", () => MyUtils.AvailableConfig.AvailableCreateWand);
        public static Condition AvailableDetectorDroneC { get; } = new Condition("Mods.ImproveGame.Conditions.AvailableDetectorDrone", () => MyUtils.AvailableConfig.AvailableDetectorDrone);
        public static Condition AvailableMoveChestC { get; } = new Condition("Mods.ImproveGame.Conditions.AvailableMoveChest", () => MyUtils.AvailableConfig.AvailableMoveChest);
        public static Condition AvailableShimmerBucketC { get; } = new Condition("Mods.ImproveGame.Conditions.AvailableShimmerBucket", () => MyUtils.AvailableConfig.AvailableShimmerBucket);
        public static Condition EnableQuickShimmerC { get; } = new Condition("Mods.ImproveGame.Conditions.EnableQuickShimmer", () => ImproveConfigs.Instance.QuickShimmer);
        public static Condition EnableMinimapMarkC { get; } = new Condition("Mods.ImproveGame.Conditions.EnableMinimapMark", () => ImproveConfigs.Instance.MinimapMark);
        public static Condition EnableWeatherControlC { get; } = new Condition("Mods.ImproveGame.Conditions.EnableWeatherControl", () => ImproveConfigs.Instance.WeatherControl);
    }
}
