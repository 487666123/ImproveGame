using ImproveGame.Common.Configs;
using ImproveGame.Content.Items;

namespace ImproveGame.Common.ModSystems;

public class RecipeSystem : ModSystem
{
    /// <summary>任意铜锭（铜/锡）</summary>
    public static RecipeGroup AnyCopperBar { get; private set; }

    /// <summary>任意铁锭（铁/铅）</summary>
    public static RecipeGroup AnyIronBar { get; private set; }

    /// <summary>任意银锭（银/钨）</summary>
    public static RecipeGroup AnySilverBar { get; private set; }

    /// <summary>任意金锭（金/铂）</summary>
    public static RecipeGroup AnyGoldBar { get; private set; }

    /// <summary>任意魔金锭（魔金/猩红）</summary>
    public static RecipeGroup AnyDemoniteBar { get; private set; }

    /// <summary>任意暗影鳞片（暗影鳞片/组织样本）</summary>
    public static RecipeGroup AnyShadowScale { get; private set; }

    /// <summary>任意钴锭（钴/钯金）</summary>
    public static RecipeGroup AnyCobaltBar { get; private set; }

    /// <summary>任意秘银锭（秘银/山铜）</summary>
    public static RecipeGroup AnyMythrilBar { get; private set; }

    /// <summary>任意精金锭（精金/钛金）</summary>
    public static RecipeGroup AnyAdamantiteBar { get; private set; }

    /// <summary>任意宝石</summary>
    internal static RecipeGroup AnyGem { get; private set; }

    public override void Unload()
    {
        AnyCopperBar = null;
        AnyIronBar = null;
        AnySilverBar = null;
        AnyGoldBar = null;
        AnyDemoniteBar = null;
        AnyShadowScale = null;
        AnyCobaltBar = null;
        AnyMythrilBar = null;
        AnyAdamantiteBar = null;
    }

    public override void AddRecipeGroups()
    {
        AnyCopperBar = new RecipeGroup(() => MyUtils.GetText($"RecipeGroup.{nameof(AnyCopperBar)}"), ItemID.CopperBar, ItemID.TinBar);
        AnyIronBar = new RecipeGroup(() => MyUtils.GetText($"RecipeGroup.{nameof(AnyIronBar)}"), ItemID.IronBar, ItemID.LeadBar);
        AnySilverBar = new RecipeGroup(() => MyUtils.GetText($"RecipeGroup.{nameof(AnySilverBar)}"), ItemID.SilverBar, ItemID.TungstenBar);
        AnyGoldBar = new RecipeGroup(() => MyUtils.GetText($"RecipeGroup.{nameof(AnyGoldBar)}"), ItemID.GoldBar, ItemID.PlatinumBar);
        AnyDemoniteBar = new RecipeGroup(() => MyUtils.GetText($"RecipeGroup.{nameof(AnyDemoniteBar)}"), ItemID.DemoniteBar, ItemID.CrimtaneBar);
        AnyShadowScale = new RecipeGroup(() => MyUtils.GetText($"RecipeGroup.{nameof(AnyShadowScale)}"), ItemID.ShadowScale, ItemID.TissueSample);
        AnyCobaltBar = new RecipeGroup(() => MyUtils.GetText($"RecipeGroup.{nameof(AnyCobaltBar)}"), ItemID.CobaltBar, ItemID.PalladiumBar);
        AnyMythrilBar = new RecipeGroup(() => MyUtils.GetText($"RecipeGroup.{nameof(AnyMythrilBar)}"), ItemID.MythrilBar, ItemID.OrichalcumBar);
        AnyAdamantiteBar = new RecipeGroup(() => MyUtils.GetText($"RecipeGroup.{nameof(AnyAdamantiteBar)}"), ItemID.AdamantiteBar, ItemID.TitaniumBar);
        AnyGem = new RecipeGroup(() => MyUtils.GetText($"RecipeGroup.{nameof(AnyGem)}"), ItemID.Sapphire, ItemID.Ruby,
            ItemID.Emerald, ItemID.Topaz, ItemID.Amethyst, ItemID.Diamond, ItemID.Amber);


        AnyCopperBar.Register();
        AnyIronBar.Register();
        AnySilverBar.Register();
        AnyGoldBar.Register();
        AnyDemoniteBar.Register();
        AnyShadowScale.Register();
        AnyCobaltBar.Register();
        AnyMythrilBar.Register();
        AnyAdamantiteBar.Register();
        AnyGem.Register();
    }

