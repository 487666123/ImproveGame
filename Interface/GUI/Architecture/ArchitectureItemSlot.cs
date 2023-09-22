using ImproveGame.Common.Animations;
using ImproveGame.Common.ModHooks;
using ImproveGame.Common.Players;
using ImproveGame.Content.Items;
using ImproveGame.Interface.Common;
using ImproveGame.Interface.GUI.BannerChest;
using ImproveGame.Interface.SUIElements;
using Terraria.GameContent.UI.Chat;
using Terraria.ModLoader.UI;
using Terraria.UI.Chat;

namespace ImproveGame.Interface.GUI.Architecture;

public abstract class ArchitectureItemSlot : View
{
    internal Item Item
    {
        get => GetItem();
        set => SetItem(value);
    }

    protected CreateWand Wand => ArchitectureGUI.Wand;
    private readonly Texture2D _textureWhenEmpty;
    private int _rightMouseTimer;
    private int _superFastStackTimer;

    public abstract Item GetItem();

    public abstract void SetItem(Item item);

    public abstract bool CheckCanPlace(Item item);

    public ArchitectureItemSlot(Texture2D textureWhenEmpty)
    {
        _textureWhenEmpty = textureWhenEmpty;
        SetSizePixels(48, 48);
        SetRoundedRectangleValues(UIColor.ItemSlotBg * 0.85f, 2f, UIColor.ItemSlotBorder * 0.85f, new Vector4(10f));

        Spacing = new Vector2(8);
        Relative = RelativeMode.Horizontal;
        Rounded = new Vector4(12f);
        BgColor = UIColor.ItemSlotBg;
        Border = 2f;
        BorderColor = UIColor.ItemSlotBorder;
    }

    public override void LeftMouseDown(UIMouseEvent evt)
    {
        base.LeftMouseDown(evt);

        if (Main.LocalPlayer.ItemAnimationActive)
            return;

        if (Wand is null || Wand.Item.IsAir)
            return;

        SetCursor();
        MouseClickSlot();
    }

    public override void RightMouseDown(UIMouseEvent evt)
    {
        base.RightMouseDown(evt);
        if (Wand is null || Wand.Item.IsAir)
            return;
        if (Item.IsAir)
            return;
        _rightMouseTimer = 0;
        _superFastStackTimer = 0;
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        // 右键长按物品持续拿出
        if (!Main.mouseRight || !IsMouseHovering || Item.IsAir)
        {
            return;
        }

        switch (_rightMouseTimer)
        {
            case >= 60:
            case >= 30 when _rightMouseTimer % 3 == 0:
            case >= 15 when _rightMouseTimer % 6 == 0:
            case 1:
                int stack = _superFastStackTimer + 1;
                stack = Math.Min(stack, Item.stack);
                TakeSlotItemToMouseItem(stack);
                break;
        }

        if (_rightMouseTimer >= 60 && _rightMouseTimer % 2 == 0 && _superFastStackTimer < 40)
            _superFastStackTimer++;

        _rightMouseTimer++;
    }

    public override void DrawSelf(SpriteBatch sb)
    {
        base.DrawSelf(sb);
        CalculatedStyle dimensions = GetDimensions();

        if (Wand is null || Wand.Item.IsAir || Item.IsAir)
        {
            Vector2 pos = dimensions.Position();
            Vector2 size = dimensions.Size();
            sb.Draw(_textureWhenEmpty, pos + size / 2f, null, Color.White * 0.2f, 0f,
                _textureWhenEmpty.Size() / 2f, 1f, 0, 0);
            return;
        }

        BigBagItemSlot.DrawItemIcon(sb, Item, Color.White, dimensions);

        if (!Item.IsAir && Item.stack > 1)
        {
            Vector2 textSize = GetFontSize(Item.stack) * 0.75f;
            Vector2 textPos = dimensions.Position() + new Vector2(52 * 0.18f, (52 - textSize.Y) * 0.9f);
            TrUtils.DrawBorderString(sb, Item.stack.ToString(), textPos, Color.White, 0.75f);
        }

        if (!IsMouseHovering)
        {
            return;
        }

        Main.hoverItemName = Item.Name;
        Main.HoverItem = Item.Clone();
        SetCursor();
    }

