using ImproveGame.Common;
using ImproveGame.UIFramework;
using ImproveGame.UIFramework.BaseViews;
using ImproveGame.UIFramework.Common;
using ImproveGame.UIFramework.SUIElements;
using Microsoft.Xna.Framework.Input;
using Terraria.Graphics.Renderers;
using Terraria.ModLoader.UI;
using Terraria.UI.Chat;

namespace ImproveGame.UI.ModernConfig;

public sealed class ModernConfigUI : UIState
{
    public static ModernConfigUI Instance { get; set; }

    public bool Enabled { get; set; }

    public bool OpenFromMasterControl;

    public static bool DrawCalledForMakingGlass;

    public static RenderTarget2D Glass; // 云母效果用

    // 主面板
    public SUIPanel MainPanel;

    // 侧栏放类别
    public CategorySidePanel CategoryPanel;

    // 主栏上放选项
    public ConfigOptionsPanel OptionsPanel;

    // 主栏下放描述
    public TooltipPanel TooltipPanel;

    // 当前打开的mod
    public Mod currentMod;

    // 额外文本提示，显示在主面板左上方之外
    public string ExtraText;

    // 有点爽的东西，中键收藏生成粒子
    private UIParticleLayer _particleSystem = new()
    {
        Width = new StyleDimension(0f, 1f),
        Height = new StyleDimension(0f, 1f),
        AnchorPositionOffsetByPercents = Vector2.One / 2f,
        AnchorPositionOffsetByPixels = Vector2.Zero
    };

    public int NoticeTimer;

    public Color NoticeColor;

    public SUIText PopNotice;

    public View PopNoticePanel;

    // 子页面路径选框
    public SUIScrollView2 PathPanel;

    // 子页面路径选框的弹出动画计时器
    public AnimationTimer PathPanelTimer;

    public View[] DividingLines = new View[2];

    private const bool ClassicStyle = true;

    public override void OnInitialize()
    {
        //const int gapBetweenPanels = 20;
        const int gapBetweenPanels = ClassicStyle ? 14 : 0;
        const int sidePanelWidth = 220;
        const int tooltipPanelHeight = 166;
        Instance = this;

        // 主面板
        MainPanel = new SUIPanel(ConfigColors.MainPanelBorder, ConfigColors.MainPanelBg)
        {
            Shaded = true,
            HAlign = 0.5f,
            VAlign = 0.5f,
        };
        MainPanel.SetPadding(gapBetweenPanels);
        MainPanel.SetPosPixels(0f, -20f);
        if (ClassicStyle)
        {
            // 原来的大小
            // MainPanel.SetSizePercent(0.86f, 0.82f).JoinParent(this);
            MainPanel.SetSizePercent(0.78f, 0.82f).JoinParent(this);
        }
        else
        {
            MainPanel.SetSizePercent(0.65f, 0.82f).JoinParent(this);
        }

        // 侧栏放类别
        CategoryPanel = new CategorySidePanel(ConfigColors.DarkBorderlessPanel * 0.75f)
        {
            RelativeMode = RelativeMode.Horizontal,
            Rounded = ClassicStyle ? new Vector4(8) : new Vector4(10f, 0f, 10f, 0f),
            EnableBlur = false,
        };
        CategoryPanel.SetSize(sidePanelWidth, 0f, 0f, 1f);
        CategoryPanel.JoinParent(MainPanel);

        // 纵向分割线
        if (!ClassicStyle)
        {
            DividingLines[0] = new View
            {
                RelativeMode = RelativeMode.Horizontal,
                Width = new StyleDimension(2f, 0f),
                Height = new StyleDimension(0f, 1f),
                BgColor = UIStyle.ListItemColor * 0.75f,
            };
            DividingLines[0].JoinParent(MainPanel);
        }

        // 把主栏框起来的容器
        var mainPanelContainer = new View
        {
            Spacing = new Vector2(gapBetweenPanels),
            RelativeMode = RelativeMode.Horizontal,
            BorderColor = Color.White,
        };
        mainPanelContainer.SetSize(-sidePanelWidth - gapBetweenPanels - 2f, 0f, 1f, 1f);
        mainPanelContainer.JoinParent(MainPanel);

        // 主栏上放选项
        OptionsPanel = new ConfigOptionsPanel(ConfigColors.DarkBorderlessPanel * 0.5f)
        {
            Spacing = new Vector2(gapBetweenPanels),
            RelativeMode = RelativeMode.Vertical,
            Rounded = ClassicStyle ? new Vector4(8) : new Vector4(0f, 10f, 0f, 0f)
        };
        OptionsPanel.SetSize(0f, -tooltipPanelHeight - gapBetweenPanels - 2, 1f, 1f);
        OptionsPanel.JoinParent(mainPanelContainer);

        // 横向分割线
        if (!ClassicStyle)
        {
            DividingLines[1] = new View
            {
                RelativeMode = RelativeMode.Vertical,
                Width = new StyleDimension(0f, 1f),
                Height = new StyleDimension(2f, 0f),
                BgColor = UIStyle.ListItemColor * 0.75f,
            };
            DividingLines[1].JoinParent(mainPanelContainer);
        }

        // 主栏下放描述
        TooltipPanel = new TooltipPanel(ConfigColors.DarkBorderlessPanel * 0.5f)
        {
            Spacing = new Vector2(gapBetweenPanels),
            RelativeMode = RelativeMode.Vertical,
            Rounded = ClassicStyle ? new Vector4(8) : new Vector4(0f, 0f, 0f, 10f)
        };
        TooltipPanel.SetSize(0f, tooltipPanelHeight, 1f, 0f);
        TooltipPanel.JoinParent(mainPanelContainer);

        MainPanel.Append(_particleSystem);

        // 返回按钮
        var backButton = new UITextPanel<LocalizedText>(Language.GetText("UI.Back"), 0.7f, large: true)
        {
            Width = { Pixels = -30f, Percent = 0.5f },
            Height = { Pixels = 50f },
            Top = { Pixels = -35f },
            VAlign = 1f,
            HAlign = 0.5f,
        }.WithFadedMouseOver();
        backButton.OnLeftClick += (_, _) => Close();

        PopNoticePanel = new()
        {
            Padding = 4f,
            Rounded = new(8),
            IgnoresMouseInteraction = true
        };
        PopNoticePanel.JoinParent(MainPanel);
        PopNotice = new();
        PopNotice.JoinParent(PopNoticePanel);

        SetupPathPanel();

        this.Append(backButton);
    }

