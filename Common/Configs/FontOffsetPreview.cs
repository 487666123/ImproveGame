using ReLogic.Graphics;
using Terraria.ModLoader.Config.UI;
using Terraria.UI.Chat;

namespace ImproveGame.Common.Configs;

public class FontOffsetPreview : FloatElement
{
    private Asset<Texture2D> _texture;
    protected Asset<DynamicSpriteFont> Font;
    protected float BaseOffset; // 基准偏移，用来让默认字体在默认值下贴合边缘
    protected string TextKey;

    public override void OnBind()
    {
        base.OnBind();
        Height.Set(70f, 0f);
        BaseOffset = -12f;
        Font = FontAssets.MouseText;
        TextKey = "Configs.UIConfigs.FontOffsetPreviewText";
        _texture = Main.Assets.Request<Texture2D>("Images/UI/CharCreation/Separator1");
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);
        CalculatedStyle dimensions = this.GetDimensions();
        var linePosition = dimensions.Position() + new Vector2(6f, dimensions.Height - 14f);
        var textPosition = linePosition + new Vector2(0f, BaseOffset + (float)GetObject());
        var text = MyUtils.GetText(TextKey);
        var textOrigin = new Vector2(0f, Font.Value.MeasureString(text).Y) / 2f;
        ChatManager.DrawColorCodedStringWithShadow(spriteBatch, Font.Value, text, textPosition, Color.White, 0f,
            textOrigin, Vector2.One, spread: 1.2f);
        TrUtils.DrawPanel(this._texture.Value, 2, 0, spriteBatch, linePosition, dimensions.Width - 12f, Color.White);
    }
}

public class BigFontOffsetPreview : FontOffsetPreview
{
    public override void OnBind()
    {
        base.OnBind();
        Height.Set(94f, 0f);
        Font = FontAssets.DeathText;
        BaseOffset = -24f;
        TextKey = "Configs.UIConfigs.BigFontOffsetPreviewText";
    }
}