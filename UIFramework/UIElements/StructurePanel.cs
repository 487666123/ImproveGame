using ImproveGame.Common.ModSystems;
using ImproveGame.Content.Functions.Construction;
using ImproveGame.UIFramework.Common;
using ImproveGame.UIFramework.SUIElements;
using Microsoft.Xna.Framework.Input;
using Terraria.GameInput;
using Terraria.Utilities;

namespace ImproveGame.UIFramework.UIElements
{
    internal class StructurePanel : SUIPanel
    {
        public string FilePath { get; private set; }
        public string Name { get; private set; }

        private string _inputName;  // 用于先装输入缓存的
        private bool _renaming;
        private int _cursorTimer;
        private bool _oldMouseLeft;
        private string _selectedButtonName = "";

        public static Color BorderSelectedColor => UIStyle.PanelBorderLight;
        public static Color BorderUnselectedColor => UIStyle.PanelBorder;
        public static Color SelectedColor => UIStyle.PanelBgLight;
        public static Color UnselectedColor => UIStyle.PanelBg;

        public UIText NameText;
        public UIText PathText;
        public UIImageButton RenameButton;
        public SUIPanel PathPanel;

        public StructurePanel(string filePath) : base(BorderUnselectedColor, UnselectedColor)
        {
            FilePath = filePath;

            _oldMouseLeft = true;

            Width = StyleDimension.FromPixels(580f);

            string name = FilePath.Split('\\').Last();
            name = name[..^FileOperator.Extension.Length]; // name.Substring(0, name.Length - FileOperator.Extension.Length)
            Name = name;
            NameText = new(name, 1.05f)
            {
                Left = StyleDimension.FromPixels(2f),
                Height = StyleDimension.FromPixels(24f)
            };
            Append(NameText);

            var buttonNameText = new UIText("")
            {
                Left = StyleDimension.FromPercent(1f),
                Top = StyleDimension.FromPixels(4f),
                Height = StyleDimension.FromPixels(20f)
            };
            buttonNameText.OnUpdate += (_) =>
            {
                string text = Language.GetTextValue(_selectedButtonName);
                var font = FontAssets.MouseText.Value;
                buttonNameText.SetText(text);
                buttonNameText.Left = new StyleDimension(-font.MeasureString(text).X, 1f);
                _selectedButtonName = "";
            };
            Append(buttonNameText);

            UIHorizontalSeparator separator = new()
            {
                Top = StyleDimension.FromPixels(NameText.Height.Pixels - 2f),
                Width = StyleDimension.FromPercent(1f),
                Color = Color.Lerp(Color.White, new Color(63, 65, 151, 255), 0.85f) * 0.9f
            };
            Append(separator);

            var detailButton = new UIImageButton(Main.Assets.Request<Texture2D>("Images/UI/ButtonPlay"))
            {
                Top = new StyleDimension(separator.Height.Pixels + separator.Top.Pixels + 3f, 0f),
                Left = new StyleDimension(-20f, 1f)
            };
            detailButton.SetSize(24f, 24f);
            detailButton.OnLeftClick += DetailButtonClick;
            detailButton.OnUpdate += (_) =>
            {
                if (detailButton.IsMouseHovering)
                {
                    _selectedButtonName = "tModLoader.ModsMoreInfo";
                }
            };
            Append(detailButton);

            var deleteButton = new UIImageButton(Main.Assets.Request<Texture2D>("Images/UI/ButtonDelete"))
            {
                Top = detailButton.Top,
                Left = new StyleDimension(detailButton.Left.Pixels - 24f, 1f)
            };
            deleteButton.SetSize(24f, 24f);
            deleteButton.OnLeftClick += DeleteButtonClick;
            deleteButton.OnUpdate += (_) =>
            {
                if (deleteButton.IsMouseHovering)
                {
                    _selectedButtonName = "UI.Delete";
                }
            };
            Append(deleteButton);

            RenameButton = new(Main.Assets.Request<Texture2D>("Images/UI/ButtonRename"))
            {
                Top = detailButton.Top,
                Left = new StyleDimension(deleteButton.Left.Pixels - 24f, 1f)
            };
            RenameButton.SetSize(24f, 24f);
            RenameButton.OnUpdate += (_) =>
            {
                if (RenameButton.IsMouseHovering)
                {
                    _selectedButtonName = "UI.Rename";
                }
            };
            Append(RenameButton);

            PathPanel = new(UIStyle.TitleBg, UIStyle.TitleBg, rounded: 10)
            {
                Top = detailButton.Top,
                OverflowHidden = true,
                PaddingLeft = 6f,
                PaddingRight = 6f,
                PaddingBottom = 0f,
                PaddingTop = 0f
            };
            PathPanel.SetSize(new(Width.Pixels + RenameButton.Left.Pixels - 44f, 23f));
            Append(PathPanel);
            PathText = new($"{MyUtils.GetText("ConstructGUI.Path")}{FilePath}", 0.7f)
            {
                Left = StyleDimension.FromPixels(2f),
                HAlign = 0f,
                VAlign = 0.5f,
                TextColor = Color.Gray
            };
            PathPanel.Append(PathText);
            SetSizedText();

            Height = StyleDimension.FromPixels(PathPanel.Top.Pixels + PathPanel.Height.Pixels + 22f);
        }

        private void DetailButtonClick(UIMouseEvent evt, UIElement listeningElement)
        {
            if (UISystem.Instance.StructureGUI is not null)
            {
                UISystem.Instance.StructureGUI.CacheSetupStructureInfos = true;
                UISystem.Instance.StructureGUI.CacheStructureInfoPath = FilePath;
            }
        }

        private void RenameButtonClick()
        {
            _inputName = Name;
            _renaming = true;
            Main.blockInput = true;
            Main.clrInput();
            SoundEngine.PlaySound(SoundID.MenuTick);
        }

