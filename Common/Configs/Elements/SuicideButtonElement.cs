using Terraria.DataStructures;
using Terraria.GameContent.UI.States;

namespace ImproveGame.Common.Configs.Elements;

internal class SuicideButtonElement : LargerPanelElement
{
    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        
        // 不在游戏里的话不显示
        float newHeight = Main.gameMenu ? 0 : 36;
        Height.Set(newHeight, 0f);

        if (Parent is UISortableElement) {
            Parent.Height.Pixels = newHeight;
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!Main.gameMenu)
            base.Draw(spriteBatch);
    }

    public override void LeftClick(UIMouseEvent evt) {
        base.LeftClick(evt);
        
        if (Main.gameMenu) return;
        
        Player player = Main.LocalPlayer;
        player.KillMe(PlayerDeathReason.ByOther(255), 9999, 0);
    }
}