using ImproveGame.Common.Configs;
using ImproveGame.Common.ModPlayers;
using ImproveGame.UIFramework.Common;
using SilkyUIFramework;
using SilkyUIFramework.Attributes;
using SilkyUIFramework.Common.Tweening;
using SilkyUIFramework.Elements;
using SilkyUIFramework.Extensions;

namespace ImproveGame.UserInterfaces.BigBag;

[RegisterUI]
public partial class BigBagUI : BaseBody
{
    public static BigBagUI Instance { get; private set; }

    private Tween Tween { get; set; } = new();

    public override bool IsInteractable => !Tween.IsPlaying;

    public override bool Enabled
    {
        get
        {
            if (!Main.playerInventory || !ImproveConfigs.Instance.SuperVault)
            {
                field = false;
                return false;
            }

            return field;
        }
        set;
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
        ItemGrid?.Items = Main.LocalPlayer.GetModPlayer<DataPlayer>().SuperVault;

    protected override void UpdateStatus(GameTime gameTime)
    {
        base.UpdateStatus(gameTime);

        RefreshLabels();
        RefreshInventory();
    }

    public override void HandleDraw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        base.HandleDraw(gameTime, spriteBatch);
    }

    protected override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        base.Draw(gameTime, spriteBatch);
        DrawButtonTooltip();
        UIPlayer.HugeInventoryUIPosition = WindowPosition;
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

    private bool _target = false;

    public void Toggle()
    {
        if (_target) Close(); else Open();
        ClientConfigCore.SaveConfig();
    }

    private void Open()
    {
        if (_target) return;
        if (Main.gameMenu || !ImproveConfigs.Instance.SuperVault) return;

        UISceneManager.Instance.Activate(this);
        SoundEngine.PlaySound(SoundID.MenuOpen);
        MyUtils.OperateInventory(true);

        AnimateTo(1f, Matrix.Identity, 0.2f);
        Tween.OnFinished += () =>
        {
            UseRenderTarget = false;
        };

        _target = true;
        Enabled = true;
        UseRenderTarget = true;
    }

    private void Close()
    {
        if (!_target) return;
        SoundEngine.PlaySound(SoundID.MenuClose);

        var center = Vector2.Transform(Bounds.Center, SilkyUI.TransformMatrix);
        var matrix = Matrix.CreateTranslation(-center.X, -center.Y, 0f)
            * Matrix.CreateScale(0.95f)
            * Matrix.CreateTranslation(center.X, center.Y, 0f);

        AnimateTo(0f, matrix, 0.2f);
        Tween.OnFinished += () =>
        {
            Enabled = false;
            UseRenderTarget = false;
        };

        _target = false;
        UseRenderTarget = true;
    }

    private void AnimateTo(float opacity, Matrix matrix, float duration)
    {
        Tween?.Kill();
        Tween = CreateTween().Parallel().SetEase(EaseType.Out).SetTrans(TransitionType.Expo);
        Tween.FadeTo(this, opacity, duration);
        Tween.MemberTo(this, nameof(RenderTargetMatrix), matrix, duration);
    }

    protected override void OnExitTree()
    {
        base.OnExitTree();
        if (Instance == this) Instance = null;
    }
}
