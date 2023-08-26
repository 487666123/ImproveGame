using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.Config;

namespace ItemMagnetPro
{
    internal class MagConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ServerSide;
        // [Header("Item Magnet Pro")]

        [DefaultValue(64)]
        public int RangeInTile;

        [DefaultValue(2)]
        public int DelayInSecond;

        [DefaultValue(false)]
        public bool SuperVaultCompact;

        private void UpdateConfig()
        {
            MagPlayer.RangeSQ = (int)Math.Pow(RangeInTile * 16, 2);
            MagPlayer.Delay = DelayInSecond * 60;
            MagPlayer.SuperVault = SuperVaultCompact;
        }

        public override void OnLoaded()
        {
            UpdateConfig();
        }
        public override void OnChanged()
        {
            UpdateConfig();
        }
    }
}
