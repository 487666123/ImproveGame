namespace ImproveGame.Interface.BaseViews;

public class BaseGrid : View
{
    #region 基本属性
    /// <summary>
    /// 行数，-1 代表不限，根据实际子元素数量计算本身大小 <br/>
    /// 不可与 <see cref="ColumnCount"/> 同时为 -1
    /// </summary>
    public int RowCount;

    /// <summary>
    /// 列数，-1 代表不限，根据实际子元素数量计算本身大小 <br/>
    /// 不可与 <see cref="RowCount"/> 同时为 -1
    /// </summary>
    public int ColumnCount;

    /// <summary>
    /// 单元格之间的水平和垂直间距
    /// </summary>
    public Vector2 CellSpacing;

    /// <summary>
    /// 单元格大小
    /// </summary>
    public Vector2 CellSize;

    /// <summary>
    /// 基准点，单元格以此点为左上角铺开 <br/>
    /// 更改此值并使用OverflowHidden以实现滚动条效果 <br/>
    /// 如果不使用该字段，而是直接改 Top.Pixels，单元格的IsMouseHovering会出Bug
    /// </summary>
    public Vector2 DatumPoint;
    #endregion

    /// <summary>
    /// 初始必须的设置
    /// </summary>
    /// <param name="rowCount"></param>
    /// <param name="columnCount"></param>
    /// <param name="cellSpacing"></param>
    /// <param name="cellSize"></param>
    public void SetBaseValues(int rowCount, int columnCount, Vector2 cellSpacing, Vector2 cellSize)
    {
        RowCount = rowCount;
        ColumnCount = columnCount;
        CellSpacing = cellSpacing;
        CellSize = cellSize;
    }

    /// <summary>
    /// 计算并设置自己身的大小，不调用 <see cref="Recalculate"/>
    /// </summary>
    public void CalculateWithSetGridSize()
    {
        if (RowCount <= 0)
        {
            if (ColumnCount > 0)
            {
                Width.Pixels = ColumnCount * (CellSize.X + CellSpacing.X);
                Height.Pixels = MathF.Ceiling(Children.Count() / (float)ColumnCount) * (CellSize.Y + CellSpacing.Y);
            }
            else
            {
                throw new Exception("RowCount 和 ColumnCount 不可同时小于 1");
            }
        }
        else
        {
            if (ColumnCount > 0)
            {
                Width.Pixels = ColumnCount * (CellSize.X + CellSpacing.X);
                Height.Pixels = RowCount * (CellSize.Y + CellSpacing.Y);
            }
            else
            {
                Width.Pixels = MathF.Ceiling(Children.Count() / (float)RowCount) * (CellSize.X + CellSpacing.X);
                Height.Pixels = RowCount * (CellSize.Y + CellSpacing.Y);
            }
        }
    }

    /// <summary>
    /// 计算并设置子元素的位置，不调用 <see cref="Recalculate"/>
    /// </summary>
    public virtual void CalculateWithSetChildrenPosition()
    {
        var uies = Children.ToList();
        int uiesCount = uies.Count;

        for (int i = 0; i < uiesCount; i++)
        {
            UIElement uie = uies[i];

            uie.Left.Pixels = i % ColumnCount * (CellSize.X + CellSpacing.X) + DatumPoint.X;
            uie.Top.Pixels = i / ColumnCount * (CellSize.Y + CellSpacing.Y) + DatumPoint.Y;
        }
    }

    public override void Recalculate()
    {
        Opacity.Recalculate();
        CalculateWithSetChildrenPosition();
        base.Recalculate();
    }
}
