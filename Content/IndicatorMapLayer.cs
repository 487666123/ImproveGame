using ImproveGame.Common.Configs;
using ImproveGame.Content.Tiles;
using ImproveGame.Packets.WorldFeatures;
using Terraria.DataStructures;
using Terraria.GameContent.Biomes;
using Terraria.Map;
using Terraria.ModLoader.IO;
using Terraria.WorldBuilding;

namespace ImproveGame.Content;

public class IndicatorMapLayer : ModMapLayer
{
    public override void Draw(ref MapOverlayDrawContext context, ref string text)
    {
        byte columns = 8; // 列数
        byte rows = 2; // 行数
                       // 空钓鱼机
        ProcessStructureWithMultiplePositions(ref context, ref text, true, StructureDatas.BaitlessAutofisherPositions,
            UIConfigs.Instance.MarkEmptyAutofisher, "Mods.ImproveGame.UI.MapLayer.EmptyAutofisher",
            new SpriteFrame(columns, rows, 0, 1), new Vector2(1f, 1f));

        // 定位球系列图标
        if (!ImproveConfigs.Instance.MinimapMark) return;

        // 地牢
        ProcessStructure(ref context, ref text, StructureDatas.DungeonUnlocked, StructureDatas.DungeonPosition,
            UIConfigs.Instance.MarkDungeon, "Bestiary_Biomes.TheDungeon", new SpriteFrame(columns, rows, 6, 0));

        // 微光
        ProcessStructure(ref context, ref text, StructureDatas.ShimmerUnlocked, StructureDatas.ShimmerPosition,
            UIConfigs.Instance.MarkAether, "Mods.ImproveGame.UI.MapLayer.TheAether",
            new SpriteFrame(columns, rows, 2, 0));

        // 神庙
        ProcessStructure(ref context, ref text, StructureDatas.TempleUnlocked, StructureDatas.TemplePosition,
            UIConfigs.Instance.MarkTemple, "Bestiary_Biomes.TheTemple", new SpriteFrame(columns, rows, 4, 0));

        // 金字塔
        ProcessStructureWithMultiplePositions(ref context, ref text, StructureDatas.PyramidsUnlocked,
            StructureDatas.PyramidPositions, UIConfigs.Instance.MarkPyramid, "Mods.ImproveGame.UI.MapLayer.Pyramid",
            new SpriteFrame(columns, rows, 1, 0));

        // 空岛
        ProcessStructureWithMultiplePositions(ref context, ref text, StructureDatas.FloatingIslandsUnlocked,
            StructureDatas.SkyHousePositions, UIConfigs.Instance.MarkFloatingIsland,
            "Mods.ImproveGame.UI.MapLayer.FloatingIsland", new SpriteFrame(columns, rows, 5, 0));
        ProcessStructureWithMultiplePositions(ref context, ref text, StructureDatas.FloatingIslandsUnlocked,
            StructureDatas.SkyLakePositions, UIConfigs.Instance.MarkFloatingIsland,
            "Mods.ImproveGame.UI.MapLayer.FloatingLake", new SpriteFrame(columns, rows, 0, 0));

        // 世纪之花
        ProcessStructureWithMultiplePositions(ref context, ref text, true, StructureDatas.PlanteraPositions,
            UIConfigs.Instance.MarkPlantera, "MapObject.PlanterasBulb", new SpriteFrame(columns, rows, 3, 0));

        // 附魔剑
        ProcessStructureWithMultiplePositions(ref context, ref text, true, StructureDatas.EnchantedSwordPositions,
            UIConfigs.Instance.MarkEnchantedSword, "ItemName.EnchantedSword", new SpriteFrame(columns, rows, 7, 0),
            new Vector2(1.5f, 1f));

        // 大理石洞
        ProcessStructureWithMultiplePositions(ref context, ref text, true, StructureDatas.MarbleCavePositions,
            UIConfigs.Instance.MarkMarbleCave, "Mods.ImproveGame.Items.MarbleCaveGlobe.BiomeName", new SpriteFrame(columns, rows, 1, 1));

        // 花岗岩洞
        ProcessStructureWithMultiplePositions(ref context, ref text, true, StructureDatas.GraniteCavePositions,
            UIConfigs.Instance.MarkGraniteCave, "Mods.ImproveGame.Items.GraniteCaveGlobe.BiomeName", new SpriteFrame(columns, rows, 2, 1));

        // 蜂巢
        ProcessStructureWithMultiplePositions(ref context, ref text, true, StructureDatas.HivePositions,
            UIConfigs.Instance.MarkGraniteCave, "Mods.ImproveGame.Items.HiveGlobe.BiomeName", new SpriteFrame(columns, rows, 3, 1));
    }

    private void ProcessStructure(ref MapOverlayDrawContext context, ref string text, bool isUnlocked, Point16 position,
        float mark, string overlayName, SpriteFrame frame)
    {
        if (!isUnlocked || mark <= 0f) return;
        DrawIcon(ref context, ref text, position.ToVector2(), frame, overlayName, mark);
    }

