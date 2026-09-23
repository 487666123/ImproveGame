using ImproveGame.Common.Configs;
using ImproveGame.Common.ModSystems;
using ImproveGame.UIFramework;
using ImproveGame.UIFramework.BaseViews;
using ImproveGame.UIFramework.Common;
using ImproveGame.UIFramework.Graphics2D;
using Terraria.GameContent.UI.States;
using Terraria.GameInput;

namespace ImproveGame.UI.ModernConfig.OptionElements;

public class OptionKeybind : TimerView
{
    public OptionKeybind(string bind)
    {
        KeybindName = bind;

        Width.Set(0f, 1f);
        Height.Set(46f, 0f);
        Rounded = new Vector4(12f);
        SetPadding(12, 4);

        RelativeMode = RelativeMode.Vertical;
        OverflowHidden = true;

        var labelElement = new SlideText(Label, 140);
        labelElement.OnUpdate += _ =>
        {
            labelElement.TextColor = ListeningThis
                ? Color.Gold
                : Color.White;

            if (KeybindName is "ImproveGame/MasterControl" && !ListeningThis &&
                !MyUtils.TryGetKeybindString(ModKeybind, out var _))
                labelElement.TextColor = new Color(255, (byte)(Main.masterColor * 200f), 0, Main.mouseTextColor);
        };
        labelElement.JoinParent(this);

        var grayBox = new View
        {
            BgColor = Color.Black * 0.4f,
            Rounded = new Vector4(12f),
            VAlign = 0.5f,
            HAlign = 1f
        };
        grayBox.SetPadding(2, 2, 2, 2); // Padding影响里面的文字绘制
        grayBox.SetSizePixels(130, 28);
        grayBox.JoinParent(this);

        MyUtils.TryGetKeybindString(ModKeybind, out var bindString);
        var bindElement = new SlideText(bindString, 0, 0.9f)
        {
            HAlign = 0.5f,
            VAlign = 0.5f
        };
        bindElement.OnUpdate += _ =>
        {
            bindElement.TextColor = ListeningThis
                ? Color.Gold
                : Color.White;
            if (!MyUtils.TryGetKeybindString(ModKeybind, out var text) && !ListeningThis)
                bindElement.TextColor = Color.Gray;
            bindElement.DisplayText = text;
        };
        bindElement.JoinParent(grayBox);
    }

    // 为了让UI之间实际上无间隔，防止鼠标滑过时Tooltip文字闪现，这里重写绘制，而不使用Spacing
    public override void DrawSelf(SpriteBatch spriteBatch)
    {
        var dimensions = GetDimensions();
        var position = dimensions.Position();
        var size = dimensions.Size();

        // 这里修改这两个值，而不使用Spacing
        position.Y += 3f;
        size.Y -= 6f;

        // 背景板
        var panelColor = HoverTimer.Lerp(UIStyle.ListItemColor * 0.2f, UIStyle.ListItemColor * 0.4f);

        SDFRectangle.NoBorder(position, size, new Vector4(8f), panelColor * 0.8f, Main.UIScaleMatrix);

        // 提示
        if (!IsMouseHovering)
            return;

        string defaultBinding = KeybindSystem.TranslateBinding(ModKeybind.DefaultBinding);

        if (ModKeybind.Mod.Name == "ImproveGame")
        {
            var tip = Language.GetOrRegister($"Mods.{ModKeybind.Mod.Name}.Keybinds.{ModKeybind.Name}.Tip", () => "");
            var text = $"{tip.Value}\n{MyUtils.GetText("ModernConfig.Keybinds.Default", defaultBinding)}";
            TooltipPanel.SetText(text);
        }
        else
        {
            var text = MyUtils.GetText("ModernConfig.Keybinds.Default", defaultBinding);
            TooltipPanel.SetText(text);
        }
    }

    public override void LeftClick(UIMouseEvent evt)
    {
        base.LeftClick(evt);
        if (PlayerInput.ListeningTrigger != KeybindName)
        {
            if (PlayerInput.CurrentProfile.AllowEditing)
                PlayerInput.ListenFor(KeybindName, InputMode);
            else
                PlayerInput.ListenFor(null, InputMode);
        }
    }

