using ImproveGame.Common.ModSystems;

namespace ImproveGame.Common.GlobalItems;

public class ItemLootDisplay : GlobalItem
{
    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
    {
        if (!ItemLoader.CanRightClick(item))
            return;

        if (Main.ItemDropsDB.GetRulesForItemID(item.type).Count <= 0)
            return;

        bool hasKeybind = MyUtils.TryGetKeybindString(KeybindSystem.GrabBagKeybind, out var keybind);
        tooltips.Add(new TooltipLine(Mod, "LootDisplay", MyUtils.GetTextWith("Tips.LootDisplay", new { KeybindName = keybind }))
        {
            Color = Color.SkyBlue
        });
        if (!hasKeybind)
        {
            tooltips.Add(new TooltipLine(Mod, "LootDisplay", MyUtils.GetText("Tips.LootDisplayBindless"))
            {
                Color = Color.SkyBlue
            });
        }
    }
}
