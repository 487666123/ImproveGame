using ImproveGame.Common.Conditions;
using ImproveGame.Common.GlobalItems;
using ImproveGame.Common.ModHooks;
using ImproveGame.Common.ModSystems;
using ImproveGame.UserInterfaces.CreateWand;
using System.Collections.ObjectModel;

namespace ImproveGame.Content.Items;

public partial class CreateWand : ModItem, IItemOverrideHover, IItemMiddleClickable, IConditionItem
{
    public Condition UseCondition => ConfigCondition.AvailableCreateWandC;

    public override void SetStaticDefaults()
    {
        Item.staff[Type] = true;
        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults()
    {
        Item.SetBaseValues(42, 42, ItemRarityID.Red, Item.sellPrice(0, 1));
        Item.SetUseValues(ItemUseStyleID.Swing, SoundID.Item1, 16, 16);
    }
    public override bool AltFunctionUse(Player player) => true;

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddRecipeGroup(RecipeGroups.Wood, 24)
            .AddRecipeGroup(RecipeSystem.AnyIronBar, 8)
            .AddIngredient(ItemID.FallenStar, 6)
            .Register();
    }

    public override bool CanUseItem(Player player)
    {
        if (player.noBuilding) return false;

        if (player.altFunctionUse == 2 && !Main.dedServ && player.whoAmI == Main.myPlayer)
        {
            CreateWandController.Instance?.Toggle(this);
            return false;
        }

        return true;
    }

    bool IItemOverrideHover.OverrideHover(Item[] inventory, int context, int slot)
    {
        ((IItemMiddleClickable)this).HandleHover(inventory, context, slot);
        return false;
    }

    void IItemMiddleClickable.OnMiddleClicked(Item item)
    {
        CreateWandController.Instance?.Toggle(item.ModItem as CreateWand);
    }

    void IItemMiddleClickable.ManageHoverTooltips(Item item, List<TooltipLine> tooltips)
    {
        // 决定文本显示的是“开启”还是“关闭”
        string text = CreateWandController.Instance?.Enabled is false or null ? "Off" : "On";
        MyUtils.TryGetKeybindString(KeybindSystem.ItemInteractKeybind, out string keybind);
        tooltips.Add(new TooltipLine(Mod, "CreateWand", MyUtils.GetTextWith($"Tips.CreateWand{text}", new { KeybindName = keybind }))
        { Color = Color.LightGreen });
    }

    public override bool PreDrawTooltip(ReadOnlyCollection<TooltipLine> lines, ref int x, ref int y)
    {
        if (ItemSlot.ShiftInUse)
            TagItem.DrawTagTooltips(lines, TagItem.GenerateDetailedTags(Mod, lines), x, y);
        return true;
    }
    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        ((IItemMiddleClickable)this).HandleTooltips(Item, tooltips);

        tooltips.Add(new(Mod, "MaterialConsume", $"[c/ffff00:{MyUtils.GetText("Architecture.MaterialsRequired")}]"));

        ModifyTooltipLine_MaterialInfo(tooltips);


        tooltips.Add(new TooltipLine(Mod, "TagDetailed.CreateWand", MyUtils.GetText("Tips.TagDetailed.CreateWand")) { Color = Color.SkyBlue });
        TagItem.AddShiftForMoreTooltip(tooltips);
    }

}