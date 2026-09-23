using Terraria.GameContent.UI.States;
using Terraria.ModLoader.Config.UI;
using Terraria.ModLoader.UI;
using Terraria.UI.Chat;

namespace ImproveGame.Common.Configs.Elements;

public class OtherFunctionsElement : ConfigElement
{
    private string _content;
    private bool _expanded;
    private UIPanel _expandedPanel;
    private UIPanel _unexpandedPanel;
    private const float RegularHeight = 36f;

    public override void OnBind()
    {
        base.OnBind();
        DrawLabel = false;

        _unexpandedPanel = new UIPanel
        {
            BorderColor = Color.Transparent,
            BackgroundColor = Color.Transparent,
            Width = { Percent = 1f },
            Height = { Pixels = RegularHeight }
        };
        _unexpandedPanel.SetPadding(0f);

        _unexpandedPanel.Append(new UIText(Label, 0.4f, true)
        {
            TextOriginX = 0.5f,
            TextOriginY = 0.5f,
            Width = StyleDimension.Fill,
            Height = StyleDimension.Fill
        });

        Append(_unexpandedPanel);

        _expandedPanel = new UIPanel
        {
            BorderColor = Color.Transparent,
            BackgroundColor = Color.Transparent,
            Width = { Percent = 1f },
            Height = { Pixels = RegularHeight }
        };
        _expandedPanel.SetPadding(0f);

        _expandedPanel.Append(new UIText(Label, 0.6f, true)
        {
            Top = { Pixels = 8f },
            Width = { Percent = 1f },
            Height = { Pixels = 40f },
            TextOriginX = 0.5f,
            TextOriginY = 0.5f
        });

        _expandedPanel.Append(new UIText(
            MyUtils.GetText("Configs.ImproveConfigs.OtherFunctions.Subtitle"),
            0.7f)
        {
            Top = { Pixels = 40f },
            Width = { Percent = 1f },
            Height = { Pixels = 40f },
            TextOriginX = 0.5f,
            TextOriginY = 0.5f
        });

        // var uiText = new UIText(TooltipFunction(), 0.9f) {
        //     Top = {Pixels = 82f},
        //     Left = {Pixels = 10f},
        //     Width = {Pixels = -20f, Percent = 1f},
        //     IsWrapped = true,
        // };
        // uiText.OnInternalTextChange += () => {
        //     float newHeight = uiText.MinHeight.Pixels + 80f;
        //     Height.Set(newHeight, 0f);
        //
        //     if (Parent is UISortableElement) {
        //         Parent.Height.Pixels = newHeight;
        //     }
        // };
        // Append(uiText);

        _content = TooltipFunction();
        TooltipFunction = null;

        OnLeftClick += (_, _) => _expanded = !_expanded;
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        if (!_expanded)
        {
            _expandedPanel.Remove();
            Append(_unexpandedPanel);
        }
        else
        {
            _unexpandedPanel.Remove();
            Append(_expandedPanel);
        }
    }

    public override void DrawSelf(SpriteBatch spriteBatch)
    {
        base.DrawSelf(spriteBatch);

        if (!_expanded)
        {
            Height.Set(RegularHeight, 0f);
            if (Parent is UISortableElement)
                Parent.Height.Pixels = RegularHeight;

            if (IsMouseHovering)
                UICommon.TooltipMouseText(MyUtils.GetText("Configs.ImproveConfigs.OtherFunctions.ExpandTip"));

            return;
        }

        var dimensions = GetInnerDimensions();
        var font = FontAssets.MouseText.Value;
        const float textScale = 0.9f;
        float width = dimensions.Width - 50f;
        var entries = _content.Split('\n');
        float currentY = 82f + dimensions.Y;

        for (var i = 0; i < entries.Length; i++)
        {
            string entry = entries[i];
            string visibleText = font.CreateWrappedText(entry, width / textScale, System.Globalization.CultureInfo.CurrentCulture);
            float x = 40f + dimensions.X;
            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, visibleText, new Vector2(x, currentY),
                Color.White, 0f, Vector2.Zero, new Vector2(textScale), spread: 1.2f);

            int entryNumber = i + 1;
            string entryIdentifier = entryNumber + ". ";
            float numberWidth = font.MeasureString(entryIdentifier).X * textScale;
            x -= numberWidth;
            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, entryIdentifier, new Vector2(x, currentY),
                Color.White, 0f, Vector2.Zero, new Vector2(textScale), spread: 1.2f);

            currentY += font.LineSpacing * (visibleText.Count(c => c == '\n') + 1) * textScale;
        }

        float height = currentY - dimensions.Y + 10f;
        Height.Set(height, 0f);
        if (Parent is UISortableElement)
        {
            Parent.Height.Pixels = height;
        }
    }
}