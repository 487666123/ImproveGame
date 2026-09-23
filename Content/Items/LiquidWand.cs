using ImproveGame.Common.Conditions;
using ImproveGame.Common.GlobalItems;
using ImproveGame.Common.ModHooks;
using ImproveGame.Common.ModPlayers;
using ImproveGame.Common.ModSystems;
using ImproveGame.UI.LiquidWandUI;
using Terraria.GameInput;

namespace ImproveGame.Content.Items
{
    public class LiquidWand : SelectorItem, IItemOverrideHover, IItemMiddleClickable, IConditionItem
    {
        public Condition UseCondition => ConfigCondition.AvailableLiquidWandC;
        public bool IsAdvancedWand;
        public bool[] CanInfiniteUse = new bool[4];

        /// <summary>
        /// 多人同步用，因为WandSystem.LiquidMode是static，在服务器用会有Bug
        /// </summary>
        public short LiquidMode = 0;

        public bool AbsorptionMode;

        // 修改物块
        public override void PostModifyTiles(Player player, int minI, int minJ, int maxI, int maxJ)
        {
            for (int j = minJ; j <= maxJ; j++)
            {
                for (int i = minI; i <= maxI; i++)
                {
                    WorldGen.SquareTileFrame(i, j, resetFrame: false);
                    if (Main.netMode is NetmodeID.Server)
                        NetMessage.sendWater(i, j);
                    else
                        Liquid.AddWater(i, j);
                }
            }
        }

        // 修改 Box 颜色
        public override Color ModifyColor(bool cancelled)
        {
            if (cancelled)
                return base.ModifyColor(true);
            if (WandSystem.AbsorptionMode)
                return new Color(188, 188, 188);
            float lerpFactor = Main.GlobalTimeWrappedHourly * 0.1f % 1f;
            // 呼吸灯
            var shimmerLight = new Color(153, 135, 198);
            var shimmerDark = new Color(135, 98, 192);
            var shimmerColor = lerpFactor < 0.5f
                ? Color.Lerp(shimmerLight, shimmerDark, lerpFactor * 2f)
                : Color.Lerp(shimmerLight, shimmerDark, (1f - lerpFactor) * 2f);
            return WandSystem.LiquidMode switch
            {
                LiquidID.Lava => new Color(253, 32, 3),
                LiquidID.Honey => new Color(255, 156, 12),
                LiquidID.Shimmer => shimmerColor,
                _ => new Color(9, 61, 191),
            };
        }

        public override bool IsNeedKill() => !Main.mouseLeft;

        // 修改选择的方块
        public override bool ModifySelectedTiles(Player player, int i, int j)
        {
            // 单人模式下直接设置成WandSystem.LiquidMode，物品的LiquidMode是给多人用的
            if (Main.netMode is not NetmodeID.Server)
            {
                LiquidMode = WandSystem.LiquidMode;
                AbsorptionMode = WandSystem.AbsorptionMode;
            }

            Tile t = Main.tile[i, j];
            var dataPlayer = DataPlayer.Get(player);

            void AddToStorageWithCap(ref int storage)
            {
                storage = Math.Min(storage + t.LiquidAmount, DataPlayer.LiquidCap);
                t.LiquidAmount = 0;
            }

            void AddToLiquidWithCap(ref int storage, int requiredAmount)
            {
                if (IsAdvancedWand || CanInfiniteUse[t.LiquidType])
                {
                    t.LiquidAmount = byte.MaxValue;
                    return;
                }

                byte usedAmount = (byte)Math.Min(storage, requiredAmount);
                t.LiquidAmount += usedAmount;
                storage -= usedAmount;
            }

            // 吸收模式
            if (AbsorptionMode)
            {
                if (t.LiquidAmount <= 0)
                    return true;

                switch (t.LiquidType)
                {
                    case LiquidID.Water:
                        AddToStorageWithCap(ref dataPlayer.LiquidWandWater);
                        break;
                    case LiquidID.Lava:
                        AddToStorageWithCap(ref dataPlayer.LiquidWandLava);
                        break;
                    case LiquidID.Honey:
                        AddToStorageWithCap(ref dataPlayer.LiquidWandHoney);
                        break;
                    case LiquidID.Shimmer:
                        if (IsAdvancedWand)
                            t.LiquidAmount = 0;
                        break;
                }
            }
            // 放置模式
            else if (!t.HasTile || !WorldGen.SolidTile(new Point(i, j)))
            {
                if (t.LiquidAmount > 0 && t.LiquidType != LiquidMode)
                    return true;

                t.LiquidType = LiquidMode; // 设置成相应的液体
                int shimmer = 255; // 无实际作用，仅用于给 ref 提供变量
                int required = byte.MaxValue - t.LiquidAmount;

                switch (t.LiquidType)
                {
                    case LiquidID.Water:
                        AddToLiquidWithCap(ref dataPlayer.LiquidWandWater, required);
                        break;
                    case LiquidID.Lava:
                        AddToLiquidWithCap(ref dataPlayer.LiquidWandLava, required);
                        break;
                    case LiquidID.Honey:
                        AddToLiquidWithCap(ref dataPlayer.LiquidWandHoney, required);
                        break;
                    case LiquidID.Shimmer:
                        AddToLiquidWithCap(ref shimmer, required);
                        break;
                }
            }

            return true;
        }

