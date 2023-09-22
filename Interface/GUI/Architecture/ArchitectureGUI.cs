using ImproveGame.Content.Items;
using ImproveGame.Interface.Common;
using ImproveGame.Interface.SUIElements;
using ImproveGame.Interface.UIElements;

namespace ImproveGame.Interface.GUI.Architecture;

public class ArchitectureGUI : ViewBody
{
    private static bool _visible;

    public static bool Visible
    {
        get
        {
            return _visible && Main.playerInventory && Main.LocalPlayer.HeldItem is not null;
        }
        private set => _visible = value;
    }

    public override bool Display { get => Visible; set => Visible = value; }

    public override bool CanPriority(UIElement target) => target != this;

    public override bool CanDisableMouse(UIElement target)
        => (target != this && MainPanel.IsMouseHovering) || MainPanel.KeepPressed;

    private static bool _prevMouseRight;
    public static CreateWand Wand;

    public SUIPanel MainPanel;
    private SUIPanel TitlePanel;
    private SUITitle Title;
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
        MainPanel.SetPosPixels(590, 120)
            .SetSizePixels(600, 306)
            .Join(this);

        SetupHeader();
        SetupContent();
    }

    private void SetupHeader()
    {
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
    }

    private void SetupContent()
    {
        var contentPanel = new View
        {
            DragIgnore = true,
            Relative = RelativeMode.Vertical
        };
        contentPanel.SetPadding(16, 14, 16, 14);
        contentPanel.SetSize(0f, 0f, 1f, 1f);
        contentPanel.Join(MainPanel);

        var itemSlotsPanel = new View
        {
            DragIgnore = true,
            Relative = RelativeMode.Vertical,
            Spacing = new Vector2(0f, 6f)
        };
        itemSlotsPanel.Left.Set(80f, 0f);
        itemSlotsPanel.SetSize(0f, 100f, 1f, 0f);
        itemSlotsPanel.Join(contentPanel);
        SetupItemSlots(itemSlotsPanel);

        var houseStylesPanel = new View
        {
            DragIgnore = true,
            Relative = RelativeMode.Vertical
        };
        houseStylesPanel.SetSize(0f, -100f, 1f, 1f);
        houseStylesPanel.Join(contentPanel);
    }

    private void SetupItemSlots(View parent)
    {
        int hNumber = 8;
        int vNumber = 1;

        var itemSlotGrid = new BaseGrid();
        itemSlotGrid.SetBaseValues(vNumber, hNumber, new Vector2(6f), new Vector2(48f));
        itemSlotGrid.Join(parent);

        new WoodSlot().Join(itemSlotGrid);
        new BlockSlot().Join(itemSlotGrid);
        new WallSlot().Join(itemSlotGrid);
        new PlatformSlot().Join(itemSlotGrid);
        new TorchSlot().Join(itemSlotGrid);
        new ChairSlot().Join(itemSlotGrid);
        new WorkbenchSlot().Join(itemSlotGrid);
        new BedSlot().Join(itemSlotGrid);

        itemSlotGrid.CalculateWithSetGridSize();
        itemSlotGrid.CalculateWithSetChildrenPosition();
        itemSlotGrid.Recalculate();
    }

    // 主要是可拖动和一些判定吧
    public override void Update(GameTime gameTime)
    {
        if (Wand is null)
        {
            Close();
            return;
        }

        base.Update(gameTime);

        if (!Main.playerInventory)
        {
            Close();
            return;
        }

        // 右键点击空白直接关闭
        if (Main.mouseRight && !_prevMouseRight && !MainPanel.IsMouseHovering && !Main.LocalPlayer.mouseInterface)
        {
            Close();
            return;
        }

        _prevMouseRight = Main.mouseRight;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        Player player = Main.LocalPlayer;

        base.Draw(spriteBatch);

        if (MainPanel.IsMouseHovering)
        {
            player.mouseInterface = true;
        }
    }

    /// <summary>
    /// 打开GUI界面
    /// </summary>
    public void Open(CreateWand wand)
    {
        Main.playerInventory = true;
        _prevMouseRight = true; // 防止一打开就关闭
        Visible = true;
        SoundEngine.PlaySound(SoundID.MenuOpen);

        Wand = wand;
    }

    /// <summary>
    /// 关闭GUI界面
    /// </summary>
    public void Close()
    {
        Wand = null;
        Visible = false;
        _prevMouseRight = false;
        Main.blockInput = false;
        SoundEngine.PlaySound(SoundID.MenuClose);
    }
}