namespace ImproveGame.Content.Items;

public partial class SpaceWand
{
    /// <summary>
    /// 根据鼠标到放置起始点的角度，判断放置的方向
    /// </summary>
    /// <returns></returns>
    public static Direction GetDirection(Vector2 startPoint, Vector2 mousePosition)
    {
        var centerToCursorVector = mousePosition - startPoint;
        var angle = centerToCursorVector.ToRotation();

        // 转换成只有正数的角度
        if (angle < 0)
            angle = MathHelper.TwoPi + angle;

        // 角度判断
        const float unitWidth = MathHelper.PiOver4; // 一个区域的角度是unitWidth
        const float unitWidthOver2 = unitWidth / 2f;
        return angle switch
        {
            < unitWidthOver2 or > MathHelper.TwoPi - unitWidthOver2 => Direction.Right,
            >= unitWidthOver2 + unitWidth * 0 and <= unitWidthOver2 + unitWidth * 1 => Direction.RightDown,
            >= unitWidthOver2 + unitWidth * 1 and <= unitWidthOver2 + unitWidth * 2 => Direction.Down,
            >= unitWidthOver2 + unitWidth * 2 and <= unitWidthOver2 + unitWidth * 3 => Direction.LeftDown,
            >= unitWidthOver2 + unitWidth * 3 and <= unitWidthOver2 + unitWidth * 4 => Direction.Left,
            >= unitWidthOver2 + unitWidth * 4 and <= unitWidthOver2 + unitWidth * 5 => Direction.LeftUp,
            >= unitWidthOver2 + unitWidth * 5 and <= unitWidthOver2 + unitWidth * 6 => Direction.Up,
            >= unitWidthOver2 + unitWidth * 6 and <= unitWidthOver2 + unitWidth * 7 => Direction.RightUp,
            _ => Direction.Right
        };
    }

    public static IEnumerable<Point> GetSelectedTiles(ShapeType shapeType, Vector2 startPoint, Vector2 mousePosition, bool lastControlLeft)
    {
        switch (shapeType)
        {
            case ShapeType.Line:
                return GetLineTiles(startPoint, mousePosition);
            case ShapeType.Corner:
                return GetCornerTiles(startPoint, mousePosition, lastControlLeft);
            case ShapeType.SquareEmpty:
                return GetSquareTiles(startPoint, mousePosition, false);
            case ShapeType.SquareFilled:
                return GetSquareTiles(startPoint, mousePosition, true);
            case ShapeType.CircleEmpty:
                return GetCircleTiles(startPoint, mousePosition, false, lastControlLeft);
            case ShapeType.CircleFilled:
                return GetCircleTiles(startPoint, mousePosition, true, lastControlLeft);
            default:
                return [];
        }
    }

    public static IEnumerable<Point> GetLineTiles(Vector2 startPoint, Vector2 mousePosition)
    {
        var direction = GetDirection(startPoint, mousePosition);
        int distanceToMouseX = (int)(Math.Abs(startPoint.X - mousePosition.X) / 16f);
        int distanceToMouseY = (int)(Math.Abs(startPoint.Y - mousePosition.Y) / 16f);
        int distanceToMouse = Math.Max(distanceToMouseX, distanceToMouseY);
        var position = startPoint.ToTileCoordinates();

        _dataText = $"{distanceToMouse + 1}";

        var increment = direction switch
        {
            Direction.Right => new Point(1, 0),
            Direction.Down => new Point(0, 1),
            Direction.Left => new Point(-1, 0),
            Direction.Up => new Point(0, -1),
            Direction.RightDown => new Point(1, 1),
            Direction.LeftDown => new Point(-1, 1),
            Direction.LeftUp => new Point(-1, -1),
            Direction.RightUp => new Point(1, -1),
            _ => throw new ArgumentOutOfRangeException()
        };

        for (int i = 0; i <= distanceToMouse; i++)
        {
            yield return position;
            position += increment;
            if (!WorldGen.InWorld(position.X, position.Y, 10))
                break;
        }
    }

