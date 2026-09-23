using ImproveGame.UI.ItemContainer;
using MonoMod.Utils;

namespace ImproveGame.Core;

// 备用名 CurrentFrameFlags，但会和原版重了
/// <summary>
/// 记录当前帧的一些属性，如是否有Boss存在，这样可以避免重复遍历NPC数组
/// </summary>
public static class CurrentFrameProperties
{
    public static class ExistItems
    {
        /// <summary>
        /// 空字典，用于在方法中返回默认值（非null）
        /// </summary>
        private readonly static Dictionary<int, ItemTotalStack> EmptyDict = [];

        /// <summary>
        /// 玩家当前帧物品栏中拥有此物品的数量
        /// </summary>
        public readonly static Dictionary<int, ItemTotalStack> InventoryCount = new(60);

        /// <summary>
        /// 玩家当前帧大背包中拥有此物品的数量
        /// </summary>
        public readonly static Dictionary<int, ItemTotalStack> BigBagCount = new(110);

        /// <summary>
        /// 玩家当前帧 Bank 中拥有此物品的数量
        /// </summary>
        public readonly static Dictionary<int, ItemTotalStack> BanksCount = new(160);

        public static int GetTotalStack(int type)
        {
            int count = 0;

            if (InventoryCount.TryGetValue(type, out var invTotalStack))
                count += invTotalStack.Number;

            if (BanksCount.TryGetValue(type, out var banksTotalStack))
                count += banksTotalStack.Number;

            if (BigBagCount.TryGetValue(type, out var bigbagTotalStack))
                count += bigbagTotalStack.Number;

            return count;
        }

        public static Dictionary<int, ItemTotalStack> GetPlayerInventoryCount(Player player)
        {
            return !player.TryGetModPlayer<PlayerUpdater>(out var modPlayer)
                ? EmptyDict
                : modPlayer.InventoryCount;
        }
    }

    /// <summary>
    /// 判断场上有没有Boss存在，避免重复遍历NPC数组
    /// 不知道为什么，原版的不包括世吞，这里包括
    /// </summary>
    public static bool AnyActiveBoss { get; set; }
}

internal class SystemUpdater : ModSystem
{
    public override void PreUpdateNPCs()
    {
        CurrentFrameProperties.AnyActiveBoss = false;

        for (int l = 0; l < Main.maxNPCs; l++)
        {
            var npc = Main.npc[l];

            // 排除四柱
            if (npc.type is NPCID.LunarTowerNebula or NPCID.LunarTowerSolar or NPCID.LunarTowerStardust
                or NPCID.LunarTowerVortex)
                continue;

            if (npc.active && (npc.boss || NPCID.Sets.ShouldBeCountedAsBossForBestiary[npc.type]))
            {
                CurrentFrameProperties.AnyActiveBoss = true;
                return;
            }
        }
    }
}

internal class PlayerUpdater : ModPlayer
{
    internal readonly Dictionary<int, ItemTotalStack> InventoryCount = new(60);

    public override void PreUpdate()
    {
        UpdateHasItemLists();
    }

    public override void UpdateDead()
    {
        UpdateHasItemLists();
    }

    private void UpdateHasItemLists()
    {
        // 所有玩家都更新的，自己的表
        InventoryCount.Clear();
        CountItemsFromArray(Player.inventory, InventoryCount);

        // 本地玩家才更新
        if (Main.myPlayer != Player.whoAmI) return;

        CurrentFrameProperties.ExistItems.InventoryCount.Clear();
        CurrentFrameProperties.ExistItems.BanksCount.Clear();
        CurrentFrameProperties.ExistItems.BigBagCount.Clear();

        // 玩家背包
        CurrentFrameProperties.ExistItems.InventoryCount.AddRange(InventoryCount);
        // CountItemsFromArray(Player.inventory, CurrentFrameProperties.ExistItems.InventoryCount);

        // Banks
        var bankItems = MyUtils.GetAllInventoryItemsList(Player, "inv mod", 160);
        CountItemsFrom(bankItems, CurrentFrameProperties.ExistItems.BanksCount);

        // 大背包
        var bigBagItems = MyUtils.GetAllInventoryItemsList(Player, "inv portable", 110);
        CountItemsFrom(bigBagItems, CurrentFrameProperties.ExistItems.BigBagCount);
    }

    #region Native Methods

    // NOTE: 这里的List<Item>不能改作IEnumerable<Item>，因为IEnumerable的foreach实现比List慢
    private static void CountItemsFrom(List<Item> items, IDictionary<int, ItemTotalStack> count)
    {
        foreach (var item in items.Where(storedItem => !storedItem.IsAir))
        {
            if (item.ModItem is IItemContainer iic)
                ItemStatistics(count, iic.ItemContainer);

            if (count.TryGetValue(item.type, out ItemTotalStack value))
                value.Number += item.stack;
            else
                count.Add(item.type, new ItemTotalStack(item));
        }
    }

    private static void CountItemsFromArray(Item[] items, IDictionary<int, ItemTotalStack> count)
    {
        foreach (var item in items.Where(storedItem => !storedItem.IsAir))
        {
            if (item.ModItem is IItemContainer iic)
                ItemStatistics(count, iic.ItemContainer);

            if (count.TryGetValue(item.type, out ItemTotalStack value))
                value.Number += item.stack;
            else
                count.Add(item.type, new ItemTotalStack(item));
        }
    }

    private static void ItemStatistics(IDictionary<int, ItemTotalStack> count, List<Item> items)
    {
        foreach (var item in items.Where(chestItem => !chestItem.IsAir))
        {
            if (count.TryGetValue(item.type, out ItemTotalStack value))
                value.Number += item.stack;
            else
                count.Add(item.type, new ItemTotalStack(item));
        }
    }

    #endregion
}

public struct ItemTotalStack(Item item)
{
    public int Number = item.stack;

    public readonly int Type = item.type;
    public readonly int BuffType = item.buffType;
    public readonly int BuffTime = item.buffTime;
    public readonly int PlaceStyle = item.placeStyle;
    public readonly int CreateTile = item.createTile;

    // https://smartcrane.tech/archives/173/
    // 为了普适性，C# 的 struct 的默认 Equals() 、GetHashCode() 和 ToString() 都是较慢实现，甚至涉及反射。
    // 用户自定义的 struct，都应重载上述3个函数，手动实现
    public override bool Equals(object obj) => obj is ItemTotalStack its && item.type == its.Type;
    public override int GetHashCode() => (Number, Type).GetHashCode();

    public static bool operator ==(ItemTotalStack left, ItemTotalStack right) => left.Equals(right);

    public static bool operator !=(ItemTotalStack left, ItemTotalStack right) => !(left == right);

    public override string ToString() => $"Item: {Type}, Count: {Number}";
}