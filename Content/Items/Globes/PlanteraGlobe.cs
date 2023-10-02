using ImproveGame.Common.Packets.WorldFeatures;

namespace ImproveGame.Content.Items.Globes;

public class PlanteraGlobe : ModItem
{
    private LocalizedText GetLocalizedText(string suffix) =>
        Language.GetText($"Mods.ImproveGame.Items.GlobeBase.{suffix}")
            .WithFormatArgs(Language.GetTextValue("NPCName.Plantera"));

    public override LocalizedText DisplayName => GetLocalizedText(nameof(DisplayName));

    public override LocalizedText Tooltip => GetLocalizedText(nameof(Tooltip));

    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.useTime = 30;
        Item.useAnimation = 30;
        Item.consumable = true;
        Item.maxStack = Item.CommonMaxStack;
        Item.rare = ItemRarityID.Pink;
    }

    public override void AddRecipes() =>
        CreateRecipe()
            .AddIngredient(ItemID.Glass, 5)
            .AddIngredient(ItemID.MudBlock, 100)
            .AddIngredient(ItemID.RichMahogany, 30)
            .AddIngredient(ItemID.JungleSpores, 1)
            .AddTile(TileID.MythrilAnvil)
            .Register();
    
    public override bool CanUseItem(Player player)
    {
        return RevealPlanteraPacket.Reveal(player);
    }

    public override bool? UseItem(Player player)
    {
        return true;
    }
}