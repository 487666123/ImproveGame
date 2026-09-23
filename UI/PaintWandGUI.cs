using ImproveGame.Common.ModSystems;
using ImproveGame.Content.Items;
using ImproveGame.UIFramework;
using ImproveGame.UIFramework.UIElements;

namespace ImproveGame.UI
{
    public class PaintWandGUI : UIState
    {
        public static bool Visible;
        public UIElement MainPanel;
        public RoundButton[] RoundButtons;
        public AnimationTimer Timer;

        public override void OnInitialize()
        {
            Timer = new() { OnClosed = () => Visible = false };

            MainPanel = new();
            MainPanel.SetPadding(0);
            MainPanel.Width.Pixels = 120;
            MainPanel.Height.Pixels = 120;
            Append(MainPanel);

            Main.instance.LoadItem(ItemID.Paintbrush);
            Main.instance.LoadItem(ItemID.PaintRoller);
            Main.instance.LoadItem(ItemID.PaintScraper);

            RoundButtons = new RoundButton[] {
                new(TextureAssets.Item[ItemID.Paintbrush])
                {
                    Selected =() => WandSystem.PaintWandMode == WandSystem.PaintMode.Tile
                },
                new(TextureAssets.Item[ItemID.PaintRoller])
                {
                    Selected = () => WandSystem.PaintWandMode == WandSystem.PaintMode.Wall
                },
                new(TextureAssets.Item[ItemID.PaintScraper])
                {
                    Selected = () => WandSystem.PaintWandMode == WandSystem.PaintMode.Remove
                }
            };

            RoundButtons[0].OnLeftClick += (_, _) => WandSystem.PaintWandMode = WandSystem.PaintMode.Tile;
            RoundButtons[1].OnLeftClick += (_, _) => WandSystem.PaintWandMode = WandSystem.PaintMode.Wall;
            RoundButtons[2].OnLeftClick += (_, _) => WandSystem.PaintWandMode = WandSystem.PaintMode.Remove;
            MainPanel.Append(RoundButtons[0]);
            MainPanel.Append(RoundButtons[1]);
            MainPanel.Append(RoundButtons[2]);
        }

        public override void DrawChildren(SpriteBatch spriteBatch)
        {
            base.DrawChildren(spriteBatch);
            var position = MyUtils.MouseScreenOffset(15);
            var borderColor = new Color(135, 0, 180);
            var textColor = Color.White;
            if (RoundButtons[0].IsMouseHovering)
            {
                MyUtils.DrawString(position, MyUtils.GetText("PaintWandGUI.Paintbrush"), textColor, borderColor);
                Main.LocalPlayer.cursorItemIconEnabled = false;
            }
            else if (RoundButtons[1].IsMouseHovering)
            {
                MyUtils.DrawString(position, MyUtils.GetText("PaintWandGUI.PaintRoller"), textColor, borderColor);
                Main.LocalPlayer.cursorItemIconEnabled = false;
            }
            else if (RoundButtons[2].IsMouseHovering)
            {
                MyUtils.DrawString(position, MyUtils.GetText("PaintWandGUI.PaintScraper"), textColor, borderColor);
                Main.LocalPlayer.cursorItemIconEnabled = false;
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (Main.LocalPlayer.HeldItem.type != ModContent.ItemType<PaintWand>() && !Timer.AnyClose)
            {
                Close();
            }
            bool flag = false;
            foreach (var button in RoundButtons)
            {
                if (button.IsMouseHovering)
                    flag = true;
            }
            if (!Timer.AnyClose && flag)
            {
                Main.LocalPlayer.mouseInterface = true;
            }
            Timer.Update();
            UpdateButton();
        }

        // 更新按钮
        public void UpdateButton()
        {
            float includedAngle = MathF.PI * 2 / RoundButtons.Length; // 夹角
            float startAngle = -MathF.PI / 2 - includedAngle / 2; // 起始角度
            for (int i = 0; i < RoundButtons.Length; i++)
            {
                Vector2 center = MainPanel.GetInnerSizePixels() / 2f;
                float angle = startAngle + includedAngle * i;
                float length = 34f + (1 - Timer.Schedule) * 25f;
                RoundButton button = RoundButtons[i];
                button.Opacity = Timer.Schedule;
                button.SetCenterPixels(center + angle.ToRotationVector2() * length).Recalculate();
            }
        }

        public void Open()
        {
            MainPanel.SetCenterPixels(MyUtils.TransformToUIPosition(Main.MouseScreen));
            MainPanel.Recalculate();
            Timer.OpenAndResetTimer();

            UpdateButton();
            Visible = true;
        }

        public void Close()
        {
            Timer.CloseAndResetTimer();
        }

        public void ToggleMode()
        {
            if (Visible && !Timer.AnyClose)
                Close();
            else
                Open();
        }
    }
}
