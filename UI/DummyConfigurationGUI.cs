using ImproveGame.Common.Configs;
using ImproveGame.Content.NPCs.Dummy;
using ImproveGame.UI.ModernConfig;
using ImproveGame.UIFramework;
using ImproveGame.UIFramework.BaseViews;
using ImproveGame.UIFramework.Common;
using ImproveGame.UIFramework.SUIElements;
using Terraria.GameInput;

namespace ImproveGame.UI
{
    [AutoCreateGUI(LayerName.Vanilla.RadialHotbars, "Dummy Configuration GUI")]
    public class DummyConfigurationUI : BaseBody
    {
        public static DummyConfigurationUI Instance { get; private set; }

        public DummyConfigurationUI() => Instance = this;

        public override bool IsNotSelectable => StartTimer.AnyClose;

        public override bool Enabled
        {
            get => StartTimer.Closing || StartTimer.AnyOpen;
            set => _enabled = value;
        }

        private bool _enabled;

        public override bool CanSetFocusTarget(UIElement target)
            => (target != this && MainPanel.IsMouseHovering) || MainPanel.IsLeftMousePressed;

        /// <summary>
        /// 启动关闭动画计时器
        /// </summary>
        public AnimationTimer StartTimer = new(3);

        // 主面板
        public SUIPanel MainPanel;

        // 标题面板
        private View TitlePanel;

        public SUIScrollBar Scrollbar;
        public UIText TipText;

        public SUIDropdownListContainer DropdownList { get; set; }

        public SUINumericText customAIStyleBox;

        public SUIImageButton aiStyleWikiOpener;

        public SUIScrollView2 Options;