        private static bool BucketExists(int bucketId) => MyUtils.LocalPlayerHasItemFast(bucketId);

        public override bool CanUseItem(Player player)
        {
            if (player.noBuilding)
                return false;
            return base.CanUseItem(player);
        }

        public override bool AltFunctionUse(Player player) => true;

        public override void SetItemDefaults()
        {
            Item.rare = ItemRarityID.Red;
            Item.value = Item.sellPrice(0, 3);
            // Item.mana = 20;

            SelectRange = new(30, 30);
            RunOnServer = true;
            MaxTilesPerFrame = 9999;
        }

        public override bool StartUseItem(Player player)
        {
            switch (player.altFunctionUse)
            {
                case 0:
                    MyUtils.ItemRotation(player);
                    break;
                case 2:
                    return false;
            }

            return base.StartUseItem(player);
        }

        public override void HoldItem(Player player)
        {
            if (Main.dedServ || Main.myPlayer != player.whoAmI)
                return;

            CanInfiniteUse[LiquidID.Water] = BucketExists(ItemID.BottomlessBucket);
            CanInfiniteUse[LiquidID.Lava] = BucketExists(ItemID.BottomlessLavaBucket);
            CanInfiniteUse[LiquidID.Honey] = BucketExists(ItemID.BottomlessHoneyBucket);
            CanInfiniteUse[LiquidID.Shimmer] = true;

            if (!IsAdvancedWand && WandSystem.LiquidMode is LiquidID.Shimmer)
            {
                WandSystem.LiquidMode = LiquidID.Water;
            }

            player.cursorItemIconEnabled = true;
            player.cursorItemIconPush = 6;
            player.cursorItemIconID = WandSystem.LiquidMode switch
            {
                LiquidID.Water => ItemID.BottomlessBucket,
                LiquidID.Lava => ItemID.BottomlessLavaBucket,
                LiquidID.Honey => ItemID.BottomlessHoneyBucket,
                LiquidID.Shimmer => ItemID.BottomlessShimmerBucket,
                _ => 0
            };

            if (WandSystem.AbsorptionMode)
                player.cursorItemIconID = ItemID.UltraAbsorbantSponge;

            // 还在用物品的时候不能打开UI
            if (player.mouseInterface || player.itemAnimation > 0 || !Main.mouseRight || !Main.mouseRightRelease ||
                Main.SmartInteractShowingGenuine || PlayerInput.LockGamepadTileUseButton || player.noThrow != 0 ||
                Main.HoveringOverAnNPC || player.talkNPC != -1)
            {
                return;
            }

            if (!LiquidWandUI.Instance.Enabled)
            {
                LiquidWandUI.Instance.Open(this);
            }
            else
            {
                LiquidWandUI.Instance.Close();
            }
        }

        public bool OverrideHover(Item[] inventory, int context, int slot)
        {
            ((IItemMiddleClickable)this).HandleHover(inventory, context, slot);

            return false;
        }

        public void OnMiddleClicked(Item item)
        {
            if (!LiquidWandUI.Instance.Enabled)
                LiquidWandUI.Instance.Open(this);
            else
                LiquidWandUI.Instance.Close();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            ((IItemMiddleClickable)this).HandleTooltips(Item, tooltips);
        }

        public void ManageHoverTooltips(Item item, List<TooltipLine> tooltips)
        {
            // 决定文本显示的是“开启”还是“关闭”
            MyUtils.TryGetKeybindString(KeybindSystem.ItemInteractKeybind, out string keybind);
            string tooltip = MyUtils.GetTextWith("Tips.LiquidWandOn", new { KeybindName = keybind });
            if (LiquidWandUI.Instance.Enabled)
                tooltip = MyUtils.GetTextWith("Tips.LiquidWandOff", new { KeybindName = keybind });

            tooltips.Add(new TooltipLine(Mod, "LiquidWand", tooltip) { Color = Color.LightGreen });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddRecipeGroup(RecipeGroups.Wood, 24)
                .AddRecipeGroup(RecipeSystem.AnyShadowScale, 8)
                .AddRecipeGroup(RecipeSystem.AnyGoldBar, 6)
                .AddTile(TileID.Anvils)
                .Register();
        }

        public override void NetSend(BinaryWriter writer)
        {
            LiquidMode = WandSystem.LiquidMode;
            AbsorptionMode = WandSystem.AbsorptionMode;
            writer.Write(LiquidMode);
            writer.Write(new BitsByte(CanInfiniteUse[0], CanInfiniteUse[1], CanInfiniteUse[2], CanInfiniteUse[3],
                AbsorptionMode));
        }

        public override void NetReceive(BinaryReader reader)
        {
            LiquidMode = reader.ReadInt16();
            var bitsByte = (BitsByte)reader.ReadByte();
            CanInfiniteUse[0] = bitsByte[0];
            CanInfiniteUse[1] = bitsByte[1];
            CanInfiniteUse[2] = bitsByte[2];
            CanInfiniteUse[3] = bitsByte[3];
            AbsorptionMode = bitsByte[4];
        }
    }
}