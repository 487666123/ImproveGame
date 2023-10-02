using ImproveGame.Common.Configs;
using ImproveGame.Common.ModSystems;
using ImproveGame.Common.Packets;
using ImproveGame.Entitys;
using ImproveGame.Interface.Common;
using ImproveGame.Interface.GUI;
using Terraria.GameInput;
using Terraria.ID;

namespace ImproveGame.Content.Items
{
    public class MagickWand : SelectorItem
    {
        public override bool IsLoadingEnabled(Mod mod) => Config.LoadModItems.MagickWand;

        public override bool ModifySelectedTiles(Player player, int i, int j)
        {
            if (UIConfigs.Instance.ExplosionEffect)
            {
                SoundEngine.PlaySound(SoundID.Item14, Main.MouseWorld);
                BongBong(new Vector2(i, j) * 16f, 16, 16);
            }

            if (Main.tile[i, j].WallType > 0 && WandSystem.WallMode)
            {
                WorldGen.KillWall(i, j);
                if (Main.netMode == NetmodeID.MultiplayerClient)
                    NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 2, i, j);
            }

            if (WandSystem.TileMode && Main.tile[i, j].HasTile)
                TryKillTile(i, j, player);
            return true;
        }

        public override void PostModifyTiles(Player player, int minI, int minJ, int maxI, int maxJ)
        {
            DoBoomPacket.Send(TileRect);
            PlaySoundPacket.PlaySound(LegacySoundIDs.Item, Main.MouseWorld, style: 14, false);
        }

        public override bool AltFunctionUse(Player player) => true;

        protected Point ExtraRange;
        protected Point KillSize;

        public override bool IsNeedKill()
        {
            if (WandSystem.FixedMode)
                return false;
            return !Main.mouseLeft;
        }

        public override void SetItemDefaults()
        {
            Item.rare = ItemRarityID.Cyan;
            Item.value = Item.sellPrice(0, 1, 0, 0);

            SelectRange = new(20, 20);
            KillSize = new(5, 3);
            ExtraRange = new(5, 3);
        }

        public override bool StartUseItem(Player player)
        {
            if (player.altFunctionUse == 0)
            {
                FixedModeAction(player);
            }
            else if (player.altFunctionUse == 2)
            {
                return false;
            }

            return base.StartUseItem(player);
        }

        public override float UseSpeedMultiplier(Player player)
        {
            return 1f + (1f - player.pickSpeed);
        }

        public override bool? UseItem(Player player)
        {
            if (player.ItemAnimationJustStarted)
            {
                FixedModeAction(player);
            }

            return base.UseItem(player);
        }

        private void FixedModeAction(Player player)
        {
            if (player.whoAmI == Main.myPlayer && WandSystem.FixedMode)
            {
                Rectangle rectangle = GetRectangle(player);
                // 带同步的音效
                PlaySoundPacket.PlaySound(LegacySoundIDs.Item, Main.MouseWorld, style: 14);
                ForeachTile(rectangle, (x, y) =>
                {
                    if (Main.tile[x, y].WallType > 0 && WandSystem.WallMode)
                    {
                        if (player.statMana < 1)
                            player.QuickMana();
                        if (player.statMana >= 1)
                        {
                            WorldGen.KillWall(x, y);
                            if (Main.netMode == NetmodeID.MultiplayerClient)
                                NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 2, x, y);
                            player.statMana -= 1;
                        }
                    }

                    if (player.statMana < 2)
                        player.QuickMana();
                    if (player.statMana >= 2)
                    {
                        if (WandSystem.TileMode && Main.tile[x, y].HasTile && TryKillTile(x, y, player))
                        {
                            player.statMana -= 2;
                        }
                    }

                    BongBong(new Vector2(x, y) * 16f, 16, 16);
                }, (x, y, wid, hei) => DoBoomPacket.Send(x, y, wid, hei)); // 同步爆炸特效
            }
        }

        public override bool CanUseSelector(Player player)
        {
            return !WandSystem.FixedMode;
        }

        public override void HoldItem(Player player)
        {
            if (!Main.dedServ && Main.myPlayer == player.whoAmI)
            {
                if (WandSystem.FixedMode)
                {
                    GameRectangle.Create(this, () => !WandSystem.FixedMode, GetRectangle(player), Color.Red * 0.35f,
                        Color.Red);
                }

                // 我给他移动到 CanUseItem 中
                // 还在用物品的时候不能打开UI (直接写在 CanUseItem 似乎就没有问题了)
                if (player.itemAnimation > 0 || !Main.mouseRight || !Main.mouseRightRelease ||
                    Main.SmartInteractShowingGenuine || PlayerInput.LockGamepadTileUseButton || player.noThrow != 0 ||
                    Main.HoveringOverAnNPC || player.talkNPC != -1)
                {
                    return;
                }
            }
        }

        public override bool CanUseItem(Player player)
        {
            if (player.noBuilding)
                return false;

            if (player.altFunctionUse == 2)
            {
                if (BrustGUI.Visible && UISystem.Instance.BrustGUI.Timer.AnyOpen)
                    UISystem.Instance.BrustGUI.Close();
                else
                    UISystem.Instance.BrustGUI.Open();
                return false;
            }

            return base.CanUseItem(player);
        }

        protected Rectangle GetRectangle(Player player)
        {
            Rectangle rect = new();
            Point playerCenter = player.Center.ToTileCoordinates();
            Point mousePosition = Main.MouseWorld.ToTileCoordinates();
            mousePosition = ModifySize(playerCenter, mousePosition, Player.tileRangeX + ExtraRange.X,
                Player.tileRangeY + ExtraRange.Y);
            rect.X = mousePosition.X - KillSize.X / 2;
            rect.Y = mousePosition.Y - KillSize.Y / 2;
            rect.Width = KillSize.X;
            rect.Height = KillSize.Y;
            return rect;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddRecipeGroup(RecipeGroupID.Wood, 18)
                .AddIngredient(ItemID.JungleSpores, 6)
                .AddIngredient(ItemID.Ruby, 1)
                .AddTile(TileID.WorkBenches).Register();
        }
    }
}