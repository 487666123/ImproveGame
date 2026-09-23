using ImproveGame.Content.Items;
using ImproveGame.Packets;
using ImproveGame.Packets.Items;

namespace ImproveGame.Content.Packets;

[AutoSync]
public class SpaceWandOperation : NetModule
{
    private Vector2 _startPoint;
    private Vector2 _mousePosition;
    private byte _shapeType;
    private byte _blockType;
    private byte _placeType;
    private bool _controlLeft;

    public static SpaceWandOperation Get(Vector2 startPoint, Vector2 mousePosition, ShapeType shapeType,
        BlockType blockType, PlaceType placeType, bool controlLeft)
    {
        var module = NetModuleLoader.Get<SpaceWandOperation>();
        module._startPoint = startPoint;
        module._mousePosition = mousePosition;
        module._shapeType = (byte)shapeType;
        module._blockType = (byte)blockType;
        module._placeType = (byte)placeType;
        module._controlLeft = controlLeft;
        return module;
    }

    public static void Proceed(Vector2 startPoint, Vector2 mousePosition, ShapeType shapeType,
        BlockType blockType, PlaceType placeType, bool controlLeft)
    {
        if (Main.netMode is NetmodeID.Server)
            return;
        var module = Get(startPoint, mousePosition, shapeType, blockType, placeType, controlLeft);
        module.Send();
    }

    public override void Receive()
    {
        bool playSound = false;
        var itemsConsumed = new Dictionary<int, int>();
        var shapeType = (ShapeType)_shapeType;
        var tiles = SpaceWand.GetSelectedTiles(shapeType, _startPoint, _mousePosition, _controlLeft);
        var tilesHashSet = tiles.ToHashSet();
        MyUtils.ForeachTile(tilesHashSet, (x, y) =>
        {
            SpaceWand.OperateTile(Main.player[Sender], x, y, tilesHashSet, (PlaceType)_placeType, (BlockType)_blockType,
                ref playSound, itemsConsumed);
        });

        SendTiles(tilesHashSet);
        ConsumeItemPacket.Proceed(itemsConsumed, Sender);

        if (playSound)
            PlaySoundPacket.PlaySound(LegacySoundIDs.Dig, _startPoint);
    }

    private void SendTiles(HashSet<Point> tilesHashSet)
    {
        var shapeType = (ShapeType)_shapeType;
        switch (shapeType)
        {
            case ShapeType.Line:
            case ShapeType.Corner:
            case ShapeType.SquareEmpty:
            case ShapeType.CircleEmpty:
                MyUtils.ForeachTile(tilesHashSet, (x, y) =>
                {
                    // NetMessage.SendTileSquare(-1, x, y);
                    NetMessage.SendTileSquare(-1, x - 1, y - 1, 3, 3);
                });
                break;
            case ShapeType.SquareFilled:
            {
                var startingPoint = _startPoint.ToTileCoordinates();
                var nowPoint = _mousePosition.ToTileCoordinates();
                int maxSize = 60;
                nowPoint = MyUtils.ModifySize(startingPoint, nowPoint, maxSize, maxSize);
                var position = PointExtensions.Min(startingPoint, nowPoint);
                var size = (startingPoint - nowPoint).Abs();
                position.X -= 1;
                position.Y -= 1;
                size.X += 2;
                size.Y += 2;
                NetMessage.SendTileSquare(-1, position.X, position.Y, size.X, size.Y);
                break;
            }
            case ShapeType.CircleFilled:
            {
                var center = _startPoint.ToTileCoordinates();
                float maxSize = 49.5f;
                float radius = (int)(_startPoint.Distance(_mousePosition) / 16f) + 0.5f;
                radius = Math.Min(maxSize, radius);
                radius *= 2f; // 传入的是size，也就是直径
                NetMessage.SendTileSquare(-1, center.X, center.Y, (int)(radius + 2));
                break;
            }
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}