using ImproveGame.Common.Configs;
using ImproveGame.Common.ModSystems;
using ImproveGame.Content.Functions.Construction;
using ImproveGame.UIFramework.BaseViews;
using ImproveGame.UIFramework.Common;
using ImproveGame.UIFramework.SUIElements;
using ImproveGame.UIFramework.UIElements;
using Terraria.GameInput;

namespace ImproveGame.UI
{
    public class StructureGUI : BaseBody
    {
        public static bool Visible { get; private set; }

        public override bool Enabled { get => Visible; set => Visible = value; }

        public override bool CanSetFocusTarget(UIElement target)
            => (target != this && BasePanel.IsMouseHovering) || BasePanel.IsLeftMousePressed;

        public bool CacheSetupStructures; // 缓存，在下一帧Setup
        public bool CacheSetupStructureInfos; // 缓存，在下一帧Setup
        public string CacheStructureInfoPath;

        private int oldScreenHeight;

        public Asset<Texture2D> RefreshTexture;
        public Asset<Texture2D> BackTexture;
        public Asset<Texture2D> ButtonBackgroundTexture;

        private SUIPanel BasePanel; // 背景板
        public SUIScrollBar Scrollbar; // 拖动条
        public UIList ComponentList; // 明细列表
        public ModImageButton RefreshButton; // 刷新/回退按钮

        public override void OnInitialize()
        {
            var saveTexture = MyUtils.GetTexture("UI/Construct/Save");
            var loadTexture = MyUtils.GetTexture("UI/Construct/Load");
            var explodeAndPlaceTexture = MyUtils.GetTexture("UI/Construct/ExplodeAndPlace");
            var placeOnlyTexture = MyUtils.GetTexture("UI/Construct/PlaceOnly");
            RefreshTexture = MyUtils.GetTexture("UI/Construct/Refresh");
            BackTexture = MyUtils.GetTexture("UI/Construct/Back");
            ButtonBackgroundTexture = Main.Assets.Request<Texture2D>("Images/UI/CharCreation/CategoryPanel");

            BasePanel = new SUIPanel(UIStyle.PanelBorder, UIStyle.PanelBg)
            {
                Shaded = true,
                Top = StyleDimension.FromPixels(150f),
                HAlign = 0.5f
            };
            BasePanel.SetSize(600f, 0f, precentHeight: 0.6f).SetPadding(12f);
            Append(BasePanel);

            ComponentList = new UIList
            {
                Width = StyleDimension.FromPercent(1f),
                Height = StyleDimension.FromPercent(1f),
                PaddingBottom = 4f,
                PaddingTop = 4f,
                ListPadding = 4f,
            };
            ComponentList.SetPadding(2f);
            ComponentList.ManualSortMethod = _ => { }; // 阻止他自动排序
            BasePanel.Append(ComponentList);

            Scrollbar = new();
            Scrollbar.Left.Set(310f, 0.5f);
            Scrollbar.Top.Set(154f, 0f);
            Scrollbar.Height.Set(-8f, 0.6f);
            Scrollbar.SetView(100f, 1000f);
            //UIList.SetScrollbar(Scrollbar); // 用自己的代码
            SetupScrollBar();
            Append(Scrollbar);

            RefreshButton = QuickButton(RefreshTexture, "{$Mods.ImproveGame.Common.Refresh}");
            RefreshButton.SetPos(new(-296f, 100f), 0.5f, 0f);
            RefreshButton.OnLeftMouseDown += (_, _) =>
            {
                FileOperator.CachedStructureDatas.Clear();
                SetupStructuresList();
            };
            Append(RefreshButton);

            var folderButton = QuickButton(MyUtils.GetTexture("UI/Construct/Folder"), "{$LegacyInterface.110}");
            folderButton.SetPos(new(-246f, 100f), 0.5f, 0f);
            folderButton.OnLeftMouseDown += (_, _) => TrUtils.OpenFolder(FileOperator.SavePath);
            Append(folderButton);

            var modeButton = QuickButton(saveTexture, "");
            modeButton.SetPos(new(-196f, 100f), 0.5f, 0f);
            modeButton.OnLeftMouseDown += (_, _) =>
            {
                if (WandSystem.ConstructMode == WandSystem.Construct.Place)
                    WandSystem.ConstructMode = WandSystem.Construct.Save;
                else
                    WandSystem.ConstructMode = WandSystem.Construct.Place;
            };
            modeButton.OnUpdate += (_) =>
            {
                if (WandSystem.ConstructMode == WandSystem.Construct.Place)
                {
                    modeButton.SetImage(loadTexture);
                    modeButton.HoverText = "{$Mods.ImproveGame.ConstructGUI.LoadMode}";
                }
                else
                {
                    modeButton.SetImage(saveTexture);
                    modeButton.HoverText = "{$Mods.ImproveGame.ConstructGUI.SaveMode}";
                }
            };
            Append(modeButton);

            var explodeButton = QuickButton(saveTexture, "");
            explodeButton.SetPos(new(-146f, 100f), 0.5f, 0f);
            explodeButton.OnLeftMouseDown += (_, _) =>
            {
                if (WandSystem.ExplodeMode == WandSystem.Construct.Place)
                    WandSystem.ExplodeMode = WandSystem.Construct.ExplodeAndPlace;
                else
                    WandSystem.ExplodeMode = WandSystem.Construct.Place;
            };
            explodeButton.OnUpdate += (_) =>
            {
                if (WandSystem.ExplodeMode == WandSystem.Construct.Place)
                {
                    explodeButton.SetImage(placeOnlyTexture);
                    explodeButton.HoverText = "{$Mods.ImproveGame.ConstructGUI.PlaceOnly}";
                }
                else
                {
                    explodeButton.SetImage(explodeAndPlaceTexture);
                    explodeButton.HoverText = "{$Mods.ImproveGame.ConstructGUI.ExplodeAndPlace}";
                }
            };
            Append(explodeButton);

            var closeButton = QuickButton(MyUtils.GetTexture("UI/Construct/Close"), "{$LegacyInterface.71}");
            closeButton.SetPos(new(246f, 100f), 0.5f, 0f);
            closeButton.OnLeftMouseDown += (_, _) => Close();
            Append(closeButton);

            var tutorialButton = QuickButton(Main.Assets.Request<Texture2D>("Images/UI/Bestiary/Icon_Locked"), "{$Mods.ImproveGame.ConstructGUI.Tutorial.Button}");
            tutorialButton.SetPos(new(196f, 100f), 0.5f, 0f);
            tutorialButton.OnLeftMouseDown += (_, _) => SetupTutorialPage();
            Append(tutorialButton);
        }

