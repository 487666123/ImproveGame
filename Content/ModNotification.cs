using ImproveGame.UIFramework.Common;
using ImproveGame.UIFramework.Graphics2D;
using ReLogic.Graphics;
using Terraria.GameInput;
using Terraria.UI.Chat;

namespace ImproveGame.Content;

public record ModNotification(string DisplayText, Color TextColor, int ItemIconType = -1)
{
    public virtual bool Equals(ModNotification other) =>
        other != null && DisplayText == other.DisplayText && TextColor == other.TextColor &&
        ItemIconType == other.ItemIconType;

    public override int GetHashCode() => (DisplayText, TextColor, ItemIconType).GetHashCode();
}

public class ModNotificationPopup : IInGameNotification
{
    private readonly ModNotification _notification;
    private bool _isMouseHovering;
    public const int TimeLeftMax = 480;
    public int TimeLeft;

    private float Scale
    {
        get
        {
            if (TimeLeft < 24)
                return MathHelper.Lerp(0f, 1f, (float)TimeLeft / 24);

            if (TimeLeft > TimeLeftMax - 15)
                return MathHelper.Lerp(1f, 0f, ((float)TimeLeft - TimeLeftMax + 15) / 15f);

            return 1f;
        }
    }

    private float Opacity
    {
        get
        {
            float scale = Scale;
            if (scale <= 0.2f)
                return 0f;

            return (scale - 0.2f) / 0.8f;
        }
    }

    public object CreationObject { get; private set; }

    public bool ShouldBeRemoved => TimeLeft <= 0;

    public ModNotificationPopup(string displayText, Color textColor, int itemIconType = -1)
    {
        _notification = new ModNotification(displayText, textColor, itemIconType);
        TimeLeft = TimeLeftMax;
        CreationObject = _notification;
    }

    public event Action OnLeftMouseClick;

    int clickCoolDown;

    public void Update()
    {
        if (!_isMouseHovering || TimeLeft <= 24 || TimeLeft >= TimeLeftMax - 15)
            TimeLeft--;

        if (_isMouseHovering && Main.mouseLeft && FocusHelper.AllowInputProcessing && clickCoolDown <= 0)
        {
            OnLeftMouseClick?.Invoke();
            clickCoolDown = 10;
        }
        clickCoolDown--;
    }

    public void DrawInGame(SpriteBatch spriteBatch, Vector2 bottomAnchorPosition)
    {
        float opacity = Opacity;
        if (opacity <= 0f)
            return;

        string displayText = _notification.DisplayText;
        DynamicSpriteFont font = FontAssets.MouseText.Value;
        Vector2 textSize = ChatManager.GetStringSize(font, displayText, Vector2.One);
        var size = (textSize + new Vector2(30, 10)) * Scale;
        var center = bottomAnchorPosition - new Vector2(0f, size.Y * 0.5f);
        var textCenter = center;

        bool iconExists = TextureAssets.Item.IndexInRange(_notification.ItemIconType);
        if (iconExists)
        {
            const int iconWidth = 24;
            textCenter.X += (iconWidth + 6) * Scale / 2f;
            size.X += (iconWidth + 6) * Scale;
            var item = new Item(_notification.ItemIconType);
            var itemCenter = new Vector2(center.X - size.X / 2f, center.Y);
            itemCenter.X += (15 + iconWidth / 2) * Scale;
            item.DrawIcon(spriteBatch, Color.White, itemCenter, maxSize: 24f, itemScale: Scale);
        }

        Rectangle r = Utils.CenteredRectangle(center, size);
        Vector2 mouseScreen = Main.MouseScreen;
        _isMouseHovering = r.Contains(mouseScreen.ToPoint());
        var panelBorder = UIStyle.PanelBorder * Opacity * (_isMouseHovering ? 0.75f : 0.5f);
        SDFRectangle.HasBorder(r.TopLeft(), r.Size(), new Vector4(UIStyle.ItemSlotBorderRound),
            UIStyle.PanelBg * Opacity, UIStyle.ItemSlotBorderSize, panelBorder, Main.UIScaleMatrix);

        textCenter.Y += 4f * Scale;
        Color textColor = _notification.TextColor * (Main.mouseTextColor / 255f) * opacity;
        ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, displayText, textCenter, textColor, 0.0f,
            textSize / 2f, new Vector2(Scale), spread: 1f);

        if (_isMouseHovering)
            OnMouseOver();
    }

    public void PushAnchor(ref Vector2 positionAnchorBottom)
    {
        float num = 70f * Opacity;
        positionAnchorBottom.Y -= num;
    }

    private void OnMouseOver()
    {
        if (PlayerInput.IgnoreMouseInterface)
            return;
        Main.LocalPlayer.mouseInterface = true;
        if (Main.mouseLeft && Main.mouseLeftRelease)
        {
            Main.mouseLeftRelease = false;
            TimeLeft = 24;
        }
    }

    // 原版中此方法无用
    public void DrawInNotificationsArea(SpriteBatch spriteBatch, Rectangle area, ref int gamepadPointLocalIndexTouse)
    {
        Utils.DrawInvBG(spriteBatch, area, Color.Red);
    }

    public override bool Equals(object obj) =>
        obj is ModNotificationPopup other && other._notification.Equals(_notification);

    public override int GetHashCode() => _notification.GetHashCode();
}