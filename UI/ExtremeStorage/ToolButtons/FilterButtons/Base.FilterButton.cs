using ImproveGame.UIFramework;
using Terraria.ModLoader.UI;

namespace ImproveGame.UI.ExtremeStorage.ToolButtons.FilterButtons;

public abstract class FilterButton : ToolButton
{
    private readonly AnimationTimer _hoverTimer = new();
    public ItemGroup Group;
    public bool Activated { get; private set; }

    protected abstract int IconIndex { get; }

    protected abstract string LocalizationKey { get; }

    public abstract bool Filter(Item item);

    public override Texture2D Texture => ModAsset.FilterIcons.Value;

    public sealed override string HoverText => MyUtils.GetText($"UI.ExtremeStorage.{Group}.{LocalizationKey}");

    // 这玩意没用，重写了DrawSelf
    public sealed override Rectangle? SourceRectangle => null;

    public sealed override void OnTakeEffect()
    {
        Activated = !Activated;
        UISystem.Instance.ExtremeStorageGUI.FindChestsAndPopulate(true);
    }

    public sealed override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        if (Activated)
            _hoverTimer.Open();
        else
            _hoverTimer.Close();
        _hoverTimer.Update();

        Opacity = _hoverTimer.Lerp(0.4f, 1f);
    }

    public override void DrawSelf(SpriteBatch spriteBatch)
    {
        if (ExtremeStorageGUI.CurrentGroup is ItemGroup.Setting)
            return;

        int frameY = Group switch
        {
            ItemGroup.Weapon => 0,
            ItemGroup.Tool => 2,
            ItemGroup.Ammo => 4,
            ItemGroup.Armor => 6,
            ItemGroup.Accessory => 8,
            ItemGroup.Block => 10,
            ItemGroup.Misc => 12,
            _ => throw new ArgumentOutOfRangeException()
        };

        const int horizontalFrames = 7;
        const int verticalFrames = 14;
        var iconRectangle = Texture.Frame(horizontalFrames, verticalFrames, IconIndex, frameY);
        var overlayRectangle = Texture.Frame(horizontalFrames, verticalFrames, IconIndex, frameY + 1);

        var dimensions = GetDimensions();
        var pos = dimensions.Position();
        var color = Color.White * Opacity;
        spriteBatch.Draw(Texture, pos, iconRectangle, color);

        if (!IsMouseHovering)
            return;

        UICommon.TooltipMouseText(HoverText);
        spriteBatch.Draw(Texture, pos, overlayRectangle, Color.White);
    }
}