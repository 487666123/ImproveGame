using ImproveGame.Packets.NetChest;
using ImproveGame.UIFramework.Graphics2D;
using Terraria.UI.Chat;

namespace ImproveGame.UI.ExtremeStorage
{
    public class ChestSelection : ModSystem
    {
        public static bool IsSelecting;

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int rulerIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Ruler"));
            if (rulerIndex != -1)
            {
                layers.Insert(rulerIndex, new LegacyGameInterfaceLayer("ImproveGame: Pools Select", delegate
                    {
                        if (IsSelecting)
                        {
                            DrawSelectionBorder();
                        }

                        return true;
                    },
                    InterfaceScaleType.Game)
                );
            }
        }

        private static void DrawSelectionBorder()
        {
            if (!IsSelecting || ExtremeStorageGUI.Storage is null)
                return;

            if (Main.mouseRight && Main.mouseRightRelease)
            {
                IsSelecting = false;
                return;
            }

            bool clicked = Main.mouseLeft;

            Main.cursorOverride = CursorOverrideID.GamepadDefaultCursor;
            Main.cursorColor = Color.SkyBlue;

            var nearbyChestIndexes = ExtremeStorageGUI.Storage.FindAllNearbyChests();
            char groupIdentifier = ExtremeStorageGUI.RealGroup.GetIdentifier();

            // 右键点击以取消 提示
            string tip = MyUtils.GetText("UI.ExtremeStorage.AddChestTip");
            var scale = new Vector2(0.4f);
            var font = FontAssets.DeathText.Value;
            var size = ChatManager.GetStringSize(font, tip, Vector2.One);
            var center = ExtremeStorageGUI.Storage.Position.ToWorldCoordinates(24, -12) - Main.screenPosition;
            if (Main.LocalPlayer.gravDir == -1) // 记得处理重力翻转
                center.Y = Main.screenHeight - center.Y;
            var realSize = size * scale;
            var position = center - realSize / 2f;
            var rectangle = new Rectangle((int)position.X, (int)position.Y, (int)realSize.X, (int)realSize.Y);
            rectangle.Inflate(40, 30);

            Main.spriteBatch.Draw(ModAsset.BlurredBar.Value, rectangle, Color.Black * 0.5f);
            ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, font, tip,
                center, Color.Cyan, 0f, size / 2f,
                scale, spread: 1.5f);

            foreach (int index in nearbyChestIndexes)
            {
                var chest = Main.chest[index];

                // 已经归类的箱子不显示
                if (!string.IsNullOrEmpty(chest.name) && chest.name[0] == groupIdentifier)
                    continue;

                var positionInWorld = new Point(chest.x * 16, chest.y * 16);
                var hitbox = new Rectangle(positionInWorld.X, positionInWorld.Y, 32, 32);

                Color color = Color.SkyBlue;
                if (hitbox.Contains(Main.MouseWorld.ToPoint()))
                {
                    color = new Color(50, 255, 50);
                    Main.cursorColor = color;
                    Main.LocalPlayer.mouseInterface = true;

                    // 点击操作
                    if (clicked)
                    {
                        // 所有的标识符
                        var allIdentifiers = new HashSet<char> { '!', '@', '#', '$', '%', '&', '=', '*', '+', '-', '?' };

                        // 如果箱子名为空，直接赋值
                        // 如果箱子名不为空，且第一个字符是标识符，那么就把标识符替换掉
                        // 如果箱子名不为空，且第一个字符不是标识符，那么就在前面加上标识符
                        chest.name = string.IsNullOrEmpty(chest.name)
                            ? groupIdentifier.ToString()
                            : allIdentifiers.Contains(chest.name[0])
                                ? $"{groupIdentifier}{chest.name[1..]}"
                                : $"{groupIdentifier}{chest.name}";

                        ChestNamePacketByID.Send((ushort)index, chest.name);
                        // IsSelecting = false;
                        return;
                    }
                }

                var drawPosition = positionInWorld.ToVector2() - Main.screenPosition;
                drawPosition.X -= 2f;
                if (Main.LocalPlayer.gravDir == -1)
                    drawPosition.Y = Main.screenHeight - drawPosition.Y - 37;
                SDFRectangle.HasBorder(drawPosition, hitbox.Size() + new Vector2(4f), new Vector4(4), color * 0.2f, 2f,
                    color, Main.GameViewMatrix.ZoomMatrix);
            }
        }
    }
}