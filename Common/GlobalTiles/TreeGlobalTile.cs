using ImproveGame.Common.Configs;

namespace ImproveGame.Common.GlobalTiles
{
    public class TreeGlobalTile : GlobalTile
    {
        // 秉承着能不用IL就不用的稳定至上原则，这里直接RandomUpdate了（其实就是懒）
        public override void RandomUpdate(int i, int j, int type, bool underground)
        {
            if (type is not TileID.Saplings and not TileID.VanityTreeSakuraSaplings
                    and not TileID.VanityTreeWillowSaplings and not TileID.GemSaplings &&
                !TileID.Sets.TreeSapling[type])
            {
                return;
            }

            // HasTile为false一般不可能，但还是判断一下
            if (!ImproveConfigs.Instance.TreeGrowFaster || !Main.tile[i, j].HasTile)
            {
                return;
            }

            // 为无视地层条件需要，以下代码不使用
            // bool inOverground = j >= 10 && j <= Main.worldSurface - 1;
            // bool inUnderground = j >= Main.worldSurface && j <= Main.maxTilesY - 20;
            // if (Main.remixWorld && inUnderground)
            // {
            //     // 颠倒世界特色，尽量仿原版
            //     inUnderground = Main.rand.NextBool();
            // }

            // 原版试了一次，我们这里多尝试4次不就5倍了吗
            for (int time = 0; time < 4; time++)
            {
                if (type is TileID.Saplings or TileID.VanityTreeSakuraSaplings or TileID.VanityTreeWillowSaplings
                    or TileID.GemSaplings)
                {
                    // WorldGen.AttemptToGrowTreeFromSapling(i, j, underground: inUnderground);
                    WorldGen.AttemptToGrowTreeFromSapling(i, j, underground: false);
                    WorldGen.AttemptToGrowTreeFromSapling(i, j, underground: true);
                }

                // Mod的，让他自己执行自己的RandomUpdate
                if (TileID.Sets.TreeSapling[type])
                {
                    TileLoader.GetTile(type)?.RandomUpdate(i, j, underground);
                }
            }
        }
    }
}