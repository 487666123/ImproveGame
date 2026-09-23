using ImproveGame.UIFramework;
using ImproveGame.UIFramework.BaseViews;
using ImproveGame.UIFramework.SUIElements;
using Terraria.ModLoader.UI;

namespace ImproveGame.UI.Autofisher;

public class FreeFilterButton : TimerView
{
    private readonly SUIPanel _panel;

    public FreeFilterButton(SUIPanel panel)
    {
        SetSizePixels(34f, 26f);
        _panel = panel;
    }

    public virtual Rectangle? SourceRectangle => null;

    public override void DrawSelf(SpriteBatch spriteBatch)
    {
        float filtersX = _panel.Left() + _panel.Width() + 10f;
        if (Left.Pixels != filtersX)
        {
            Left.Set(filtersX, 0f);
            Recalculate();
        }

        base.DrawSelf(spriteBatch);

        var dimensions = GetDimensions();
        var pos = dimensions.Position();
        var color = Color.White;
        var texture = ModAsset.IconFreeFilter.Value;
        var hoverTexture = ModAsset.IconFreeFilterHover.Value;
        spriteBatch.Draw(texture, pos, SourceRectangle, color);
        if (IsMouseHovering)
            spriteBatch.Draw(hoverTexture, pos, SourceRectangle, color * HoverTimer.Schedule);

        if (IsMouseHovering)
            UICommon.TooltipMouseText(MyUtils.GetText("UI.Autofisher.PerItemFilter"));
    }

    public override void LeftMouseDown(UIMouseEvent evt)
    {
        base.LeftMouseDown(evt);
        FreeFilter.Visible = !FreeFilter.Visible;
        if (FreeFilter.Visible)
            EventTriggerManager.FocusUIElement = FreeFilter.Instance;
        FreeFilter.Instance.Recalculate();
    }

    public override void MouseOver(UIMouseEvent evt)
    {
        base.MouseOver(evt);
        SoundEngine.PlaySound(SoundID.MenuTick);
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        if (IsMouseHovering)
            Main.LocalPlayer.mouseInterface = true;
    }
}