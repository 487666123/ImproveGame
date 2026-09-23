using ImproveGame.Common.Configs;
using ImproveGame.Content.Items.ItemContainer;
using ImproveGame.Content.Tiles;
using ImproveGame.UI.ExtremeStorage;
using Terraria.DataStructures;

namespace ImproveGame.Content.Functions;

public class BannerPatches : ModSystem
{
    public static HashSet<Item> AvailableBanners = new();

    /// <summary>
    /// 旗帜BUFF在背包生效
    /// </summary>
    private static void AddBannerBuff(Item item)
    {
        if (item is null)
            return;

        int bannerID = MyUtils.ItemToBanner(item);
        if (bannerID != -1)
        {
            Main.SceneMetrics.NPCBannerBuff[bannerID] = true;
            Main.SceneMetrics.hasBanner = true;
            AvailableBanners.Add(item);
        }
    }

    public override void TileCountsAvailable(ReadOnlySpan<int> tileCounts)
    {
        // 不要模拟
        if (TileCounter.Simulating)
            return;
        // 随身旗帜（增益站）
        if (ImproveConfigs.Instance.NoPlace_BUFFTile_Banner && Main.netMode is not NetmodeID.Server)
        {
            AvailableBanners = new HashSet<Item>();

            TryAddBuff(Main.LocalPlayer);
            if (ImproveConfigs.Instance.ShareInfBuffs)
                PlayerHelper.ForEachTeammate(Main.myPlayer, TryAddBuff);

            // 从TE中获取所有的无尽Buff物品
            foreach ((int _, TileEntity tileEntity) in TileEntity.ByID)
            {
                if (tileEntity is not TEExtremeStorage { UsePortableBanner: true } storage)
                {
                    continue;
                }

                var banners = storage.FindAllNearbyChestsWithGroup(ItemGroup.Furniture);
                banners.ForEach(i => CheckBanners(Main.chest[i].item));
            }
        }
    }

    private static void TryAddBuff(Player player) =>
        CheckBanners(MyUtils.GetAllInventoryItemsList(player));

    /// <summary>
    /// 从某个玩家的各种物品栏中拿效果
    /// </summary>
    private static void CheckBanners(IEnumerable<Item> items)
    {
        foreach (var item in items)
        {
            AddBannerBuff(item);
            if (item is not null && !item.IsAir && item.ModItem is BannerChest bannerChest &&
                bannerChest.ItemContainer.Count > 0)
            {
                foreach (var p in bannerChest.ItemContainer)
                {
                    AddBannerBuff(p);
                }
            }
        }
    }
}