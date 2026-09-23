using ImproveGame.Common.RenderTargetContents;
using ImproveGame.Packets.NetStorager;
using ImproveGame.UIFramework.Common;
using ImproveGame.UIFramework.SUIElements;
using Terraria.ModLoader.UI;

namespace ImproveGame.UI.ExtremeStorage
{
    /// <summary>
    /// 继承 BigBagItemSlot，但是有修改，以适合作为箱子的物品槽
    /// </summary>
    public class ItemSlot : BigBagItemSlot
    {
        private bool ChestBeingUsed => Main.netMode is not NetmodeID.SinglePlayer &&
                                       StorageGrids.ChestsThatBeingUsedCache is not null &&
                                       StorageGrids.ChestsThatBeingUsedCache.Contains(_chestIndex) &&
                                       Main.LocalPlayer.chest != _chestIndex;

        private readonly int _chestIndex;

        public ItemSlot(Item[] items, int index, int chestIndex) : base(items, index)
        {
            _chestIndex = chestIndex;
            FavoriteAllowed = false;
            Spacing = new Vector2(4f);
        }

        // 覆写父类的方法，以添加多人相关操作
        public override void LeftMouseDown(UIMouseEvent evt)
        {
            if (!Interactable)
                return;

            CallBaseLeftMouseDown(evt);

            SetCursorOverride();
            switch (Main.netMode)
            {
                case NetmodeID.MultiplayerClient when Main.cursorOverride is CursorOverrideID.Magnifiers:
                case NetmodeID.SinglePlayer:
                    MouseClickSlot();
                    ExtremeStorageGUI.RefreshCachedAllItems();
                    // Recipe.FindRecipes();
                    break;
                case NetmodeID.MultiplayerClient:
                    switch (Main.cursorOverride)
                    {
                        case CursorOverrideID.ChestToInventory:
                            // 左键全部拿取
                            SoundEngine.PlaySound(SoundID.Grab);
                            RemoveItemOperationPacket.Send(_chestIndex, Index,
                                RemoveItemOperationPacket.RemovedItemDestination.Inventory);
                            break;
                        case CursorOverrideID.TrashCan:
                            SoundEngine.PlaySound(SoundID.Grab);
                            RemoveItemOperationPacket.Send(_chestIndex, Index,
                                RemoveItemOperationPacket.RemovedItemDestination.Trash);
                            break;
                        default:
                            Item chestItem = Main.chest[_chestIndex].item[Index];
                            if (Main.mouseItem.IsAir && chestItem.IsAir)
                            {
                                break;
                            }
                            else if (Main.mouseItem.IsAir)
                            {
                                // 拿走
                                RemoveItemOperationPacket.Send(_chestIndex, Index,
                                    RemoveItemOperationPacket.RemovedItemDestination.Mouse);
                            }
                            else if (chestItem.IsAir)
                            {
                                // 放入
                                AddItemOperationPacket.Send(_chestIndex, Index);
                            }
                            else if (Main.mouseItem.type == chestItem.type && chestItem.stack < chestItem.maxStack)
                            {
                                // 放入一部分
                                AddItemOperationPacket.Send(_chestIndex, Index);
                            }
                            else
                            {
                                // 交换
                                SwapItemOperationPacket.Send(_chestIndex, Index);
                            }

                            SoundEngine.PlaySound(SoundID.Grab);

                            break;
                    }

                    break;
            }
        }

        public override void RightMouseUp(UIMouseEvent evt)
        {
            base.RightMouseUp(evt);

            if (Main.netMode is not NetmodeID.SinglePlayer) return;
            ExtremeStorageGUI.RefreshCachedAllItems();
            // Recipe.FindRecipes();
        }

        // 覆写父类的方法，让宝藏袋右键不开启而是拿取
        public override void RightMouseDown(UIMouseEvent evt)
        {
            CallBaseRightMouseDown(evt);

            if (!Interactable)
                return;

            if (Item.IsAir)
                return;

            RightMouseDownTimer = 0;
            SuperFastStackTimer = 0;
        }