    public override void AddRecipes()
    {
        /* Materials
         * 18 Bottled Water, 1 Obsidian, 1 Mushroom, 1 Cactus, 1Iron/Lead Ore, 2 Fallen Star, 1 Feather, 1 Gold/Platinum, 1 Lens, 1 Shark Fin, 1 Antlion Mandible, 10 Cobweb
         *
         * Flowers
         * 2 Fireblossom, 2 Waterleaf, 7 Daybloom, 6 Blinkroot, 4 Moonglow, 3 Deathweed 2 Shiverthorn
         *
         * Fish
         * 1 Crimson Tigerfish, 1 Armored Cavefish, 1 Prismite, 1 Flarefin Koi, 2 Obsidifish, 1 Hemopiranha, 1 Ebonkoi
         */
        var redPotionCondition = new Condition("Mods.ImproveGame.Configs.ImproveConfigs.InfiniteRedPotion.Condition",
            () => ImproveConfigs.Instance.InfiniteRedPotion);
        var ftwCondition = new Condition("Mods.ImproveGame.Configs.ImproveConfigs.RedPotionEverywhere.Condition",
            () => ImproveConfigs.Instance.RedPotionEverywhere || Condition.ForTheWorthyWorld.IsMet());

        Recipe.Create(ItemID.RedPotion)
            .AddTile(TileID.Bottles)
            .AddCondition(ftwCondition)
            .AddCondition(redPotionCondition)
            // Materials
            .AddIngredient(ItemID.BottledWater, 18)
            .AddIngredient(ItemID.Obsidian)
            .AddIngredient(ItemID.Mushroom)
            .AddIngredient(ItemID.Cactus)
            .AddIngredient(ItemID.Lens)
            .AddIngredient(ItemID.SharkFin)
            .AddIngredient(ItemID.AntlionMandible)
            .AddIngredient(ItemID.Feather)
            .AddIngredient(ItemID.Cobweb, 10)
            .AddIngredient(ItemID.FallenStar, 2)
            .AddIngredient(ItemID.IronBar)
            // Flowers
            .AddIngredient(ItemID.Fireblossom, 2)
            .AddIngredient(ItemID.Waterleaf, 2)
            .AddIngredient(ItemID.Daybloom, 7)
            .AddIngredient(ItemID.Blinkroot, 6)
            .AddIngredient(ItemID.Moonglow, 4)
            .AddIngredient(ItemID.Deathweed, 3)
            .AddIngredient(ItemID.Shiverthorn, 2)
            // Fish
            .AddIngredient(ItemID.CrimsonTigerfish)
            .AddIngredient(ItemID.ArmoredCavefish)
            .AddIngredient(ItemID.Prismite)
            .AddIngredient(ItemID.FlarefinKoi)
            .AddIngredient(ItemID.Obsidifish, 2)
            .AddIngredient(ItemID.Hemopiranha)
            .AddIngredient(ItemID.Ebonkoi)
            .DisableDecraft()
            .Register();
    }

    public override void PreUpdateItems()
    {
        // 控制微光
        // 液体法杖月后可直接微光转换为终极液体法杖
        if (!MyUtils.AvailableConfig.AvailableLiquidWandAdvanced)
            return;
        int liquidWand = ModContent.ItemType<LiquidWand>();
        int liquidWandAdvanced = ModContent.ItemType<LiquidWandAdvanced>();
        ItemID.Sets.ShimmerTransformToItem[liquidWand] = NPC.downedMoonlord ? liquidWandAdvanced : -1;
    }
}