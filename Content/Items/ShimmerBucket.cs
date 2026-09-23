using ImproveGame.Common.Conditions;
using ImproveGame.Common.GlobalItems;

namespace ImproveGame.Content.Items;

public class ShimmerBucket : ModItem, IConditionItem
{
    public Condition UseCondition => ConfigCondition.AvailableShimmerBucketC;
    public override void SetDefaults()
    {
        Item.CloneDefaults(ItemID.WaterBucket);
        base.SetDefaults();
    }
    public override void UseStyle(Player player, Rectangle heldItemFrame)
    {
        if (player.whoAmI != Main.myPlayer) return;

        var item = player.inventory[player.selectedItem];
        if (item.type != Item.type) return;

        if (!Main.GamepadDisableCursorItemIcon)
        {
            player.cursorItemIconEnabled = true;
            Main.ItemIconCacheUpdate(item.type);
        }

        if (!player.ItemTimeIsZero || player.itemAnimation <= 0 || !player.controlUseItem)
            return;

        var pointedTile = Main.tile[Player.tileTargetX, Player.tileTargetY];
        if (pointedTile.liquid >= 200 || (pointedTile.nactive() && Main.tileSolid[pointedTile.type] && !Main.tileSolidTop[pointedTile.type] && pointedTile.type != TileID.Grate))
            return;

        if (pointedTile.liquid == 0 || pointedTile.liquidType() == LiquidID.Shimmer)
        {
            SoundEngine.PlaySound(SoundID.Splash, (int)player.position.X, (int)player.position.Y);
            pointedTile.liquidType(LiquidID.Shimmer);
            pointedTile.liquid = byte.MaxValue;
            WorldGen.SquareTileFrame(Player.tileTargetX, Player.tileTargetY);

            item.stack--;
            player.PutItemInInventoryFromItemUsage(ItemID.EmptyBucket);

            player.ApplyItemTime(item);
            if (Main.netMode == NetmodeID.MultiplayerClient)
                NetMessage.sendWater(Player.tileTargetX, Player.tileTargetY);
        }
        base.UseStyle(player, heldItemFrame);
    }
}
public class ShimmerBucketGlobalItem : GlobalItem
{
    public override void UseStyle(Item item, Player player, Rectangle heldItemFrame)
    {
        if (player.whoAmI != Main.myPlayer) return;

        var sItem = player.inventory[player.selectedItem];

        if (!MyUtils.AvailableConfig.AvailableShimmerBucket) return;
        if (sItem.type != ItemID.EmptyBucket) return;
        if (!player.ItemTimeIsZero || player.itemAnimation <= 0 || !player.controlUseItem)
            return;

        var tileTargetX = Player.tileTargetX;
        var tileTargetY = Player.tileTargetY;
        var pointedTile = Main.tile[tileTargetX, y: tileTargetY];

        int num = pointedTile.liquidType();
        int num2 = 0;
        for (int i = tileTargetX - 1; i <= tileTargetX + 1; i++)
        {
            for (int j = tileTargetY - 1; j <= tileTargetY + 1; j++)
            {
                if (Main.tile[i, j].liquidType() == num)
                    num2 += Main.tile[i, j].liquid;
            }
        }

        if (pointedTile.liquid <= 0 || num2 <= 100)
            return;

        int liquidType = pointedTile.liquidType();
        if (pointedTile.shimmer())
        {
            sItem.stack--;
            player.PutItemInInventoryFromItemUsage(ModContent.ItemType<ShimmerBucket>());
        }
        else return;

        SoundEngine.PlaySound(SoundID.Splash, (int)player.position.X, (int)player.position.Y);
        player.ApplyItemTime(sItem);
        int num3 = pointedTile.liquid;
        pointedTile.liquid = 0;
        pointedTile.lava(lava: false);
        pointedTile.honey(honey: false);
        WorldGen.SquareTileFrame(tileTargetX, tileTargetY, resetFrame: false);
        if (Main.netMode == NetmodeID.MultiplayerClient)
            NetMessage.sendWater(tileTargetX, tileTargetY);
        else
            Liquid.AddWater(tileTargetX, tileTargetY);

        if (num3 >= 255)
            return;

        for (int k = tileTargetX - 1; k <= tileTargetX + 1; k++)
        {
            for (int l = tileTargetY - 1; l <= tileTargetY + 1; l++)
            {
                if ((k != tileTargetX || l != tileTargetY) && Main.tile[k, l].liquid > 0 && Main.tile[k, l].liquidType() == num)
                {
                    int num4 = Main.tile[k, l].liquid;
                    if (num4 + num3 > 255)
                        num4 = 255 - num3;

                    num3 += num4;
                    Main.tile[k, l].liquid -= (byte)num4;
                    Main.tile[k, l].liquidType(liquidType);
                    if (Main.tile[k, l].liquid == 0)
                    {
                        Main.tile[k, l].lava(lava: false);
                        Main.tile[k, l].honey(honey: false);
                    }

                    WorldGen.SquareTileFrame(k, l, resetFrame: false);
                    if (Main.netMode == NetmodeID.MultiplayerClient)
                        NetMessage.sendWater(k, l);
                    else
                        Liquid.AddWater(k, l);
                }
            }
        }

        base.UseStyle(item, player, heldItemFrame);
    }
}