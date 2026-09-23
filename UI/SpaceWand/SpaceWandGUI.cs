using ImproveGame.Content.Items;
using ImproveGame.UIFramework;
using ImproveGame.UIFramework.UIElements;

namespace ImproveGame.UI.SpaceWand;

public class SpaceWandGUI : UIState
{
    public enum PageType : int
    {
        Material,
        Slope,
        Shape
    }

    private static bool visible;
    public static PageType CurrentPage = PageType.Material;

    public static bool Visible
    {
        get => visible;
        set => visible = value;
    }

    public UIElement MainPanel;
    public Content.Items.SpaceWand SpaceWand;
    public AnimationTimer timer; // 这是一个计时器哦~
    public SelectionButton ModeButton;

    // 这么写能剩下很多重复的代码, 但是你必须保证他们长度是相同的.
    public readonly RoundButton[] RoundButtons = new RoundButton[6];
    public readonly int[] ItemTypes = [94, 9, 2996, 2340, 62, 3215];

    public readonly PlaceType[] PlaceTypes =
    {
        PlaceType.Platform, PlaceType.Soild, PlaceType.Rope, PlaceType.Rail, PlaceType.GrassSeed, PlaceType.PlantPot
    };

    public readonly BlockType[] BlockTypes =
    {
        BlockType.SlopeDownRight, BlockType.SlopeDownLeft, BlockType.HalfBlock, BlockType.SlopeUpLeft,
        BlockType.SlopeUpRight, BlockType.Solid
    };

    public readonly ShapeType[] ShapeTypes =
    {
        ShapeType.Line, ShapeType.Corner, ShapeType.CircleFilled, ShapeType.CircleEmpty,
        ShapeType.SquareEmpty, ShapeType.SquareFilled
    };

    public override void OnInitialize()
    {
        timer = new() { OnClosed = () => Visible = false };

        Append(MainPanel = new());
        MainPanel.SetSize(200f, 200f).SetPadding(0);

        for (int i = 0; i < RoundButtons.Length; i++)
        {
            int itemType = ItemTypes[i];
            PlaceType placeType = PlaceTypes[i];
            Main.instance.LoadItem(itemType);
            MainPanel.Append(RoundButtons[i] = new(TextureAssets.Item[itemType])
            {
                text = () => MyUtils.GetText($"SpaceWandGUI.{placeType}"),
                Selected = () => SpaceWand.PlaceType == placeType
            });
        }

        MainPanel.Append(ModeButton = new SelectionButton(this));
    }

    private Color textColor = new(135, 0, 180);

    public override void DrawChildren(SpriteBatch spriteBatch)
    {
        base.DrawChildren(spriteBatch);
        if (!timer.AnyClose)
        {
            foreach (RoundButton button in RoundButtons)
            {
                // 悬浮文本
                if (button.IsMouseHovering)
                {
                    MyUtils.DrawString(MyUtils.MouseScreenOffset(20), button.Text, Color.White, textColor, spread: 1f);
                    Main.LocalPlayer.cursorItemIconEnabled = false;
                }
            }
        }
    }

    public override void Update(GameTime gameTime)
    {
        timer.Update();
        base.Update(gameTime);
        if (Main.LocalPlayer.HeldItem != SpaceWand.Item && !timer.AnyClose)
        {
            Close();
        }

        UpdateButton();
    }

    public void UpdateButton()
    {
        Vector2 center = MainPanel.GetInnerSizePixels() / 2f;
        float includedAngle = MathF.PI * 2 / RoundButtons.Length; // 夹角
        float startAngle = -MathF.PI / 2 - includedAngle / 2; // 起始角度

        if (ModeButton.IsMouseHovering && !timer.AnyClose)
            Main.LocalPlayer.mouseInterface = true;
        ModeButton.Opacity = timer.Schedule;
        ModeButton.SetCenterPixels(center).Recalculate();

        for (int i = 0; i < RoundButtons.Length; i++)
        {
            if (RoundButtons[i].IsMouseHovering)
                Main.LocalPlayer.mouseInterface = true;
            float angle = startAngle + includedAngle * i;
            float length = 48 + (1 - timer.Schedule) * 25f;
            RoundButtons[i].Opacity = timer.Schedule;
            RoundButtons[i].SetCenterPixels(center + angle.ToRotationVector2() * length).Recalculate();
        }
    }

