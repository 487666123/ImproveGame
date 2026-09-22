using SilkyUIFramework;
using SilkyUIFramework.Common.Tweening;
using SilkyUIFramework.Extensions;

namespace ImproveGame.UserInterfaces.BigBag;

public partial class BigBagItemSlot
{
    private const float HighlightTransitionDuration = 0.2f;

    [Flags]
    private enum AppearanceState
    {
        None = 0,
        Favorited = 1 << 0,
        NewItem = 1 << 1,
        Disabled = 1 << 2
    }

    private Tween _stateTween;
    private AppearanceState? _state;

    private void InitializeAppearance()
    {
        SetSize(BigBagItemGrid.SlotSize, BigBagItemGrid.SlotSize);

        BorderRadius = new Vector4(8f);
        BorderColor = SUIColor.Border * 0.75f;
        BackgroundColor = SUIColor.Background * 0.25f;
    }

    protected override void UpdateStatus(GameTime _)
    {
        var canInteract = Interactable;

        // 仅在允许交互时将悬停物品标记为已查看，让新物品高亮自然淡出。
        if (IsMouseHovering && canInteract)
            Item.newAndShiny = false;

        SetState(GetAppearanceState(canInteract));
        DisplayItemInfo = canInteract;
    }

    private AppearanceState GetAppearanceState(bool canInteract)
    {
        var state = AppearanceState.None;

        if (Item.favorited) state |= AppearanceState.Favorited;
        if (Item.newAndShiny) state |= AppearanceState.NewItem;
        if (!canInteract) state |= AppearanceState.Disabled;

        return state;
    }

    private void SetState(AppearanceState state)
    {
        if (_state == state) return;

        _state = state;
        PlayStateTween(state);
    }

    /// <summary>根据状态创建边框和背景的共同过渡。</summary>
    private void PlayStateTween(AppearanceState state)
    {
        var (border, background) = GetAppearanceTarget(state);

        _stateTween?.Kill();
        _stateTween = CreateTween().Parallel()
            .SetEase(EaseType.Out)
            .SetTrans(TransitionType.Expo);
        _stateTween.BorderColorTo(this, border, HighlightTransitionDuration);
        _stateTween.BgColorTo(this, background, HighlightTransitionDuration);
    }

    private static (Color Border, Color Background) GetAppearanceTarget(AppearanceState state)
    {
        var border = SUIColor.Border * 0.8f;
        var background = Color.Lerp(SUIColor.Border, SUIColor.Background, 0.7f) * 0.75f;

        // 新物品高亮优先级高于收藏高亮，保持原版显示效果。
        if ((state & AppearanceState.NewItem) != 0)
        {
            border = new Color(99, 161, 157, 180);
            background = new Color(55, 93, 131, 180);
        }
        else if ((state & AppearanceState.Favorited) != 0)
        {
            border = SUIColor.Highlight;
            background = Color.Lerp(background, SUIColor.Highlight, 0.22f);
        }

        if ((state & AppearanceState.Disabled) != 0)
            background = Color.Gray * 0.3f;

        return (border, background);
    }

    protected override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        if (IsMouseHovering && Interactable && !Item.IsAir)
        {
            PlayerLoader.HoverSlot(Main.LocalPlayer, Items, ItemSlot.Context.InventoryItem, Index);
            UpdateCursorOverride();
        }

        base.Draw(gameTime, spriteBatch);
    }
}
