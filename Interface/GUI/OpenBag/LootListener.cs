using Terraria.DataStructures;

namespace ImproveGame.Interface.GUI.OpenBag;

public class LootListener : GlobalItem
{
    /// <summary>
    /// -1为不在监听中，此外的数值为正在为某个袋子监听，该袋子的ID
    /// </summary>
    public static int _listening;

    public override void OnSpawn(Item item, IEntitySource source)
    {
        if (_listening is -1 || Main.netMode is NetmodeID.Server)
            return;

        if (source is not EntitySource_ItemOpen openSource || openSource.ItemType != _listening)
            return;

        // Main.NewText($"[i:{item.type}] {Lang.GetItemName(item.type)}");

        if (Main.LocalPlayer is null || !Main.LocalPlayer.TryGetModPlayer(out LootKeeper keeper))
            return;

        // 堆叠和物品相同的
        foreach (var destination in keeper.Loots.Where(destination =>
                     destination is not null && !destination.IsAir && destination.type == item.type))
        {
            ItemLoader.TryStackItems(destination, item, out var _);

            if (item.IsAir)
                return;
        }

        // 还有剩余，加在后面
        keeper.Loots.Add(item.Clone());

        item.TurnToAir();
    }
}