using ImproveGame.UI.AutoTrash;
using SilkyUIFramework;
using SilkyUIFramework.Elements;

namespace ImproveGame.UserInterfaces.AutoTrash;

public class AutoTrashItemSlot : SUIItemSlot
{
    public AutoTrashItemSlot(Item item)
    {
        Item = item;
        ItemInteractive = false;
        DisplayItemStack = false;
        ItemScale = 0.85f;

        SetSize(40, 40);
        BorderRadius = new Vector4(8f);
        BorderColor = SUIColor.Border * 0.75f;
        BackgroundColor = SUIColor.Background * 0.5f;
    }

    protected override void HandleItemSlotLeftClick()
    {
        var player = Main.LocalPlayer.GetModPlayer<AutoTrashPlayer>();
        if (Item.IsAir || !player.ThrowAwayItems.Contains(Item)) return;

        if (player.RecentlyThrownAwayItems.Any(item => item.type == Item.type))
            Main.LocalPlayer.QuickSpawnItem(null, Item.type, Item.stack);

        player.RemoveItem(Item);
        SoundEngine.PlaySound(SoundID.Grab);
    }

    protected override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        ItemIconSizeLimit = 32f * HoverTimer.Lerp(1f, 0.9f);
        ItemOffset = HoverTimer.Lerp(Vector2.Zero,
            new Vector2(-InnerBounds.Width, InnerBounds.Height) * 0.075f);

        base.Draw(gameTime, spriteBatch);

        if (Item.IsAir || HoverTimer.IsReverseCompleted) return;

        var texture = ModAsset.TakeOutFromTrash.Value;
        var position = Bounds.Position + (Vector2)Bounds.Size * new Vector2(2f / 3f, 1f / 3f);
        spriteBatch.Draw(texture, position, null, HoverTimer.Lerp(Color.Transparent, Color.White),
            0f, texture.Size() / 2f, HoverTimer.Lerp(0.5f, 0.65f), SpriteEffects.None, 0f);
    }
}