    private void SetupPathPanel()
    {
        PathPanel = new(Orientation.Horizontal)
        {
            BgColor = UIStyle.PanelBg,
            BorderColor = UIStyle.PanelBorder,
            Width = new(0, 0.5f),
            Height = new(30, 0),
            Rounded = new(8),
            Border = 1f,
        };
        PathPanel.ListView.SetPadding(8f, 2f);
        PathPanel.SetPos(0, -58, 0.5f - 0.86f * 0.5f, 0f);
        PathPanel.JoinParent(this);
        PathPanel.ScrollBar.Height = default;

        PathPanelTimer = new(3);
        PathPanelTimer.ImmediateClose();
        PathPanelTimer.OnClosed += () =>
        {
            Instance.PathPanel.ListView.RemoveAllChildren();
            Instance.PathPanel.Recalculate();
        };
        PathPanelTimer.OnOpened += Instance.PathPanel.Recalculate;
    }

    public static void PopNewInfo(string info, Vector2 position, Color color)
    {
        Instance.NoticeTimer = 120;
        Instance.NoticeColor = color;
        Instance.PopNotice.TextOrKey = info;
        Instance.PopNoticePanel.SetPos(position - Instance.MainPanel.GetDimensions().Position());

        Instance.PopNoticePanel.Recalculate();
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        // SubPage目录缓动
        PathPanelTimer.UpdateHighFps();
        PathPanel.Top.Percent = PathPanelTimer.Lerp(0f, 0.5f - 0.82f * 0.5f);
        if (PathPanelTimer.Closing || PathPanelTimer.Opening)
            PathPanel.Recalculate();

        CenteredItemTagHandler.ModernConfigDrawing = true;
        base.Draw(spriteBatch);
        CenteredItemTagHandler.ModernConfigDrawing = false;

        if (!PathPanelTimer.AnyOpen && !string.IsNullOrWhiteSpace(ExtraText))
        {
            var font = FontAssets.MouseText.Value;
            var textPosition = MainPanel.GetDimensions().Position();
            textPosition += new Vector2(6f, -font.LineSpacing + 4f);
            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, ExtraText, textPosition, Color.LightGray, 0f, Vector2.Zero, Vector2.One, spread: 1.2f);
        }
    }

    public void Open(Mod mod)
    {
        SoundEngine.PlaySound(SoundID.MenuOpen);

        PathPanelTimer?.ImmediateClose();
        if (mod.Name == "ImproveGame")
        {
            ExtraText = "";
            ConfigOptionsPanel.CategoryToSelectOnOpen = CategorySidePanel.Cards["AboutPage"].Category;
        }
        else
        {
            ExtraText = MyUtils.GetText("ModernConfig.ModdedExtraText", mod.DisplayName);
            if (CategorySidePanel.ModdedAboutPage.TryGetValue(mod, out var value))
                ConfigOptionsPanel.CategoryToSelectOnOpen = value;
            else
                ConfigOptionsPanel.CategoryToSelectOnOpen = CategorySidePanel.AboutPage_ModConfig;
        }
        Enabled = true;
        NoticeTimer = 1;

        if (Main.gameMenu)
        {
            Main.menuMode = 888;
            Main.MenuUI.SetState(this);
        }
        else
        {
            IngameFancyUI.OpenUIState(this);
        }
        currentMod = mod;
        CategoryPanel.ChangeMod(mod, true);
    }

    public void Close()
    {
        SoundEngine.PlaySound(SoundID.MenuClose);

        PathPanel.ListView.RemoveAllChildren();
        PathPanelTimer.ImmediateClose();

        NoticeTimer = 1;
        Enabled = false;
        if (!Main.gameMenu)
        {
            IngameFancyUI.Close();
        }
        else
        {
            Main.menuMode = Interface.modConfigListID;
            Interface.modConfigList.ModToSelectOnOpen = currentMod ?? ImproveGame.Instance;
        }
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        if (!ClassicStyle)
        {
            foreach (var line in DividingLines)
            {
                line.BgColor = UIStyle.ListItemColor * 0.75f;
            }
        }

        if (Main.keyState.IsKeyDown(Keys.Escape) && !Main.oldKeyState.IsKeyDown(Keys.Escape) &&
            UISystem.FocusedEditableText is null && Main.gameMenu) // 游戏里按照物品栏快捷键关闭，只有gameMenu才用Esc
        {
            Close();
        }

        // 适配即时风格切换
        MainPanel.BorderColor = ConfigColors.MainPanelBorder;
        MainPanel.BgColor = ConfigColors.MainPanelBg;
        PathPanel.BorderColor = ConfigColors.MainPanelBorder;
        PathPanel.BgColor = ConfigColors.MainPanelBg;

        if (NoticeTimer-- > 0)
        {
            float k = NoticeTimer switch
            {
                > 105 => MathHelper.SmoothStep(0, 1, (120 - NoticeTimer) / 15f),
                > 30 => 1f,
                _ => MathHelper.SmoothStep(0, 1, NoticeTimer / 30f)
            };
            PopNoticePanel.BorderColor = Color.Lerp(ConfigColors.MainPanelBorder, Color.Black, .5f) * k;
            PopNoticePanel.BgColor = Color.Lerp(ConfigColors.MainPanelBg, Color.Black, .5f) * k;
            // PopNoticePanel.SetSize(PopNotice.TextSize * k + new Vector2(8));
            PopNotice.TextColor = NoticeColor * k;
            PopNotice.TextBorderColor = Color.Black * k;
            PopNotice.RecalculateText();
            // PopNoticePanel.SetPos(0, 0, 0, 0.5f);
            // PopNoticePanel.HAlign = 0.5f;
            PopNoticePanel.Recalculate();
        }
    }

    public void GenerateParticleAtMouse()
    {
        var parentPosition = Instance.MainPanel.GetInnerDimensions().Center();
        var mousePosition = Main.MouseScreen;
        var finalPosition = mousePosition - parentPosition;
        finalPosition.Y += 4f;
        Instance.GenerateParticleAt(finalPosition);
    }

    public void GenerateParticleAt(Vector2 position)
    {
        Vector2 accelerationPerFrame = new(0f, 0.16350001f);
        var texture = Main.Assets.Request<Texture2D>("Images/UI/Creative/Research_Spark");

        for (int i = 0; i < 12; i++)
        {
            Vector2 initialVelocity = Main.rand.NextVector2Circular(4f, 3f);

            initialVelocity.Y -= 2f;

            _particleSystem.AddParticle(new CreativeSacrificeParticle(texture, null, initialVelocity, position)
            {
                AccelerationPerFrame = accelerationPerFrame,
                ScaleOffsetPerFrame = -1f / 60f,
                _scale = Main.rand.NextFloat(0.2f, 0.5f)
            });
        }
    }
}