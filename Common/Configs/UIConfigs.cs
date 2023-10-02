using ImproveGame.Common.Configs.Elements;
using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace ImproveGame.Common.Configs;

public class UIConfigs : ModConfig
{
    public static UIConfigs Instance { get; set; }
    public override ConfigScope Mode => ConfigScope.ClientSide;
    public override void OnLoaded() => Instance = this;

    [CustomModConfigItem(typeof(OpenUIConfigElement))]
    public object OpenConfig;

    [CustomModConfigItem(typeof(SuicideButtonElement))]
    public object SuicideButton;

    [ReloadRequired]
    public bool ResetNativeUI;

    [DefaultValue(7f)]
    [Range(0, 10f)]
    [Slider]
    [Increment(0.5f)]
    [CustomModConfigItem(typeof(FontOffsetPreview))]
    public float GeneralFontOffsetY;

    [DefaultValue(16f)]
    [Range(0, 20f)]
    [Increment(0.5f)]
    [CustomModConfigItem(typeof(BigFontOffsetPreview))]
    [Slider]
    public float BigFontOffsetY;

    [DefaultValue(false)]
    public bool ShowMoreData;

    public enum PAPDisplayMode
    {
        AlwaysDisplayed, WhenOpeningBackpack, NotDisplayed
    }

    [DefaultValue(PAPDisplayMode.AlwaysDisplayed)]
    [DrawTicks]
    [Slider]
    public PAPDisplayMode PlyInfo;

    [DefaultValue(true)]
    public bool QoLAutoTrash;

    [DefaultValue(false)]
    public bool KeepFocus;

    [DefaultValue(true)]
    public bool ExplosionEffect;

    [Header("MinimapHeader")]
    [DefaultValue(true)]
    public bool MarkDungeon;
    
    [DefaultValue(true)]
    public bool MarkTemple;
    
    [DefaultValue(true)]
    public bool MarkAether;
    
    [DefaultValue(true)]
    public bool MarkFloatingIsland;
    
    [DefaultValue(true)]
    public bool MarkPyramid;
    
    [DefaultValue(true)]
    public bool MarkPlantera;
}
