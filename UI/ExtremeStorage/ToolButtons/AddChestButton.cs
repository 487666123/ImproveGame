namespace ImproveGame.UI.ExtremeStorage.ToolButtons;

public class AddChestButton : ToolButton
{
    public override void OnTakeEffect()
    {
        ChestSelection.IsSelecting = !ChestSelection.IsSelecting;
    }

    public override Texture2D Texture => ToolIcons.Value;

    public override Rectangle? SourceRectangle => ToolIcons.Frame(2, FrameVertically, IsMouseHovering ? 1 : 0, 6);

    public override string HoverText => ChestSelection.IsSelecting
        ? MyUtils.GetText("Common.Cancel")
        : MyUtils.GetText("UI.ExtremeStorage.AddChest");
}