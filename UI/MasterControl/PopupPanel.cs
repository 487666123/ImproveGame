using ImproveGame.UI.ModernConfig;
using ImproveGame.UIFramework;
using ImproveGame.UIFramework.BaseViews;
using ImproveGame.UIFramework.Common;
using ImproveGame.UIFramework.SUIElements;

namespace ImproveGame.UI.MasterControl;

[AutoCreateGUI(LayerName.Vanilla.RadialHotbars, "Popup Panel")]
// 没绑定快捷键的时候弹出
public class PopupPanel : BaseBody
{
    public static PopupPanel Instance { get; private set; }
    public PopupPanel() => Instance = this;
    public override bool Enabled { get => Visible; set => Visible = value; }
    public static bool Visible { get; private set; }

    public override bool CanSetFocusTarget(UIElement target)
        => (target != this && MainPanel.IsMouseHovering) || MainPanel.IsLeftMousePressed;

    // 主面板
    public SUIPanel MainPanel;

    // 标题面板
    private View TitlePanel;

    public override void OnInitialize()
    {
        int panelWidth = int.TryParse(MyUtils.GetText("UI.MasterControl.PopupPanelWidth"), out int wValue)
            ? wValue
            : 280;
        int panelHeight = int.TryParse(MyUtils.GetText("UI.MasterControl.PopupPanelHeight"), out int hValue)
            ? hValue
            : 180;

        // 主面板
        MainPanel = new SUIPanel(UIStyle.PanelBorder, UIStyle.PanelBg)
        {
            Rounded = new Vector4(10f),
            Shaded = true,
            Draggable = true,
            FinallyDrawBorder = true,
            IsAdaptiveHeight = true
        };
        MainPanel.SetPadding(1.5f);
        MainPanel.SetPosPixels(490, 400)
            .SetSizePixels(panelWidth, 0)
            .JoinParent(this);

        TitlePanel = ViewHelper.CreateHead(Color.Black * 0.25f, 45f, 10f);
        TitlePanel.SetPadding(0f);
        TitlePanel.JoinParent(MainPanel);

        // 标题
        var title = new SUIText
        {
            IsLarge = true,
            UseKey = true,
            TextOrKey = "Mods.ImproveGame.UI.MasterControl.PopupTitle",
            TextAlign = new Vector2(0f, 0.5f),
            TextScale = 0.45f,
            Height = StyleDimension.Fill,
            Width = StyleDimension.Fill,
            DragIgnore = true,
            Left = new StyleDimension(16f, 0f)
        };
        title.JoinParent(TitlePanel);

        var cross = new SUICross
        {
            HAlign = 1f,
            Rounded = new Vector4(0f, 10f, 0f, 0f),
            CrossSize = 20f,
            CrossRounded = 4.5f * 0.85f,
            Border = 0f,
            BorderColor = Color.Transparent,
            BgColor = Color.Transparent,
        };
        cross.CrossOffset.X = 1f;
        cross.Width.Pixels = 46f;
        cross.Height.Set(0f, 1f);
        cross.OnUpdate += _ =>
        {
            cross.BgColor = cross.HoverTimer.Lerp(Color.Transparent, Color.Black * 0.25f);
        };
        cross.OnLeftMouseDown += (_, _) =>
        {
            Close();

            var ui = ModernConfigUI.Instance;
            if (ui is null || ui.Enabled) return;

            ui.Open(ImproveGame.Instance);
            ui.OpenFromMasterControl = true;
            ConfigOptionsPanel.CategoryToSelectOnOpen = CategorySidePanel.Cards["Keybinds"].Category;
        };
        cross.JoinParent(TitlePanel);

        var bottomArea = new View
        {
            DragIgnore = true,
            RelativeMode = RelativeMode.Vertical,
            OverflowHidden = true,
            HAlign = 0.5f
        };
        bottomArea.SetPadding(12f);
        bottomArea.SetSize(0f, panelHeight, 1f, 0f);
        bottomArea.JoinParent(MainPanel);

        var text = new SUIText
        {
            TextBorder = 1,
            IsWrapped = true,
            UseKey = true,
            HAlign = 0.5f,
            TextOrKey = "Mods.ImproveGame.UI.MasterControl.PopupText"
        };
        text.SetSize(-4f, 0f, 1f, 1f);
        text.JoinParent(bottomArea);

        Recalculate();
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        if (!MainPanel.IsMouseHovering)
            return;

        Main.LocalPlayer.mouseInterface = true;
    }

    public void Open()
    {
        SoundEngine.PlaySound(SoundID.MenuOpen);
        Visible = true;
    }

    public void Close()
    {
        SoundEngine.PlaySound(SoundID.MenuClose);
        Visible = false;
    }
}