        private void EndRename()
        {
            _renaming = false;
            Main.blockInput = false;
            // 原始名称有可能是空，空字符串使用Replace会报错，所以要分情况
            string newPath = Path.Combine(FileOperator.SavePath, $"{_inputName}{FileOperator.Extension}");
            if (!string.IsNullOrEmpty(Name))
                newPath = FilePath.Replace(Name, _inputName);

            if (File.Exists(newPath) && Name != _inputName)
            {
                MyUtils.AddNotification(MyUtils.GetText("ConstructGUI.RenameTip.Exists"));
                NameText.SetText(Name);
                return;
            }
            if (File.Exists(FilePath) && UISystem.Instance.StructureGUI is not null)
            {
                File.Move(FilePath, newPath);
                UISystem.Instance.StructureGUI.CacheSetupStructures = true;
                FileOperator.CallFileListChanged();
            }
        }

        private void DeleteButtonClick(UIMouseEvent evt, UIElement listeningElement)
        {
            SoundEngine.PlaySound(SoundID.MenuTick);
            if (File.Exists(FilePath))
                FileUtilities.Delete(FilePath, false);
            if (UISystem.Instance.StructureGUI is not null)
            {
                UISystem.Instance.StructureGUI.CacheSetupStructures = true;
                FileOperator.CallFileListChanged();
            }
        }

        public override void Update(GameTime gameTime)
        {
            _cursorTimer++;
            _cursorTimer %= 60;
            if (Main.mouseLeft && !_oldMouseLeft)
            {
                var renameDimensions = RenameButton.GetOuterDimensions();
                if (renameDimensions.ToRectangle().Contains(Main.MouseScreen.ToPoint()) && !_renaming)
                {
                    RenameButtonClick();
                }
                else if (_renaming)
                {
                    EndRename();
                }
            }

            base.Update(gameTime);

            if (IsMouseHovering)
            {
                BorderColor = BorderSelectedColor;
                BgColor = SelectedColor;
                NameText.TextColor = Color.White;
            }
            else
            {
                BorderColor = BorderUnselectedColor;
                BgColor = UnselectedColor;
                NameText.TextColor = Color.LightGray;
            }
            if (WandSystem.ConstructFilePath == FilePath)
            {
                NameText.TextColor = new(255, 231, 69);
            }
            SetSizedText();
            _oldMouseLeft = Main.mouseLeft;
        }

        public override void LeftMouseDown(UIMouseEvent evt)
        {
            base.LeftMouseDown(evt);

            if (Children.Any(i => i is UIImageButton && i.IsMouseHovering))
                return;

            SoundEngine.PlaySound(SoundID.MenuTick);
            if (WandSystem.ConstructFilePath == FilePath)
            {
                WandSystem.ConstructFilePath = string.Empty;
                return;
            }

            if (string.IsNullOrEmpty(FilePath) || !File.Exists(FilePath))
                return;

            var tag = FileOperator.GetTagFromFile(FilePath);

            if (tag is null)
            {
                return;
            }

            WandSystem.ConstructFilePath = FilePath;
            PreviewRenderer.ResetPreviewTarget = PreviewRenderer.ResetState.WaitReset;
            int width = tag.GetShort("Width");
            int height = tag.GetShort("Height");
            PreviewRenderer.PreviewTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, width * 16 + 20, height * 16 + 20, false, default, default, default, RenderTargetUsage.PreserveContents);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // 放Update，不行。放Draw，行！ReLogic我囸你_
            if (_renaming)
            {
                PlayerInput.WritingText = true;
                Main.instance.HandleIME();

                var inputText = Main.GetInputText(_inputName);
                if (inputText.Length > 40)
                {
                    inputText = inputText[..40];
                    MyUtils.AddNotification(MyUtils.GetText("ConstructGUI.RenameTip.TooLong"));
                }
                if (inputText.Contains('\\') || inputText.Contains('/') || inputText.Contains(':') || inputText.Contains('*') || inputText.Contains('?') || inputText.Contains('\"') || inputText.Contains('\'') || inputText.Contains('<') || inputText.Contains('>') || inputText.Contains('|'))
                {
                    MyUtils.AddNotification(MyUtils.GetText("PathIllegal"));
                    return;
                }
                else
                {
                    _inputName = inputText;
                    NameText.SetText(_inputName + (_cursorTimer >= 30 ? "|" : ""));
                    NameText.Recalculate();
                }

                // Enter 或者 Esc
                if (KeyTyped(Keys.Enter) || KeyTyped(Keys.Tab) || KeyTyped(Keys.Escape))
                {
                    EndRename();
                }
            }

            base.Draw(spriteBatch);
        }

        public void SetSizedText()
        {
            string pathString = MyUtils.GetText("ConstructGUI.Path");
            var innerDimensions = PathPanel.GetInnerDimensions();
            var font = FontAssets.MouseText.Value;
            float scale = 0.7f;
            float dotWidth = font.MeasureString("...").X * scale;
            float pathWidth = font.MeasureString(pathString).X * scale;
            if (font.MeasureString(FilePath).X * scale >= innerDimensions.Width - 6f - pathWidth - dotWidth)
            {
                float width = 0f;
                int i;
                for (i = FilePath.Length - 1; i > 0; i--)
                {
                    width += font.MeasureString(FilePath[i].ToString()).X * scale;
                    if (width >= innerDimensions.Width - 6f - pathWidth - dotWidth)
                    {
                        break;
                    }
                }
                PathText.SetText($"{pathString}...{FilePath[i..]}");
            }
        }

        public static bool KeyTyped(Keys key) => Main.keyState.IsKeyDown(key) && !Main.oldKeyState.IsKeyDown(key);
    }
}
