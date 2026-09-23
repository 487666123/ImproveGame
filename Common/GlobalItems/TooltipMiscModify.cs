using ImproveGame.Common.Configs;
using ImproveGame.Common.ModHooks;
using ImproveGame.Common.ModSystems;

namespace ImproveGame.Common.GlobalItems
{
    internal class TooltipMiscModify : GlobalItem
    {
        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            if (!ModIntegrationsSystem.WMITFLoaded && UIConfigs.Instance.ShowModName &&
                item.type != ModIntegrationsSystem.UnloadedItemType)
            {
                if (item.ModItem is not null && !item.Name.Contains("[" + item.ModItem.Mod.Name + "]") && !item.Name.Contains("[" + item.ModItem.Mod.DisplayName + "]") && item.ModItem is not IHideExtraTooltips)
                {
                    string text = MyUtils.GetTextWith("Tips.FromMod", new { item.ModItem.Mod.DisplayName });
                    TooltipLine line = new(Mod, "FromModTip", text)
                    {
                        Color = Colors.RarityBlue
                    };
                    tooltips.Add(line);
                }
            }
        }
    }
}