        private ModImageButton QuickButton(Asset<Texture2D> texture, string hoverText)
        {
            var button = new ModImageButton(texture, Color.White, Color.White);
            button.SetBackgroundImage(ButtonBackgroundTexture);
            button.SetHoverImage(Main.Assets.Request<Texture2D>("Images/UI/CharCreation/CategoryPanelBorder"));
            button.SetSize(44, 44);
            button.OnMouseOver += (_, _) => SoundEngine.PlaySound(SoundID.MenuTick);
            button.HoverText = hoverText;
            return button;
        }

        private void SetupScrollBar(bool resetViewPosition = true)
        {
            float height = ComponentList.GetInnerDimensions().Height;
            float totalHeight = ComponentList.GetTotalHeight();
            Scrollbar.SetView(height, totalHeight);
            if (resetViewPosition)
                Scrollbar.BarTop = 0f;

            Scrollbar.Visible = true;
            if (height >= totalHeight)
            {
                Scrollbar.Visible = false;
            }
        }

        public override void ScrollWheel(UIScrollWheelEvent evt)
        {
            base.ScrollWheel(evt);
            if (BasePanel.IsMouseHovering || Scrollbar.IsMouseHovering)
                Scrollbar.BarTopBuffer += evt.ScrollWheelValue;
        }

        public override void DrawSelf(SpriteBatch spriteBatch)
        {
            if (Scrollbar is not null)
            {
                ComponentList._innerList.Top.Set(-Scrollbar.BarTop, 0);
            }
            ComponentList.Recalculate();

            base.DrawSelf(spriteBatch);
        }

