using ImproveGame.Common.Conditions;
using ImproveGame.Common.GlobalItems;
using ImproveGame.Common.ModSystems;
using ImproveGame.Content.Functions;
using ImproveGame.Packets.Weather;

namespace ImproveGame.Content.Items;

public sealed class WeatherBook : ModItem, IConditionItem
{
    Condition IConditionItem.UseCondition => ConfigCondition.EnableWeatherControlC;
    string IConditionItem.ConfigKey => "WeatherControl";
    public override void SetDefaults()
    {
        Item.CloneDefaults(ItemID.CombatBook);
        Item.Size = new Vector2(60);
        Item.useStyle = ItemUseStyleID.RaiseLamp; // 用4的话物品会错位不在手上
        Item.value = Item.sellPrice(gold: 5);
    }

    public override bool? UseItem(Player player)
    {
        if (player.whoAmI != Main.myPlayer)
            return null;

        if (WeatherController.Unlocked)
        {
            if (player.itemAnimation == player.itemAnimationMax)
                MyUtils.AddNotification(MyUtils.GetText("UI.WeatherGUI.AlreadyUnlocked"), Color.Pink);
            return null;
        }

        WeatherLockerPacket.Unlock();
        WorldGen.BroadcastText(NetworkText.FromKey("Mods.ImproveGame.UI.WeatherGUI.Unlocked"), Color.Pink);
        return true;
    }
    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.Book)
            .AddIngredient(ItemID.Cloud, 50)
            .AddIngredient(ItemID.RainCloud, 20)
            .AddIngredient(ItemID.FallenStar, 3)
            .AddRecipeGroup(RecipeSystem.AnyShadowScale, 5)
            .AddTile(TileID.WorkBenches)
            .Register();
    }
}