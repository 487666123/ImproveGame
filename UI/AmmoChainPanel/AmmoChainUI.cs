using ImproveGame.Content.Functions.ChainedAmmo;
using ImproveGame.Core;
using ImproveGame.UIFramework;
using ImproveGame.UIFramework.BaseViews;
using ImproveGame.UIFramework.Common;
using ImproveGame.UIFramework.SUIElements;
using Terraria.Graphics.Renderers;

namespace ImproveGame.UI.AmmoChainPanel;

[AutoCreateGUI(LayerName.Vanilla.RadialHotbars, "Ammo Chain UI")]
public class AmmoChainUI : BaseBody
{
    public static AmmoChainUI Instance { get; private set; }

    public AmmoChainUI() => Instance = this;

    public override bool IsNotSelectable => StartTimer.AnyClose;

    public override bool Enabled
    {
        get => StartTimer.Closing || _enabled;
        set => _enabled = value;
    }

    private bool _enabled;

    public override bool CanSetFocusTarget(UIElement target)
        => target != this && (MainPanel.IsMouseHovering || MainPanel.IsLeftMousePressed);

    /// <summary>
    /// 启动关闭动画计时器
    /// </summary>
    public AnimationTimer StartTimer = new(3);

    /// <summary>
    /// 两个页面之间滑动动画计时器
    /// </summary>
    public AnimationTimer PageSlideTimer = new();

    #region Components

    // 主面板
    public SUIPanel MainPanel = new(UIStyle.PanelBorder, UIStyle.PanelBg)
    {
        Shaded = true,
        Draggable = true,
        FinallyDrawBorder = true,
        OverflowHidden = true
    };

    // 标题面板
    private View _titlePanel;

    // 标题
    private SUIText _title = new()
    {
        TextOrKey = "Mods.ImproveGame.UI.AmmoChain.Title",
        UseKey = true,
        IsLarge = true,
        TextScale = 0.55f,
        VAlign = 0.5f,
        TextBorder = 2f,
        PaddingLeft = 20f,
        DragIgnore = true
    };

    // 关闭按钮
    private SUICross _cross = new()
    {
        HAlign = 1f,
        VAlign = 0.5f,
        Width = { Pixels = 45f, Percent = 0f },
        Height = { Pixels = 0f, Precent = 1f },
        Rounded = new Vector4(0f, 10f, 0f, 0f),
        Border = 0f,
        BorderColor = Color.Transparent,
        CrossOffset = new Vector2(0f, 0f),
        CrossRounded = UIStyle.CrossThickness * 0.95f
    };

    // 用来放页的空间
    private View _shownPage = new()
    {
        RelativeMode = RelativeMode.Vertical
    };

    // 武器页
    private WeaponPage.WeaponPage _weaponPage = new();

    // 编辑页
    private ChainEditPage.ChainEditPage _chainEditPage = new();

    private UIParticleLayer _particleSystem = new()
    {
        Width = new StyleDimension(0f, 1f),
        Height = new StyleDimension(0f, 1f),
        AnchorPositionOffsetByPercents = Vector2.One / 2f,
        AnchorPositionOffsetByPixels = Vector2.Zero
    };

    #endregion

    public override void OnInitialize()
    {
        Instance = this;

        // 主面板
        MainPanel.SetPadding(0f);
        MainPanel.SetPosPixels(610, 320)
            .SetSizePixels(600, 460)
            .JoinParent(this);

        _titlePanel = ViewHelper.CreateHead(UIStyle.TitleBg * 0.75f, 50f, 10f);
        _titlePanel.RelativeMode = RelativeMode.Vertical;
        _titlePanel.SetPadding(0f);
        _titlePanel.JoinParent(MainPanel);

        // 标题
        _title.SetInnerPixels(_title.TextSize * _title.TextScale);
        _title.JoinParent(_titlePanel);

        // Cross
        _cross.OnLeftMouseDown += (_, _) => Close();
        _cross.OnUpdate += _ =>
        {
            _cross.BgColor = _cross.HoverTimer.Lerp(Color.Transparent, UIStyle.PanelBorder * 0.5f);
        };
        _cross.JoinParent(_titlePanel);

        // 灯泡 - 使用方法界面
        var lightBulb = new LightBulbHelp
        {
            VAlign = 0.5f,
            Left = { Pixels = -80f, Percent = 1f },
            BorderColor = Color.Transparent,
            BgColor = Color.Transparent
        };
        lightBulb.JoinParent(_titlePanel);

        // 文件夹 - 打开文件夹
        var folder = new OpenFolderButton
        {
            VAlign = 0.5f,
            Left = { Pixels = -124f, Percent = 1f },
            BorderColor = Color.Transparent,
            BgColor = Color.Transparent
        };
        folder.JoinParent(_titlePanel);

        _shownPage.SetSize(0f, 400f, 1f, 0f);
        _shownPage.SetPadding(0f);
        _shownPage.JoinParent(MainPanel);
        // _shownPage.SetPage(_weaponPage);

        _weaponPage.JoinParent(_shownPage);
        _chainEditPage.JoinParent(_shownPage);

        PageSlideTimer.OnClosed = ResetPagePosition;
        PageSlideTimer.OnOpened = ResetPagePosition;

        MainPanel.Append(_particleSystem);
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
    }