        public override void Update(GameTime gameTime)
        {
            if (CacheSetupStructures)
            {
                SetupStructuresList();
                CacheSetupStructures = false;
            }
            if (CacheSetupStructureInfos)
            {
                SetupCurrentStructureList();
                CacheSetupStructureInfos = false;
            }

            Recalculate();

            base.Update(gameTime);

            if (BasePanel.IsMouseHovering || (Scrollbar.IsMouseHovering && Scrollbar.Visible))
            {
                if (Scrollbar.Visible)
                {
                    PlayerInput.LockVanillaMouseScroll("ImproveGame: Structure GUI");
                }
                Main.LocalPlayer.mouseInterface = true;
            }

            if (oldScreenHeight != Main.screenHeight)
            {
                SetupScrollBar(false);
            }

            oldScreenHeight = Main.screenHeight;
        }

        // 当前结构的信息
        public void SetupCurrentStructureList()
        {
            if (string.IsNullOrEmpty(CacheStructureInfoPath) || !File.Exists(CacheStructureInfoPath))
            {
                SetupStructuresList();
                return;
            }

            ComponentList.Clear();

            var structure = new QoLStructure(CacheStructureInfoPath);

            if (structure.Tag is null)
            {
                SetupStructuresList();
                return;
            }


            static UIText QuickSmallUIText(string text) => new(text)
            {
                Height = StyleDimension.FromPixels(24f),
                Width = StyleDimension.FromPercent(1f),
                TextOriginX = 0.5f,
                TextOriginY = 0f
            };

            ComponentList.Add(QuickTitleText(MyUtils.GetText("ConstructGUI.FileInfo.Title"), 0.5f));
            // 检查模组是否被加载了
            foreach ((string blockName, _) in structure.entries)
            {
                if (ModLoader.HasMod(blockName.Split('/')[0]))
                {
                    continue;
                }

                char countChar = MyUtils.GetText("ConstructGUI.FileInfo.ModMissing.Count")[0];
                int count = char.IsNumber(countChar) ? countChar - '0' : 2;
                for (int i = 1; i <= count; i++)
                {
                    var text = QuickSmallUIText(MyUtils.GetText($"ConstructGUI.FileInfo.ModMissing.{i}"));
                    text.TextColor = new(244, 208, 68);
                    ComponentList.Add(text);
                }
                break;
            }
            string name = CacheStructureInfoPath.Split('\\').Last();
            name = name[..^FileOperator.Extension.Length];
            ComponentList.Add(QuickSmallUIText(MyUtils.GetTextWith("ConstructGUI.FileInfo.Name", new { Name = name }))); // 文件名
            ComponentList.Add(QuickSmallUIText(MyUtils.GetTextWith("ConstructGUI.FileInfo.Time", new { Time = DateTime.Parse(structure.BuildTime) }))); // 保存时间
            ComponentList.Add(QuickSmallUIText(MyUtils.GetTextWith("ConstructGUI.FileInfo.Version", new { Version = $"v{structure.ModVersion}" }))); // 模组版本
            ComponentList.Add(QuickSmallUIText(MyUtils.GetTextWith("ConstructGUI.FileInfo.Size", new { Size = $"{structure.Width + 1}x{structure.Height + 1}" }))); // 结构尺寸

            var materialsAndStacks = MaterialCore.CountMaterials(structure);
            if (materialsAndStacks.Count > 0)
            {
                ComponentList.Add(QuickTitleText(MyUtils.GetText("ConstructGUI.MaterialInfo.Title"), 0.6f));
                if (ImproveConfigs.Instance.SuperVault)
                    ComponentList.Add(QuickSmallUIText(MyUtils.GetTextWith("ConstructGUI.MaterialInfo.Tip", new { Name = name }))); // 文件名

                var sortedResult = from pair in materialsAndStacks orderby pair.Key select pair; // 排序
                foreach ((int itemType, int stack) in from mat in sortedResult where mat.Value > 0 select mat)
                {
                    ComponentList.Add(new MaterialInfoElement(itemType, stack));
                }
            }

            ComponentList.Add(QuickTitleText(MyUtils.GetText("ConstructGUI.Preview.Title"), 0.85f));

            var viewPanel = new StructurePreviewPanel(CacheStructureInfoPath);
            viewPanel.OnResetHeight += (_) => SetupScrollBar(false);
            ComponentList.Add(viewPanel);

            RefreshButton.SetImage(BackTexture);
            RefreshButton.HoverText = "{$UI.Back}";

            Recalculate();
            SetupScrollBar();
        }

