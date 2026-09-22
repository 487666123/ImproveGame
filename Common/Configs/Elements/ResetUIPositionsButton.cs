using ImproveGame.UI;
using ImproveGame.UI.ItemSearcher;
using ImproveGame.UI.OpenBag;
using ImproveGame.UI.PlayerStats;
using ImproveGame.UIFramework;
using ImproveGame.UIFramework.Common;
using ImproveGame.UserInterfaces.BigBag;

namespace ImproveGame.Common.Configs.Elements;

internal class ResetUIPositionsButton : LargerPanelElement
{
    public override void LeftClick(UIMouseEvent evt)
    {
        base.LeftClick(evt);

        // 重设UI的位置
        UIPlayer.HugeInventoryUIPosition = UIPlayer.HugeInventoryDefPosition;
        //UIPlayer.BuffTrackerPosition = UIPlayer.BuffTrackerDefPosition;
        UIPlayer.WorldFeaturePosition = UIPlayer.WorldFeatureDefPosition;
        UIPlayer.ItemSearcherPosition = UIPlayer.ItemSearcherDefPosition;
        UIPlayer.OpenBagPosition = UIPlayer.OpenBagDefPosition;
        UIPlayer.PlayerInfoTogglePosition = UIPlayer.PlayerInfoToggleDefPosition;

        // 应用
        UISystem uiSystem = UISystem.Instance;
        BigBagGUI.Instance.MainPanel.SetPos(UIPlayer.HugeInventoryUIPosition).Recalculate();
        BigBagUI.Instance?.SetWindowPosition(UIPlayer.HugeInventoryUIPosition);
        //uiSystem.BuffTrackerGUI.MainPanel.SetPos(UIPlayer.BuffTrackerPosition).Recalculate();
        ItemSearcherGUI.Instance.MainPanel.SetPos(UIPlayer.ItemSearcherPosition).Recalculate();
        OpenBagGUI.Instance.MainPanel.SetPos(UIPlayer.OpenBagPosition).Recalculate();
        PlayerStatsGUI.Instance.ControllerSwitch.SetPosPixels(UIPlayer.PlayerInfoTogglePosition).Recalculate();
    }
}