    private void ResetPagePosition()
    {
        _weaponPage.Left.Pixels = PageSlideTimer.Lerp(0, -600);
        _weaponPage.Recalculate();
        _chainEditPage.Left.Pixels = PageSlideTimer.Lerp(600, 0);
        _chainEditPage.Recalculate();
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

    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);

        StartTimer.UpdateHighFps();

        PageSlideTimer.UpdateHighFps();

        if (PageSlideTimer.Opened)
            _chainEditPage.DrawImePanel();

        if (PageSlideTimer.Closing || PageSlideTimer.Opening)
            ResetPagePosition();
    }

    public void RefreshWeaponPage()
    {
        ChainSaver.ReadAllAmmoChains();
        _weaponPage.ReloadPresetsElement();
        _weaponPage.SetupPreview();
    }

    public void Open()
    {
        Enabled = true;
        StartTimer.Open();

        _chainEditPage.DisableTextInput();
        RefreshWeaponPage();
        ResetPagePosition();

        SoundEngine.PlaySound(SoundID.MenuOpen);
        MyUtils.OperateInventory(true);
    }

    public void Close()
    {
        Enabled = false;
        StartTimer.Close();
        _chainEditPage.DisableTextInput();

        SoundEngine.PlaySound(SoundID.MenuClose);
    }

    public void GoToWeaponPage()
    {
        // _shownPage.SetPage(_weaponPage);
        PageSlideTimer.Close();
        ChainSaver.ReadAllAmmoChains();
        _weaponPage.ReloadPresetsElement();
    }

    public void StartEditingChain(AmmoChain chain, bool isCreatingAChain, string chainName)
    {
        // _chainEditPage = new ChainEditPage.ChainEditPage();
        // _shownPage.SetPage(_chainEditPage);
        PageSlideTimer.Open();
        _chainEditPage.StartEditing(chain, isCreatingAChain, chainName);
    }

    public void TryAddToChain(Item item)
    {
        _chainEditPage.EditingChain.Chain.Add(new AmmoChain.Ammo(new ItemTypeData(item.Clone()), 10));
        _chainEditPage.ShouldResetCurrentChain = true;
        SoundEngine.PlaySound(SoundID.MenuTick);
    }

    public void TryQuickPlaceIntoSlot(ref Item item)
    {
        // 一般来说不可能，但还是判断
        if (!SlotQuickPutAvailable(item))
            return;

        var slot = _weaponPage.WeaponSlot;
        (item, slot.Items[slot.Index]) = (slot.Items[slot.Index], item);
        slot.DoPossibleItemChange();
        SoundEngine.PlaySound(SoundID.Grab);
        // Recipe.FindRecipes();
    }

    public bool SlotQuickPutAvailable(Item item) =>
        PageSlideTimer.AnyClose && MyUtils.CanPlaceInSlot(_weaponPage.SlotItem, item) is 3;

    public override bool RenderTarget2DDraw => !StartTimer.Opened;
    public override float RenderTarget2DOpacity => StartTimer.Schedule;
    public override Vector2 RenderTarget2DOrigin => MainPanel.GetDimensionsCenter();
    public override Vector2 RenderTarget2DPosition => MainPanel.GetDimensionsCenter();
    public override Vector2 RenderTarget2DScale => new(0.95f + StartTimer.Lerp(0, 0.05f));
}