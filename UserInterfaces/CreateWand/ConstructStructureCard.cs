using SilkyUIFramework.Elements;
using SilkyUIFramework.Extensions;
using SilkyUIFramework.StyleSystem;

namespace ImproveGame.UserInterfaces.CreateWand;

public class ConstructStructureCard : UIElementGroup
{
    protected override object CommandParameter => InnerText.Text;
    public UITextView InnerText { get; }
    public ConstructStructureCard()
    {
        InnerText = new UITextView()
        {
            TextScale = 0.85f,
            TextAlign = new(0, 0.5f)
        }.Join(this);

        StyleSheet.SetStyle(UIElementState.Normal, new StyleDefinition()
        {
            [nameof(BackgroundColor)] = Color.Black * 0.2f
        });

        StyleSheet.SetStyle(UIElementState.Hover, new StyleDefinition()
        {
            [nameof(BackgroundColor)] = Color.Black * 0.3f
        });
    }

    public override void OnLeftMouseClick(SilkyUIFramework.UIMouseEvent evt)
    {
        base.OnLeftMouseClick(evt);
        SoundEngine.PlaySound(SoundID.ResearchComplete);
    }
}
public class ConstructStructureCardTemplate : ISourcedUIViewTemplate
{
    public static ConstructStructureCardTemplate Instance { get; } = new();
    UIView ISourcedUIViewTemplate.ConstructFromSource(object sourceData)
    {
        if (sourceData is not string path)
            throw new ArgumentException($"The type of source should be string, but this sourceData is {sourceData.GetType()}");

        ConstructStructureCard fileCard = new()
        {
            Width = new(0, 1),
            FitHeight = true,
            Padding = new(8),
            BorderRadius = new(8),
            BackgroundColor = Color.Black * .25f,
            InnerText = {
                Text= Path.GetFileName(path),
            }
        };

        fileCard.Bind(nameof(CreateWandViewModel.RegisterFromQotStructureCommand), nameof(ConstructStructureCard.Command));
        return fileCard;
    }
}