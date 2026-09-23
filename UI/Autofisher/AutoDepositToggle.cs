using ImproveGame.Common.ModPlayers;
using ImproveGame.Content.Tiles;
using ImproveGame.Packets.NetAutofisher;
using ImproveGame.UIFramework.BaseViews;
using ImproveGame.UIFramework.SUIElements;
using Terraria.ModLoader.UI;

namespace ImproveGame.UI.Autofisher;

public sealed class AutoDepositToggle : TimerView
{
    private TEAutofisher Autofisher => AutofishPlayer.LocalPlayer.Autofisher;
    private readonly SUIPanel _panel;

    public AutoDepositToggle(SUIPanel panel)
    {
        SetSizePixels(32, 30);
        _panel = panel;
    }

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
        var texture = ModAsset.ChestAutoDeposit.Value;
        var hoverTexture = ModAsset.ChestAutoDeposit_Hover.Value;
        if (Autofisher is not null && !Autofisher.AutoDeposit)
            color = color.MultiplyRGB(Color.White * 0.4f);
        spriteBatch.Draw(texture, pos, null, color);

        if (IsMouseHovering)
        {
            spriteBatch.Draw(hoverTexture, pos, null, Color.White * HoverTimer.Schedule);
            UICommon.TooltipMouseText(MyUtils.GetText("UI.Autofisher.ChestAutoDeposit"));
        }
    }

    public override void LeftMouseDown(UIMouseEvent evt)
    {
        base.LeftMouseDown(evt);
        if (Autofisher is not null)
            FishFiltersPacket.Get(Autofisher.Position, !Autofisher.AutoDeposit, 5).Send(runLocally: true);
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