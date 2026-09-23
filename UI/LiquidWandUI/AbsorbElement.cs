using ImproveGame.Common.ModSystems;
using ImproveGame.UIFramework;
using ImproveGame.UIFramework.BaseViews;
using ImproveGame.UIFramework.Common;
using ImproveGame.UIFramework.SUIElements;

namespace ImproveGame.UI.LiquidWandUI;

public class AbsorbElement : TimerView
{
    private readonly AnimationTimer _selectTimer = new();

    public AbsorbElement()
    {
        this.SetSize(-16, 40f, 1f, 0f);
        HAlign = 0.5f;

        RelativeMode = RelativeMode.Vertical;
        Spacing = new Vector2(8f);
        Border = 2;
        Rounded = new Vector4(12f);
        PreventOverflow = true;
        DragIgnore = false;

        UITextPanel<string> liquidName = new(MyUtils.GetText("UI.LiquidWandUI.Absorb"), 1f)
        {
            IgnoresMouseInteraction = true,
            DrawPanel = false,
            HAlign = 0f,
            VAlign = 0.5f,
            Left = { Pixels = 34f }
        };
        Append(liquidName);
    }
    public override void LeftMouseDown(UIMouseEvent evt)
    {
        base.LeftMouseDown(evt);
        WandSystem.AbsorptionMode = true;
    }

    public override void DrawSelf(SpriteBatch spriteBatch)
    {
        // 控制计时器
        if (WandSystem.AbsorptionMode)
            _selectTimer.Open();
        else
            _selectTimer.Close();
        _selectTimer.UpdateHighFps();

        // 绘制UI
        var item = new Item(ItemID.UltraAbsorbantSponge);

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
}