    /// <summary>
    /// 我执行右键了，你看着办吧！
    /// </summary>
    public void ProcessRightClick(Content.Items.SpaceWand spaceWand)
    {
        if (Visible && timer.AnyOpen)
        {
            Close();
        }
        else
        {
            Open(spaceWand);
        }
    }

    #region 不同页面的Setup

    private void SetupSlopePage()
    {
        MainPanel.RemoveAllChildren();

        for (int i = 0; i < RoundButtons.Length; i++)
        {
            BlockType blockType = BlockTypes[i];
            string path = $"UI/SpaceWand/{blockType}";
            MainPanel.Append(RoundButtons[i] = new RoundButton(MyUtils.GetTexture(path))
            {
                text = () => "",
                Selected = () => SpaceWand.BlockType == blockType
            });
            RoundButtons[i].OnLeftMouseDown += (_, _) =>
            {
                if (timer.AnyClose)
                    return;
                SpaceWand.BlockType = blockType;
                SoundEngine.PlaySound(SoundID.MenuTick);
            };
        }

        MainPanel.Append(ModeButton = new SelectionButton(this));
    }

    private void SetupMaterialPage()
    {
        MainPanel.RemoveAllChildren();

        for (int i = 0; i < RoundButtons.Length; i++)
        {
            int itemType = ItemTypes[i];
            PlaceType placeType = PlaceTypes[i];
            Main.instance.LoadItem(itemType);
            MainPanel.Append(RoundButtons[i] = new(TextureAssets.Item[itemType])
            {
                text = () => MyUtils.GetText($"SpaceWandGUI.{placeType}"),
                Selected = () => SpaceWand.PlaceType == placeType
            });
            RoundButtons[i].OnLeftMouseDown += (_, _) =>
            {
                if (timer.AnyClose)
                    return;
                SpaceWand.PlaceType = placeType;
                SoundEngine.PlaySound(SoundID.MenuTick);
            };
        }

        MainPanel.Append(ModeButton = new SelectionButton(this));
    }

    private void SetupShapePage()
    {
        MainPanel.RemoveAllChildren();

        for (int i = 0; i < RoundButtons.Length; i++)
        {
            ShapeType shapeType = ShapeTypes[i];
            string path = $"UI/SpaceWand/{shapeType}";
            MainPanel.Append(RoundButtons[i] = new RoundButton(MyUtils.GetTexture(path))
            {
                text = () => "",
                Selected = () => SpaceWand.ShapeType == shapeType,
                IconScale = 1f
            });
            RoundButtons[i].OnLeftMouseDown += (_, _) =>
            {
                if (timer.AnyClose)
                    return;
                SpaceWand.ShapeType = shapeType;
                SoundEngine.PlaySound(SoundID.MenuTick);
            };
        }

        MainPanel.Append(ModeButton = new SelectionButton(this));
    }

    public void SetupPage()
    {
        switch (CurrentPage)
        {
            case PageType.Material:
                SetupMaterialPage();
                break;
            case PageType.Slope:
                SetupSlopePage();
                break;
            case PageType.Shape:
                SetupShapePage();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        foreach (var t in RoundButtons)
        {
            t.OnRightMouseDown += (_, _) =>
            {
                if (Visible && timer.Opened)
                    Close();
            };
        }
    }

    #endregion

    public void Open(Content.Items.SpaceWand spaceWand)
    {
        SetupPage();
        this.SpaceWand = spaceWand;
        Visible = true;
        SoundEngine.PlaySound(SoundID.MenuOpen);
        timer.OpenAndResetTimer();
        MainPanel.SetCenterPixels(MyUtils.MouseScreenUI).Recalculate();
        UpdateButton();
    }

    public void Close()
    {
        SoundEngine.PlaySound(SoundID.MenuClose);
        timer.CloseAndResetTimer();
    }
}