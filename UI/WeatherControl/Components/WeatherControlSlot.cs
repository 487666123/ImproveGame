using ImproveGame.Content.Functions.WeatherControl;
using ImproveGame.UIFramework.BaseViews;
using ImproveGame.UIFramework.Common;
using ImproveGame.UIFramework.SUIElements;
using Terraria.ModLoader.UI;

namespace ImproveGame.UI.WeatherControl.Components;

/// <summary>
/// 模组项栏里的一个槽位，绑定一个 <see cref="IWeatherControl"/>
/// 左键循环档位，右键切换锁定
/// </summary>
public class WeatherControlSlot : TimerView
{
    public readonly IWeatherControl Control;

    private readonly SUIImage _icon;

    public WeatherControlSlot(IWeatherControl control)
    {
        Control = control;

        // 嵌在艺术画面里，所以用更小的尺寸和半透明边框，避免抢镜
        Border = 1f;
        Rounded = new Vector4(4f);
        SetSizePixels(32f, 32f);

        _icon = new SUIImage(control?.Icon, false)
        {
            ImageAlign = new Vector2(0.5f),
            ImageScale = 1f,
        };
        _icon.SetSizePercent(1f, 1f);
        _icon.JoinParent(this);
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        if (Control is null) return;

        var tex = Control.Icon;
        _icon.Texture = tex;
        // 给槽位留两像素的呼吸空间，超过的图标按比例缩放
        if (tex != null)
        {
            float maxSide = Math.Max(tex.Width, tex.Height);
            _icon.ImageScale = maxSide > 28f ? 28f / maxSide : 1f;
        }

        BgColor = Color.Black * HoverTimer.Lerp(0.25f, 0.55f);
        BorderColor = Control.GetLocked()
            ? UIStyle.ItemSlotBorderFav
            : Color.White * HoverTimer.Lerp(0.15f, 0.45f);
    }

    public override void LeftMouseDown(UIMouseEvent evt)
    {
        base.LeftMouseDown(evt);
        if (Control is null) return;

        int count = Control.Stages.Count;
        if (count < 2) return;

        int current = Control.GetStage();
        if (current < 0 || current >= count) current = -1;
        int next = (current + 1) % count;
        SoundEngine.PlaySound(SoundID.MenuTick);
        WeatherControlRegistry.Instance.DispatchStage(Control.Id, next);
    }

    public override void RightMouseDown(UIMouseEvent evt)
    {
        base.RightMouseDown(evt);
        if (Control is null || !Control.SupportsLock) return;

        bool target = !Control.GetLocked();
        SoundEngine.PlaySound(SoundID.MenuTick);
        WeatherControlRegistry.Instance.DispatchLocked(Control.Id, target);
    }

    public override void DrawSelf(SpriteBatch spriteBatch)
    {
        base.DrawSelf(spriteBatch);

        if (Control is null || !IsMouseHovering) return;

        var sb = new System.Text.StringBuilder();
        sb.Append(Control.GetDisplayName() ?? Control.Name);
        var tip = Control.GetTooltip();
        if (!string.IsNullOrEmpty(tip)) sb.Append('\n').Append(tip);
        if (Control.SupportsLock)
            sb.Append('\n').Append(Control.GetLocked()
                ? GetText("UI.WeatherGUI.ModdedSlotLocked")
                : GetText("UI.WeatherGUI.ModdedSlotUnlocked"));
        UICommon.TooltipMouseText(sb.ToString());
    }
}
