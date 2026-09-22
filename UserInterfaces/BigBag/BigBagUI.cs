using ImproveGame.Common.Configs;
using ImproveGame.Common.ModPlayers;
using ImproveGame.Packets;
using ImproveGame.UIFramework.Common;
using SilkyUIFramework;
using SilkyUIFramework.Animation;
using SilkyUIFramework.Attributes;
using SilkyUIFramework.Elements;

namespace ImproveGame.UserInterfaces.BigBag;

[RegisterUI]
public partial class BigBagUI : BaseBody
{
    public static BigBagUI Instance { get; private set; }

    private readonly AnimationTimer _openAnimation = new(3);
    private bool _isOpen;

    public bool IsOpen => _isOpen && Main.playerInventory && ImproveConfigs.Instance.SuperVault;
    public override bool IsInteractable => IsOpen;

    public override bool Enabled
    {
        get
        {
            if (_isOpen && !IsOpen) Close();
            return _isOpen || _openAnimation.IsReverseUpdating;
        }
        set
        {
            if (value) Open();
            else Close();
        }
    }

    public Vector2 WindowPosition => new Vector2(Left.Pixels, Top.Pixels) + DragOffset;

    protected override void OnInitialize()
    {
        Instance = this;

        InitializeComponent();

        Header.ControlTarget = this;

        InitializeAppearance();
        CloseButton.LeftMouseDown += (_, _) => Close();

        InitializeSettings();
        InitializeInventoryActions();
        RefreshInventory();
        RestorePosition();
        PlayerBigBagSettingPacket.SendMyPlayer();
    }

    private void RefreshInventory() =>
        ItemGrid?.Items = DataPlayer.Get(Main.LocalPlayer).SuperVault;

    protected override void UpdateStatus(GameTime gameTime)
    {
        base.UpdateStatus(gameTime);
        _openAnimation.Update(gameTime);

        RefreshLabels();

        RefreshInventory();
    }

    public override void HandleDraw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        UseRenderTarget = !_openAnimation.IsForwardCompleted;
        Opacity = _openAnimation.Schedule;
        var center = Vector2.Transform(Bounds.Center, SilkyUI.TransformMatrix);
        RenderTargetMatrix = Matrix.CreateTranslation(-center.X, -center.Y, 0f)
            * Matrix.CreateScale(_openAnimation.Lerp(0.95f, 1f))
            * Matrix.CreateTranslation(center.X, center.Y, 0f);
        base.HandleDraw(gameTime, spriteBatch);
        DrawButtonTooltip();
    }

    public void SetWindowPosition(Vector2 position)
    {
        Left = new Anchor(position.X);
        Top = new Anchor(position.Y);
        DragOffset = Vector2.Zero;
        UIPlayer.HugeInventoryUIPosition = position;
    }

    private void RestorePosition()
    {
        UIPlayer.CheckPositionValid(ref UIPlayer.HugeInventoryUIPosition, UIPlayer.HugeInventoryDefPosition);
        SetWindowPosition(UIPlayer.HugeInventoryUIPosition);
    }

    public void Toggle()
    {
        if (IsOpen) Close();
        else Open();
    }

    public void Open()
    {
        if (IsOpen || Main.gameMenu || !ImproveConfigs.Instance.SuperVault) return;

        RefreshInventory();
        if (!_isOpen) RestorePosition();
        OperateInventory(true);
        _isOpen = true;
        _openAnimation.StartUpdate();
        UISceneManager.Instance.Activate(this);
        SoundEngine.PlaySound(SoundID.MenuOpen);
    }

    public void Close()
    {
        if (!_isOpen) return;

        _isOpen = false;
        _openAnimation.StartReverseUpdate();
        UIPlayer.HugeInventoryUIPosition = WindowPosition;
        SoundEngine.PlaySound(SoundID.MenuClose);
        ClientConfigCore.SaveConfig();
    }

    protected override void OnExitTree()
    {
        if (Instance == this) Instance = null;
        base.OnExitTree();
    }
}