        // 覆写父类的方法，让宝藏袋右键不开启而是拿取，以及添加多人相关操作
        public override void Update(GameTime gameTime)
        {
            // base.Update(gameTime);
            // 跳过父类的 Update 方法，直接调用 UIElement 的
            base.CallBaseUpdate(gameTime);

            if (ChestBeingUsed)
                return;

            // 以防万一的设置，虽然可能不需要
            Item.favorited = false;

            // 右键长按物品持续拿出
            if (!Main.mouseRight || !IsMouseHovering || Item.IsAir || !Interactable)
                return;

            DoFastStackLogic(stack =>
            {
                switch (Main.netMode)
                {
                    case NetmodeID.SinglePlayer:
                        TakeSlotItemToMouseItem(stack);
                        // 移动到RightMouseUp中，防止卡顿
                        // ExtremeStorageGUI.RefreshCachedAllItems();
                        // Recipe.FindRecipes();
                        break;
                    // 本地先保证可以拿出物品，然后再发送给服务器
                    case NetmodeID.MultiplayerClient
                        when ((Main.mouseItem.type == Item.type && ItemLoader.CanStack(Main.mouseItem, Item)) ||
                              Main.mouseItem.type is ItemID.None) && (Main.mouseItem.stack < Main.mouseItem.maxStack ||
                                                                      Main.mouseItem.type is ItemID.None):
                    {
                        RemoveItemOperationPacket.Send(_chestIndex, Index,
                            RemoveItemOperationPacket.RemovedItemDestination.Mouse, stack);
                        SoundEngine.PlaySound(SoundID.MenuTick);
                        break;
                    }
                }
            });
        }

        public void ModifySlotColor(ref Color slotColor)
        {
            if (!Interactable || Item.IsAir || !ExtremeStorageGUI.ChestSlotsGlowHue.ContainsKey(_chestIndex)) return;

            var hueArray = ExtremeStorageGUI.ChestSlotsGlowHue[_chestIndex];
            float hue = hueArray[Index];

            if (hue is -1f) return;

            Color color = Main.hslToRgb(hue, 1f, 0.5f);
            float opacity = ExtremeStorageGUI.ChestSlotsGlowTimer / 300f;
            opacity *= opacity;
            slotColor = Color.Lerp(UIStyle.ItemSlotBg, color, opacity / 2f);
        }

        public override void DrawSelf(SpriteBatch sb)
        {
            Vector2 pos = GetDimensions().Position();
            float size = GetDimensions().Size().X;

            // 超出屏幕的不绘制
            if (pos.Y < Main.instance.invBottom || pos.Y > Main.screenHeight)
                return;

            var slotColor = Color.White;
            if (!Interactable)
                slotColor = Color.Gray * 0.3f;

            ModifySlotColor(ref slotColor);
            // ModifyDrawColor();
            // DrawSDFRectangle();

            var slotTarget = RenderTargetContentSystem.ItemSlotTarget;
            slotTarget.Request();
            if (slotTarget.IsReady)
            {
                RenderTarget2D target = slotTarget.GetTarget();
                sb.Draw(target, pos, null, slotColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            }

            if (Item.IsAir)
                return;

            if (IsMouseHovering && Interactable)
            {
                PlayerLoader.HoverSlot(Main.LocalPlayer, Items, Terraria.UI.ItemSlot.Context.InventoryItem, Index);
                Main.hoverItemName = Item.Name;
                Main.HoverItem = Item.Clone();
                SetCursorOverride();
            }

            Rectangle scissorRectangle = sb.GraphicsDevice.ScissorRectangle;
            RasterizerState rasterizerState = sb.GraphicsDevice.RasterizerState;

            DrawItemIcon(sb, Item, Color.White, GetDimensions(), size * 0.6154f);

            // 防止某些mod的不规范绘制炸UI（比如灾厄）
            if (rasterizerState != sb.GraphicsDevice.RasterizerState || scissorRectangle != sb.GraphicsDevice.ScissorRectangle)
            {
                Main.spriteBatch.End();

                sb.GraphicsDevice.ScissorRectangle = scissorRectangle;
                sb.GraphicsDevice.RasterizerState = rasterizerState;

                Main.spriteBatch.Begin(0, BlendState.AlphaBlend,
                    SamplerState.LinearClamp, DepthStencilState.Default,
                    rasterizerState, null, Main.UIScaleMatrix);
            }

            if (Item.stack <= 1)
            {
                return;
            }

            Vector2 textSize = FontAssets.ItemStack.Value.MeasureString(Item.stack.ToString()) * 0.75f;
            Vector2 textPos = pos + new Vector2(size * 0.16f, (size - textSize.Y) * 0.92f);
            sb.DrawItemStackByRenderTarget(Item.stack, textPos, GetDimensions().Width * 0.016f);
            // sb.DrawItemStackString(Item.stack.ToString(), textPos, GetDimensions().Width * 0.016f);

            // base.DrawSelf(sb);
            if (!Interactable && IsMouseHovering)
            {
                UICommon.TooltipMouseText(MyUtils.GetText("UI.ExtremeStorage.ChestBeingUsed"));
            }
        }

        public override bool Interactable => !ChestBeingUsed && !Main.LocalPlayer.ItemAnimationActive;
    }
}