        public override void OnInitialize()
        {
            DropdownList = new SUIDropdownListContainer();

            // 主面板
            MainPanel = new SUIPanel(UIStyle.PanelBorder, UIStyle.PanelBg)
            {
                Shaded = true,
                Draggable = true,
                Resizeable = true,
                MinResizeHeight = 200,
                MinResizeWidth = 400
            };
            MainPanel.SetPadding(0f);
            MainPanel.SetPosPixels(410, 360)
                .SetSizePixels(500, 500)
                .JoinParent(this);

            TitlePanel = ViewHelper.CreateHead(Color.Black * 0.25f, 45f, 10f);
            TitlePanel.SetPadding(0f);
            TitlePanel.JoinParent(MainPanel);

            // 标题
            var title = new SUIText
            {
                IsLarge = true,
                UseKey = true,
                TextOrKey = "Mods.ImproveGame.UI.DummyConfiguration.Title",
                TextAlign = new Vector2(0f, 0.5f),
                TextScale = 0.5f,
                Height = StyleDimension.Fill,
                Width = StyleDimension.Fill,
                DragIgnore = true,
                Left = new StyleDimension(16f, 0f)
            };
            title.JoinParent(TitlePanel);

            var cross = new SUICross
            {
                HAlign = 1f,
                Rounded = new Vector4(0f, 10f, 0f, 0f),
                CrossSize = 20f,
                CrossRounded = 4.5f * 0.85f,
                Border = 0f,
                BorderColor = Color.Transparent,
                BgColor = Color.Transparent,
            };
            cross.CrossOffset.X = 1f;
            cross.Width.Pixels = 46f;
            cross.Height.Set(0f, 1f);
            cross.OnUpdate += _ =>
            {
                cross.BgColor = cross.HoverTimer.Lerp(Color.Transparent, Color.Black * 0.25f);
            };
            cross.OnLeftMouseDown += (_, _) => Close();
            cross.JoinParent(TitlePanel);

            Options = new SUIScrollView2(Orientation.Vertical)
            {
                RelativeMode = RelativeMode.Vertical
            };
            Options.SetPadding(8);
            Options.SetSize(0f, -TitlePanel.Height.Pixels, 1f, 1f);
            Options.JoinParent(MainPanel);

            customAIStyleBox = new SUINumericText
            {
                HAlign = 1f,
                Left = new(-180, 0),
                Width = new(60f, 0),
                Height = new(0, 1f),
                BgColor = Color.Black * 0.4f,
                Rounded = new Vector4(14f),
                MinValue = -65,
                MaxValue = NPCID.Count - 1,
                InnerText =
                {
                    TextAlign = new Vector2(0.5f, 0.5f),
                    TextOffset = new Vector2(0f, -2f),
                    MaxCharacterCount = 10,
                    MaxLines = 1,
                    IsWrapped = false
                },
                MaxLength = 3,
                DefaultValue = 0,
                Format = "0",
                VAlign = 0.5f,
            };
            customAIStyleBox.ContentsChanged += (ref string content) =>
            {
                if (int.TryParse(content, out int value))
                    DummyNPC.LocalConfig.customAIType = value;
            };
            customAIStyleBox.EndTakingInput += () =>
            {
                if (int.TryParse(customAIStyleBox.Text, out int value))
                {
                    DummyNPC.LocalConfig.customAIType = value;

                    SyncDummyModule.Get(null, Main.myPlayer, DummyNPC.LocalConfig).Send(runLocally: true);
                }
            };
            customAIStyleBox.OnUpdate += (elem) =>
            {
                var value = DummyNPC.LocalConfig.customAIType;
                if (!customAIStyleBox.IsWritingText)
                    customAIStyleBox.Value = value;
            };
            aiStyleWikiOpener = new SUIImageButton(TextureAssets.Item[ItemID.Book].Value,
                MyUtils.GetText("UI.DummyConfiguration.OpenWikiTip"), false)
            {
                HAlign = 1f,
                Left = new(Language.ActiveCulture.Name is not "zh-Hans" ? -140 : -125, 0),
                Width = new(40f, 0),
                Height = new(0, 1f),
                VAlign = 0.5f
            };
            aiStyleWikiOpener.OnLeftClick += (evt, elem) =>
            {
                TrUtils.OpenToURL(MyUtils.GetText("UI.DummyConfiguration.WikiURL"));
            };
            var fieldInfos = typeof(DummyConfig).GetFields();
            foreach (var fInfo in fieldInfos)
            {
                if (fInfo.Name == nameof(DummyConfig.customAIType)) continue;
                var fPanel = new TimerView
                {
                    RelativeMode = RelativeMode.Horizontal,
                    DirectLineBreak = true,
                    Rounded = new Vector4(6),
                    Spacing = new Vector2(0, 8)
                };
                fPanel.SetPadding(6, 6, 6, 6);
                fPanel.SetSize(0f, 40, 1f, 0f);
                fPanel.JoinParent(Options.ListView);
                fPanel.BgColor = Color.Black * .125f;
                fPanel.OnUpdate += delegate
                {
                    // 这里和LongSwitch保持一致，为了整体协调
                    fPanel.BgColor = fPanel.HoverTimer.Lerp(UIStyle.PanelBgLight, UIStyle.PanelBgLightHover);
                    fPanel.Border = 0;
                };
                var fieldName = new SUIText
                {
                    UseKey = true,
                    TextOrKey = $"Mods.ImproveGame.UI.DummyConfiguration.FieldName.{fInfo.Name}",
                    TextAlign = new Vector2(0f, 0f),
                    TextScale = 1f,
                    Height = new StyleDimension(40, 0),
                    Width = StyleDimension.Fill,
                    Left = new StyleDimension(10f, 0f)
                };
                fieldName.JoinParent(fPanel);
                var fType = fInfo.FieldType;
                if (fType == typeof(bool))
                {
                    SUISwitch uiSwitch = new SUISwitch(
                        () => (bool)fInfo.GetValue(DummyNPC.LocalConfig),
                        flag =>
                        {
                            // 开关只是拿来看的，真正操作加到了fPanel.OnLeftMouseDown
                            // fInfo.SetValueDirect(__makeref(DummyNPC.LocalConfig), flag);
                            // SyncDummyModule.Get(null, Main.myPlayer, DummyNPC.LocalConfig).Send(runLocally: true);
                        }, ""
                    )
                    {
                        HAlign = 1f,
                        Width = new StyleDimension(60, 0),
                        Height = StyleDimension.Fill
                    };
                    fPanel.OnLeftMouseDown += (evt, self) =>
                    {
                        SoundEngine.PlaySound(SoundID.MenuTick);
                        bool flag = !(bool)fInfo.GetValue(DummyNPC.LocalConfig);
                        fInfo.SetValueDirect(__makeref(DummyNPC.LocalConfig), flag);
                        SyncDummyModule.Get(null, Main.myPlayer, DummyNPC.LocalConfig).Send(runLocally: true);
                    };
                    uiSwitch.JoinParent(fPanel);
                }
                else if (fType == typeof(int))
                {
                    if (fInfo.Name == "LifeMax")
                    {
                        SUINumericText numberBox =
                            new SUINumericText
                            {
                                HAlign = 1f,
                                Width = new(120f, 0),
                                Height = new(0, 1f),
                                BgColor = Color.Black * 0.4f,
                                Rounded = new Vector4(14f),
                                MinValue = 1,
                                MaxValue = 2147483647,
                                InnerText =
                                {
                                    TextAlign = new Vector2(0.5f, 0.5f),
                                    TextOffset = new Vector2(0f, -2f),
                                    MaxCharacterCount = 10,
                                    MaxLines = 1,
                                    IsWrapped = false
                                },
                                MaxLength = 10,
                                DefaultValue = 2000000000,
                                Format = "0",
                                VAlign = 0.5f
                            };
                        numberBox.ContentsChanged += (ref string content) =>
                        {
                            if (int.TryParse(content, out int value))
                                DummyNPC.LocalConfig.LifeMax = value;
                        };
                        numberBox.EndTakingInput += () =>
                        {
                            if (int.TryParse(numberBox.Text, out int value))
                            {
                                DummyNPC.LocalConfig.LifeMax = value;
                                SyncDummyModule.Get(null, Main.myPlayer, DummyNPC.LocalConfig).Send(runLocally: true);
                            }
                        };
                        numberBox.OnUpdate += (elem) =>
                        {
                            var value = DummyNPC.LocalConfig.LifeMax;
                            if (!numberBox.IsWritingText)
                                numberBox.Value = value;
                        };
                        numberBox.JoinParent(fPanel);
                    }
                    else
                    {
                        int defaultValue = 0;
                        int minValue = 0;
                        int maxValue = 1000;
                        if (fInfo.Name.Equals("ResetTimer"))
                        {
                            defaultValue = 30;
                            minValue = 30;
                            maxValue = 60 * 60 * 60;
                        }

                        SUISlider<int> sUISlider = new SUISlider<int>(
                            () => (int)fInfo.GetValue(DummyNPC.LocalConfig),
                            obj =>
                            {
                                fInfo.SetValueDirect(__makeref(DummyNPC.LocalConfig), obj);
                                SyncDummyModule.Get(null, Main.myPlayer, DummyNPC.LocalConfig).Send(runLocally: true);
                            }, "",
                            minValue, maxValue, defaultValue
                        )
                        {
                            HAlign = 1f,
                            Width = new StyleDimension(180, 0),
                            Height = StyleDimension.Fill
                        };
                        sUISlider.JoinParent(fPanel);
                    }
                }
                else if (fType == typeof(DummyConfig.AIType))
                {
                    SUIDropdownList<DummyConfig.AIType> list = new(
                        () => (DummyConfig.AIType)fInfo.GetValue(DummyNPC.LocalConfig), obj =>
                        {
                            if ((DummyConfig.AIType)obj == DummyConfig.AIType.SelfDefine &&
                                !(ImproveConfigs.Instance.DummyCustomAIStyleAllowed || Main.netMode == NetmodeID.SinglePlayer))
                            {
                                DummyNPC.LocalConfig.AIStyle = DummyConfig.AIType.Default;
                                MyUtils.AddNotificationFromKey("UI.DummyConfiguration.CustomDisabled", Color.Yellow, -1, () =>
                                {
                                    ModernConfigUI.Instance.Open(ImproveGame.Instance);
                                    ModernConfigUI.Instance.OpenFromMasterControl = true;
                                    ConfigOptionsPanel.CategoryToSelectOnOpen =
                                        CategorySidePanel.Cards["ModFeatures"].Category;
                                    ConfigOptionsPanel.Instance.SetSearchBarText(
                                        MyUtils.GetText("UI.DummyConfiguration.DummyConfigLabel"));
                                });
                                SyncDummyModule.Get(null, Main.myPlayer, DummyNPC.LocalConfig).Send(runLocally: true);
                                return;
                            }

                            var aistyle = (DummyConfig.AIType)obj;
                            DummyNPC.LocalConfig.AIStyle = aistyle;
                            if (aistyle != DummyConfig.AIType.Default && aistyle != DummyConfig.AIType.SelfDefine)
                                DummyNPC.LocalConfig = aistyle switch
                                {
                                    DummyConfig.AIType.Slime or DummyConfig.AIType.Soilder
                                        or DummyConfig.AIType.JellyFish or DummyConfig.AIType.GoldenFish
                                        or DummyConfig.AIType.HugeMimic => DummyNPC.LocalConfig with
                                        {
                                            NoTileCollide = false,
                                            NoGravity = false
                                        },
                                    DummyConfig.AIType.EvilEye => DummyNPC.LocalConfig with { NoGravity = true },
                                    _ => DummyNPC.LocalConfig
                                };
                            SyncDummyModule.Get(null, Main.myPlayer, DummyNPC.LocalConfig).Send(runLocally: true);
                            if (aistyle == DummyConfig.AIType.SelfDefine && customAIStyleBox.Parent == null)
                            {
                                customAIStyleBox.JoinParent(fPanel);
                                aiStyleWikiOpener.JoinParent(fPanel);
                            }
                            else if (aistyle != DummyConfig.AIType.SelfDefine && customAIStyleBox.Parent != null)
                            {
                                customAIStyleBox.Remove();
                                aiStyleWikiOpener.Remove();
                            }
                        }, DropdownList, "")
                    {
                        HAlign = 1f,
                        Width = new StyleDimension(180, 0),
                        Height = StyleDimension.Fill,
                    };
                    list.JoinParent(fPanel);
                    list.OnUpdate += elem =>
                    {
                        if (DummyNPC.LocalConfig.AIStyle == DummyConfig.AIType.SelfDefine &&
                            !(ImproveConfigs.Instance.DummyCustomAIStyleAllowed || Main.netMode == NetmodeID.SinglePlayer))
                        {
                            if (customAIStyleBox.Parent != null)
                            {
                                customAIStyleBox.Remove();
                                aiStyleWikiOpener.Remove();
                            }

                            ;
                            DummyNPC.LocalConfig.AIStyle = DummyConfig.AIType.Default;
                            MyUtils.AddNotificationFromKey("UI.DummyConfiguration.CustomDisabled", Color.Yellow, -1, () =>
                            {
                                ModernConfigUI.Instance.Open(ImproveGame.Instance);
                                ModernConfigUI.Instance.OpenFromMasterControl = true;
                                ConfigOptionsPanel.CategoryToSelectOnOpen =
                                    CategorySidePanel.Cards["ModFeatures"].Category;
                                ConfigOptionsPanel.Instance.SetSearchBarText(
                                    MyUtils.GetText("UI.DummyConfiguration.DummyConfigLabel"));
                            });
                            SyncDummyModule.Get(null, Main.myPlayer, DummyNPC.LocalConfig).Send(runLocally: true);
                        }
                    };
                    if (DummyNPC.LocalConfig.AIStyle == DummyConfig.AIType.SelfDefine &&
                        customAIStyleBox.Parent == null)
                    {
                        customAIStyleBox.JoinParent(fPanel);
                        aiStyleWikiOpener.JoinParent(fPanel);
                    }
                    else if (DummyNPC.LocalConfig.AIStyle != DummyConfig.AIType.SelfDefine &&
                             customAIStyleBox.Parent != null)
                    {
                        customAIStyleBox.Remove();
                        aiStyleWikiOpener.Remove();
                    }
                }
                else
                {
                    double minValue = fInfo.Name == nameof(DummyConfig.Scale) ? 0.5 : 0;
                    double maxValue = fInfo.Name == nameof(DummyConfig.Scale) ? 3 : 2;

                    SUISlider<float> sUISlider = new SUISlider<float>(
                        () => (float)fInfo.GetValue(DummyNPC.LocalConfig),
                        obj =>
                        {
                            fInfo.SetValueDirect(__makeref(DummyNPC.LocalConfig), obj);
                            SyncDummyModule.Get(null, Main.myPlayer, DummyNPC.LocalConfig).Send(runLocally: true);
                        }, "",
                        minValue, maxValue, 0
                    )
                    {
                        HAlign = 1f,
                        Width = new StyleDimension(180, 0),
                        Height = StyleDimension.Fill,
                    };
                    sUISlider.JoinParent(fPanel);
                }
            }


            DropdownList.JoinParent(this);
        }


        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            StartTimer.Update();
            if (!MainPanel.IsMouseHovering)
                return;

