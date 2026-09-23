using ImproveGame.Common.ModSystems;
using ImproveGame.UI.MasterControl.Components;
using ImproveGame.UIFramework;
using ImproveGame.UIFramework.BaseViews;
using ImproveGame.UIFramework.Common;
using ImproveGame.UIFramework.SUIElements;
using Terraria.GameInput;

namespace ImproveGame.UI.MasterControl;

[AutoCreateGUI(LayerName.Vanilla.RadialHotbars, "Master Control GUI", 100)]
public class MasterControlGUI : BaseBody
{
    public readonly AnimationTimer OpacityTimer = new(2.5f);

    public override bool Enabled
    {
        get => Visible || OpacityTimer.AnyOpen || OpacityTimer.Closing;
        set => Opened = value;
    }

    public override bool CanSetFocusTarget(UIElement target) => Window.IsMouseHovering;
    public override bool IsNotSelectable => OpacityTimer.AnyClose;

    /// <summary>
    /// 固定面板，开启关闭改为按下快捷键而不是按住
    /// </summary>
    public static bool Pinned;

    /// <summary>
    /// 固定面板后，使用Opened来控制面板的开启关闭
    /// </summary>
    public static bool Opened;

    public bool Visible => (!Pinned && KeybindSystem.MasterControlKeybind.Current) || (Pinned && Opened);

    public SUIPanel Window { get; private set; }
    public SUIScrollView2 AvailableFunctions { get; private set; }
    public SUIScrollView2 UnavailableFunctions { get; private set; }

    #region HeadBar and TailBar

    /// <summary>
    /// 面板头部
    /// </summary>
    public View HeadBar { get; private set; }

    /// <summary>
    /// 标题
    /// </summary>
    public SUIText Title { get; private set; }

    /// <summary>
    /// 面板尾部
    /// </summary>
    public View TailBar { get; private set; }

    /// <summary>
    /// 版本号文本框
    /// </summary>
    public SUIText VersionText { get; private set; }

    /// <summary>
    /// 更新日志文本框
    /// </summary>
    public SUIText ChangelogText { get; private set; }

    #endregion

    public override void OnInitialize()
    {
        #region Window

        Window = new SUIPanel(Color.Transparent, Color.Transparent)
        {
            Padding = 0f,
            FinallyDrawBorder = true,
            IsAdaptiveWidth = true,
            IsAdaptiveHeight = true,
            HAlign = 0.5f,
            VAlign = 0.5f,
        };
        Window.SetRoundedRectProperties(UIStyle.PanelBg, 2f, UIStyle.PanelBorder, 12);
        Window.JoinParent(this);

        #endregion

        #region HeadBar

        HeadBar = ViewHelper.CreateHead(UIStyle.PanelBorder * 0.5f, 45f, 12f);
        HeadBar.JoinParent(Window);

        Title = new SUIText
        {
            TextAlign = new Vector2(0.5f),
            TextBorder = 1.5f,
            TextOrKey = "Mods.ImproveGame.ModName",
            TextOffset = new Vector2(12f, 0f),
            UseKey = true,
            HAlign = 0.5f
        };
        Title.SetSizePercent(0.8f, 1f);
        Title.JoinParent(HeadBar);

        var pingButton = new MasterControlPing
        {
            HAlign = 1f,
            VAlign = 0.5f
        };
        pingButton.JoinParent(HeadBar);

        #endregion

        var availableText = new SUIText
        {
            RelativeMode = RelativeMode.Vertical,
            Spacing = new Vector2(4f),
            TextScale = 0.8f,
            TextOrKey = "Mods.ImproveGame.UI.MasterControl.Available",
            TextOffset = new Vector2(12f, 0f),
            UseKey = true
        };
        availableText.SetSize(0f, availableText.TextSize.Y * availableText.TextScale, 1f, 0f);
        availableText.JoinParent(Window);

        AvailableFunctions = new SUIScrollView2(Orientation.Vertical)
        {
            RelativeMode = RelativeMode.Vertical,
            Spacing = new Vector2(4f),
        };
        AvailableFunctions.SetPadding(6f, 0f);
        AvailableFunctions.SetSizePixels(500, 120);
        AvailableFunctions.JoinParent(Window);

        ReloadAvailableFunctionsElement();

        var unavailableText = new SUIText
        {
            RelativeMode = RelativeMode.Vertical,
            Spacing = new Vector2(4f),
            TextScale = 0.8f,
            TextOrKey = "Mods.ImproveGame.UI.MasterControl.Unavailable",
            TextOffset = new Vector2(12f, 0f),
            UseKey = true
        };
        unavailableText.SetSize(0f, unavailableText.TextSize.Y * unavailableText.TextScale, 1f, 0f);
        unavailableText.JoinParent(Window);

        UnavailableFunctions = new SUIScrollView2(Orientation.Vertical)
        {
            RelativeMode = RelativeMode.Vertical,
            Spacing = new Vector2(4f)
        };
        UnavailableFunctions.SetPadding(6f, 0f);
        UnavailableFunctions.SetSizePixels(500, 120);
        UnavailableFunctions.JoinParent(Window);

        ReloadUnavailableFunctionsElement();

        #region TailBar

        TailBar = ViewHelper.CreateTail(Color.Black * 0.3f, 35f, 12f);
        TailBar.JoinParent(Window);

        VersionText = new SUIText
        {
            TextScale = 0.9f,
            // KeyMode暂不支持传参
            TextOrKey = MyUtils.GetText("UI.MasterControl.Version", ImproveGame.Instance.Version),
            TextAlign = new Vector2(0.5f),
            TextBorder = 1.5f,
            HAlign = 1f,
        };
        VersionText.Width.Pixels = VersionText.TextSize.X * VersionText.TextScale;
        VersionText.Height.Percent = 1f;
        VersionText.JoinParent(TailBar);

        ChangelogText = new SUIText
        {
            TextScale = 0.9f,
            // KeyMode暂不支持传参
            TextOrKey = MyUtils.GetText("UI.MasterControl.Changelog", ImproveGame.Instance.Version),
            TextAlign = new Vector2(0.5f),
            TextBorder = 1.5f
        };
        ChangelogText.OnMouseOver += (_, _) => SoundEngine.PlaySound(SoundID.MenuTick);
        ChangelogText.OnLeftMouseDown += (_, _) =>
        {
            var versionArray = ImproveGame.Instance.Version.ToString().Split('.');
            versionArray[3] = "x";
            string link = MyUtils.GetText("UI.MasterControl.ChangelogLink", string.Join('.', versionArray));
            TrUtils.OpenToURL(link);
        };
        ChangelogText.Width.Pixels = ChangelogText.TextSize.X * ChangelogText.TextScale;
        ChangelogText.Height.Percent = 1f;
        ChangelogText.JoinParent(TailBar);

        #endregion
    }

