using ImproveGame.UI;
using ImproveGame.UIFramework;

namespace ImproveGame.Content.Functions
{
    public class LifeAnalyzeCore : ModSystem
    {
        private bool _mouseLeftPrev;
        internal static List<NPC> RaritiedNpcs;
        internal static Dictionary<int, bool> Blacklist = new();

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int index = layers.FindIndex(layer => layer.Name == "Vanilla: Info Accessories Bars");
            if (index != -1)
            {
                layers.Insert(index + 1, new LegacyGameInterfaceLayer("ImproveGame: Modify Info Acc Display",
                    () =>
                    {
                        var cursorText = Main.instance._mouseTextCache.cursorText;

                        if (Main.LocalPlayer.ItemAnimationActive || !Main.mouseText || string.IsNullOrEmpty(cursorText) ||
                            cursorText != Lang.inter[105].Value + "\n" + MyUtils.GetText("LifeAnalyzer.Tip"))
                        {
                            return true;
                        }

                        Main.LocalPlayer.mouseInterface = true;

                        if (Main.mouseLeft && !_mouseLeftPrev)
                        {
                            switch (LifeformAnalyzerGUI.Visible)
                            {
                                case false:
                                    UISystem.Instance.LifeformAnalyzerGUI.Open();
                                    break;
                                case true:
                                    UISystem.Instance.LifeformAnalyzerGUI.Close();
                                    break;
                            }
                        }

                        _mouseLeftPrev = Main.mouseLeft;
                        return true;
                    }, InterfaceScaleType.UI));
            }
        }

        public override void Load()
        {
            RaritiedNpcs = new();
            foreach ((_, NPC npc) in ContentSamples.NpcsByNetId)
            {
                if (npc?.rarity > 0)
                {
                    RaritiedNpcs.Add(npc);
                }
            }
        }
    }

    public class LifeformAnaliyzerModify : GlobalInfoDisplay
    {
        public static int counter;
        public static int number;

        public override void ModifyDisplayParameters(InfoDisplay currentDisplay, ref string displayValue, ref string displayName,
            ref Color displayColor, ref Color displayShadowColor)
        {
            return; // 暂时取消此功能，待重置
            if (currentDisplay != InfoDisplay.LifeformAnalyzer)
                return;

            displayName += "\n" + MyUtils.GetText("LifeAnalyzer.Tip");

            const int maxDistance = 1300;
            int maxRarity = 0;
            int npcIndex = -1;
            if (counter <= 0)
            {
                counter = 15;
                for (int k = 0; k < 200; k++)
                {
                    var n = Main.npc[k];
                    if (!LifeAnalyzeCore.Blacklist.GetValueOrDefault(n.netID) && n.active && n.rarity > maxRarity && (n.Center - Main.LocalPlayer.Center).Length() < maxDistance)
                    {
                        npcIndex = k;
                        maxRarity = n.rarity;
                    }
                }

                number = (byte)npcIndex;
            }
            else
            {
                counter--;
                npcIndex = number;
            }

            displayValue = (npcIndex is < 0 or >= 200 || !Main.npc[npcIndex].active || Main.npc[npcIndex].rarity <= 0) ? Language.GetTextValue("GameUI.NoRareCreatures") : Main.npc[npcIndex].GivenOrTypeName;
            if (npcIndex >= 0 && npcIndex < 200 && Main.npc[npcIndex].active && Main.npc[npcIndex].rarity > 0)
            {
                displayValue = Main.npc[npcIndex].GivenOrTypeName;
                Main.instance.DrawInfoAccs_AdjustInfoTextColorsForNPC(Main.npc[npcIndex], ref displayColor, ref displayShadowColor);
            }
            else
            {
                displayValue = Language.GetTextValue("GameUI.NoRareCreatures");
                displayColor = new Color(100, 100, 100, Main.mouseTextColor);
            }
        }
    }
}
