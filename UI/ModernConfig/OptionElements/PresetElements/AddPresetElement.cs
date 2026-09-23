using ImproveGame.UIFramework.BaseViews;
using ImproveGame.UIFramework.SUIElements;

namespace ImproveGame.UI.ModernConfig.OptionElements.PresetElements;

public class AddPresetElement : BasePresetElement
{
    private SUIImageButton _infoButton;

    public AddPresetElement() : base(MyUtils.GetText("ModernConfig.Presets.AddPreset.Label"),
        MyUtils.GetText("ModernConfig.Presets.AddPreset.Tooltip"))
    {
        var buttonBox = new View
        {
            IsAdaptiveWidth = true,
            Height = { Percent = 1f },
            VAlign = 0.5f,
            HAlign = 1f
        };
        buttonBox.SetPadding(0);
        buttonBox.JoinParent(this);

        var infoTexture = Main.Assets.Request<Texture2D>("Images/UI/ButtonSeed", AssetRequestMode.ImmediateLoad).Value;
        var infoTooltip = MyUtils.GetText("ModernConfig.ButtonInfo");
        _infoButton = new SUIImageButton(infoTexture, infoTooltip)
        {
            Spacing = new Vector2(2),
            RelativeMode = RelativeMode.Horizontal,
            ImageScale = 1.3f,
            VAlign = 0.5f
        };
        _infoButton.ResetSize();
        _infoButton.OnLeftMouseDown += (_, _) =>
        {
            SoundEngine.PlaySound(SoundID.MenuOpen);
            TrUtils.OpenToURL(MyUtils.GetText("ModernConfig.Presets.AddPreset.Link"));
        };
        _infoButton.JoinParent(buttonBox);
    }

    public override void LeftMouseDown(UIMouseEvent evt)
    {
        base.LeftMouseDown(evt);

        if (_infoButton.IsMouseHovering)
            return;

        SoundEngine.PlaySound(SoundID.Item37);
        PresetHandler.SaveAsPreset(MyUtils.GetText("ModernConfig.Presets.DefaultPresetName"));
    }

    protected override bool Interactable => true;
}