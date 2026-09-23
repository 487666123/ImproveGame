using ImproveGame.Common.ModSystems;
using ImproveGame.Core;
using System.Collections;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace ImproveGame.Content.Functions.Construction
{
    internal class GenerateCore
    {
        private static int _taskProcessed;

        public static void GenerateFromTag(TagCompound tag, Point position)
        {
            CoroutineSystem.GenerateRunner.Run(Generate(tag, position));
        }

        public static IEnumerator Generate(TagCompound tag, Point position)
        {
            var structure = new QoLStructure(tag);

            if (structure.StructureDatas is null || structure.StructureDatas.Count is 0)
            {
                // 此处应有Logger.Warn
                yield break;
            }

            // 添加Origin偏移
            position.X -= structure.OriginX;
            position.Y -= structure.OriginY;

            TipRenderer.CurrentState = TipRenderer.State.Placing;

            _taskProcessed = 0;
            yield return KillTiles(structure, position);
            yield return GenerateWalls(structure, position);
            yield return GenerateSingleTiles(structure, position);
            yield return GenerateMultiTiles(structure, position);
            yield return GenerateOutSet(structure, position);
            yield return SquareTiles(structure, position);
            yield return TextSigns(structure, position);

            SoundEngine.PlaySound(SoundID.ResearchComplete);
            TipRenderer.CurrentState = TipRenderer.State.Placed;
            TipRenderer.TextDisplayCountdown = 60;
        }

        public static IEnumerator KillTiles(QoLStructure structure, Point position)
        {
            if (WandSystem.ExplodeMode != WandSystem.Construct.ExplodeAndPlace)
            {
                yield break;
            }

            int width = structure.Width;
            int height = structure.Height;
            for (int x = 0; x <= width; x++)
            {
                for (int y = 0; y <= height; y++)
                {
                    var placePosition = position + new Point(x, y);
                    Tile tile = Main.tile[placePosition.X, placePosition.Y];
                    if (tile.HasTile && MyUtils.TryKillTile(placePosition.X, placePosition.Y, Main.LocalPlayer))
                    {
                        _taskProcessed++;
                    }

                    if (tile.WallType > 0)
                    {
                        WorldGen.KillWall(placePosition.X, placePosition.Y);
                        _taskProcessed++;
                    }

                    if (_taskProcessed >= 60)
                    {
                        _taskProcessed = 0;
                        yield return null;
                    }
                }
            }
        }

        public static IEnumerator GenerateSingleTiles(QoLStructure structure, Point position)
        {
            int width = structure.Width;
            int height = structure.Height;
            for (int x = 0; x <= width; x++)
            {
                for (int y = height; y >= 0; y--)
                {
                    int index = y + x * (height + 1);
                    var placePosition = position + new Point(x, y);
                    TileDefinition tileData = structure.StructureDatas[index];
                    int tileType = structure.ParseTileType(tileData); // 实际上的Type
                    int tileItemFindType = tileType; // 用于找到合适物品的Type
                    if (tileItemFindType is not -1 && TileID.Sets.Grass[tileItemFindType])
                    {
                        tileItemFindType = TileID.Dirt;
                    }

                    int tileItemType = MyUtils.GetTileItem(tileItemFindType, tileData.TileFrameX, tileData.TileFrameY);
                    if (tileItemType == -1 || Main.tile[placePosition].HasTile)
                    {
                        continue;
                    }

                    var tileObjectData = TileObjectData.GetTileData(tileType, 0);
                    if (tileObjectData is not null && (tileObjectData.CoordinateFullWidth > 18 ||
                                                       tileObjectData.CoordinateFullHeight > 18))
                    {
                        continue;
                    }
                    if (!MyUtils.HasDevMark)
                    {
                        var inventory = MyUtils.GetAllInventoryItemsList(Main.LocalPlayer, "portable").ToArray();
                        MyUtils.PickItemFromArray(Main.LocalPlayer, inventory, item =>
                                item.type == tileItemType &&
                                MyUtils.TryPlaceTile(placePosition.X, placePosition.Y, item, Main.LocalPlayer, forced: true),
                            true);
                    }
                    else
                    {
                        var item = new Item(tileItemType);
                        MyUtils.TryPlaceTile(placePosition.X, placePosition.Y, item, Main.LocalPlayer, forced: true);
                    }

                    // 挖掉重来！
                    if (TileID.Sets.Grass[tileType])
                    {
                        Main.tile[placePosition].ResetToType((ushort)tileType);
                        WorldGen.TileFrame(placePosition.X, position.Y, true, false);
                    }

                    if (WorldGen.CanPoundTile(placePosition.X, placePosition.Y))
                    {
                        if (tileData.BlockType is BlockType.HalfBlock)
                        {
                            WorldGen.SlopeTile(placePosition.X, placePosition.Y, 0);
                            WorldGen.PoundTile(placePosition.X, placePosition.Y);
                        }
                        else if (tileData.BlockType is not BlockType.Solid)
                        {
                            WorldGen.SlopeTile(placePosition.X, placePosition.Y, (int)tileData.BlockType - 1);
                        }
                    }

                    var tile = Main.tile[placePosition];
                    tile.TileColor = tileData.TileColor;
                    if (tileData.ExtraDatas[2] && !tile.IsActuated)
                    {
                        Wiring.ActuateForced(placePosition.X, placePosition.Y);
                    }

                    _taskProcessed++;
                    if (_taskProcessed >= 50)
                    {
                        _taskProcessed = 0;
                        yield return null;
                    }
                }
            }
        }

        public static IEnumerator GenerateWalls(QoLStructure structure, Point position)
        {
            int width = structure.Width;
            int height = structure.Height;
            for (int x = 0; x <= width; x++)
            {
                for (int y = 0; y <= height; y++)
                {
                    int index = y + x * (height + 1);
                    var placePosition = position + new Point(x, y);
                    TileDefinition tileData = structure.StructureDatas[index];
                    int wallItemType = MyUtils.GetWallItem(structure.ParseWallType(tileData));
                    if (wallItemType == -1 || Main.tile[placePosition].WallType != 0)
                    {
                        continue;
                    }

                    if (!MyUtils.HasDevMark)
                    {
                        var inventory = MyUtils.GetAllInventoryItemsList(Main.LocalPlayer, "portable").ToArray();
                        MyUtils.PickItemFromArray(Main.LocalPlayer, inventory, item =>
                                item.type == wallItemType &&
                                TryPlaceWall(item, placePosition.X, placePosition.Y),
                            true);
                    }
                    else
                    {
                        var item = new Item(wallItemType);
                        TryPlaceWall(item, placePosition.X, placePosition.Y);
                    }

                    var tile = Main.tile[placePosition];
                    tile.WallColor = tileData.WallColor;
                    _taskProcessed++;
                    if (_taskProcessed >= 60)
                    {
                        _taskProcessed = 0;
                        yield return null;
                    }
                }
            }
        }

        private static bool TryPlaceWall(Item item, int x, int y)
        {
            if (Main.tile[x, y].WallType != 0)
            {
                return false;
            }

            WorldGen.PlaceWall(x, y, item.createWall);
            return true;
        }

        public static IEnumerator GenerateMultiTiles(QoLStructure structure, Point position)
        {
            int width = structure.Width;
            int height = structure.Height;
            for (int x = 0; x <= width; x++)
            {
                for (int y = height; y >= 0; y--)
                {
                    var placePosition = position + new Point(x, y);
                    int index = y + x * (height + 1);
                    TileDefinition tileData = structure.StructureDatas[index];
                    int tileType = structure.ParseTileType(tileData);
                    if (tileType is -1)
                        continue;
                    var tileObjectData = MyUtils.GetTileData(tileType, tileData.TileFrameX, tileData.TileFrameY);
                    if (tileObjectData is null || (tileObjectData.CoordinateFullWidth <= 18 &&
                                                   tileObjectData.CoordinateFullHeight <= 18))
                    {
                        continue;
                    }
                    // 转换为帧坐标
                    int subX = (tileData.TileFrameX / tileObjectData.CoordinateFullWidth) *
                               tileObjectData.CoordinateFullWidth;
                    int subY = (tileData.TileFrameY / tileObjectData.CoordinateFullHeight) *
                               tileObjectData.CoordinateFullHeight;
                    int tileItemType = MyUtils.GetTileItem(tileType, subX, subY);
                    if (tileItemType == -1)
                    {
                        continue;
                    }
                    subX = tileData.TileFrameX % tileObjectData.CoordinateFullWidth;
                    subY = tileData.TileFrameY % tileObjectData.CoordinateFullHeight;
                    Point16 frame = new(subX / 18, subY / 18);
                    var origin = tileObjectData.Origin.ToPoint();
                    if (tileType is TileID.OpenDoor &&
                        tileData.TileFrameX / tileObjectData.CoordinateFullWidth % 2 ==
                        1) // 开启的门实际上OriginX为0，这里对于向左开的要重新定位到他的轴心
                    {
                        origin.X = 1;
                    }

                    if (frame.X != origin.X || frame.Y != origin.Y)
                    {
                        continue;
                    }

                    // 朝向
                    int direction = tileObjectData.Direction switch
                    {
                        TileObjectDirection.PlaceLeft => -1,
                        TileObjectDirection.PlaceRight => 1,
                        _ => 0
                    };

                    bool _TryPlace(Item item)
                    {
                        if (TileID.Sets.BasicChest[item.createTile])
                            return MyUtils.PlaceChestNoSync(placePosition.X, placePosition.Y, (ushort)item.createTile, false, item.placeStyle) != -1;
                        else if (TileID.Sets.BasicDresser[item.createTile])
                            return MyUtils.Place3x2NoSyncDresser(placePosition.X, placePosition.Y, (ushort)item.createTile, item.placeStyle);
                        else if (tileObjectData is { Width: 1, Height: 1 })
                            return MyUtils.TryPlaceTile(placePosition.X, placePosition.Y, item, Main.LocalPlayer, forced: true);
                        else
                            return MyUtils.TryPlaceMultiTileDirect(placePosition, item.createTile, item.placeStyle, direction, out _);
                    }

                    if (!MyUtils.HasDevMark)
                    {
                        var inventory = MyUtils.GetAllInventoryItemsList(Main.LocalPlayer, "portable").ToArray();
                        MyUtils.PickItemFromArray(Main.LocalPlayer, inventory, item =>
                                item is not null && item.type == tileItemType &&
                                _TryPlace(item),
                            true);
                    }
                    else
                    {
                        var item = new Item(tileItemType);
                        _TryPlace(item);
                    }
                }

                yield return null;
            }
        }

        public static IEnumerator GenerateOutSet(QoLStructure structure, Point position)
        {
            int width = structure.Width;
            int height = structure.Height;
            for (int x = 0; x <= width; x++)
            {
                for (int y = 0; y <= height; y++)
                {
                    int index = y + x * (height + 1);
                    TileDefinition tileData = structure.StructureDatas[index];

                    var placePosition = position + new Point(x, y);
                    var tile = Main.tile[placePosition];
                    if (tileData.ExtraDatas[2] && !tile.IsActuated)
                    {
                        Wiring.ActuateForced(placePosition.X, placePosition.Y);
                    }

                    if (tileData.RedWire && !tile.RedWire && TryConsumeWire())
                    {
                        WorldGen.PlaceWire(placePosition.X, placePosition.Y);
                    }

                    if (tileData.BlueWire && !tile.BlueWire && TryConsumeWire())
                    {
                        WorldGen.PlaceWire2(placePosition.X, placePosition.Y);
                    }

                    if (tileData.GreenWire && !tile.GreenWire && TryConsumeWire())
                    {
                        WorldGen.PlaceWire3(placePosition.X, placePosition.Y);
                    }

                    if (tileData.YellowWire && !tile.YellowWire && TryConsumeWire())
                    {
                        WorldGen.PlaceWire4(placePosition.X, placePosition.Y);
                    }

                    if (tileData.HasActuator && !tile.HasActuator) // 促动器
                    {
                        bool TryConsume(Item item)
                        {
                            if (item.type == ItemID.Actuator)
                            {
                                WorldGen.PlaceActuator(placePosition.X, placePosition.Y);
                                return true;
                            }

                            return false;
                        }

                        var inventory = MyUtils.GetAllInventoryItemsList(Main.LocalPlayer, "portable").ToArray();
                        var item = MyUtils.PickItemFromArray(Main.LocalPlayer, inventory, TryConsume, false);
                        if (MyUtils.HasDevMark)
                            item = new Item(ItemID.Actuator);
                        MyUtils.TryConsumeItem(ref item, Main.LocalPlayer, true); // 要手动consume (即无视consumable)
                    }

                    tile.WallColor = tileData.WallColor;
                    tile.TileColor = tileData.TileColor;
                    tile.Get<TileWallBrightnessInvisibilityData>() = tileData.CoatingData;

                    bool TryConsumeWire()
                    {
                        bool hasWire = false;

                        bool TryConsume(Item item)
                        {
                            if (item.type != ItemID.Wire)
                            {
                                return false;
                            }

                            hasWire = true;
                            return true;
                        }

                        var inventory = MyUtils.GetAllInventoryItemsList(Main.LocalPlayer, "portable").ToArray();
                        var item = MyUtils.PickItemFromArray(Main.LocalPlayer, inventory, TryConsume, false);
                        MyUtils.TryConsumeItem(ref item, Main.LocalPlayer, true); // 要手动consume (即无视consumable)
                        return hasWire;
                    }

                    _taskProcessed++;
                    if (_taskProcessed >= 5000)
                    {
                        _taskProcessed = 0;
                        yield return null;
                    }
                }
            }
        }

        public static IEnumerator SquareTiles(QoLStructure structure, Point position)
        {
            int width = structure.Width;
            int height = structure.Height;
            for (int x = 0; x <= width; x++)
            {
                for (int y = 0; y <= height; y++)
                {
                    var placePosition = position + new Point(x, y);
                    if (Main.tile[placePosition].HasTile)
                    {
                        WorldGen.TileFrame(placePosition.X, position.Y, true, false);
                    }

                    if (Main.tile[placePosition].WallType > 0)
                    {
                        Framing.WallFrame(placePosition.X, position.Y, true);
                    }

                    _taskProcessed++;
                    if (_taskProcessed >= 5000)
                    {
                        _taskProcessed = 0;
                        yield return null;
                    }
                }
            }

            if (Main.netMode is NetmodeID.MultiplayerClient)
            {
                NetMessage.SendTileSquare(Main.myPlayer, position.X - 1, position.Y - 1, width + 2, height + 2);
            }
        }

        public static IEnumerator TextSigns(QoLStructure structure, Point position)
        {
            if (structure.SignTexts is not { Count: > 0 })
                yield break;

            int index = 0;
            int width = structure.Width;
            int height = structure.Height;
            for (int x = 0; x <= width; x++)
            {
                for (int y = 0; y <= height; y++)
                {
                    var placePosition = position + new Point(x, y);
                    var tile = Main.tile[placePosition];
                    if (!tile.HasTile || !Main.tileSign[tile.TileType])
                        continue;
                    if ((tile.TileFrameX / 18) % 2 is not 0 || tile.TileFrameY / 18 is not 0)
                        continue;

                    int sign = Sign.ReadSign(placePosition.X, placePosition.Y);

                    if (sign >= 0)
                    {
                        Sign.TextSign(sign, structure.SignTexts[index]);
                        NetMessage.SendData(MessageID.OpenSignResponse, -1, -1, null, sign, 0f, (byte)new BitsByte(b1: true));
                        index++;
                    }

                    if (index >= structure.SignTexts.Count)
                        yield break;
                }
            }
        }
    }
}