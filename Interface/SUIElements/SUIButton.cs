using ImproveGame.Common.Configs;
using ImproveGame.Interface.Common;
using ReLogic.Graphics;
using Terraria.UI.Chat;

namespace ImproveGame.Interface.SUIElements;

/// <summary>
/// 按钮，有 “图标模式” 和 “无图标” 两种模式。
/// </summary>
public class SUIButton : TimerView
{
    public bool IconMode;
    public bool TextHasBorder = true;
    public Vector2 TextAlign;

    private static readonly float iconAndTextSpacing = 6f;
    private readonly Texture2D _texture;
    private string _text;
    public Vector2 TextSize { get; private set; }
    public string Text
    {
        get => _text;
        set
        {
            _text = value;
            TextSize = FontAssets.MouseText.Value.MeasureString(value);
        }
    }

    public Color BeginBorderColor = UIColor.PanelBorder;
    public Color EndBorderColor = UIColor.ItemSlotBorderFav;
    public Color BeginBgColor = UIColor.ButtonBg;
    public Color EndBgColor = UIColor.ButtonBgHover;
    public Color TextColor = Color.White;

    public SUIButton(string text)
    {
        Text = text;
        SetPadding(18f, 8f);
        SetInnerPixels(TextSize);
        OnMouseOver += (_, _) => SoundEngine.PlaySound(SoundID.MenuTick);

        Rounded = new Vector4(10f);
        Border = 2f;
    }

    public SUIButton(Texture2D texture, string text)
    {
        IconMode = true;
        _texture = texture;
        Text = text;
        SetPadding(18f, 0f);
        SetInnerPixels(_texture.Width + TextSize.X + 4 + iconAndTextSpacing, 40f);
        OnMouseOver += (_, _) => SoundEngine.PlaySound(SoundID.MenuTick);

        Rounded = new Vector4(10f);
        Border = 2f;
    }

    public override void DrawSelf(SpriteBatch spriteBatch)
    {
        BgColor = HoverTimer.Lerp(BeginBgColor, EndBgColor);
        BorderColor = HoverTimer.Lerp(BeginBorderColor, EndBorderColor);
        base.DrawSelf(spriteBatch);

        Vector2 innerPos = GetInnerDimensions().Position();
        Vector2 innerSize = GetInnerDimensionsSize();

        if (IconMode)
        {
            Vector2 texturePos = innerPos + new Vector2(0, (innerSize.Y - _texture.Size().Y) / 2);
            spriteBatch.Draw(_texture, texturePos, Color.White);
        }

        Vector2 textPos = innerPos;

        if (IconMode)
        {
            textPos += new Vector2(_texture.Width + 2 + iconAndTextSpacing, (innerSize.Y - TextSize.Y) / 2);
        }
        else
        {
            textPos += (innerSize - TextSize) * TextAlign;
        }

        textPos.Y -= 2f;
        textPos.Y += UIConfigs.Instance.GeneralFontOffsetY;
        DynamicSpriteFont font = FontAssets.MouseText.Value;
        TextSnippet[] array = ChatManager.ParseMessage(_text, TextColor).ToArray();
        ChatManager.ConvertNormalSnippets(array);
        if (TextHasBorder)
            ChatManager.DrawColorCodedStringShadow(spriteBatch, font, array, textPos, new Color(0, 0, 0, TextColor.A),
                0f, Vector2.Zero, Vector2.One, spread: 1f);
        ChatManager.DrawColorCodedString(spriteBatch, font, array, textPos, Color.White, 0f, Vector2.Zero, Vector2.One, out int _, -1f);
        // TrUtils.DrawBorderString(spriteBatch, _text, textPos, TextColor);
    }
}
