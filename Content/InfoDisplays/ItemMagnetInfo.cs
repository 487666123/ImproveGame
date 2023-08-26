using Terraria;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Localization;
using Microsoft.Xna.Framework;

namespace ItemMagnetPro.InfoDisplays
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
            return Language.GetTextValue(DiKey, Language.GetTextValue(SelectionKey + mp.Selection.Value), Language.GetTextValue(ItemActionKey + mp.ItemAction.Value));
        }
    }
}