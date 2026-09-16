using ImproveGame.Common.Configs;

namespace ImproveGame.UI.ModernConfig.Categories;

public sealed class PlantSettings : Category
{
    public override int ItemIconId => ItemID.Blinkroot;

    public override void AddOptions(ConfigOptionsPanel panel)
    {
        panel.AddToggle(ImproveConfigs.Instance, nameof(ImproveConfigs.Instance.AlchemyGrassGrowsFaster));
        panel.AddToggle(ImproveConfigs.Instance, nameof(ImproveConfigs.Instance.AlchemyGrassAlwaysBlooms));
        panel.AddToggle(ImproveConfigs.Instance, nameof(ImproveConfigs.Instance.PumpkinGrowsFaster));
        panel.AddToggle(ImproveConfigs.Instance, nameof(ImproveConfigs.Instance.LifeFruitGrowsFaster));
        panel.AddValueSlider(ImproveConfigs.Instance, nameof(ImproveConfigs.Instance.LifeFruitLimit));
        // panel.AddToggle(ImproveConfigs.Instance, nameof(ImproveConfigs.Instance.StaffOfRegenerationAutomaticPlanting));
        panel.AddToggle(ImproveConfigs.Instance, nameof(ImproveConfigs.Instance.TreeGrowFaster));
        panel.AddToggle(ImproveConfigs.Instance, nameof(ImproveConfigs.Instance.ShakeTreeFruit));
        panel.AddToggle(ImproveConfigs.Instance, nameof(ImproveConfigs.Instance.GemTreeAlwaysDropGem));
        panel.AddValueSlider(ImproveConfigs.Instance, nameof(ImproveConfigs.Instance.MostTreeMin));
        panel.AddValueSlider(ImproveConfigs.Instance, nameof(ImproveConfigs.Instance.MostTreeMax));
        panel.AddValueSlider(ImproveConfigs.Instance, nameof(ImproveConfigs.Instance.PalmTreeMin));
        panel.AddValueSlider(ImproveConfigs.Instance, nameof(ImproveConfigs.Instance.PalmTreeMax));
        panel.AddValueSlider(ImproveConfigs.Instance, nameof(ImproveConfigs.Instance.GemTreeMin));
        panel.AddValueSlider(ImproveConfigs.Instance, nameof(ImproveConfigs.Instance.GemTreeMax));
    }
}