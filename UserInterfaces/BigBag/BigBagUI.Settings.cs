using ImproveGame.Common.ModPlayers;
using ImproveGame.UIFramework.Common;
using SilkyUIFramework.Elements;

namespace ImproveGame.UserInterfaces.BigBag;

public partial class BigBagUI
{
    private void InitializeSettings()
    {
        SettingsButton.LeftMouseDown += (_, _) => SetSettingsState(true);
        SettingsCloseButton.LeftMouseDown += (_, _) => SetSettingsState(false);

        SettingsOverlay.LeftMouseDown += (el, evt) => { if (evt.Source == el) SetSettingsState(false); };


        if (!Main.LocalPlayer.TryGetModPlayer<UIPlayerSetting>(out var setting)) return;

        BindSwitch(RecipesRow, RecipesSwitch, () => setting.SuperVault_ParticipateSynthesis,
            value =>
            {
                setting.SuperVault_ParticipateSynthesis = value;
                DataPlayer.RefreshRecipes = true;
            });

        BindSwitch(SmartGrabRow, SmartGrabSwitch, () => setting.SuperVault_PrioritizeGrabbing,
            value =>
            {
                setting.SuperVault_PrioritizeGrabbing = value;
                PlayerBigBagSettingPacket.SendMyPlayer();
            });

        BindSwitch(AutoGrabRow, AutoGrabSwitch, () => setting.SuperVault_GrabItemsWhenOverflowing,
            value =>
            {
                setting.SuperVault_GrabItemsWhenOverflowing = value;
                PlayerBigBagSettingPacket.SendMyPlayer();
            });
    }

    private void SetSettingsState(bool visible)
    {
        if (SettingsOverlay.Invalid == !visible) return;
        SettingsOverlay.Invalid = !visible;
        SoundEngine.PlaySound(SoundID.MenuTick);
    }

    private static void BindSwitch(UIElementGroup itemBar, SUIToggleSwitch @switch, Func<bool> getter, Action<bool> setter)
    {
        @switch.Status = getter();

        @switch.SwitchDown += value =>
        {
            if (getter() == value) return;
            setter(value);
            SoundEngine.PlaySound(SoundID.MenuTick);
        };

        @switch.OnUpdateStatus += _ =>
        {
            if (getter() == @switch.Status) return;
            setter(@switch.Status);
        };

        itemBar.LeftMouseDown += (el, evt) =>
        {
            if (evt.Source != @switch) @switch.OnSwitchDown(!@switch.Status);
        };
    }
}