    public override void RightMouseDown(UIMouseEvent evt)
    {
        base.RightMouseDown(evt);

        string copyableProfileName = UIManageControls.GetCopyableProfileName();
        PlayerInput.CurrentProfile.CopyIndividualModKeybindSettingsFrom(
            PlayerInput.OriginalProfiles[copyableProfileName], InputMode, KeybindName);
    }

    public string Label => ModKeybind.DisplayName.Value;

    public ModKeybind ModKeybind => KeybindLoader.modKeybinds[KeybindName];

    public InputMode InputMode => InputMode.Keyboard;

    public string KeybindName { get; }

    private bool ListeningThis => PlayerInput.ListeningTrigger == KeybindName;
}

public sealed class KeybindChineseToggle : TimerView
{
    public KeybindChineseToggle()
    {
        Width.Set(0f, 1f);
        Height.Set(46f, 0f);
        Rounded = new Vector4(12f);
        SetPadding(12, 4);

        RelativeMode = RelativeMode.Vertical;
        OverflowHidden = true;

        var labelElement = new SlideText("控件汉化", 140);
        labelElement.JoinParent(this);

        if (Enabled)
            _timer.ImmediateOpen();
    }

    private void DrawBackground(SpriteBatch spriteBatch)
    {
        var dimensions = GetDimensions();
        var position = dimensions.Position();
        var size = dimensions.Size();

        // 这里修改这两个值，而不使用Spacing
        position.Y += 3f;
        size.Y -= 6f;

        // 背景板
        var panelColor = HoverTimer.Lerp(UIStyle.ListItemColor * 0.2f, UIStyle.ListItemColor * 0.4f);

        SDFRectangle.NoBorder(position, size, new Vector4(8f), panelColor * 0.8f, Main.UIScaleMatrix);

        // 提示
        if (!IsMouseHovering)
            return;

        TooltipPanel.SetText("开启后，部分快捷键名称将显示为中文，强烈建议开启");
    }

    public override void Update(GameTime gameTime)
    {
        _timer.Update();
        base.Update(gameTime);
        if (Enabled)
            _timer.Open();
        else
            _timer.Close();
    }

    public override void LeftMouseDown(UIMouseEvent evt)
    {
        base.LeftMouseDown(evt);
        Enabled = !Enabled;
        ClientConfigCore.SaveConfig();
        SoundEngine.PlaySound(SoundID.MenuTick);
    }

    public override void DrawSelf(SpriteBatch sb)
    {
        Color color = Color.Lerp(UIStyle.SwitchBg, UIStyle.SwitchBgHover, _timer.Schedule);
        Color color2 = Color.Lerp(UIStyle.SwitchBorder, UIStyle.SwitchBorderHover, _timer.Schedule);
        Color color3 = Color.Lerp(UIStyle.SwitchRound, UIStyle.SwitchRoundHover, _timer.Schedule);

        var dimensions = GetDimensions();
        var position = dimensions.Position();
        var size = dimensions.Size();

        base.DrawSelf(sb);
        DrawBackground(sb);

        // 开关
        var boxSize = new Vector2(48, 26);
        var boxPosition = new Vector2(position.X + size.X - boxSize.X - 6f, position.Y);

        Vector2 position1 = boxPosition + new Vector2(0, size.Y / 2 - boxSize.Y / 2);
        SDFRectangle.HasBorder(position1, boxSize, new Vector4(MathF.Min(boxSize.X, boxSize.Y) / 2), color, 2, color2, Main.UIScaleMatrix);

        Vector2 boxSize2 = new(boxSize.Y - 10);
        Vector2 position2 = boxPosition + Vector2.Lerp(new Vector2(3 + 2, size.Y / 2 - boxSize2.Y / 2),
            new Vector2(boxSize.X - 3 - 2 - boxSize2.X, size.Y / 2 - boxSize2.Y / 2), _timer.Schedule);
        SDFGraphics.NoBorderRound(position2, default, boxSize2.X, color3, MyUtils.GetMatrix(true));
    }

    private readonly AnimationTimer _timer = new(4);

    public bool Enabled
    {
        get => KeybindSystem.UseKeybindTranslation;
        set => KeybindSystem.UseKeybindTranslation = value;
    }
}