    /// <summary>
    /// 拿物品槽内物品到鼠标物品上
    /// </summary>
    protected void TakeSlotItemToMouseItem(int stack)
    {
        if (((!Main.mouseItem.IsTheSameAs(Item) || !ItemLoader.CanStack(Main.mouseItem, Item)) &&
             Main.mouseItem.type is not ItemID.None) || (Main.mouseItem.stack >= Main.mouseItem.maxStack &&
                                                         Main.mouseItem.type is not ItemID.None))
        {
            return;
        }

        if (Main.mouseItem.type is ItemID.None)
        {
            Main.mouseItem = ItemLoader.TransferWithLimit(Item, stack);
            ItemSlot.AnnounceTransfer(new ItemSlot.ItemTransferInfo(Item, ItemSlot.Context.InventoryItem,
                ItemSlot.Context.MouseItem));
            SoundEngine.PlaySound(SoundID.MenuTick);
        }
        else
        {
            ItemLoader.StackItems(Main.mouseItem, Item, out _, numToTransfer: stack);
            if (Item.stack <= 0)
                Item.SetDefaults();
            SoundEngine.PlaySound(SoundID.MenuTick);
        }
    }

    private static void SetCursor()
    {
        // 快速取出
        if (ItemSlot.ShiftInUse)
        {
            Main.cursorOverride = CursorOverrideID.ChestToInventory; // 快捷放回物品栏图标
        }

        // 放入聊天框
        if (Main.keyState.IsKeyDown(Main.FavoriteKey) && Main.drawingPlayerChat)
        {
            Main.cursorOverride = CursorOverrideID.Magnifiers;
        }
    }

    /// <summary>
    /// 左键点击物品
    /// </summary>
    protected void MouseClickSlot()
    {
        switch (Main.cursorOverride)
        {
            // 放大镜图标 - 输入到聊天框
            case CursorOverrideID.Magnifiers:
                {
                    if (ChatManager.AddChatText(FontAssets.MouseText.Value, ItemTagHandler.GenerateTag(Item),
                            Vector2.One))
                        SoundEngine.PlaySound(SoundID.MenuTick);
                    return;
                }
            // 收藏图标
            case CursorOverrideID.FavoriteStar:
                Item.favorited = !Item.favorited;
                SoundEngine.PlaySound(SoundID.MenuTick);
                return;
            // 垃圾箱图标
            case CursorOverrideID.TrashCan:
            case CursorOverrideID.QuickSell:
                // 假装自己是一个物品栏物品
                var temp = new Item[1];
                temp[0] = Item;
                ItemSlot.SellOrTrash(temp, ItemSlot.Context.InventoryItem, 0);
                return;
            // 放回物品栏图标
            case CursorOverrideID.ChestToInventory:
                Item = Main.player[Main.myPlayer].GetItem(Main.myPlayer, Item,
                    GetItemSettings.InventoryEntityToPlayerInventorySettings);
                SoundEngine.PlaySound(SoundID.Grab);
                return;
        }

        if (ItemSlot.ShiftInUse)
            return;

        // 常规单点
        if (Item.IsAir)
        {
            if (Main.mouseItem.IsAir || !CheckCanPlace(Main.mouseItem))
            {
                return;
            }

            Item = Main.mouseItem;
            Main.mouseItem = new Item();
            SoundEngine.PlaySound(SoundID.Grab);
        }
        else
        {
            if (Main.mouseItem.IsAir)
            {
                Main.mouseItem = Item;
                Item = new Item();
                SoundEngine.PlaySound(SoundID.Grab);
            }
            else
            {
                // 同种物品
                if (Main.mouseItem.type == Item.type)
                {
                    ItemLoader.StackItems(Item, Main.mouseItem, out _);
                    if (Item.stack <= 0)
                        Item.SetDefaults();
                }
                else if (CheckCanPlace(Main.mouseItem))
                {
                    (Item, Main.mouseItem) = (Main.mouseItem, Item);
                }

                SoundEngine.PlaySound(SoundID.Grab);
            }
        }
    }
}