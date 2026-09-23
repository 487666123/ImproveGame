using ImproveGame.UIFramework;
using ImproveGame.UIFramework.BaseViews;
using Terraria.ModLoader.UI;

namespace ImproveGame.UI.AmmoChainPanel;

public class LightBulbHelp : TimerView
{
    public LightBulbHelp()
    {
        HoverTimer = new AnimationTimer(3);
        SetSizePixels(30, 38);
    }

    public override void LeftMouseDown(UIMouseEvent evt)
    {
        base.LeftMouseDown(evt);
        TrUtils.OpenToURL(Language.ActiveCulture.Name is "zh-Hans"
            ? "https://gitee.com/MyGoold/improve-game/wikis/%E5%BC%B9%E8%8D%AF%E9%93%BE"
            : "https://github.com/487666123/ImproveGame/wiki/Ammo-Chain");
    }

    public override void DrawSelf(SpriteBatch spriteBatch)
    {
        base.DrawSelf(spriteBatch);

        var center = GetDimensions().Center();
        spriteBatch.Draw(ModAsset.LightBulb.Value, center, null, Color.White, 0f, ModAsset.LightBulb.Size() / 2f, 1f,
            SpriteEffects.None, 0f);
        spriteBatch.Draw(ModAsset.LightBulb_Highlight.Value, center, null, Color.White * HoverTimer.Schedule, 0f,
            ModAsset.LightBulb_Highlight.Size() / 2f, 1f, SpriteEffects.None, 0f);

        if (IsMouseHovering)
            UICommon.TooltipMouseText(MyUtils.GetText("UI.AmmoChain.GetHelp"));
    }
}