using ImproveGame.Common.Configs;
using ImproveGame.Common.ModSystems;
using ImproveGame.UIFramework.BaseViews;
using ImproveGame.UIFramework.Common;
using ImproveGame.UIFramework.Graphics2D;
using Terraria.ModLoader.UI;

namespace ImproveGame.UI.ModernConfig.OptionElements.PresetElements;

public class BasePresetElement : TimerView
{
    public string Tooltip;

    public BasePresetElement(string label, string tooltip, bool hasLabelElement = true)
    {
        Tooltip = tooltip;
        Width.Set(0f, 1f);
        Height.Set(46f, 0f);
        Rounded = new Vector4(12f);
        SetPadding(12, 4);

        RelativeMode = RelativeMode.Vertical;
        OverflowHidden = true;

        if (!hasLabelElement)
            return;

        var labelElement = new SlideText(label);
        labelElement.JoinParent(this);
    }

    // 为了让UI之间实际上无间隔，防止鼠标滑过时Tooltip文字闪现，这里重写绘制，而不使用Spacing
    public override void DrawSelf(SpriteBatch spriteBatch)
    {
        var dimensions = GetDimensions();
        var position = dimensions.Position();
        var size = dimensions.Size();

        // 这里修改这两个值，而不使用Spacing
        position.Y += 3f;
        size.Y -= 6f;

        // 背景板
        var panelColor = HoverTimer.Lerp(UIStyle.ListItemColor * 0.2f, UIStyle.ListItemColor * 0.4f);
        if (!Interactable)
            panelColor = Color.Gray * 0.3f;

        SDFRectangle.NoBorder(position, size, new Vector4(8f), panelColor * 0.8f, Main.UIScaleMatrix);

        // 提示
        if (!IsMouseHovering)
            return;

        TooltipPanel.SetText(Tooltip);

        // 不可控制，为什么呢？
        if (Interactable || !CanShowInteractTip)
            return;

        if (CantOperateDueToHostVerification)
        {
            string hostTip = MyUtils.GetText("Configs.ImproveConfigs.OnlyHost.Tips");
            UICommon.TooltipMouseText(hostTip);
        }
        else if (CantOperateDueToPasswordVerification)
        {
            string passwordTip = MyUtils.GetText("Configs.ImproveConfigs.OnlyHostByPassword.Tips");
            UICommon.TooltipMouseText(passwordTip);
        }
    }

    protected virtual bool Interactable => !CantOperateInGame || Main.gameMenu;

    private bool CantOperateInGame => CantOperateDueToPasswordVerification || CantOperateDueToHostVerification;

    private bool CantOperateDueToHostVerification =>
        Main.netMode is NetmodeID.MultiplayerClient &&
ImproveConfigs.Instance.OnlyHost && !Main.countsAsHostForGameplay[Main.myPlayer];

    private bool CantOperateDueToPasswordVerification =>
        Main.netMode is NetmodeID.MultiplayerClient &&
ImproveConfigs.Instance.OnlyHostByPassword && !NetPasswordSystem.LocalPlayerRegistered;

    internal bool CanShowInteractTip = true;
}