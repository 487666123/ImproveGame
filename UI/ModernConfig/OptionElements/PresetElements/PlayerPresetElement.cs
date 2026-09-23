using ImproveGame.Core;
using ImproveGame.UIFramework.BaseViews;
using ImproveGame.UIFramework.SUIElements;

namespace ImproveGame.UI.ModernConfig.OptionElements.PresetElements;

public class PlayerPresetElement : BasePresetElement
{
    private string _presetName;
    private SUIEditableText _editableText;

    public PlayerPresetElement(string presetName) : base(presetName, "", false)
    {
        _presetName = presetName;
        SetPadding(6, 4, 12, 4);

        _editableText = new SUIEditableText
        {
            RelativeMode = RelativeMode.Horizontal,
            BgColor = Color.Transparent,
            Rounded = new Vector4(8f),
            InnerText =
            {
                TextAlign = new Vector2(0f, 0.5f),
                MaxLines = 1
            },
            MaxLength = 24,
            Text = presetName,
            VAlign = 0.5f
        };
        _editableText.EndTakingInput += () =>
        {
            if (_editableText.Text.Length is 0 || _editableText.Text.IsPathIllegal())
            {
                _editableText.Text = _presetName;
                return;
            }

            PresetHandler.RenamePreset(_presetName, _editableText.Text);
            _presetName = _editableText.Text;
        };
        _editableText.SetPadding(8, 0); // Padding影响里面的文字绘制
        _editableText.SetSizePixels(500, 32);
        _editableText.JoinParent(this);

        var buttonBox = new View
        {
            IsAdaptiveWidth = true,
            Height = { Percent = 1f },
            VAlign = 0.5f,
            HAlign = 1f,
            DontDisableTextEditing = true
        };
        buttonBox.OnUpdate += element =>
        {
            CanShowInteractTip = element.Children.ToList().All(i => !i.IsMouseHovering);
        };
        buttonBox.SetPadding(0);
        buttonBox.JoinParent(this);

        var deleteTexture = Main.Assets.Request<Texture2D>("Images/UI/ButtonDelete", AssetRequestMode.ImmediateLoad)
            .Value;
        var deleteTooltip = MyUtils.GetText("ModernConfig.ButtonDelete");
        var deleteButton = new SUIImageButton(deleteTexture, deleteTooltip)
        {
            Spacing = new Vector2(2),
            RelativeMode = RelativeMode.Horizontal,
            ImageScale = 1.3f,
            VAlign = 0.5f
        };
        deleteButton.ResetSize();
        deleteButton.OnLeftMouseDown += (_, _) =>
        {
            SoundEngine.PlaySound(SoundID.MenuClose);
            PresetHandler.DeletePreset(presetName);
        };
        deleteButton.JoinParent(buttonBox);

        var renameTexture = Main.Assets.Request<Texture2D>("Images/UI/ButtonRename", AssetRequestMode.ImmediateLoad)
            .Value;
        var renameTooltip = MyUtils.GetText("ModernConfig.ButtonRename");
        var renameButton = new SUIImageButton(renameTexture, renameTooltip)
        {
            Spacing = new Vector2(2),
            RelativeMode = RelativeMode.Horizontal,
            ImageScale = 1.3f,
            VAlign = 0.5f,
            DontDisableTextEditing = true
        };
        renameButton.ResetSize();
        renameButton.OnLeftMouseDown += (_, _) =>
        {
            SoundEngine.PlaySound(SoundID.MenuTick);
            _editableText.ToggleTakingText();
        };
        renameButton.JoinParent(buttonBox);

        var applyTexture = Main.Assets.Request<Texture2D>("Images/UI/ButtonPlay", AssetRequestMode.ImmediateLoad).Value;
        var applyTooltip = MyUtils.GetText("ModernConfig.ButtonApply");
        var applyButton = new SUIImageButton(applyTexture, applyTooltip)
        {
            Spacing = new Vector2(2),
            RelativeMode = RelativeMode.Horizontal,
            ImageScale = 1.3f,
            VAlign = 0.5f
        };
        applyButton.ResetSize();
        applyButton.OnLeftMouseDown += (_, _) =>
        {
            SoundEngine.PlaySound(SoundID.Chat);
            PresetHandler.LoadAndApplyPreset(presetName);
        };
        applyButton.OnUpdate += _ =>
        {
            applyButton.IgnoresMouseInteraction = !Interactable;
        };
        applyButton.JoinParent(buttonBox);
    }

    public override void DrawSelf(SpriteBatch spriteBatch)
    {
        base.DrawSelf(spriteBatch);

        if (_editableText.Text.IsPathIllegal())
            TooltipPanel.SetText(MyUtils.GetText("PathIllegal"));
        if (_editableText.Text.Length is 0)
            TooltipPanel.SetText(MyUtils.GetText("FolderNameEmpty"));

        // 提示
        if (IsMouseHovering)
            TooltipPanel.SetText(MyUtils.GetText("ModernConfig.Presets.PlayerPresetTooltip"));
    }
}