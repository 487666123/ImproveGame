using ImproveGame.Common.Configs;

namespace ImproveGame.Interface.SUIElements
{
    public class SUITitle : View
    {
        private string _text;
        private float _textScale;

        public float TextScale
        {
            get => _textScale;
            set
            {
                _textScale = value;
                TextSize = GetFontSize(_text, true) * _textScale;
            }
        }

        public Vector2 TextSize { get; private set; }

        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                TextSize = GetFontSize(_text, true) * TextScale;
            }
        }

        public Vector2 TextOffset;
        public Vector2 TextAlign = new Vector2(0.5f);
        public Color TextColor = Color.White;
        public Color TextBorderColor = Color.Transparent;

        public SUITitle(string text, float textScale)
        {
            _textScale = textScale;
            DragIgnore = true;
            Rounded = new Vector4(10f);

            Text = text;
            SetPadding(20f, 5f);
            SetInnerPixels(TextSize);
        }

        public override void DrawSelf(SpriteBatch sb)
        {
            base.DrawSelf(sb);
            Vector2 innerPos = GetInnerDimensions().Position();
            Vector2 innerSize = GetInnerDimensions().Size();
            Vector2 textOffset = (innerSize - TextSize) * TextAlign + TextOffset;
            textOffset.Y += UIConfigs.Instance.BigFontOffsetY * TextScale;
            Vector2 textPos = innerPos + textOffset;
            DrawString(textPos, _text, TextColor, TextBorderColor, TextScale, true);
        }
    }
}
