namespace ImproveGame.Entitys
{
    public class TileData
    {
        public enum TileSort { None, Block, Platform, Torch, Chair, Table, Workbench, Bed, Wall, NoWall }
        public TileSort tileSort;
        public int x;
        public int y;

        public TileData(TileSort tileSort, int x, int y)
        {
            this.tileSort = tileSort;
            this.x = x;
            this.y = y;
        }


        /// <summary>
        /// 平台或实体块位置不放置
        /// </summary>
        public static bool ShouldPlaceWall(TileSort tileSort) =>
            tileSort is not TileSort.Block and not TileSort.Platform and not TileSort.NoWall and not TileSort.Bed;
    }
}
