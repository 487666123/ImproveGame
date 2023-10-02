using ImproveGame.Common.Configs;
using ImproveGame.Common.Packets.WorldFeatures;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.Map;
using Terraria.ModLoader.IO;
using Terraria.WorldBuilding;

namespace ImproveGame.Content;

public class IndicatorMapLayer : ModMapLayer
{
    public override void Draw(ref MapOverlayDrawContext context, ref string text)
    {
        if (!Config.MinimapMark) return;

        if (StructureDatas.DungeonUnlocked && UIConfigs.Instance.MarkDungeon)
            DrawIcon(ref context, ref text, StructureDatas.DungeonPosition, new SpriteFrame(7, 1, 6, 0),
                "Bestiary_Biomes.TheDungeon");
        if (StructureDatas.ShimmerUnlocked && UIConfigs.Instance.MarkAether)
            DrawIcon(ref context, ref text, StructureDatas.ShimmerPosition, new SpriteFrame(7, 1, 2, 0),
                "Mods.ImproveGame.UI.MapLayer.TheAether");
        if (StructureDatas.TempleUnlocked && UIConfigs.Instance.MarkTemple)
            DrawIcon(ref context, ref text, StructureDatas.TemplePosition, new SpriteFrame(7, 1, 4, 0),
                "Bestiary_Biomes.TheTemple");

        if (StructureDatas.PyramidsUnlocked && UIConfigs.Instance.MarkPyramid)
            foreach (Point16 pyramidPosition in StructureDatas.PyramidPositions)
                DrawIcon(ref context, ref text, pyramidPosition, new SpriteFrame(7, 1, 1, 0),
                    "Mods.ImproveGame.UI.MapLayer.Pyramid");

        if (StructureDatas.FloatingIslandsUnlocked && UIConfigs.Instance.MarkFloatingIsland)
        {
            foreach (Point16 skylandPosition in StructureDatas.SkyHousePositions)
                DrawIcon(ref context, ref text, skylandPosition, new SpriteFrame(7, 1, 5, 0),
                    "Mods.ImproveGame.UI.MapLayer.FloatingIsland");

            foreach (Point16 skylandPosition in StructureDatas.SkyLakePositions)
                DrawIcon(ref context, ref text, skylandPosition, new SpriteFrame(7, 1, 0, 0),
                    "Mods.ImproveGame.UI.MapLayer.FloatingLake");
        }

        if (UIConfigs.Instance.MarkPlantera)
            foreach (Point16 planteraPosition in StructureDatas.PlanteraPositions)
                DrawIcon(ref context, ref text, planteraPosition.X, planteraPosition.Y, new SpriteFrame(7, 1, 3, 0),
                    "MapObject.PlanterasBulb");
    }

    private MapOverlayDrawContext.DrawResult DrawIcon(ref MapOverlayDrawContext context, ref string text,
        Point16 position, SpriteFrame frame, string hoverTextKey) =>
        DrawIcon(ref context, ref text, position.ToVector2(), frame, hoverTextKey);

    private MapOverlayDrawContext.DrawResult DrawIcon(ref MapOverlayDrawContext context, ref string text,
        float x, float y, SpriteFrame frame, string hoverTextKey) =>
        DrawIcon(ref context, ref text, new Vector2(x, y), frame, hoverTextKey);

    private MapOverlayDrawContext.DrawResult DrawIcon(ref MapOverlayDrawContext context, ref string text,
        Vector2 position, SpriteFrame frame, string hoverTextKey)
    {
        if (position == Vector2.Zero) return default;

        const float scale = 1f;
        var tex = ModAsset.MinimapIcons.Value;
        var drawResult = context.Draw(tex, position, Color.White, frame, scale, scale, Alignment.Center);
        if (drawResult.IsMouseOver)
        {
            text = Language.GetTextValue(hoverTextKey);
        }

        return drawResult;
    }
}

public class StructureDatas : ModSystem
{
    public enum UnlockID : byte
    {
        Shimmer,
        Temple,
        Pyramids,
        FloatingIslands,
        Dungeon
    }

    internal static BitsByte StructuresUnlocked;

    public static bool ShimmerUnlocked => StructuresUnlocked[(byte)UnlockID.Shimmer];
    public static bool TempleUnlocked => StructuresUnlocked[(byte)UnlockID.Temple];
    public static bool PyramidsUnlocked => StructuresUnlocked[(byte)UnlockID.Pyramids];
    public static bool FloatingIslandsUnlocked => StructuresUnlocked[(byte)UnlockID.FloatingIslands];
    public static bool DungeonUnlocked => StructuresUnlocked[(byte)UnlockID.Dungeon];

