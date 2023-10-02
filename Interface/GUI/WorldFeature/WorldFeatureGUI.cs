using ImproveGame.Common.Packets.WorldFeatures;
using ImproveGame.Content.Patches;
using ImproveGame.Interface.Common;
using ImproveGame.Interface.ExtremeStorage;
using ImproveGame.Interface.SUIElements;
using Terraria.DataStructures;
using Terraria.GameInput;

namespace ImproveGame.Interface.GUI.WorldFeature;

public class WorldFeatureGUI : ViewBody
{
    public override bool Display { get => Visible; set => Visible = value; }
    public static bool Visible { get; private set; }

    public override bool CanPriority(UIElement target) => target != this;

    public override bool CanDisableMouse(UIElement target)
        => (target != this && MainPanel.IsMouseHovering) || MainPanel.KeepPressed;

    // 主面板
    public SUIPanel MainPanel;

    // 标题面板
    private SUIPanel TitlePanel;

    // 标题
    private SUITitle Title;

    // 关闭按钮
    private SUICross Cross;

    public override void OnInitialize()
    {
        // 主面板
        MainPanel = new SUIPanel(UIColor.PanelBorder, UIColor.PanelBg)
        {
            Shaded = true,
            Draggable = true
        };
        MainPanel.SetPadding(0f);
        MainPanel.SetPosPixels(410, 360)
            .SetSizePixels(300, 306)
            .Join(this);

        TitlePanel = new SUIPanel(UIColor.PanelBorder, UIColor.TitleBg2)
        {
            DragIgnore = true,
            Width = {Pixels = 0f, Precent = 1f},
            Height = {Pixels = 50f, Precent = 0f},
            Rounded = new Vector4(10f, 10f, 0f, 0f),
            Relative = RelativeMode.Vertical
        };
        TitlePanel.SetPadding(0f);
        TitlePanel.Join(MainPanel);

        // 标题
        Title = new SUITitle(GetText("UI.WorldFeature.Title"), 0.5f)
        {
            VAlign = 0.5f
        };
        Title.Join(TitlePanel);

        // Cross
        Cross = new SUICross
        {
            HAlign = 1f,
            VAlign = 0.5f,
            Height = {Pixels = 0f, Precent = 1f},
            Rounded = new Vector4(0f, 10f, 0f, 0f)
        };
        Cross.OnLeftMouseDown += (_, _) => Close();
        Cross.Join(TitlePanel);

        var contentPanel = new View
        {
            DragIgnore = true,
            Relative = RelativeMode.Vertical
        };
        contentPanel.SetPadding(16, 14, 16, 14);
        contentPanel.SetSize(0f, 0f, 1f, 1f);
        contentPanel.Join(MainPanel);
        
        var festivalPanel = new View
        {
            DragIgnore = true,
            Relative = RelativeMode.Vertical,
            Spacing = new Vector2(0f, 6f)
        };
        festivalPanel.SetSize(0f, 100f, 1f, 0f);
        festivalPanel.Join(contentPanel);
        
        var seedPanel = new View
        {
            DragIgnore = true,
            Relative = RelativeMode.Vertical
        };
        seedPanel.SetSize(0f, -100f, 1f, 1f);
        seedPanel.Join(contentPanel);

        SetupFestivalButtons(festivalPanel);
        SetupSeedButtons(seedPanel);
    }

    private void SetupFestivalButtons(UIElement parent)
    {
        var icon = Main.Assets.Request<Texture2D>("Images/UI/Bestiary/Icon_Tags_Shadow");

        var halloweenSwitch = new LongSwitch(
            () => ForceFestivalSystem.ForceHalloween,
            FestivalPacket.SetHalloween,
            "UI.WorldFeature.Halloween")
        {
            First = true,
            Relative = RelativeMode.Vertical,
            Spacing = new Vector2(0f, 4f)
        };
        halloweenSwitch.SetIcon(icon, new SpriteFrame(16, 5, 13, 2));
        halloweenSwitch.Join(parent);

        var xMasSwitch = new LongSwitch(
            () => ForceFestivalSystem.ForceXMas,
            FestivalPacket.SetXMas,
            "UI.WorldFeature.XMas")
        {
            Relative = RelativeMode.Vertical
        };
        xMasSwitch.SetIcon(icon, new SpriteFrame(16, 5, 14, 2));
        xMasSwitch.Join(parent);
    }

    private void SetupSeedButtons(UIElement parent)
    {
        parent.Append(new SeedButton(SeedType.Drunk, 0, 0));
        parent.Append(new SeedButton(SeedType.Bees, 68, 0));
        parent.Append(new SeedButton(SeedType.Ftw, 136, 0));
        parent.Append(new SeedButton(SeedType.Anniversary, 204, 0));
        parent.Append(new SeedButton(SeedType.DontStarve, 0, 68));
        parent.Append(new SeedButton(SeedType.Traps, 68, 68));
        parent.Append(new SeedButton(SeedType.Remix, 136, 68));
        parent.Append(new SeedButton(SeedType.Zenith, 204, 68));
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        if (MainPanel.IsMouseHovering)
        {
            PlayerInput.LockVanillaMouseScroll("ImproveGame: World Feature GUI");
            Main.LocalPlayer.mouseInterface = true;
        }
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