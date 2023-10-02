using ImproveGame.Common;
using ImproveGame.Common.GlobalItems;
using ImproveGame.Common.Packets.WorldFeatures;
using ImproveGame.Interface.GUI.WorldFeature;
using PinyinNet;
using Steamworks;
using System.Collections.ObjectModel;
using Terraria.ModLoader.UI;

namespace ImproveGame.Interface.GUI.ItemSearcher;

public class BankSlot : View
{
    private readonly ItemSearcherGUI _parent;
    private readonly BankType _bankType;
    private readonly Asset<Texture2D> _icon;

    public BankSlot(BankType bankType, int left, ItemSearcherGUI parent)
    {
        _bankType = bankType;
        _parent = parent;
        _icon = GetIcon();

        Left.Set(left, 0f);
        // Top.Set(top, 0f);
        Width.Set(70, 0f);
        Height.Set(70, 0f);
    }

    public override void DrawSelf(SpriteBatch spriteBatch)
    {
        base.DrawSelf(spriteBatch);

        var dimensions = GetDimensions();
        var position = dimensions.Position();

        spriteBatch.Draw(_icon.Value, position, Color.White);

        var items = GetBank().item;
        if (items.AnyMatchWithString(_parent.SearchContent))
            spriteBatch.Draw(ModAsset.Slot_Selection.Value, position, Color.White);
    }

    public void DrawItems()
    {
        if (!IsMouseHovering)
            return;

        var items = GetBank().item;
        List<TooltipLine> list = new();
        for (int i = 0; i < 4; i++)
        {
            string line = "";
            for (int j = 0; j <= 9; j++)
            {
                var realItem = items[i * 10 + j];
                var item = new Item(realItem.type, realItem.stack); // 不使用Clone，提升性能
                if (item.MatchWithString(_parent.SearchContent, false))
                    item.favorited = true;
                line += BgItemTagHandler.GenerateTag(item);
            }

            list.Add(new TooltipLine(ImproveGame.Instance, $"ChestItemLine_{i}", line));
        }

        TagItem.DrawTooltips(new ReadOnlyCollection<TooltipLine>(new List<TooltipLine>()), list, Main.mouseX,
            Main.mouseY + 10);
    }

    private Chest GetBank() =>
        _bankType switch
        {
            BankType.Piggy => Main.LocalPlayer.bank,
            BankType.Safe => Main.LocalPlayer.bank2,
            BankType.Forge => Main.LocalPlayer.bank3,
            BankType.Void => Main.LocalPlayer.bank4,
            _ => throw new ArgumentOutOfRangeException()
        };

    private Asset<Texture2D> GetIcon() =>
        _bankType switch
        {
            BankType.Piggy => ModAsset.Slot_Piggy,
            BankType.Safe => ModAsset.Slot_Safe,
            BankType.Forge => ModAsset.Slot_Forge,
            BankType.Void => ModAsset.Slot_Void,
            _ => throw new ArgumentOutOfRangeException()
        };
}