    public const float HNumber = 5f;
    public const float ChildrenSpacing = 4f;
    public const float ItemWidthPercent = 1f / HNumber;

    /// <summary>
    /// 重新加载 可用的 功能列表视图
    /// </summary>
    public void ReloadAvailableFunctionsElement()
    {
        AvailableFunctions.ListView.RemoveAllChildren();

        for (int i = 0; i < MasterControlManager.Instance.AvailableOrderedMCFunctions.Count; i++)
        {
            var functionComponent = new MCFIconComponent(MasterControlManager.Instance.AvailableOrderedMCFunctions, i)
            {
                RelativeMode = RelativeMode.Horizontal,
                Spacing = new Vector2(ChildrenSpacing),
            };

            if (i % HNumber == 0)
                functionComponent.DirectLineBreak = true;

            functionComponent.SetSize(-ChildrenSpacing * (HNumber - 1) / HNumber, 52f, ItemWidthPercent);
            functionComponent.JoinParent(AvailableFunctions.ListView);
        }
    }

    /// <summary>
    /// 重新加载 不可用的 功能列表视图
    /// </summary>
    public void ReloadUnavailableFunctionsElement()
    {
        UnavailableFunctions.ListView.RemoveAllChildren();

        for (int i = 0; i < MasterControlManager.Instance.UnavailableOrderedMCFunctions.Count; i++)
        {
            var functionComponent = new MCFIconComponent(MasterControlManager.Instance.UnavailableOrderedMCFunctions, i)
            {
                RelativeMode = RelativeMode.Horizontal,
                Spacing = new Vector2(ChildrenSpacing),
            };

            if (i % HNumber == 0)
                functionComponent.DirectLineBreak = true;

            functionComponent.SetSize(-ChildrenSpacing * (HNumber - 1) / HNumber, 52f, ItemWidthPercent);
            functionComponent.JoinParent(UnavailableFunctions.ListView);
        }
    }

    public override void CheckWhetherRecalculate(out bool recalculate)
    {
        base.CheckWhetherRecalculate(out recalculate);

        if (AvailableFunctions.ListView.Children.Count() !=
            MasterControlManager.Instance.AvailableOrderedMCFunctions.Count &&
            UnavailableFunctions.ListView.Children.Count() !=
            MasterControlManager.Instance.UnavailableOrderedMCFunctions.Count)
        {
            ReloadAvailableFunctionsElement();
            ReloadUnavailableFunctionsElement();

            recalculate = true;
        }
    }

    public override void Update(GameTime gameTime)
    {
        OpacityTimer.Update();
        base.Update(gameTime);

        ChangelogText.TextColor = ChangelogText.HoverTimer.Lerp(Color.White, Color.Yellow);
        ChangelogText.RecalculateText();

        if (Window.IsMouseHovering)
            PlayerInput.LockVanillaMouseScroll($"{ImproveGame.Instance.Name}: Control Center GUI");

        if (Visible)
        {
            if (!OpacityTimer.AnyOpen)
                OpacityTimer.Open();
        }
        else
        {
            if (!OpacityTimer.AnyClose)
                OpacityTimer.Close();
        }
    }

    #region Animation 开关动画

    public override bool RenderTarget2DDraw => OpacityTimer.State != AnimationState.Opened;
    public override float RenderTarget2DOpacity => OpacityTimer.Schedule;
    public override Vector2 RenderTarget2DPosition => Window.GetDimensionsCenter();
    public override Vector2 RenderTarget2DOrigin => Window.GetDimensionsCenter();
    public override Vector2 RenderTarget2DScale => new Vector2(0.95f + OpacityTimer * 0.05f);

    #endregion
}