using ImproveGame.UI.AutoTrash;
using SilkyUIFramework;
using SilkyUIFramework.Attributes;
using SilkyUIFramework.Elements;
using SilkyUIFramework.Layout;

namespace ImproveGame.UserInterfaces.AutoTrash;

[XmlElementMapping("GarbageListGrid")]
public class AutoTrashListView : SUIScrollView
{
    public AutoTrashListView()
    {
        SetGap(8f);

        FitWidth = true;
        Mask.FitWidth = true;
        Container.FitWidth = true;

        // 类原版样式，不好看先留着。
        //ScrollBar.SetWidth(16f);
        //ScrollBar.SetPadding(2f);
        //ScrollBar.Border = 2f;
        //ScrollBar.BorderRadius = new Vector4(8f);

        //ScrollBar.BackgroundColor = new Color(44, 57, 105);
        //ScrollBar.BorderColor = new Color(20, 25, 60);

        //ScrollBar.Thumb.BorderRadius = new Vector4(4f);
        //ScrollBar.Thumb.BarColor = (Color.White, new Color(220, 220, 220));

        Container.LayoutType = LayoutType.Grid;
        Container.GridDirection = GridDirection.Row;

        Container.SetGap(8f);
        Container.SetTemplateColumns(GridTrack.Repeat(4, TemplateType.Auto));
        Container.SetAutoRows([GridTrack.Auto]);

        Container.SetMinHeight(0f, 1f);
    }

    public void RefreshItems()
    {
        if (!Main.LocalPlayer.TryGetModPlayer<AutoTrashPlayer>(out var trashPlayer)) return;

        var items = trashPlayer.ThrowAwayItems;
        if (MatchesItems(items)) return;

        Container.RemoveAllChildren();
        foreach (var item in items)
            Container.AddChild(new AutoTrashItemSlot(item));
    }

    private bool MatchesItems(List<Item> items)
    {
        if (Container.Children.Count != items.Count) return false;

        for (int i = 0; i < items.Count; i++)
        {
            if (Container.Children[i] is not AutoTrashItemSlot slot || !ReferenceEquals(slot.Item, items[i]))
                return false;
        }

        return true;
    }
}
