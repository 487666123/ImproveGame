using ImproveGame.Common.Configs;
using Terraria.DataStructures;

namespace ImproveGame.Common.GlobalTiles
{
    public class ImproveTile : GlobalTile
    {
        public override void DrawEffects(int i, int j, int type, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
        {
            if (UIConfigs.Instance.SpelunkerColor != new Color(0, 0, 0, 0) && Main.LocalPlayer.findTreasure && Main.IsTileSpelunkable(i, j))
            {
                drawData.tileLight = UIConfigs.Instance.SpelunkerColor;
            }
        }
    }
}