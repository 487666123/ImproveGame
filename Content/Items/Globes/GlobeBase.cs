using ImproveGame.Common.Packets.WorldFeatures;

namespace ImproveGame.Content.Items.Globes;

public abstract class GlobeBase : ModItem
{
    public abstract StructureDatas.UnlockID StructureType { get; }

    public virtual bool NotFoundCheck() => false;
    
    private LocalizedText GetLocalizedText(string suffix) =>
        Language.GetText($"Mods.ImproveGame.Items.GlobeBase.{suffix}").WithFormatArgs(this.GetLocalizedValue("BiomeName"));

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
        Item.rare = ItemRarityID.Quest;
    }

    public override bool CanUseItem(Player player)
    {
        if (StructureType is StructureDatas.UnlockID.Pyramids && StructureDatas.PyramidPositions.Count is 0)
        {
            if (player.whoAmI == Main.myPlayer)
                Main.NewText(this.GetLocalizedValue("NotFound"), Color.PaleVioletRed);
            return false;
        }

        if (NotFoundCheck())
        {
            if (player.whoAmI == Main.myPlayer)
                Main.NewText(GetLocalizedText("NotFound").Value, Color.PaleVioletRed);
            return false;
        }

        if (!StructureDatas.StructuresUnlocked[(byte)StructureType])
            return true;

        if (player.whoAmI == Main.myPlayer)
            Main.NewText(GetLocalizedText("AlreadyRevealed").Value, Color.PaleVioletRed);

        return false;
    }

    public override bool? UseItem(Player player)
    {
        if (StructureDatas.StructuresUnlocked[(byte)StructureType])
        {
            return base.UseItem(player);
        }

        StructureDatas.StructuresUnlocked[(byte)StructureType] = true;
        var text = Language.GetText("Mods.ImproveGame.Items.GlobeBase.Reveal")
            .WithFormatArgs(this.GetLocalizedValue("BiomeName"), player.name);
        Main.NewText(text.Value, Color.Pink);
        return true;
    }

    protected abstract Recipe AddCraftingMaterials(Recipe recipe);

    public override void AddRecipes()
    {
        var r = CreateRecipe();
        AddCraftingMaterials(r);
        r.Register();
    }
}