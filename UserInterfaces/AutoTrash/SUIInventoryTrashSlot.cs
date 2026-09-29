using ImproveGame.UI.AutoTrash;
using SilkyUIFramework;
using SilkyUIFramework.Elements;

namespace ImproveGame.UserInterfaces.AutoTrash;

public sealed class SUIInventoryTrashSlot : SUIItemSlot
{
    private readonly IList<Item> _items;
    private readonly int _index;

    public override Item Item
    {
        get => _index < _items.Count ? _items[_index] : AirItem;
        set
        {
            while (_items.Count <= _index)
                _items.Add(new Item());

            _items[_index] = value ?? new Item();
        }
    }

    private static Item AirItem => new();

    public SUIInventoryTrashSlot(IList<Item> items, int index)
    {
        _items = items;
        _index = index;
        DisplayItemStack = false;
        ItemScale = 0.85f;
        Border = 2f;
        BorderRadius = new Vector4(12f * 0.8f);
        BorderColor = new Color(28, 28, 28) * 0.8f;
        BackgroundColor = new Color(84, 115, 130) * 0.8f;
        SetSize(44f, 44f);
    }

    protected override void HandleItemSlotLeftClick()
    {
        if (PlayerInUseItem) return;

        var player = Main.LocalPlayer.GetModPlayer<AutoTrashPlayer>();
        if (Main.mouseItem.IsAir)
        {
            if (Item.IsAir) return;

            Main.mouseItem = Item.Clone();
            player.RemoveItem(Item);
            player.CleanUpRecentlyThrownAwayItems();
        }
        else
        {
            player.EnterThrowAwayItems(Main.mouseItem);
            player.EnterRecentlyThrownAwayItems(Main.mouseItem);
            Main.mouseItem.TurnToAir();
        }

        SoundEngine.PlaySound(SoundID.Grab);
    }

    protected override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        base.Draw(gameTime, spriteBatch);

        if (Item.IsAir)
        {
            var texture = ModAsset.Trash.Value;
            var position = InnerBounds.Position + (Vector2)InnerBounds.Size * 0.5f;
            spriteBatch.Draw(texture, position, null, Color.White * 0.5f, 0f,
                texture.Size() * 0.5f, ItemScale, SpriteEffects.None, 0f);
        }
    }
}
