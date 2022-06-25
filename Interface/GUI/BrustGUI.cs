﻿using ImproveGame.Common.Systems;
using ImproveGame.Interface.UIElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.ID;

namespace ImproveGame.Interface.GUI
{
    public class BrustGUI : UIState
    {
        public static bool Visible { get; private set; }
        internal static bool MouseOnMenu;

        private static Asset<Texture2D> fixedModeButton;
        private static Asset<Texture2D> freeModeButton;

        private static ModImageButton modeButton;
        private static ModImageButton tileButton;
        private static ModImageButton wallButton;

        public override void OnInitialize() {
            base.OnInitialize();

            fixedModeButton = ModContent.Request<Texture2D>("ImproveGame/Assets/Images/UI/Brust/FixedMode");
            freeModeButton = ModContent.Request<Texture2D>("ImproveGame/Assets/Images/UI/Brust/FreeMode");
            Asset<Texture2D> hoverImage = ModContent.Request<Texture2D>("ImproveGame/Assets/Images/UI/Brust/Hover");
            Asset<Texture2D> backgroundImage = ModContent.Request<Texture2D>("ImproveGame/Assets/Images/UI/Brust/Background");
            var inactiveColor = new Color(120, 120, 120);
            var activeColor = Color.White;

            modeButton = new ModImageButton(
                fixedModeButton,
                activeColor: activeColor, inactiveColor: inactiveColor);
            modeButton.SetHoverImage(hoverImage);
            modeButton.SetBackgroundImage(backgroundImage);
            modeButton.Width.Set(40, 0f);
            modeButton.Height.Set(40, 0f);
            modeButton.DrawColor += () => Color.White;
            modeButton.OnMouseDown += SwitchMode;
            modeButton.OnMouseOver += MouseOver;
            modeButton.OnMouseOut += MouseOut;
            Append(modeButton);

            tileButton = new ModImageButton(
                ModContent.Request<Texture2D>("ImproveGame/Assets/Images/UI/Brust/TileMode"),
                activeColor: activeColor, inactiveColor: inactiveColor);
            tileButton.SetHoverImage(hoverImage);
            tileButton.SetBackgroundImage(backgroundImage);
            tileButton.Width.Set(40, 0f);
            tileButton.Height.Set(40, 0f);
            tileButton.OnMouseOver += MouseOver;
            tileButton.OnMouseOut += MouseOut;
            tileButton.DrawColor += () => BrustWandSystem.TileMode ? Color.White : inactiveColor;
            tileButton.OnMouseDown += (UIMouseEvent _, UIElement _) => BrustWandSystem.TileMode = !BrustWandSystem.TileMode;
            Append(tileButton);

            wallButton = new ModImageButton(
                ModContent.Request<Texture2D>("ImproveGame/Assets/Images/UI/Brust/WallMode"),
                activeColor: activeColor, inactiveColor: inactiveColor);
            wallButton.SetHoverImage(hoverImage);
            wallButton.SetBackgroundImage(backgroundImage);
            wallButton.Width.Set(40, 0f);
            wallButton.Height.Set(40, 0f);
            wallButton.OnMouseOver += MouseOver;
            wallButton.OnMouseOut += MouseOut;
            wallButton.DrawColor += () => BrustWandSystem.WallMode ? Color.White : inactiveColor;
            wallButton.OnMouseDown += (UIMouseEvent _, UIElement _) => BrustWandSystem.WallMode = !BrustWandSystem.WallMode;
            Append(wallButton);
        }

        private void MouseOut(UIMouseEvent evt, UIElement listeningElement) {
            MouseOnMenu = false;
        }

        private void MouseOver(UIMouseEvent evt, UIElement listeningElement) {
            MouseOnMenu = true;
        }

        public override void Update(GameTime gameTime) {
            // 与蓝图相同的UI关闭机制
            if (Main.LocalPlayer.mouseInterface && !MouseOnMenu) {
                Close();
                return;
            }

            if (Main.LocalPlayer.dead || Main.mouseItem.type > ItemID.None) {
                Close();
                return;
            }

            base.Update(gameTime);
        }

        private void SwitchMode(UIMouseEvent evt, UIElement listeningElement) {
            BrustWandSystem.FixedMode = !BrustWandSystem.FixedMode;
            modeButton.SetImage(BrustWandSystem.FixedMode ? fixedModeButton : freeModeButton);
        }

        /// <summary>
        /// 打开GUI界面
        /// </summary>
        public static void Open() {
            int x = Main.mouseX;
            int y = Main.mouseY;
            MyUtils.TransformToUIPosition(ref x, ref y);
            modeButton.SetCenter(x, y);
            tileButton.SetCenter(x - 44, y);
            wallButton.SetCenter(x + 44, y);
            modeButton.SetImage(BrustWandSystem.FixedMode ? fixedModeButton : freeModeButton);
            Visible = true;
        }

        /// <summary>
        /// 关闭GUI界面
        /// </summary>
        public static void Close() {
            Visible = false;
            Main.blockInput = false;
        }
    }
}