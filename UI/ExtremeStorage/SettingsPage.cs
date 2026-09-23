using ImproveGame.Content.Tiles;
using ImproveGame.Packets.NetStorager;
using ImproveGame.UIFramework.BaseViews;

namespace ImproveGame.UI.ExtremeStorage
{
    public class SettingsPage : View
    {
        private TEExtremeStorage Storage => ExtremeStorageGUI.Storage;

        public SettingsPage()
        {
            Left.Set(ExtremeStorageGUI.CurrentGroup is ItemGroup.Setting ? 0f : -9999f, 0f);
            Width.Set(0f, 1f);
            Height.Set(0f, 1f);

            LongSwitch recipesSwitch = new(() => Storage.UseForCrafting,
                state =>
                {
                    Storage.UseForCrafting = state;
                    SyncDataPacket.Get(Storage.ID).Send();
                },
                "UI.ExtremeStorage.CountForCrafting")
            {
                ResetAnotherPosition = true,
                RelativeMode = RelativeMode.Vertical
            };
            recipesSwitch.JoinParent(this);

            LongSwitch buffsSwitch = new(
                () => Storage.UseUnlimitedBuffs,
                state =>
                {
                    Storage.UseUnlimitedBuffs = state;
                    SyncDataPacket.Get(Storage.ID).Send();
                },
                "UI.ExtremeStorage.UseUnlimitedPotion")
            {
                RelativeMode = RelativeMode.Vertical
            };
            buffsSwitch.JoinParent(this);

            LongSwitch stationsSwitch = new(
                () => Storage.UsePortableStations,
                state =>
                {
                    Storage.UsePortableStations = state;
                    SyncDataPacket.Get(Storage.ID).Send();
                    // Recipe.FindRecipes();
                },
                "UI.ExtremeStorage.UsePortableStation")
            {
                RelativeMode = RelativeMode.Vertical
            };
            stationsSwitch.JoinParent(this);

            LongSwitch bannersSwitch = new(
                () => Storage.UsePortableBanner,
                state =>
                {
                    Storage.UsePortableBanner = state;
                    SyncDataPacket.Get(Storage.ID).Send();
                    // Recipe.FindRecipes();
                },
                "UI.ExtremeStorage.UsePortableBanner")
            {
                RelativeMode = RelativeMode.Vertical
            };
            bannersSwitch.JoinParent(this);

            var uiText = new UIText(MyUtils.GetText("UI.ExtremeStorage.BasicIntroduction"))
            {
                // IsWrapped = true, // 为了看着舒服，使用手动换行
                TextOriginX = 0f,
                Top = StyleDimension.FromPixels(190f),
                Width = StyleDimension.FromPixels(430f),
                Left = StyleDimension.FromPixels(8f)
            };
            Append(uiText);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (ExtremeStorageGUI.CurrentGroup is not ItemGroup.Setting && Left.Pixels != -9999f)
            {
                Left.Set(-9999f, 0f);
                Recalculate();
            }

            if (ExtremeStorageGUI.CurrentGroup is ItemGroup.Setting && Left.Pixels != 0f)
            {
                Left.Set(0f, 0f);
                Recalculate();
            }
        }
    }
}