namespace ImproveGame.UI.ExtremeStorage.ToolButtons;

public class RecipeToggleButton : ToolButton
{
    public override void OnTakeEffect() =>
        ExtremeStorageGUI.DisplayCrafting = !ExtremeStorageGUI.DisplayCrafting;

    public override Texture2D Texture =>
        TextureAssets.CraftToggle[ExtremeStorageGUI.DisplayCrafting.ToInt() * 2 + IsMouseHovering.ToInt()].Value;

    public override string HoverText => ExtremeStorageGUI.DisplayCrafting
        ? MyUtils.GetText("UI.ExtremeStorage.CloseCraftingWindow")
        : MyUtils.GetText("UI.ExtremeStorage.OpenCraftingWindow");
}