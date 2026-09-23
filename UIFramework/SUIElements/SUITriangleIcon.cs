using ImproveGame.UIFramework.BaseViews;
using ImproveGame.UIFramework.Common;
using ImproveGame.UIFramework.Graphics2D;

namespace ImproveGame.UIFramework.SUIElements
{
    public class SUITriangleIcon : TimerView
    {
        public float CrossBorder;
        public Color TriangleBorderColor, TriangleBorderHoverColor, TriangleBeginColor, TriangleEndColor;
        public Vector2[] trianglePercentCoord;

        public SUITriangleIcon()
        {
            SetSizePixels(50f, 50f);
            trianglePercentCoord = [new(0.5f, 0.0f), new(1.0f, 0.5f), new(0.5f, 1.0f)];
            TriangleBeginColor = UIStyle.Cross * 0.5f;
            TriangleEndColor = UIStyle.Cross;
            CrossBorder = UIStyle.CrossBorderSize;
            TriangleBorderColor = UIStyle.PanelBorder;
            TriangleBorderHoverColor = UIStyle.ItemSlotBorderFav;

            // MarginRight = 2f;
            Rounded = new Vector4(10f);
            Border = 2;
            BorderColor = UIStyle.PanelBorder;
        }

        public override void MouseOver(UIMouseEvent evt)
        {
            base.MouseOver(evt);
            SoundEngine.PlaySound(SoundID.MenuTick);
        }

        public override void DrawSelf(SpriteBatch sb)
        {
            var borderColor = HoverTimer.Lerp(TriangleBorderColor, TriangleBorderHoverColor);
            base.DrawSelf(sb);
            Vector2 pos = GetDimensions().Position();
            Vector2 size = GetDimensions().Size();
            Color fork = HoverTimer.Lerp(TriangleBeginColor, TriangleEndColor);
            SDFGraphics.HasBorderTriangle(pos + size * trianglePercentCoord[0], pos + size * trianglePercentCoord[1], pos + size * trianglePercentCoord[2], fork, CrossBorder, borderColor, MyUtils.GetMatrix(true));
        }
    }
}
