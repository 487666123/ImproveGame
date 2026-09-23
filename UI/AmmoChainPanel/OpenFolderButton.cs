using ImproveGame.Content.Functions.ChainedAmmo;
using ImproveGame.UIFramework;
using ImproveGame.UIFramework.BaseViews;
using Terraria.ModLoader.UI;

namespace ImproveGame.UI.AmmoChainPanel;

public class OpenFolderButton : TimerView
{
    public OpenFolderButton()
    {
        HoverTimer = new AnimationTimer(3);
        SetSizePixels(32, 32);
    }

    public override void LeftMouseDown(UIMouseEvent evt)
    {
        base.LeftMouseDown(evt);
        TrUtils.OpenFolder(ChainSaver.SavePath);
    }

    public override void DrawSelf(SpriteBatch spriteBatch)
    {
        base.DrawSelf(spriteBatch);

        var center = GetDimensions().Center();
        spriteBatch.Draw(ModAsset.Folder.Value, center, null, Color.White, 0f, ModAsset.Folder.Size() / 2f, 1f,
            SpriteEffects.None, 0f);
        spriteBatch.Draw(ModAsset.Folder_Highlight.Value, center, null, Color.White * HoverTimer.Schedule, 0f,
            ModAsset.Folder_Highlight.Size() / 2f, 1f, SpriteEffects.None, 0f);

        if (IsMouseHovering)
            UICommon.TooltipMouseText(MyUtils.GetText("UI.AmmoChain.OpenFolder"));
    }
}