using ImproveGame.Common.Configs;

namespace ImproveGame.Content.Items;

public class WallPlaceSelectorMode : SelectorItem
{
    public override string Texture => "ImproveGame/Content/Items/WallPlace";

    // 修改物块
    public override void PostModifyTiles(Player player, int minI, int minJ, int maxI, int maxJ)
    {
        for (int j = minJ; j <= maxJ; j++)
        {
            for (int i = minI; i <= maxI; i++)
            {
                WorldGen.SquareWallFrame(i, j, false);
            }
        }
    }

    public override bool IsNeedKill() => !Main.mouseLeft;

    // 修改选择的方块
    public override bool ModifySelectedTiles(Player player, int i, int j)
    {
        Tile t = Main.tile[i, j];
        Item firstWall = MyUtils.FirstWall(player);
        if (firstWall is null)
            return false;
        if (firstWall.createWall == t.WallType)
            return true;

        switch (t.WallType)
        {
            case > 0:
                WorldGen.KillWall(i, j);
                break;
            case <= 0:
            {
                WorldGen.PlaceWall(i, j, firstWall.createWall);
                NetMessage.SendTileSquare(-1, i, j);
                // 大于等于 999 不消耗墙
                // ItemLoader.ConsumeItem 判断手持物品是否是科技法杖，但是他是机器人放置墙体的，即使手持不是科技也可能不消耗
                bool vanillaHooks = firstWall.consumable && ItemLoader.ConsumeItem(firstWall, player);
                bool modSpecificChecks = firstWall.stack < 999 || !ImproveConfigs.Instance.WandMaterialNoConsume;
                if (vanillaHooks && modSpecificChecks && --firstWall.stack == 0)
                    firstWall.TurnToAir();
                break;
            }
        }

        return true;
    }

    public override bool CanUseItem(Player player)
    {
        if (player.noBuilding)
            return false;
        Item firstWall = MyUtils.FirstWall(player);
        if (firstWall is null)
            return false;
        return base.CanUseItem(player);
    }

    public override void HoldItem(Player player)
    {
        Item firstWall = MyUtils.FirstWall(player);
        if (firstWall is null)
            return;

        player.cursorItemIconEnabled = true;
        player.cursorItemIconID = firstWall.type;
        player.cursorItemIconPush = 26;
    }

    public override bool AltFunctionUse(Player player) => true;

    public override void SetItemDefaults()
    {
        Item.SetBaseValues(46, 42, ItemRarityID.Red, Item.sellPrice(0, 0, 50));

        SelectRange = new(50, 50);
        RunOnServer = true;
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

    public override void RightClick(Player player)
    {
        bool favorited = Item.favorited;
        Item.SetDefaults(ModContent.ItemType<WallPlace>());
        Item.favorited = favorited;
    }

    public override bool ConsumeItem(Player player) => false;

    public override bool CanRightClick() => true;
}