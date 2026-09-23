namespace ImproveGame.UI.ModernConfig.OptionElements.PresetElements;

public class OpenFolderElement() : BasePresetElement(MyUtils.GetText("ModernConfig.Presets.OpenFolder.Label"),
    MyUtils.GetText("ModernConfig.Presets.OpenFolder.Tooltip"))
{
    public override void LeftMouseDown(UIMouseEvent evt)
    {
        base.LeftMouseDown(evt);

        TrUtils.OpenFolder(PresetHandler.ConfigPresetsPath);
    }

    protected override bool Interactable => true;
}