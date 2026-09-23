using ImproveGame.Common.ModHooks;
using ImproveGame.UIFramework.BaseViews;
using ImproveGame.UIFramework.Common;
using ImproveGame.UIFramework.UIElements;
using Terraria.GameContent.UI.Chat;
using Terraria.UI.Chat;

namespace ImproveGame.UIFramework.SUIElements
{
    /// <summary>
    /// 用于操作数组中的物品，初始化的时候必须给定数组 + 下标。
    /// </summary>
    public class BigBagItemSlot : View
    {
        /// <summary>
        /// 新物品计时器
        /// </summary>
        public AnimationTimer NewAndShinyTimer = new AnimationTimer(3);

        /// <summary>
        /// 收藏物品计时器
        /// </summary>
        public AnimationTimer FavoritedTimer = new AnimationTimer(3);

        public bool FavoriteAllowed = true;

        protected bool DrawStack = true;

        public readonly int Index;
        public Item[] Items { get; set; }

        public Item Item
        {
            get => Items[Index];
            private set => Items[Index] = value;
        }

        protected int RightMouseDownTimer = -1;
        protected int SuperFastStackTimer;

        public Action FastStackAction;

        public BigBagItemSlot(Item[] items, int index)
        {
            Width.Pixels = 52;
            Height.Pixels = 52;
            Items = items;
            Index = index;

            RelativeMode = RelativeMode.Horizontal;
            Spacing = new Vector2(10, 10);
            PreventOverflow = true;
            Border = UIStyle.ItemSlotBorderSize;
            Rounded = new Vector4(UIStyle.ItemSlotBorderRound);
        }

        /// <summary> 为了使该类的子类可以越过该类的 RightMouseDown 而直接调用 UIElement 的 RightMouseDown </summary>
        protected void CallBaseLeftMouseDown(UIMouseEvent evt) => base.LeftMouseDown(evt);

        public override void LeftMouseDown(UIMouseEvent evt)
        {
            if (!Interactable)
                return;

            SetCursorOverride();
            MouseClickSlot();
            base.LeftMouseDown(evt);
        }

        /// <summary> 为了使该类的子类可以越过该类的 RightMouseDown 而直接调用 UIElement 的 RightMouseDown </summary>
        protected void CallBaseRightMouseDown(UIMouseEvent evt) => base.RightMouseDown(evt);

        public override void RightMouseDown(UIMouseEvent evt)
        {
            if (!Interactable || Item.IsAir)
                return;

            if (ItemLoader.CanRightClick(Item))
            {
                Main.mouseRightRelease = true;

                if (Main.ItemDropsDB.GetRulesForItemID(Item.type).Count != 0)
                    ItemSlot.TryOpenContainer([Item], 0, 0, Main.LocalPlayer);
                else
                    ItemLoader.RightClick(Item, Main.LocalPlayer);

                Main.mouseRightRelease = false;
                return;
            }

            RightMouseDownTimer = 0;
            SuperFastStackTimer = 0;

            base.RightMouseDown(evt);
        }

        /// <summary> 为了使该类的子类可以越过该类的 Update 而直接调用 UIElement 的 Update </summary>
        protected void CallBaseUpdate(GameTime gameTime) => base.Update(gameTime);

        public override void Update(GameTime gameTime)
        {
            CallBaseUpdate(gameTime);

            // 右键长按物品持续拿出
            if (Interactable && Main.mouseRight && IsMouseHovering && !Item.IsAir && !ItemID.Sets.BossBag[Item.type] &&
                !ItemID.Sets.IsFishingCrate[Item.type] && !ItemLoader.CanRightClick(Item))
            {
                DoFastStackLogic(TakeSlotItemToMouseItem);
            }
        }

        protected void DoFastStackLogic(Action<int> stackCallback)
        {
            switch (RightMouseDownTimer)
            {
                case >= 60:
                case >= 30 when RightMouseDownTimer % 3 == 0:
                case >= 15 when RightMouseDownTimer % 6 == 0:
                case 1:
                    int stack = SuperFastStackTimer + 1;
                    stack = Math.Min(stack, Item.stack);
                    stackCallback(stack);
                    FastStackAction?.Invoke();
                    break;
            }

            if (RightMouseDownTimer >= 60 && RightMouseDownTimer % 2 == 0 && SuperFastStackTimer < 40)
                SuperFastStackTimer++;

            RightMouseDownTimer++;
        }