    public static Point16 DungeonPosition; // 多人模式客户端下，dungeonX和dungeonY均为0
    public static Point16 ShimmerPosition;
    public static Point16 TemplePosition;
    public static List<Point16> PyramidPositions;
    public static List<Point16> SkyHousePositions;
    public static List<Point16> SkyLakePositions;
    public static List<Point16> PlanteraPositions;

    public override void PostUpdateWorld()
    {
        if (Main.netMode is NetmodeID.MultiplayerClient)
            return;

        DungeonPosition = new Point16(Main.dungeonX, Main.dungeonY - 6);

        int elementsRemoved = PlanteraPositions.RemoveAll(coords =>
        {
            if (!WorldGen.InWorld(coords.X, coords.Y, 10))
                return true;

            var tile = Main.tile[coords.ToPoint()];
            return !tile.HasTile || tile.TileType is not TileID.PlanteraBulb ||
                   tile.TileFrameX is not 18 || tile.TileFrameY is not 18;
        });

        if (Main.netMode is NetmodeID.Server && elementsRemoved > 0)
            PlanteraPositionsPacket.Sync();
    }

    public override void Load()
    {
        On_WorldGen.Pyramid += (orig, x, y) =>
        {
            if (orig(x, y))
            {
                PyramidPositions.Add(new Point16(x, y + 50));
                return true;
            }

            return false;
        };
    }

    public override void PreWorldGen()
    {
        PyramidPositions = new List<Point16>();
    }

    public override void PostWorldGen()
    {
        ShimmerPosition = new Point16((int)GenVars.shimmerPosition.X, (int)GenVars.shimmerPosition.Y - 8);
        TemplePosition = new Point16((GenVars.tLeft + GenVars.tRight) / 2, (GenVars.tTop + GenVars.tBottom) / 2);

        // 不是所有金字塔都能成功生成，所以要在对Pyramid的On里面设置
        // PyramidPositions = new List<Point16>();
        // for (int i = 0; i < GenVars.numPyr; i++)
        //     PyramidPositions.Add(new Point16(GenVars.PyrX[i], GenVars.PyrY[i] + 50));

        SkyHousePositions = new List<Point16>();
        for (int i = 0; i < GenVars.numIslandHouses; i++)
        {
            if (GenVars.skyLake[i])
                SkyLakePositions.Add(new Point16(GenVars.floatingIslandHouseX[i], GenVars.floatingIslandHouseY[i] - 6));
            else
                SkyHousePositions.Add(new Point16(GenVars.floatingIslandHouseX[i],
                    GenVars.floatingIslandHouseY[i] - 6));
        }
    }

    public override void ClearWorld()
    {
        StructuresUnlocked = new BitsByte();
        ShimmerPosition = new Point16();
        TemplePosition = new Point16();
        PyramidPositions = new List<Point16>();
        SkyHousePositions = new List<Point16>();
        SkyLakePositions = new List<Point16>();
        PlanteraPositions = new List<Point16>();
    }

    public override void SaveWorldData(TagCompound tag)
    {
        tag["unlocked"] = (byte)StructuresUnlocked;
        tag["shimmer"] = ShimmerPosition;
        tag["temple"] = TemplePosition;
        tag["pyramids"] = PyramidPositions;
        tag["skyHouses"] = SkyHousePositions;
        tag["skyLakes"] = SkyLakePositions;
        tag["planteras"] = PlanteraPositions;
    }

    public override void LoadWorldData(TagCompound tag)
    {
        StructuresUnlocked = tag.Get<byte>("unlocked");
        ShimmerPosition = tag.Get<Point16>("shimmer");
        TemplePosition = tag.Get<Point16>("temple");
        PyramidPositions = tag.Get<List<Point16>>("pyramids");
        SkyHousePositions = tag.Get<List<Point16>>("skyHouses");
        SkyLakePositions = tag.Get<List<Point16>>("skyLakes");
        PlanteraPositions = tag.Get<List<Point16>>("planteras");
    }

    public override void NetSend(BinaryWriter writer)
    {
        writer.Write(StructuresUnlocked);
        writer.Write((short)Main.dungeonX);
        writer.Write((short)(Main.dungeonY - 6));
        writer.Write(ShimmerPosition);
        writer.Write(TemplePosition);
        writer.Write(PyramidPositions);
        writer.Write(SkyHousePositions);
        writer.Write(SkyLakePositions);
        writer.Write(PlanteraPositions);
    }

    public override void NetReceive(BinaryReader reader)
    {
        StructuresUnlocked = reader.ReadByte();
        DungeonPosition = reader.ReadPoint16();
        ShimmerPosition = reader.ReadPoint16();
        TemplePosition = reader.ReadPoint16();
        PyramidPositions = reader.ReadPoint16List().ToList();
        SkyHousePositions = reader.ReadPoint16List().ToList();
        SkyLakePositions = reader.ReadPoint16List().ToList();
        PlanteraPositions = reader.ReadPoint16List().ToList();
    }
}