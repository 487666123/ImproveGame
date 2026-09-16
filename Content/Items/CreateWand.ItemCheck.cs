using Terraria.ObjectData;

namespace ImproveGame.Content.Items;

public partial class CreateWand
{
    private static class CreateWandHelper
    {
        /*
         *  之前散装的判定函数我丢这里了
         *  一方面是我觉得散装的lambda很难看
         *  另一方面是给模组家具支持留后路
         */

        public static bool IsPlatform(Item item) => IsTilePlatform(item.createTile);

        public static bool IsBlock(Item item) => !ItemID.Sets.GrassSeeds[item.type] && item.type is not ItemID.StaffofRegrowth and not ItemID.AcornAxe && IsTileBlock(item.createTile);

        public static bool IsTorch(Item item) => IsTileTorch(item.createTile);

        public static bool IsWorkbench(Item item) => IsTileWorkbench(item.createTile);

        public static bool IsChair(Item item) => IsTileChair(item.createTile, item.placeStyle);

        public static bool IsTable(Item item) => IsTileTable(item.createTile);

        public static bool IsDoor(Item item) => IsTileDoor(item.createTile);

        public static bool IsBed(Item item) => IsTileBed(item.createTile);
        public static bool IsChest(Item item) => IsTileChest(item.createTile);

        public static bool IsBookcase(Item item) => IsTileBookcase(item.createTile);

        public static bool IsBathtub(Item item) => IsTileBathtub(item.createTile);

        public static bool IsCandelabra(Item item) => IsTileCandelabra(item.createTile);

        public static bool IsCandle(Item item) => IsTileCandle(item.createTile);

        public static bool IsChandelier(Item item) => IsTileChandelier(item.createTile);

        public static bool IsClock(Item item) => IsTileClock(item.createTile);
        public static bool IsDresser(Item item) => IsTileDresser(item.createTile);
        public static bool IsLamp(Item item) => IsTileLamp(item.createTile);

        public static bool IsLantern(Item item) => IsTileLantern(item.createTile);

        public static bool IsPiano(Item item) => IsTilePiano(item.createTile);

        public static bool IsSink(Item item) => IsTileSink(item.createTile);

        public static bool IsBench(Item item) => IsTileBench(item.createTile);

        public static bool IsToilet(Item item) => IsTileToilet(item.createTile, item.placeStyle);

        public static bool IsCampfire(Item item) => IsTileCampfire(item.createTile);

        public static bool IsWall(Item item) =>
            item.createWall > WallID.None;



        public static readonly Func<Item, bool>[] CheckersForItem =
            [
                IsBlock,
                IsPlatform,
                IsWorkbench,
                IsTable,
                IsChair,
                IsDoor,
                IsChest,
                IsBed,
                IsBookcase,
                IsBathtub,
                IsCandelabra,
                IsCandle,
                IsChandelier,
                IsClock,
                IsDresser,
                IsLamp,
                IsLantern,
                IsPiano,
                IsSink,
                IsBench,
                IsToilet,
                IsTorch,
                IsCampfire,
                IsWall
            ];




        public static bool IsTilePlatform(int tileType) =>
            tileType >= TileID.Dirt
            && TileID.Sets.Platforms[tileType];

        public static bool IsTileBlock(int tileType) =>
            tileType >= TileID.Dirt
            && TileObjectData.GetTileData(tileType, 0) is null
            && Main.tileSolid[tileType]
            && !Main.tileSolidTop[tileType];

        public static bool IsTileTorch(int tileType) =>
            tileType >= TileID.Dirt
            && TileID.Sets.Torches[tileType];

        public static bool IsTileWorkbench(int tileType) =>
            tileType is TileID.WorkBenches;

        public static bool IsTileChair(int tileType, int placeStyle) =>
            tileType is TileID.Chairs
            && placeStyle is not 1 and not 20;

        public static bool IsTileTable(int tileType) =>
            tileType is TileID.Tables or TileID.Tables2;

        public static bool IsTileDoor(int tileType) =>
            tileType is TileID.ClosedDoor;

        public static bool IsTileBed(int tileType) =>
            tileType is TileID.Beds;

        public static bool IsTileChest(int tileType) =>
            tileType is TileID.Containers or TileID.Containers2;

        public static bool IsTileBookcase(int tileType) =>
            tileType is TileID.Bookcases;

        public static bool IsTileBathtub(int tileType) =>
            tileType is TileID.Bathtubs;

        public static bool IsTileCandelabra(int tileType) =>
            tileType is TileID.Candelabras or TileID.PlatinumCandelabra;

        public static bool IsTileCandle(int tileType) =>
            tileType is TileID.Candles or TileID.PlatinumCandle or TileID.WaterCandle or TileID.PeaceCandle or TileID.ShadowCandle;

        public static bool IsTileChandelier(int tileType) =>
            tileType is TileID.Chandeliers;

        public static bool IsTileClock(int tileType) =>
             tileType is TileID.GrandfatherClocks;
        public static bool IsTileDresser(int tileType)
            => tileType is TileID.Dressers;

        public static bool IsTileLamp(int tileType) =>
            tileType is TileID.Lamps;

        public static bool IsTileLantern(int tileType) =>
            tileType is TileID.HangingLanterns;

        public static bool IsTilePiano(int tileType) =>
            tileType is TileID.Pianos;

        public static bool IsTileSink(int tileType) =>
            tileType is TileID.Sinks;

        public static bool IsTileBench(int tileType) =>
            tileType is TileID.Benches;

        public static bool IsTileToilet(int tileType, int placeStyle) =>
            tileType is TileID.Toilets || (tileType is TileID.Chairs && placeStyle is 1 or 20);

        public static bool IsTileCampfire(int tileType) =>
            tileType is TileID.Campfire;

        public static readonly Func<int, bool>[] CheckersForTile =
        [
                IsTileBlock,
                IsTilePlatform,
                IsTileWorkbench,
                IsTileTable,
                null, // IsTileChair
                IsTileDoor,
                IsTileChest,
                IsTileBed,
                IsTileBookcase,
                IsTileBathtub,
                IsTileCandelabra,
                IsTileCandle,
                IsTileChandelier,
                IsTileClock,
                IsTileDresser,
                IsTileLamp,
                IsTileLantern,
                IsTilePiano,
                IsTileSink,
                IsTileBench,
                null, // IsTileToilet
                IsTileTorch,
                IsTileCampfire,
                null // IsTileWall
        ];
    }
}
