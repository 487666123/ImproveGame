using ImproveGame.Common.ModItems;

namespace ImproveGame.Common.ModSystems
{
    internal class MagSystem : ModSystem
    {
        public static bool Timely => Main.GameUpdateCount % 15 == 0;
        public static List<Item> ItemPool = new List<Item>();
        public override void PreUpdatePlayers()
        {
            if (Timely)
            {
                foreach (Item item in Main.item)
                {
                    if (!item.TryGetGlobalItem(out MagItem mag))
                    {
                        continue;
                    }
                    mag.Target = null;
                }
                ItemPool = Main.item.ToList();
                ItemPool.RemoveAll(i => !i.active);
            }
        }
    }
}
