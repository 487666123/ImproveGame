using ImproveGame.Common.Conditions;
using ImproveGame.Common.GlobalItems;

namespace ImproveGame.Content.Items;

public class ActuationRodMkII : SelectorItem, IConditionItem
{
    public Condition UseCondition => ConfigCondition.AvailableActuationRodMkIIC;
    public override bool IsNeedKill() => !Main.mouseLeft;

    // 修改选择的方块
    public override bool ModifySelectedTiles(Player player, int i, int j)
    {
        Tile t = Main.tile[i, j];
        if (t.IsActuated)
            Wiring.ReActive(i, j);
        else
            Wiring.DeActive(i, j);

        return true;
    }

    public override bool CanUseItem(Player player)
    {
        if (player.noBuilding)
            return false;
        return base.CanUseItem(player);
    }

    public override bool AltFunctionUse(Player player) => true;

    public override void SetItemDefaults()
    {
        Item.SetBaseValues(46, 42, ItemRarityID.Red, Item.sellPrice(0, 5));

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

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.ActuationRod)
            .AddIngredient(ItemID.Actuator, 300)
            .AddIngredient(ItemID.Wire, 500)
            .AddIngredient(ItemID.Lever, 10)
            .AddTile(TileID.AlchemyTable)
            .AddCondition()
            .Register();
    }
}