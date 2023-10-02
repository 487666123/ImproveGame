using ImproveGame.Common.ModSystems;

namespace ImproveGame.Content.Items.Globes;

public class AetherGlobe : GlobeBase
{
    public override StructureDatas.UnlockID StructureType => StructureDatas.UnlockID.Shimmer;

    public override bool NotFoundCheck() => StructureDatas.ShimmerPosition == default;

    protected override Recipe AddCraftingMaterials(Recipe recipe) =>
        recipe.AddIngredient(ItemID.Glass, 10)
            .AddIngredient(ItemID.StoneBlock, 80)
            .AddRecipeGroup(RecipeSystem.AnyGem, 6)
            .AddTile(TileID.WorkBenches);
}

public class DungeonGlobe : GlobeBase
{
    public override StructureDatas.UnlockID StructureType => StructureDatas.UnlockID.Dungeon;

    protected override Recipe AddCraftingMaterials(Recipe recipe) =>
        recipe.AddRecipeGroup(RecipeGroupID.Sand, 10)
            .AddIngredient(ItemID.DirtBlock, 30)
            .AddIngredient(ItemID.StoneBlock, 50)
            .AddTile(TileID.WorkBenches);
}

public class TempleGlobe : GlobeBase
{
    public override StructureDatas.UnlockID StructureType => StructureDatas.UnlockID.Temple;

    public override bool NotFoundCheck() => StructureDatas.TemplePosition == default;

    protected override Recipe AddCraftingMaterials(Recipe recipe) =>
        recipe.AddIngredient(ItemID.Glass, 10)
            .AddIngredient(ItemID.MudBlock, 100)
            .AddIngredient(ItemID.JungleSpores, 3)
            .AddIngredient(ItemID.Stinger, 3)
            .AddTile(TileID.WorkBenches);
}

public class PyramidGlobe : GlobeBase
{
    public override StructureDatas.UnlockID StructureType => StructureDatas.UnlockID.Pyramids;

    protected override Recipe AddCraftingMaterials(Recipe recipe) =>
        recipe.AddRecipeGroup(RecipeGroupID.Sand, 50)
            .AddTile(TileID.WorkBenches);
}

public class FloatingIslandGlobe : GlobeBase
{
    public override StructureDatas.UnlockID StructureType => StructureDatas.UnlockID.FloatingIslands;

    public override bool NotFoundCheck() =>
        StructureDatas.SkyHousePositions.Count is 0 && StructureDatas.SkyLakePositions.Count is 0;

    protected override Recipe AddCraftingMaterials(Recipe recipe) =>
        recipe.AddIngredient(ItemID.Glass, 18)
            .AddRecipeGroup(RecipeGroupID.Wood, 100)
            .AddIngredient(ItemID.Rope, 100)
            .AddTile(TileID.WorkBenches);
}