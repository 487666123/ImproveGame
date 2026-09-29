//using ImproveGame.UIFramework.Common;
//using ImproveGame.UIFramework.SUIElements;

//namespace ImproveGame.UI.AutoTrash;

//public class InventoryTrashSlot : GenericItemSlot
//{
//    public static AutoTrashPlayer AutoTrashPlayer =>
//        Main.LocalPlayer.GetModPlayer<AutoTrashPlayer>();

//    public InventoryTrashSlot(IList<Item> items, int index) : base(items, index)
//    {
//        SetBaseItemSlotValues(true, true);
//        SetSizePixels(44, 44);
//        SetRoundedRectProperties(
//            UIStyle.TrashSlotBg,
//            UIStyle.ItemSlotBorderSize,
//            UIStyle.TrashSlotBorder,
//            new Vector4(UIStyle.ItemSlotBorderRound * 0.8f));
//    }

//    public override void LeftMouseDown(UIMouseEvent evt)
//    {
//        base.LeftMouseDown(evt);

//        if (Main.LocalPlayer.ItemAnimationActive) { return; }

//        if (Main.mouseItem.IsAir)
//        {
//            if (!Item.IsAir)
//            {
//                Main.mouseItem = Item.Clone();

//                AutoTrashPlayer.RemoveItem(Item);
//                AutoTrashPlayer.CleanUpRecentlyThrownAwayItems();

//                SoundEngine.PlaySound(SoundID.Grab);
//            }
//        }
//        else
//        {
//            AutoTrashPlayer.EnterThrowAwayItems(Main.mouseItem);
//            AutoTrashPlayer.EnterRecentlyThrownAwayItems(Main.mouseItem);

//            Main.mouseItem.TurnToAir();

//            SoundEngine.PlaySound(SoundID.Grab);
//        }
//    }

//    public override void DrawSelf(SpriteBatch spriteBatch)
//    {
//        base.DrawSelf(spriteBatch);

//        if (Item.IsAir)
//        {
//            Vector2 pos = GetDimensions().Position();
//            Vector2 size = GetDimensionsSize();
//            var texture = ModAsset.Trash.Value;
//            //var opacity = GlassVfxAvailable ? 0.25f : 0.5f;

//            spriteBatch.Draw(texture, pos + size / 2f, null, Color.White * 0.5f, 0f, texture.Size() / 2f, ItemIconScale, 0, 0);
//        }
//    }
//}