    public static List<Point> GetCornerTiles(Vector2 startPoint, Vector2 mousePosition, bool lastControlLeft)
    {
        var tilePositions = new List<Point>();
        var position = startPoint.ToTileCoordinates();
        var destination = mousePosition.ToTileCoordinates();
        tilePositions.Add(position);

        // 尺寸+总共放置的物块数量
        var size = (position - destination).Abs();
        _dataText = $"{size.X + 1}x{size.Y + 1}";
        _dataText += $" ({size.X + 1 + size.Y + 1 - 1})";

        // 类原版蓝图
        if (lastControlLeft)
        {
            DoHorizontal();
            DoVertical();
        }
        else
        {
            DoVertical();
            DoHorizontal();
        }

        return tilePositions;

        void DoVertical()
        {
            int tried = 0;
            while (position.Y != destination.Y)
            {
                int increment = Math.Sign(destination.Y - position.Y);
                position.Y += increment;
                tilePositions.Add(position);
                if (!WorldGen.InWorld(position.X, position.Y, 10))
                    break;

                // 令人“啊？”的情况
                tried++;
                if (tried > 10000)
                    break;
            }
        }

        void DoHorizontal()
        {
            int tried = 0;
            while (position.X != destination.X)
            {
                int increment = Math.Sign(destination.X - position.X);
                position.X += increment;
                tilePositions.Add(position);
                if (!WorldGen.InWorld(position.X, position.Y, 10))
                    break;

                // 令人“啊？”的情况
                tried++;
                if (tried > 10000)
                    break;
            }
        }
    }

    public static IEnumerable<Point> GetSquareTiles(Vector2 startPoint, Vector2 mousePosition, bool filled)
    {
        var startingPoint = startPoint.ToTileCoordinates();
        var nowPoint = mousePosition.ToTileCoordinates();
        int maxSize = filled ? 60 : 160;
        nowPoint = MyUtils.ModifySize(startingPoint, nowPoint, maxSize, maxSize);
        var position = PointExtensions.Min(startingPoint, nowPoint);
        var size = (startingPoint - nowPoint).Abs();

        // 尺寸+总共放置的物块数量
        _dataText = $"{size.X + 1}x{size.Y + 1}";
        if (filled)
            _dataText += $" ({(size.X + 1) * (size.Y + 1)})";
        else
            _dataText += $" ({(size.X + 1) * 2 + (size.Y + 1) * 2 - 4})";

        for (int i = 0; i <= size.X; i++)
        {
            for (int j = 0; j <= size.Y; j++)
            {
                switch (filled)
                {
                    case false when (i == 0 || i == size.X || j == 0 || j == size.Y):
                    case true:
                        yield return position + new Point(i, j);
                        break;
                }
            }
        }
    }

    public static IEnumerable<Point> GetCircleTiles(Vector2 startPoint, Vector2 mousePosition, bool filled, bool lastControlLeft)
    {
        var center = startPoint.ToTileCoordinates();
        float maxSize = filled ? 49.5f : 99.5f;
        float radius = (int)(startPoint.Distance(mousePosition) / 16f) + 0.5f;
        radius = Math.Min(maxSize, radius);
        float radiusSquared = radius * radius;
        bool fat = lastControlLeft; // 边缘像素连起来的空心圆
        int tileCount = 0;

        // 尺寸
        _dataText = $"{(int)(radius + 0.5f)}";

        bool IsInCircle(int x, int y)
        {
            var now = new Point(x, y);
            var distance = center.DistanceSQ(now);
            return distance < radiusSquared;
        }

        for (int i = -(int)radius; i <= radius; i++)
        {
            for (int j = -(int)radius; j <= radius; j++)
            {
                int x = center.X + i;
                int y = center.Y + j;
                if (!IsInCircle(x, y))
                    continue;

                switch (filled)
                {
                    case false when fat && !(IsInCircle(x + 1, y) && IsInCircle(x - 1, y) &&
                                             IsInCircle(x, y + 1) && IsInCircle(x, y - 1)):
                    case false when !fat && !(IsInCircle(x + 1, y) && IsInCircle(x - 1, y) &&
                                              IsInCircle(x, y + 1) && IsInCircle(x, y - 1) &&
                                              IsInCircle(x + 1, y + 1) && IsInCircle(x - 1, y + 1) &&
                                              IsInCircle(x + 1, y - 1) && IsInCircle(x - 1, y - 1)):
                    case true:
                        yield return new Point(x, y);
                        tileCount++;
                        break;
                }
            }
        }

        // 总共放置的物块数量
        _dataText += $" ({tileCount})";
    }
}