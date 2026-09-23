using ImproveGame.Common.ModSystems;
using ImproveGame.Content.Items;
using ImproveGame.UIFramework;
using ImproveGame.UIFramework.UIElements;
using Terraria.GameInput;

namespace ImproveGame.UI
{
    public class BurstGUI : UIState
    {
        public static bool Visible { get; private set; }
        internal static bool MouseOnMenu;
        private static bool _mouseRightPrev;

        private Asset<Texture2D> fixedModeButton;
        private Asset<Texture2D> freeModeButton;

        private RoundButton modeButton;
        private RoundButton tileButton;
        private RoundButton wallButton;
        private RoundButton chestButton;

        public AnimationTimer Timer;

        public override void OnInitialize()
        {
            base.OnInitialize();

            Timer = new() { OnClosed = () => Visible = false };

            fixedModeButton = ModAsset.FixedMode;
            freeModeButton = ModAsset.FreeMode;
            
            modeButton = new(fixedModeButton);
            modeButton.OnLeftMouseDown += SwitchMode;
            modeButton.OnMouseOver += MouseOver;
            modeButton.OnMouseOut += MouseOut;
            modeButton.Selected += () => true;
            Append(modeButton);

            tileButton = new(ModAsset.TileMode);
            tileButton.OnMouseOver += MouseOver;
            tileButton.OnMouseOut += MouseOut;
            tileButton.Selected += () => WandSystem.TileMode;
            tileButton.OnLeftMouseDown += (_, _) =>
            {
                if (Timer.AnyClose)
                    return;
                WandSystem.TileMode = !WandSystem.TileMode;
            };
            Append(tileButton);

            wallButton = new(ModAsset.WallMode);
            wallButton.OnMouseOver += MouseOver;
            wallButton.OnMouseOut += MouseOut;
            wallButton.Selected += () => WandSystem.WallMode;
            wallButton.OnLeftMouseDown += (_, _) =>
            {
                if (Timer.AnyClose)
                    return;
                WandSystem.WallMode = !WandSystem.WallMode;
            };
            Append(wallButton);

            chestButton = new(ModAsset.ChestMode);
            chestButton.OnMouseOver += MouseOver;
            chestButton.OnMouseOut += MouseOut;
            chestButton.Selected += () => WandSystem.ChestMode;
            chestButton.OnLeftMouseDown += (_, _) =>
            {
                if (Timer.AnyClose)
                    return;
                WandSystem.ChestMode = !WandSystem.ChestMode;
            };
            Append(chestButton);
        }

        private void MouseOut(UIMouseEvent evt, UIElement listeningElement)
        {
            MouseOnMenu = false;
        }

        private void MouseOver(UIMouseEvent evt, UIElement listeningElement)
        {
            MouseOnMenu = true;
        }

        public override void Update(GameTime gameTime)
        {
            // 与蓝图相同的UI关闭机制
            if (Main.LocalPlayer.mouseInterface && !MouseOnMenu)
            {
                Close();
            }

            if (Main.LocalPlayer.dead || Main.mouseItem.type > ItemID.None ||
                Main.LocalPlayer.HeldItem?.ModItem is not MagickWand)
            {
                Close();
            }

            if (!_mouseRightPrev && Main.mouseRight)
            {
                Close();
            }

            Timer.Update();
            UpdateButton();

            _mouseRightPrev = Main.mouseRight;

            base.Update(gameTime);
            if (Timer.AnyClose)
                return;
            if (wallButton.IsMouseHovering || tileButton.IsMouseHovering || chestButton.IsMouseHovering ||
                modeButton.IsMouseHovering)
            {
                Main.LocalPlayer.mouseInterface = true;
            }
        }

        // 更新按钮
        public void UpdateButton()
        {
            Vector2 center = modeButton.GetDimensions().Center();
            float length = 44f + (1 - Timer.Schedule) * 25f;
            tileButton.Opacity = Timer.Schedule;
            tileButton.SetCenterPixels(center + new Vector2(-1, 0) * length).Recalculate();
            wallButton.Opacity = Timer.Schedule;
            wallButton.SetCenterPixels(center + new Vector2(1, 0) * length).Recalculate();
            chestButton.Opacity = Timer.Schedule;
            chestButton.SetCenterPixels(center + new Vector2(0, -1) * length).Recalculate();
            modeButton.Opacity = Timer.Schedule;
            Recalculate();
        }

        private void SwitchMode(UIMouseEvent evt, UIElement listeningElement)
        {
            if (Timer.AnyClose)
                return;
            WandSystem.FixedMode = !WandSystem.FixedMode;
            modeButton.MainTexture = WandSystem.FixedMode ? fixedModeButton : freeModeButton;
        }

        /// <summary>
        /// 打开GUI界面
        /// </summary>
        public void Open()
        {
            bool center = PlayerInput.UsingGamepad && Main.SmartCursorWanted;
            int x = center ? Main.screenWidth / 2 : Main.mouseX;
            int y = center ? Main.screenHeight / 2 - 60 : Main.mouseY;
            MyUtils.TransformToUIPosition(ref x, ref y);
            Timer.OpenAndResetTimer();
            modeButton.SetCenterPixels(x, y);
            modeButton.MainTexture = WandSystem.FixedMode ? fixedModeButton : freeModeButton;
            Visible = true;
            _mouseRightPrev = true; // 防止一打开就关闭
            Recalculate();
        }

        /// <summary>
        /// 关闭GUI界面
        /// </summary>
        public void Close()
        {
            if (Timer.AnyClose)
                return;
            Timer.CloseAndResetTimer();
            //Visible = false;
            Main.blockInput = false;
        }
    }
}