        /// <summary>
        /// 拿物品槽内物品到鼠标物品上
        /// </summary>
        protected void TakeSlotItemToMouseItem(int stack)
        {
            if (((Main.mouseItem.type != Item.type || !ItemLoader.CanStack(Main.mouseItem, Item)) &&
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
                    Item.TurnToAir();
                SoundEngine.PlaySound(SoundID.MenuTick);
            }
        }

        /// <summary>
        /// 改原版的 <see cref="Main.cursorOverride"/>
        /// </summary>
        protected void SetCursorOverride()
        {
            if (Item.IsAir)
            {
                return;
            }

            if (!Item.favorited && ItemSlot.ShiftInUse)
            {
                Main.cursorOverride = CursorOverrideID.ChestToInventory; // 快捷放回物品栏图标
            }

            if (Main.keyState.IsKeyDown(Main.FavoriteKey))
            {
                if (FavoriteAllowed)
                    Main.cursorOverride = CursorOverrideID.FavoriteStar; // 收藏图标
                if (Main.drawingPlayerChat)
                    Main.cursorOverride = CursorOverrideID.Magnifiers; // 放大镜图标 - 输入到聊天框
            }

            void TryTrashCursorOverride()
            {
                if (Item.favorited)
                {
                    return;
                }

                Main.cursorOverride = Main.npcShop <= 0
                    ? CursorOverrideID.TrashCan
                    : CursorOverrideID.QuickSell; // 垃圾箱图标
            }

            if (ItemSlot.ControlInUse && ItemSlot.Options.DisableLeftShiftTrashCan && !ItemSlot.ShiftForcedOn)
            {
                TryTrashCursorOverride();
            }
            // 如果左Shift快速丢弃打开了，按原版物品栏的物品应该是丢弃，但是我们这应该算箱子物品，所以不丢弃
            //if (!ItemSlot.Options.DisableLeftShiftTrashCan && ItemSlot.ShiftInUse) {
            //    TryTrashCursorOverride();
            //}
        }

        /// <summary>
        /// 左键点击物品
        /// </summary>
        protected void MouseClickSlot()
        {
            bool result = false;

            if (Item.ModItem is IItemOverrideLeftClick iItemOverrideLeftClick)
                result |= iItemOverrideLeftClick.OverrideLeftClick(Items, 114514, Index);

            if (result)
                return;

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
                    ModItemSlot.SellOrTrash(temp, ItemSlot.Context.InventoryItem, 0);
                    return;
                // 放回物品栏图标
                case CursorOverrideID.ChestToInventory:
                    Item = Main.LocalPlayer.GetItem(Item,
                        GetItemSettings.QuickTransferFromSlot);
                    SoundEngine.PlaySound(SoundID.Grab);
                    return;
            }

            if (ItemSlot.ShiftInUse)
                return;

            // 常规单点
            if (Item.IsAir)
            {
                if (Main.mouseItem.IsAir)
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
                        if (Item.stack < Item.maxStack)
                        {
                            if (Item.stack + Main.mouseItem.stack <= Item.maxStack)
                            {
                                Item.stack += Main.mouseItem.stack;
                                Main.mouseItem.TurnToAir();
                            }
                            else
                            {
                                Main.mouseItem.stack -= Item.maxStack - Item.stack;
                                Item.stack = Item.maxStack;
                            }
                        }
                        else if (Main.mouseItem.stack < Main.mouseItem.maxStack)
                        {
                            if (Main.mouseItem.stack + Item.stack <= Main.mouseItem.maxStack)
                            {
                                Main.mouseItem.stack += Item.stack;
                                Item.TurnToAir();
                            }
                            else
                            {
                                Item.stack -= Main.mouseItem.maxStack - Main.mouseItem.stack;
                                Main.mouseItem.stack = Main.mouseItem.maxStack;
                            }
                        }
                        else
                        {
                            (Main.mouseItem, Item) = (Item, Main.mouseItem);
                        }
                    }
                    else
                    {
                        (Item, Main.mouseItem) = (Main.mouseItem, Item);
                    }

                    SoundEngine.PlaySound(SoundID.Grab);
                }
            }
        }

        public virtual void ModifyDrawColor() { }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (IsMouseHovering)
            {
                Item.newAndShiny = false;
            }

            if (Item.newAndShiny)
            {
                NewAndShinyTimer.Open();
            }
            else
            {
                NewAndShinyTimer.Close();
            }

            if (Item.favorited && !Item.IsAir)
            {
                FavoritedTimer.Open();
            }
            else
            {
                FavoritedTimer.Close();
            }

            NewAndShinyTimer.Update();
            FavoritedTimer.Update();

            base.Draw(spriteBatch);
        }

        public override void DrawSelf(SpriteBatch sb)
        {
            BorderColor = FavoritedTimer.Lerp(UIStyle.ItemSlotBorder, UIStyle.ItemSlotBorderFav);
            BgColor = FavoritedTimer.Lerp(UIStyle.ItemSlotBg, UIStyle.ItemSlotBgFav);

            BorderColor = NewAndShinyTimer.Lerp(BorderColor, new Color(99, 161, 157, 180));
            BgColor = NewAndShinyTimer.Lerp(BgColor, new Color(55, 93, 131, 180));
            if (!Interactable)
            {
                BgColor = Color.Gray * 0.3f;
            }

            ModifyDrawColor();
            base.DrawSelf(sb);
            Vector2 pos = GetDimensions().Position();
            float size = GetDimensions().Size().X;
            if (Item.IsAir)
                return;

            if (IsMouseHovering && Interactable)
            {
                PlayerLoader.HoverSlot(Main.LocalPlayer, Items, ItemSlot.Context.InventoryItem, Index);
                Main.hoverItemName = Item.Name;
                Main.HoverItem = Item.Clone();
                SetCursorOverride();
            }

            DrawItemIcon(sb, Item, Color.White, GetDimensions(), size * 0.6154f);
            if (Item.stack <= 1 || !DrawStack)
            {
                return;
            }

            Vector2 textSize = FontAssets.ItemStack.Value.MeasureString(Item.stack.ToString()) * 0.75f;
            Vector2 textPos = pos + new Vector2(size * 0.16f, (size - textSize.Y) * 0.92f);
            sb.DrawItemStackString(Item.stack.ToString(), textPos, GetDimensions().Width * 0.016f);
            // 这段是直接绘制文字，不带边框的，留到这里防止忘了咋写。
            /*DynamicSpriteFontExtensionMethods.DrawString(
                    sb,
                    FontAssets.ItemStack.Value,
                    Item.stack.ToString(),
                    textPos,
                    Color.White,
                    0f,
                    new Vector2(0),
                    0.75f, 0, 0f);*/
        }

        public static void DrawItemIcon(SpriteBatch sb, Item item, Color lightColor, CalculatedStyle dimensions,
            float maxSize = 32f, float itemScale = 1f)
        {
            item.DrawIcon(sb, lightColor, dimensions.Center(), maxSize, itemScale);
        }

        public static void DrawItemIcon(SpriteBatch sb, Texture2D texture2D, Color color, CalculatedStyle dimensions,
            float maxSize = 32f)
        {
            Vector2 size = texture2D.Size();
            float scale = size.X > maxSize || size.Y > maxSize
                ? size.X > size.Y ? maxSize / size.X : maxSize / size.Y
                : 1f;
            Vector2 position = dimensions.Center();
            Vector2 origin = size / 2;
            sb.Draw(texture2D, position, null, color, 0f, origin, scale, SpriteEffects.None, 0f);
        }

        public static void OpenItemGlow(SpriteBatch sb)
        {
            Effect effect = ModAsset.Transform.Value;
            effect.Parameters["uTime"].SetValue(Main.GlobalTimeWrappedHourly * 0.2f);
            effect.CurrentTechnique.Passes["EnchantedPass"].Apply();
            Main.instance.GraphicsDevice.Textures[1] = MyUtils.GetTexture("Enchanted").Value; // 传入调色板
            sb.ReBegin(effect, Main.spriteBatch.transformMatrix);
        }

        /// <summary>
        /// 是否可交互，否则不能执行左右键操作
        /// </summary>
        public virtual bool Interactable => !Main.LocalPlayer.ItemAnimationActive;
    }
}