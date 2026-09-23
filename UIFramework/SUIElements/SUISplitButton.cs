using ImproveGame.UIFramework.BaseViews;
using ImproveGame.UIFramework.Common;
using ImproveGame.UIFramework.Graphics2D;

namespace ImproveGame.UIFramework.SUIElements
{
    public class SUISplitButton : View
    {
        private AnimationTimer _upButtonTimer = new(3);
        private AnimationTimer _downButtonTimer = new(3);

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // 更新Timer
            _upButtonTimer.Update();
            _downButtonTimer.Update();

            if (IsUP)
                _upButtonTimer.Open();
            else
                _upButtonTimer.Close();

            if (IsDown)
                _downButtonTimer.Open();
            else
                _downButtonTimer.Close();
        }

        public override void DrawSelf(SpriteBatch spriteBatch)
        {
            // base.DrawSelf(spriteBatch);
            var dimension = GetDimensions();
            var pos = dimension.Position();
            var size = dimension.Size();

            float h = (pos + size * Vector2.UnitY * .25f).Y;
            var flip = Matrix.CreateTranslation(0, -h, 0) * Matrix.CreateScale(1, -1, 1) * Matrix.CreateTranslation(0, h, 0);

            Color upColor = _upButtonTimer.Lerp(UIStyle.SliderRound, UIStyle.SliderRoundHover);
            SDFGraphics.HasBorderTriangleIsosceles(pos + size * Vector2.UnitY * .05f + size * Vector2.UnitX * .5f, new Vector2(.5f, 0), size * new Vector2(.75f, .4f), buttonColor, 1, upColor, flip * MyUtils.GetMatrix(true));

            Color downColor = _downButtonTimer.Lerp(UIStyle.SliderRound, UIStyle.SliderRoundHover);
            SDFGraphics.HasBorderTriangleIsosceles(pos + size * Vector2.UnitY * .55f + size * Vector2.UnitX * .5f, new Vector2(.5f, 0), size * new Vector2(.75f, .4f), buttonColor, 1, downColor, MyUtils.GetMatrix(true));
        }
        public Color buttonColor;
        public Color buttonBorderColor;
        // Cy修改：添加了IgnoresMouseInteraction判断，在这个属性为true时，不会响应鼠标悬停事件（不会高光边框）
        public bool IsUP => !IgnoresMouseInteraction && IsMouseHovering && Main.mouseY < GetDimensions().ToRectangle().Center.Y;
        public bool IsDown => !IgnoresMouseInteraction && IsMouseHovering && Main.mouseY >= GetDimensions().ToRectangle().Center.Y;
    }
}
