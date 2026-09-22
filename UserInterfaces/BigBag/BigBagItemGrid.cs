using SilkyUIFramework;
using SilkyUIFramework.Attributes;
using SilkyUIFramework.Elements;
using SilkyUIFramework.Layout;

namespace ImproveGame.UserInterfaces.BigBag;

[XmlElementMapping("BigBagItemGrid")]
public class BigBagItemGrid : SUIScrollView
{
    public static int Columns => 10;
    public static float VisibleRows => 6f;
    public static float SlotSize => 52f;

    public Item[] Items
    {
        get; set
        {
            if (ReferenceEquals(field, value)) return;
            field = value;

            Container.RemoveAllChildren();
            for (int i = 0; i < field.Length; i++)
                Container.AddChild(new BigBagItemSlot(field, i));

            ScrollToStart(false);
        }
    } = [];

    public BigBagItemGrid()
    {
        SetHeight(SlotSize * VisibleRows + 8 * (VisibleRows - 1));
        SetGap(8f);

        Mask.FitWidth = true;
        Container.FitWidth = true;
        Mask.IndependentRenderTarget = false;

        Container.LayoutType = LayoutType.Grid;
        Container.GridDirection = GridDirection.Row;

        Container.SetGap(8);
        Container.SetTemplateColumns(GridTrack.Repeat(Columns, TemplateType.Pixels, SlotSize));
        Container.SetAutoRows([GridTrack.Pixels(SlotSize)]);
        Container.SetMinHeight(0f, 1f);
    }
}
