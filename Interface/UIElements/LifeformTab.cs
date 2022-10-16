﻿using ImproveGame.Common.Systems;
using ImproveGame.Interface.UIElements_Shader;

namespace ImproveGame.Interface.UIElements
{
    public class LifeformTab : SUIPanel
    {
        private UIImage tickUI;
        private int _npcId;

        public LifeformTab(int npcId) : base(new(89, 116, 213), new(44, 57, 105, 160), CalculateBorder: false)
        {
            _npcId = npcId;
            this.SetSize(new Vector2(0f, 36f), 1f);
            //IgnoresMouseInteraction = true;
            
            Append(new UITextPanel<string>($"{Lang.GetNPCNameValue(npcId)}")
            {
                IgnoresMouseInteraction = true,
                DrawPanel = false,
                Left = StyleDimension.FromPixels(-14f),
                VAlign = 0.5f
            });

            tickUI = new UIImage(TextureAssets.InventoryTickOn)
            {
                Top = StyleDimension.FromPixels(-4f),
                HAlign = 1f,
                VAlign = 0.5f
            };
            Append(tickUI);
        }

        public override void Update(GameTime gameTime)
        {
            bool hide = LifeAnalyzeCore.Blacklist.GetValueOrDefault(_npcId);
            tickUI.SetImage(hide ? TextureAssets.InventoryTickOff : TextureAssets.InventoryTickOn);
            if (tickUI.IsMouseHovering)
                Main.instance.MouseText(Language.GetTextValue($"LegacyInterface.{(hide ? "60" : "59")}"));
        }

        public override void MouseDown(UIMouseEvent evt)
        {
            base.MouseDown(evt);
            SoundEngine.PlaySound(SoundID.MenuTick);
            LifeAnalyzeCore.Blacklist[_npcId] = !LifeAnalyzeCore.Blacklist.GetValueOrDefault(_npcId);
        }
    }
}