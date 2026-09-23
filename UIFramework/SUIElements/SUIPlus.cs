using ImproveGame.UIFramework.BaseViews;
using ImproveGame.UIFramework.Common;
using ImproveGame.UIFramework.Graphics2D;

namespace ImproveGame.UIFramework.SUIElements
{
    public class SUIPlus : TimerView
    {
        public float PlusSize, PlusRounded, PlusBorder;
        public Color PlusBorderColor, PlusBorderHoverColor, PlusBeginColor, PlusEndColor;

        public Vector2 CrossOffset;

        public SUIPlus()
        {
            SetSizePixels(50f, 50f);

            PlusSize = 24f;
            PlusRounded = UIStyle.CrossThickness;
            PlusBeginColor = UIStyle.Cross * 0.5f;
            PlusEndColor = UIStyle.Cross;
            PlusBorder = UIStyle.CrossBorderSize;
            PlusBorderColor = UIStyle.PanelBorder;
            PlusBorderHoverColor = UIStyle.ItemSlotBorderFav;

            // MarginRight = 2f;
            Rounded = new Vector4(4f);
            Border = 2;
            BorderColor = UIStyle.PanelBorder;
        }

        public override void MouseOver(UIMouseEvent evt)
        {
            base.MouseOver(evt);
            //SoundEngine.PlaySound(SoundID.MenuTick);
        }

        public override void DrawSelf(SpriteBatch sb)
        {
            var borderColor = HoverTimer.Lerp(PlusBorderColor, PlusBorderHoverColor);
            base.DrawSelf(sb);
            Vector2 pos = GetDimensions().Position();
            Vector2 size = GetDimensions().Size();
            Color fork = HoverTimer.Lerp(PlusBeginColor, PlusEndColor);
            //Vector2 forkPos = pos + (size - new Vector2(PlusSize)) / 2f;
            //SDFGraphics.HasBorderCross(forkPos + CrossOffset, default, PlusSize, PlusRounded, fork, PlusBorder, borderColor, GetMatrix(true));

            SDFGraphics.HasBorderPlus(GetDimensions().Center(), new Vector2(.5f), PlusSize * .6f, PlusSize * .3f, PlusRounded, fork, PlusBorder, borderColor, MyUtils.GetMatrix(true));
        }
    }
}
