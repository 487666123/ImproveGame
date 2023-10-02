using ImproveGame.Common.Animations;
using ImproveGame.Common.Configs;
using ImproveGame.Interface.Common;
using Terraria.DataStructures;
using Terraria.ModLoader.UI;

namespace ImproveGame.Interface.ExtremeStorage
{
    public class LongSwitch : View
    {
        private const float InnerRowSpacing = 8;
        private readonly Func<bool> _getState;
        private readonly Action<bool> _setState;

        private bool State
        {
            get => _getState();
            set => _setState(value);
        }

        private readonly string _text;
        private readonly Vector2 _textSize;
        private readonly Color _textColor = Color.White;
        private readonly Color _textBorderColor = Color.Black;
        private readonly AnimationTimer _timer = new (4);
        private readonly bool _hasTooltip;
        private Asset<Texture2D> _icon;
        private SpriteFrame _iconFrame;

        public LongSwitch(Func<bool> getState, Action<bool> setState, string text, bool hasTooltip = true)
        {
            _text = text;
            _textSize = GetFontSize(text);
            this._getState = getState;
            this._setState = setState;
            _hasTooltip = hasTooltip;
            
            Spacing = new Vector2(0f, 4f);

            Width.Set(0f, 1f);
            Height.Set(40f, 0f);
        }
        
        public void SetIcon(Asset<Texture2D> icon, SpriteFrame frame)
        {
            _icon = icon;
            _iconFrame = frame;
        }

        public override void Update(GameTime gameTime)
        {
            _timer.Update();
            base.Update(gameTime);
            if (State)
            {
                _timer.Open();
            }
            else
            {
                _timer.Close();
            }
        }

        public override void LeftMouseDown(UIMouseEvent evt)
        {
            base.LeftMouseDown(evt);
            State = !State;
            SoundEngine.PlaySound(SoundID.MenuTick);
        }

        public override void DrawSelf(SpriteBatch sb)
        {
            Color color = Color.Lerp(UIColor.SwitchBg, UIColor.SwitchBgHover, _timer.Schedule);
            Color color2 = Color.Lerp(UIColor.SwitchBorder, UIColor.SwitchBorderHover, _timer.Schedule);
            Color color3 = Color.Lerp(UIColor.SwitchRound, UIColor.SwitchRoundHover, _timer.Schedule);

            var dimensions = GetDimensions();
            var position = dimensions.Position();
            var center = dimensions.Center();
            var size = dimensions.Size();

            // 背景板
            var panelColor = IsMouseHovering ? UIColor.PanelBgLightHover : UIColor.PanelBgLight;
            SDFRectangle.NoBorder(position, size, new Vector4(8f), panelColor);

            // 开关
            var boxSize = new Vector2(48, 26);
            var boxPosition = new Vector2(position.X + size.X - boxSize.X - 6f, position.Y);
            
            Vector2 position1 = boxPosition + new Vector2(0, size.Y / 2 - boxSize.Y / 2);
            SDFRectangle.HasBorder(position1, boxSize, new Vector4(MathF.Min(boxSize.X, boxSize.Y) / 2), color, 2, color2);

            Vector2 boxSize2 = new(boxSize.Y - 10);
            Vector2 position2 = boxPosition + Vector2.Lerp(new Vector2(3 + 2, size.Y / 2 - boxSize2.Y / 2),
                new Vector2(boxSize.X - 3 - 2 - boxSize2.X, size.Y / 2 - boxSize2.Y / 2), _timer.Schedule);
            SDFGraphic.NoBorderRound(position2, boxSize2.X, color3);

            var textOffsetX = 0;
            if (_icon is not null)
            {
                var icon = _icon.Value;
                var drawPosition = new Vector2(position.X + 6, center.Y - 2);
                var alignment = Alignment.Left;
                Rectangle sourceRectangle = _iconFrame.GetSourceRectangle(icon);
                Vector2 origin = sourceRectangle.Size() * alignment.OffsetMultiplier;
                sb.Draw(icon, drawPosition, sourceRectangle, Color.White, 0f, origin, 1f, SpriteEffects.None, 0f);
                textOffsetX += sourceRectangle.Width;
            }

            // 文字
            string text = GetText(!_hasTooltip ? _text : $"{_text}.Label");
            var textCenter = new Vector2(position.X + 10 + textOffsetX, center.Y - _textSize.Y / 2f + UIConfigs.Instance.GeneralFontOffsetY);
            textCenter.Y -= 4f;
            DrawString(textCenter, text, _textColor, _textBorderColor);

            // 提示
            if (IsMouseHovering && _hasTooltip)
            {
                UICommon.TooltipMouseText(GetText($"{_text}.Tooltip"));
            }
        }
    }
}