    private void ProcessStructureWithMultiplePositions(ref MapOverlayDrawContext context, ref string text,
        bool isUnlocked, IEnumerable<Point16> positions, float mark, string overlayName, SpriteFrame frame,
        Vector2 positionOffset = default)
    {
        if (!isUnlocked || mark <= 0f) return;
        foreach (Point16 position in positions)
        {
            DrawIcon(ref context, ref text, position.ToVector2() + positionOffset, frame, overlayName, mark);
        }
    }

    private void DrawIcon(ref MapOverlayDrawContext context, ref string text, Vector2 position, SpriteFrame frame,
        string hoverTextKey, float scale)
    {
        if (position == Vector2.Zero) return;

        var tex = ModAsset.MinimapIcons.Value;
        var drawResult = context.Draw(tex, position, Color.White, frame, scale, scale, Alignment.Center);
        if (drawResult.IsMouseOver)
        {
            text = Language.GetTextValue(hoverTextKey);
        }
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

    public static Point16 DungeonPosition { get; set; } // 多人模式客户端下，dungeonX和dungeonY均为0
    public static Point16 ShimmerPosition { get; set; }
    public static Point16 TemplePosition { get; set; }
    public static List<Point16> PyramidPositions { get; set; }
    public static List<Point16> SkyHousePositions { get; set; }
    public static List<Point16> SkyLakePositions { get; set; }
    public static List<Point16> PlanteraPositions { get; set; }
    public static List<Point16> EnchantedSwordPositions { get; set; }
    public static List<Point16> AllMarbleCavePositions { get; set; }
    public static List<Point16> MarbleCavePositions { get; set; }
    public static List<Point16> AllGraniteCavePositions { get; set; }
    public static List<Point16> GraniteCavePositions { get; set; }
    public static List<Point16> AllHivePositions { get; set; }
    public static List<Point16> HivePositions { get; set; }
    public static List<Point16> BaitlessAutofisherPositions { get; set; }

    public static string QotVersionInWorldGeneration
    {
        get
        {
            if (!string.IsNullOrEmpty(field)) return field;
            // if (DungeonPosition != default) return field = true; // 原版会记录数据
            // if (ShimmerPosition != default) return field = true; // 微光目前有补充检测
            if (TemplePosition != default) return field = ImproveGame.Instance.Version.ToString();
            if (PyramidPositions.Count != 0) return field = ImproveGame.Instance.Version.ToString();
            if (SkyHousePositions.Count != 0) return field = ImproveGame.Instance.Version.ToString();
            if (SkyLakePositions.Count != 0) return field = ImproveGame.Instance.Version.ToString();
            return "";
        }
        set;
    }

    public static bool QotEanbledInWorldGeneration => !string.IsNullOrEmpty(QotVersionInWorldGeneration);

    private static int _autofisherValidateTimer;

    public override void PostUpdateWorld()
    {
        if (Main.netMode is NetmodeID.MultiplayerClient)
            return;

        DungeonPosition = new Point16(Main.dungeonX, Main.dungeonY - 6);

        ValidatePlanteraPositions();
        ValidateEnchantedSwordPositions();

        _autofisherValidateTimer++;
        if (_autofisherValidateTimer % 300 != 0)
            return;

        BaitlessAutofisherPositions ??= [];
        var existingBaitlessAutofishers = TileEntity.ByID
            .Where(pair => pair.Value is TEAutofisher { HasBait: false })
            .Select(pair => pair.Value.Position)
            .ToList();
        // 内置单人支持
        if (!existingBaitlessAutofishers.SequenceEqual(BaitlessAutofisherPositions))
            BaitlessAutofisherSyncPacket.Sync(existingBaitlessAutofishers);
    }

    private void ValidatePlanteraPositions()
    {
        int elementsRemoved = PlanteraPositions.RemoveAll(coords =>
        {
            if (!WorldGen.InWorld(coords.X, coords.Y, 10))
                return true;

            var tile = Main.tile[coords.ToPoint()];
            return !tile.HasTile || tile.TileType is not TileID.PlanteraBulb;
        });

        if (Main.netMode is NetmodeID.Server && elementsRemoved > 0)
            PlanteraPositionsPacket.Sync();
    }

    private void ValidateEnchantedSwordPositions()
    {
        int elementsRemoved = EnchantedSwordPositions.RemoveAll(coords =>
        {
            if (!WorldGen.InWorld(coords.X, coords.Y, 10))
                return true;

            var tile = Main.tile[coords.ToPoint()];
            return !tile.HasTile || tile.TileType is not TileID.LargePiles2 ||
                   tile.TileFrameX is not 918 || tile.TileFrameY is not 0;
        });

        if (Main.netMode is NetmodeID.Server && elementsRemoved > 0)
            EnchantedSwordPositionsPacket.Sync();
    }

    public override void Load()
    {
        On_WorldGen.Pyramid += (orig, x, y) =>
        {
            QotVersionInWorldGeneration = Mod.Version.ToString();
            if (orig(x, y))
            {
                PyramidPositions.Add(new Point16(x, y + 50));
                return true;
            }

            return false;
        };

        // 可恶的醉酒会互换大理石和花岗岩，但没关系，我也会互换嘻嘻
        On_MarbleBiome.Place += (orig, marbleBiome, origin, structures) =>
        {
            if (orig(marbleBiome, origin, structures))
            {
                if (Main.drunkWorld)
                    AllGraniteCavePositions.Add(new Point16(origin.X, origin.Y));
                else
                    AllMarbleCavePositions.Add(new Point16(origin.X, origin.Y));
                return true;
            }

            return false;
        };

        On_GraniteBiome.Place += (orig, graniteBiome, origin, structures) =>
        {
            if (orig(graniteBiome, origin, structures))
            {
                if (Main.drunkWorld)
                    AllMarbleCavePositions.Add(new Point16(origin.X, origin.Y));
                else
                    AllGraniteCavePositions.Add(new Point16(origin.X, origin.Y));
                return true;
            }

            return false;
        };

        On_HiveBiome.Place += (orig, hiveBiome, origin, structures) =>
        {
            if (orig(hiveBiome, origin, structures))
            {
                AllHivePositions.Add(new Point16(origin.X, origin.Y));
                return true;
            }

            return false;
        };
    }

    public override void PreWorldGen()
    {
        PyramidPositions = new List<Point16>();
        BaitlessAutofisherPositions = new List<Point16>();
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
        EnchantedSwordPositions = new List<Point16>();
        AllMarbleCavePositions = new List<Point16>();
        MarbleCavePositions = new List<Point16>();
        AllGraniteCavePositions = new List<Point16>();
        GraniteCavePositions = new List<Point16>();
        AllHivePositions = new List<Point16>();
        HivePositions = new List<Point16>();
        BaitlessAutofisherPositions = new List<Point16>();
    }

    public override void SaveWorldData(TagCompound tag)
    {
        tag["unlocked"] = (byte)StructuresUnlocked;
        tag["shimmer"] = ShimmerPosition;
        tag["temple"] = TemplePosition;
        tag["pyramids"] = PyramidPositions ?? [];
        tag["skyHouses"] = SkyHousePositions ?? [];
        tag["skyLakes"] = SkyLakePositions ?? [];
        tag["planteras"] = PlanteraPositions ?? [];
        tag["swords"] = EnchantedSwordPositions ?? [];
        tag["allMarbleCave"] = AllMarbleCavePositions ?? [];
        tag["marbleCave"] = MarbleCavePositions ?? [];
        tag["allGraniteCave"] = AllGraniteCavePositions ?? [];
        tag["graniteCave"] = GraniteCavePositions ?? [];
        tag["allHive"] = AllHivePositions ?? [];
        tag["hive"] = HivePositions ?? [];
        tag["versionData"] = QotVersionInWorldGeneration;
    }

    public override void LoadWorldData(TagCompound tag)
    {
        StructuresUnlocked = tag.Get<byte>("unlocked");
        ShimmerPosition = tag.Get<Point16>("shimmer");
        TemplePosition = tag.Get<Point16>("temple");
        PyramidPositions = tag.Get<List<Point16>>("pyramids") ?? [];
        SkyHousePositions = tag.Get<List<Point16>>("skyHouses") ?? [];
        SkyLakePositions = tag.Get<List<Point16>>("skyLakes") ?? [];
        PlanteraPositions = tag.Get<List<Point16>>("planteras") ?? [];
        EnchantedSwordPositions = tag.Get<List<Point16>>("swords") ?? [];
        AllMarbleCavePositions = tag.Get<List<Point16>>("allMarbleCave") ?? [];
        MarbleCavePositions = tag.Get<List<Point16>>("marbleCave") ?? [];
        AllGraniteCavePositions = tag.Get<List<Point16>>("allGraniteCave") ?? [];
        GraniteCavePositions = tag.Get<List<Point16>>("graniteCave") ?? [];
        AllHivePositions = tag.Get<List<Point16>>("allHive") ?? [];
        HivePositions = tag.Get<List<Point16>>("hive") ?? [];
        if (tag.TryGet<string>("versionData", out var value))
            QotVersionInWorldGeneration = value;
        else
            QotVersionInWorldGeneration = "";
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
        writer.Write(EnchantedSwordPositions);
        writer.Write(AllMarbleCavePositions);
        writer.Write(MarbleCavePositions);
        writer.Write(AllGraniteCavePositions);
        writer.Write(GraniteCavePositions);
        writer.Write(AllHivePositions);
        writer.Write(HivePositions);
        writer.Write(BaitlessAutofisherPositions);
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
        EnchantedSwordPositions = reader.ReadPoint16List().ToList();
        AllMarbleCavePositions = reader.ReadPoint16List().ToList();
        MarbleCavePositions = reader.ReadPoint16List().ToList();
        AllGraniteCavePositions = reader.ReadPoint16List().ToList();
        GraniteCavePositions = reader.ReadPoint16List().ToList();
        AllHivePositions = reader.ReadPoint16List().ToList();
        HivePositions = reader.ReadPoint16List().ToList();
        BaitlessAutofisherPositions = reader.ReadPoint16List().ToList();
    }
}