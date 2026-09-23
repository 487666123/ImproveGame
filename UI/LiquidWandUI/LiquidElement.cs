using ImproveGame.Common.ModPlayers;
using ImproveGame.Common.ModSystems;
using ImproveGame.UIFramework;
using ImproveGame.UIFramework.BaseViews;
using ImproveGame.UIFramework.Common;
using ImproveGame.UIFramework.SUIElements;

namespace ImproveGame.UI.LiquidWandUI;

public class LiquidElement : TimerView
{
    private readonly AnimationTimer _selectTimer = new();
    private readonly UITextPanel<string> _percentageLeft;
    private readonly short _liquidID;
    public bool Infinite;
    public bool Hide;

    public LiquidElement(short liquidID)
    {
        this.SetSize(-16, 40f, 1f, 0f);
        HAlign = 0.5f;
        _liquidID = liquidID;

        RelativeMode = RelativeMode.Vertical;
        Spacing = new Vector2(8f);
        Border = 2;
        Rounded = new Vector4(12f);
        PreventOverflow = true;
        DragIgnore = false;

        UITextPanel<string> liquidName = new(MyUtils.GetText($"UI.LiquidWandUI.{LiquidName}"), 1f)
        {
            IgnoresMouseInteraction = true,
            DrawPanel = false,
            HAlign = 0f,
            VAlign = 0.5f,
            Left = { Pixels = 34f }
        };
        Append(liquidName);

        _percentageLeft = new("100.0%")
        {
            IgnoresMouseInteraction = true,
            DrawPanel = false,
            HAlign = 1f,
            VAlign = 0.5f,
            TextHAlign = 1f
        };

        Append(_percentageLeft);
    }

    public override void LeftMouseDown(UIMouseEvent evt)
    {
        if (Hide) return;
        base.LeftMouseDown(evt);
        WandSystem.LiquidMode = _liquidID;
        WandSystem.AbsorptionMode = false;
    }

    public override void DrawSelf(SpriteBatch spriteBatch)
    {
        // 控制计时器
        if (WandSystem.LiquidMode == _liquidID && !WandSystem.AbsorptionMode)
            _selectTimer.Open();
        else
            _selectTimer.Close();
        _selectTimer.UpdateHighFps();

        // 绘制
        var item = new Item(BucketId);

        BorderColor = _selectTimer.Lerp(UIStyle.PanelBorder, UIStyle.ItemSlotBorderFav);
        BgColor = HoverTimer.Lerp(UIStyle.PanelBgLight, UIStyle.PanelBgLightHover);

        base.DrawSelf(spriteBatch);

        var dimensions = GetDimensions();
        dimensions.Width = 40f;
        BigBagItemSlot.DrawItemIcon(sb: spriteBatch,
            item: item,
            lightColor: Color.White,
            dimensions: dimensions,
            maxSize: 28);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (Hide)
            return;

        // 绘制一个文本，液体剩余多少
        // 转换为百分数，保留后一位，来自: https://www.jianshu.com/p/3f88338bde60
        float liquidPercentage = (float)LiquidAmount / DataPlayer.LiquidCap;
        string text = $"{liquidPercentage:p1}";
        if (BucketExists || Infinite)
        {
            text = "∞"; // 无限使用
        }

        _percentageLeft.SetText(text);
        base.Draw(spriteBatch);
    }

    private int LiquidAmount
    {
        get
        {
            return _liquidID switch
            {
                LiquidID.Water => DataPlayer.LiquidWandWater,
                LiquidID.Lava => DataPlayer.LiquidWandLava,
                LiquidID.Honey => DataPlayer.LiquidWandHoney,
                LiquidID.Shimmer => DataPlayer.LiquidCap,
                _ => 0
            };
        }
        set
        {
            switch (_liquidID)
            {
                case LiquidID.Water:
                    DataPlayer.LiquidWandWater = value;
                    break;
                case LiquidID.Lava:
                    DataPlayer.LiquidWandLava = value;
                    break;
                case LiquidID.Honey:
                    DataPlayer.LiquidWandHoney = value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private short BucketId => _liquidID switch
    {
        LiquidID.Water => ItemID.BottomlessBucket,
        LiquidID.Lava => ItemID.BottomlessLavaBucket,
        LiquidID.Honey => ItemID.BottomlessHoneyBucket,
        LiquidID.Shimmer => ItemID.BottomlessShimmerBucket,
        _ => -1
    };

    private string LiquidName => _liquidID switch
    {
        LiquidID.Water => "Water",
        LiquidID.Lava => "Lava",
        LiquidID.Honey => "Honey",
        LiquidID.Shimmer => "Shimmer",
        _ => throw new ArgumentOutOfRangeException()
    };

    private DataPlayer DataPlayer => DataPlayer.Get(Main.LocalPlayer);

    public bool BucketExists => MyUtils.LocalPlayerHasItemFast(BucketId);
}