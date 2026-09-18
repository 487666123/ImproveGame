using SilkyUIFramework;
using SilkyUIFramework.Elements;
using BuildingData = ImproveGame.Content.Items.CreateWand.BuildingData;
using CWand = ImproveGame.Content.Items.CreateWand;
namespace ImproveGame.UserInterfaces.CreateWand;

public class StructurePreviewCard(BuildingData data, Texture2D preview) : UIView
{
    BuildingData Data { get; } = data;
    Texture2D Preview { get; } = preview;
    protected override object CommandParameter => Data;
    protected override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        base.Draw(gameTime, spriteBatch);
        var dimension = Bounds;
        var scalerY = (dimension.Height * .9f - 16) / Preview.Height;
        var scalerX = (dimension.Width * .9f - 16) / Preview.Width;
        float scaler = Math.Min(scalerX, scalerY);
        spriteBatch.Draw(Preview, Bounds.Center, null, Color.White * HoverTimer.Lerp(0.5f, 1f), 0, Preview.Size() * .5f, scaler, 0, 0);
    }

    public override void OnLeftMouseClick(SilkyUIFramework.UIMouseEvent evt)
    {
        base.OnLeftMouseClick(evt);
        SoundEngine.PlaySound(SoundID.ResearchComplete);
    }
    protected override void UpdateStatus(GameTime gameTime)
    {
        base.UpdateStatus(gameTime);
        BackgroundColor = Color.Black * HoverTimer.Lerp(0.25f, 0.1f);
    }
}

public class StructurePreviewCardTemplate : ISourcedUIViewTemplate 
{
    public static StructurePreviewCardTemplate Instance { get; } = new();

    UIView ISourcedUIViewTemplate.ConstructFromSource(object sourceData)
    {
        if (sourceData is not BuildingData data)
            throw new ArgumentException($"The type of source should be BuildingData, but this sourceData is {sourceData.GetType()}");
        var card = new StructurePreviewCard(data, CWand.BuildingDataPreview[data])
        {
            Width = new(0, 0.45f),
            Height = new(200, 0),
            BorderColor = SUIColor.Border,
            BackgroundColor = Color.Black * .25f,
            BorderRadius = new(8),
            Margin = new(4, 2, 2, 2),
            FlexGrow = 1,
        };
        card.Bind(nameof(CreateWandViewModel.SetBuildingDataCommand), nameof(StructurePreviewCard.Command));
        return card;
    }
}