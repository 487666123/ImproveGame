using ImproveGame.Common.ModPlayers;

namespace ImproveGame.Content.InfoDisplays
{
    public class AttractionInfo : InfoDisplay
    {
        public static Color Encumber = Color.GreenYellow;
        public static Color Exhaust = Color.Red;
        // public static string DiKey = "Mods.ItemMagnetPro.General.Di";
        public static LocalizedText MagSelection => Language.GetOrRegister("Selection");
        public static string InfoKey = "Mods.ItemMagnetPro.InfoDisplays.";
        public static string SelectionKey = InfoKey + "Selections.";
        public static string ItemActionKey = InfoKey + "ItemActions.";

        public override void SetStaticDefaults()
        {
            _ = MagSelection;
            base.SetStaticDefaults();
        }

        public override bool Active()
        {
            return true;
        }

        public override string DisplayValue(ref Color displayColor)
        {
            var mp = Main.LocalPlayer.GetModPlayer<MagPlayer>();
            if (mp.ItemAction == ItemAction.Encumber)
            {
                displayColor = Encumber;
            }
            else if (mp.ItemAction == ItemAction.Exhaust)
            {
                displayColor = Exhaust;
            }
            //TODO use 1.4.4 standards
            return Language.GetTextValue(DiKey, Language.GetTextValue(SelectionKey + mp.Selection.Value), Language.GetTextValue(ItemActionKey + mp.ItemAction.Value));
        }
    }
}