            PlayerInput.LockVanillaMouseScroll("ImproveGame: Dummy Configuration GUI");
            Main.LocalPlayer.mouseInterface = true;
        }

        public void Open()
        {
            SoundEngine.PlaySound(SoundID.MenuOpen);
            StartTimer.Open();

            var center = Main.MouseScreen;
            float zoom = Main.GameZoomTarget * Main.ForcedMinimumZoom;
            center = (center - Main.ScreenSize.ToVector2() * .5f) * zoom + Main.ScreenSize.ToVector2() * .5f;
            center /= Main.UIScale;

            // 保证UI不超出屏幕范围
            // center是UI当前位置的坐标，需要修正这个坐标，使UI框在屏幕内
            var size = MainPanel.GetDimensions().Size();
            Vector2 screenSizeScaled = new Vector2(Main.screenWidth, Main.screenHeight) / Main.UIScale;
            const float margin = 20f;

            float min;
            float max;
            // 限制X坐标
            min = size.X / 2 + margin;
            max = screenSizeScaled.X - min;
            if (max > min)
                center.X = Math.Clamp(center.X, min, max);

            // 限制Y坐标
            min = size.Y / 2 + margin;
            max = screenSizeScaled.Y - min;
            if (max > min)
                center.Y = Math.Clamp(center.Y, min, max);


            MainPanel.SetCenterPixels(center.X, center.Y);
            MainPanel.Recalculate();
        }

        public void Close()
        {
            SoundEngine.PlaySound(SoundID.MenuClose);
            StartTimer.Close();
        }

        public override bool RenderTarget2DDraw => !StartTimer.Opened;
        public override float RenderTarget2DOpacity => StartTimer.Schedule;
        public override Vector2 RenderTarget2DOrigin => MainPanel.GetDimensionsCenter();
        public override Vector2 RenderTarget2DPosition => MainPanel.GetDimensionsCenter();
        public override Vector2 RenderTarget2DScale => new Vector2(0.95f + StartTimer.Lerp(0, 0.05f));
    }
}