        // 结构列表
        public void SetupStructuresList()
        {
            ComponentList.Clear();

            var filePaths = Directory.GetFiles(FileOperator.SavePath);
            foreach (string path in filePaths)
            {
                string extension = Path.GetExtension(path);
                // 添加识别 .qolstruct，旧版支持
                if (extension == FileOperator.Extension || extension == ".qolstruct")
                {
                    ComponentList.Add(new StructurePanel(path));
                }
            }

            RefreshButton.SetImage(RefreshTexture);
            RefreshButton.HoverText = "{$Mods.ImproveGame.Common.Refresh}";

            Recalculate();
            SetupScrollBar();
        }

        // 显示“教程”
        public void SetupTutorialPage()
        {
            ComponentList.Clear();

            ComponentList.Add(QuickTitleText(MyUtils.GetText("ConstructGUI.Tutorial.Button"), 0.5f));
            char countChar = MyUtils.GetText("ConstructGUI.Tutorial.AlphaTest.Count")[0];
            int count = char.IsNumber(countChar) ? countChar - '0' : 2;
            for (int i = 1; i <= count; i++)
            {
                ComponentList.Add(new UIText(MyUtils.GetText($"ConstructGUI.Tutorial.AlphaTest.{i}"))
                {
                    TextOriginX = 0.5f,
                    Width = StyleDimension.FromPercent(1f),
                    Height = StyleDimension.FromPixels(24f),
                    TextColor = new(244, 208, 68)
                });
            }
            ComponentList.Add(QuickSeparator());

            static UIPanel QuickTransparentPanel()
            {
                var panel = (UIPanel)new UIPanel()
                {
                    BackgroundColor = Color.Transparent,
                    BorderColor = Color.Transparent
                }.SetSize(new Vector2(600f, 6f));
                panel.SetPadding(6f);
                return panel;
            }

            static void Seperate(UIList list)
            {
                list.Add(QuickTransparentPanel()); // 创建大的空域
                list.Add(QuickSeparator());
                list.Add(QuickTransparentPanel()); // 创建大的空域
            }

            #region 保存
            var panel = QuickTransparentPanel();

            ComponentList.Add(QuickTitleText(MyUtils.GetText("ConstructGUI.Tutorial.Save.Title"), 0.5f, 0f).SetPos(6f, 0f));
            var uiText = new UIText(MyUtils.GetText("ConstructGUI.Tutorial.Save.Text"))
            {
                IsWrapped = true,
                TextOriginX = 0f,
                Width = StyleDimension.FromPixels(460f)
            };
            uiText.Recalculate();
            panel.Append(uiText);

            var buttonExample = new UIImage(ButtonBackgroundTexture).SetPos(490f, -10f).SetAlign(verticalAlign: 0.5f);
            buttonExample.Append(new UIImage(MyUtils.GetTexture("UI/Construct/Save")).SetAlign(0.5f, 0.5f));
            buttonExample.OnLeftMouseDown += (_, _) => WandSystem.ConstructMode = WandSystem.Construct.Save;
            panel.Append(buttonExample);

            panel.Height = StyleDimension.FromPixels(uiText.MinHeight.Pixels - 4f);
            panel.Recalculate();

            ComponentList.Add(panel);
            int horizontalFrames = 13;
            int verticalFrames = 8;
            int totalFrames = 93;
            string gifName = "SaveStructureEn";
            if (Language.ActiveCulture.Name == "zh-Hans")
            {
                gifName = "SaveStructureZh";
            }
            ComponentList.Add(new GIFImage(gifName, horizontalFrames, verticalFrames, totalFrames, 2).SetAlign(0.5f));

            Seperate(ComponentList);
            #endregion

            #region 放置
            panel = QuickTransparentPanel();

            ComponentList.Add(QuickTitleText(MyUtils.GetText("ConstructGUI.Tutorial.Place.Title"), 0.5f, 0f).SetPos(6f, 0f));
            uiText = new UIText(MyUtils.GetText("ConstructGUI.Tutorial.Place.Text"))
            {
                IsWrapped = true,
                TextOriginX = 0f,
                Width = StyleDimension.FromPixels(470f)
            };
            uiText.Recalculate();
            panel.Append(uiText);

            buttonExample = new UIImage(ButtonBackgroundTexture).SetPos(490f, -10f).SetAlign(verticalAlign: 0.5f);
            buttonExample.Append(new UIImage(MyUtils.GetTexture("UI/Construct/Load")).SetAlign(0.5f, 0.5f));
            buttonExample.OnLeftMouseDown += (_, _) => WandSystem.ConstructMode = WandSystem.Construct.Place;
            panel.Append(buttonExample);

            panel.Height = StyleDimension.FromPixels(uiText.MinHeight.Pixels - 4f);
            panel.Recalculate();

            ComponentList.Add(panel);
            ComponentList.Add(new GIFImage("PlaceStructure", 16, 5, 77, 2).SetAlign(0.5f));

            Seperate(ComponentList);
            #endregion

            #region 爆破
            panel = QuickTransparentPanel();

            ComponentList.Add(QuickTitleText(MyUtils.GetText("ConstructGUI.Tutorial.Explode.Title"), 0.5f, 0f).SetPos(6f, 0f));
            uiText = new UIText(MyUtils.GetText("ConstructGUI.Tutorial.Explode.Text"))
            {
                IsWrapped = true,
                TextOriginX = 0f,
                Width = StyleDimension.FromPixels(430f)
            };
            uiText.Recalculate();
            panel.Append(uiText);

            buttonExample = new UIImage(ButtonBackgroundTexture).SetPos(490f, -20f).SetAlign(verticalAlign: 0.5f);
            buttonExample.Append(new UIImage(MyUtils.GetTexture("UI/Construct/ExplodeAndPlace")).SetAlign(0.5f, 0.5f));
            buttonExample.OnLeftMouseDown += (_, _) => WandSystem.ExplodeMode = WandSystem.Construct.ExplodeAndPlace;
            panel.Append(buttonExample);

            buttonExample = new UIImage(ButtonBackgroundTexture).SetPos(440f, -20f).SetAlign(verticalAlign: 0.5f);
            buttonExample.Append(new UIImage(MyUtils.GetTexture("UI/Construct/PlaceOnly")).SetAlign(0.5f, 0.5f));
            buttonExample.OnLeftMouseDown += (_, _) => WandSystem.ExplodeMode = WandSystem.Construct.Place;
            panel.Append(buttonExample);

            panel.Height = StyleDimension.FromPixels(uiText.MinHeight.Pixels - 4f);
            panel.Recalculate();

            ComponentList.Add(panel);
            ComponentList.Add(new GIFImage("ExplodePlace", 15, 5, 75, 2).SetAlign(0.5f));

            Seperate(ComponentList);
            #endregion

            #region 列表单元
            panel = QuickTransparentPanel();

            ComponentList.Add(QuickTitleText(MyUtils.GetText("ConstructGUI.Tutorial.Panel.Title"), 0.5f, 0f).SetPos(6f, 0f));
            uiText = new UIText(MyUtils.GetText("ConstructGUI.Tutorial.Panel.Text"))
            {
                IsWrapped = true,
                TextOriginX = 0f,
                Width = StyleDimension.FromPixels(562f)
            };
            uiText.Recalculate();
            panel.Append(uiText);

            panel.Height = StyleDimension.FromPixels(uiText.MinHeight.Pixels - 4f);
            panel.Recalculate();

            ComponentList.Add(panel);

            string imageName = "Tutorial_StructList_En";
            if (Language.ActiveCulture.Name == "zh-Hans")
                imageName = "Tutorial_StructList_Zh";
            var uiImage = new UIImage(MyUtils.GetTexture($"UI/Construct/{imageName}"));
            ComponentList.Add(uiImage);

            Seperate(ComponentList);
            #endregion

            #region 材料明细
            panel = QuickTransparentPanel();

            panel.Append(QuickTitleText(MyUtils.GetText("ConstructGUI.Tutorial.Materials.Title"), 0.5f, 0f).SetPos(0f, -10f));
            uiText = new UIText(MyUtils.GetText("ConstructGUI.Tutorial.Materials.Text"))
            {
                IsWrapped = true,
                TextOriginX = 0f,
                Top = StyleDimension.FromPixels(50f),
                Width = StyleDimension.FromPixels(260f)
            };
            uiText.Recalculate();
            panel.Append(uiText);

            var dollList = new UIList
            {
                Left = StyleDimension.FromPixels(260f),
                Width = StyleDimension.FromPixels(325f),
                Height = StyleDimension.FromPercent(1f),
                PaddingBottom = 4f,
                PaddingTop = 4f,
                ListPadding = 4f,
            };
            dollList.SetPadding(2f);
            dollList.ManualSortMethod = (list) => { }; // 阻止他自动排序
            dollList.Add(new MaterialInfoDoll(ItemID.Torch, 3));
            dollList.Add(new MaterialInfoDoll(ItemID.Wood, 45));
            dollList.Add(new MaterialInfoDoll(ItemID.WoodenChair, 3));
            dollList.Add(new MaterialInfoDoll(ItemID.WorkBench, 3));
            dollList.Add(new MaterialInfoDoll(ItemID.WoodWall, 87));
            dollList.Add(new MaterialInfoDoll(ItemID.WoodPlatform, 28));
            panel.Append(dollList);

            panel.Height = StyleDimension.FromPixels(Math.Max(uiText.MinHeight.Pixels + 50f, dollList.GetTotalHeight()) + 10f);
            panel.Recalculate();

            ComponentList.Add(panel);

            Seperate(ComponentList);
            #endregion

            #region 结构预览
            panel = QuickTransparentPanel();

            panel.Append(QuickTitleText(MyUtils.GetText("ConstructGUI.Tutorial.Preview.Title"), 0.5f, 0f).SetPos(0f, -10f));
            uiText = new UIText(MyUtils.GetText("ConstructGUI.Tutorial.Preview.Text"))
            {
                IsWrapped = true,
                TextOriginX = 0f,
                Top = StyleDimension.FromPixels(50f),
                Width = StyleDimension.FromPixels(280f)
            };
            uiText.Recalculate();
            panel.Append(uiText);

            uiImage = new UIImage(MyUtils.GetTexture("UI/Construct/Tutorial_Preview"));
            uiImage.SetPos(290f, 8f).SetAlign(verticalAlign: 0.5f);
            panel.Append(uiImage);

            panel.Height = StyleDimension.FromPixels(Math.Max(uiText.MinHeight.Pixels, uiImage.Height.Pixels) + 26f);
            panel.Recalculate();

            ComponentList.Add(panel);
            #endregion

            RefreshButton.SetImage(BackTexture);
            RefreshButton.HoverText = "{$UI.Back}";

            Recalculate();
            SetupScrollBar();
        }

        private static UIHorizontalSeparator QuickSeparator() => new()
        {
            Width = StyleDimension.FromPercent(1f),
            Color = Color.Lerp(Color.White, new Color(63, 65, 151, 255), 0.85f) * 0.9f
        };

        private static UIText QuickTitleText(string text, float originY, float originX = 0.5f) => new(text, 0.6f, true)
        {
            Height = StyleDimension.FromPixels(50f),
            Width = StyleDimension.FromPercent(1f),
            TextOriginX = originX,
            TextOriginY = originY
        };

        /// <summary>
        /// 打开GUI界面
        /// </summary>
        public void Open()
        {
            Visible = true;
            SoundEngine.PlaySound(SoundID.MenuOpen);
            SetupStructuresList();
        }

        /// <summary>
        /// 关闭GUI界面
        /// </summary>
        public void Close()
        {
            Visible = false;
            Main.blockInput = false;
            SoundEngine.PlaySound(SoundID.MenuClose);
        }
    }
}
