using ImproveGame.UIFramework.BaseViews;
using Terraria.ModLoader.UI;

namespace ImproveGame.UI.MasterControl.Components;

public class MasterControlPing : View
{
    public MasterControlPing()
    {
        SetSizePixels(22f, 22f);
    }

    public override void LeftMouseDown(UIMouseEvent evt)
    {
        base.LeftMouseDown(evt);
        MasterControlGUI.Pinned = !MasterControlGUI.Pinned;
        MasterControlGUI.Opened = MasterControlGUI.Pinned;
    }

    public override void DrawSelf(SpriteBatch spriteBatch)
    {
        DrawPin(spriteBatch, ModAsset.Pin.Value);
        if (!IsMouseHovering)
            return;

        DrawPin(spriteBatch, ModAsset.PinSelected.Value);
        UICommon.TooltipMouseText(MyUtils.GetText("UI.MasterControl.PinIntro"));
    }

    private void DrawPin(SpriteBatch spriteBatch, Texture2D texture)
    {
        var center = GetDimensionsCenter();
        var frame = MasterControlGUI.Pinned
            ? texture.Frame(2, 1, 0, 0)
            : texture.Frame(2, 1, 1, 0);
        spriteBatch.Draw(texture, center, frame, Color.White, 0f, frame.Size() / 2f, 1f, SpriteEffects.None, 0f);
    }

    public override void MouseOver(UIMouseEvent evt)
    {
        base.MouseOver(evt);
        SoundEngine.PlaySound(SoundID.MenuTick);
    }

    public override void MouseOut(UIMouseEvent evt)
    {
        base.MouseOut(evt);
        SoundEngine.PlaySound(SoundID.MenuTick);
    }
}