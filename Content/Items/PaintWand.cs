using ImproveGame.Common.Conditions;
using ImproveGame.Common.GlobalItems;
using ImproveGame.Common.ModSystems;
using ImproveGame.UIFramework;
using Terraria.DataStructures;

namespace ImproveGame.Content.Items
{
    public class PaintWand : SelectorItem, IConditionItem
    {
        public Condition UseCondition => ConfigCondition.AvailablePaintWandC;

        public override bool ModifySelectedTiles(Player player, int i, int j)
        {
            var tile = Main.tile[i, j];
            bool tileMode = WandSystem.PaintWandMode is WandSystem.PaintMode.Tile;
            bool wallMode = WandSystem.PaintWandMode is WandSystem.PaintMode.Wall;
            bool removeMode = WandSystem.PaintWandMode is WandSystem.PaintMode.Remove;
            if (removeMode)
            {
                if (tile.HasTile)
                {
                    if (tile.TileColor > 0)
                        WorldGen.paintTile(i, j, 0, broadCast: true);
                    if (tile.IsTileInvisible || tile.IsTileFullbright)
                        WorldGen.paintCoatTile(i, j, 0, broadcast: true);
                }

                if (tile.WallType > 0)
                {
                    if (tile.WallColor > 0)
                        WorldGen.paintWall(i, j, 0, broadCast: true);
                    if (tile.IsWallInvisible || tile.IsWallFullbright)
                        WorldGen.paintCoatWall(i, j, 0, broadcast: true);
                }

                // 漆铲可用于清除图格上的苔藓
                if (tile.TileType != TileID.LongMoss)
                    return true;

                int frameX = tile.TileFrameX;
                WorldGen.KillTile(i, j);
                if (tile.HasTile)
                    return true;

                if (Main.netMode == NetmodeID.MultiplayerClient)
                    NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 0, i, j);

                if (Main.rand.NextBool(4))
                {
                    int type = (frameX / 22) switch
                    {
                        6 => 4377,
                        7 => 4378,
                        8 => 4389,
                        9 => 5127,
                        10 => 5128,
                        _ => 4349 + frameX / 22
                    };

                    int number = Item.NewItem(new EntitySource_ItemUse(player, player.HeldItem), player.Center, new Vector2(16), type);
                    NetMessage.SendData(MessageID.SyncItem, -1, -1, null, number, 1f);
                }
            }
            else
            {
                bool ConsumeCurrentItem(Item item)
                {
                    if (item.stack >= 999)
                        return true;
                    if (ItemLoader.ConsumeItem(item, player))
                        item.stack--;
                    if (item.stack <= 0)
                        item.TurnToAir();
                    return true;
                }

                PickItemInInventory(
                    player,
                    item => item.PaintOrCoating,
                    false,
                    out int index
                );

                if (index == -1)
                    return true;

                var item = player.inventory[index];

                // 原版逻辑
                if (item.paint > 0 && ((tileMode && WorldGen.paintTile(i, j, item.paint, broadCast: true)) ||
                                       (wallMode && WorldGen.paintWall(i, j, item.paint, broadCast: true))))
                    return ConsumeCurrentItem(item);

                if (item.paintCoating > 0 && ((tileMode && WorldGen.paintCoatTile(i, j, item.paintCoating, broadcast: true)) ||
                                              (wallMode && WorldGen.paintCoatWall(i, j, item.paintCoating, broadcast: true))))
                    return ConsumeCurrentItem(item);
            }

            return true;
        }

        public override void HoldItem(Player player)
        {
            PickItemInInventory(
                player,
                item => item.PaintOrCoating,
                false,
                out int index
            );

            if (index == -1)
                return;

            player.cursorItemIconEnabled = true;
            player.cursorItemIconID = player.inventory[index].type;
            player.cursorItemIconPush = 6;
        }

        public override bool IsNeedKill()
        {
            return !Main.mouseLeft;
        }

        public override bool AltFunctionUse(Player player) => true;

        public override void SetItemDefaults()
        {
            Item.rare = ItemRarityID.Red;
            Item.value = Item.sellPrice(0, 1, 0, 0);

            SelectRange = new(1000, 1000);
        }

        public override bool StartUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                UISystem.Instance.PaintWandGUI.ToggleMode();
                return false;
            }

            return base.StartUseItem(player);
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddRecipeGroup(RecipeGroups.Wood, 18)
                .AddRecipeGroup(RecipeGroups.IronBar, 6)
                .AddIngredient(ItemID.Paintbrush, 1)
                .AddIngredient(ItemID.PaintRoller, 1)
                .AddIngredient(ItemID.PaintScraper, 1)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}