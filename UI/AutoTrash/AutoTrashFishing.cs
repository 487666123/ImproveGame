using ImproveGame.Common.Configs;
using Terraria.ModLoader;

namespace ImproveGame.UI.AutoTrash;

/// <summary>
/// 自动垃圾桶的钓鱼适配 <br/>
/// 有两处「直接塞进背包」的物品不经过世界掉落物的拾取流程（<see cref="GlobalItem.OnPickup"/> 不会触发）：
/// <list type="bullet">
/// <item>收杆时由 <c>Projectile.AI_061_FishingBobber_GiveItemToPlayer</c> 发放钓获物</item>
/// <item>渔夫任务奖励由 <c>Player.GetAnglerReward</c> 发放</item>
/// </list>
/// 所以在这两处拦截，命中丢弃列表就不把物品给玩家。
/// </summary>
public static class AutoTrashFishing
{
    internal class AutoTrashFishingLoader : ILoadable
    {
        void ILoadable.Load(Mod mod)
        {
            On_Projectile.AI_061_FishingBobber_GiveItemToPlayer += On_GiveItemToPlayer;
            On_Player.GetAnglerReward += On_GetAnglerReward;
            On_Player.GetItem += On_GetItem;
        }

        void ILoadable.Unload() { }
    }

    /// <summary>
    /// 是否正在发放渔夫任务奖励
    /// </summary>
    private static bool _givingAnglerReward;

    private static void On_GiveItemToPlayer(On_Projectile.orig_AI_061_FishingBobber_GiveItemToPlayer orig,
        Projectile self, Player thePlayer, int itemType)
    {
        // 命中丢弃列表就不发物品，原版的进背包、放不下掉到地上、拾取弹字都不会发生
        if (TryThrowAway(thePlayer, itemType))
        {
            return;
        }

        orig.Invoke(self, thePlayer, itemType);
    }

    private static void On_GetAnglerReward(On_Player.orig_GetAnglerReward orig, Player self, NPC angler, int questItemType)
    {
        // 渔夫奖励是一条条调 GetItem 塞进背包的，这里只做标记，实际拦截放到 GetItem 里
        _givingAnglerReward = true;

        try
        {
            orig.Invoke(self, angler, questItemType);
        }
        finally
        {
            _givingAnglerReward = false;
        }
    }

    private static Item On_GetItem(On_Player.orig_GetItem orig, Player self, int plr, Item newItem, GetItemSettings settings)
    {
        if (_givingAnglerReward && Main.myPlayer == self.whoAmI && TryThrowAway(self, newItem.type))
        {
            // GetItem 返回的是「没能装下的部分」。整叠都当作已收下再返回空物品，
            // 原版发奖励的循环看到 stack 为 0 就不会把它丢到地上。
            newItem.stack = 0;
            return new Item();
        }

        return orig.Invoke(self, plr, newItem, settings);
    }

    /// <summary>
    /// 把钓上来的物品计入自动垃圾桶
    /// </summary>
    /// <returns>是否已经丢弃</returns>
    private static bool TryThrowAway(Player player, int itemType)
    {
        if (!UIConfigs.Instance.QoLAutoTrash || itemType <= 0 || itemType >= ItemID.Count)
            return false;

        if (!player.TryGetModPlayer(out AutoTrashPlayer autoTrashPlayer))
            return false;

        if (autoTrashPlayer.ThrowAwayItems.All(item => item.type != itemType))
            return false;

        var caughtItem = new Item();
        caughtItem.SetDefaults(itemType);

        autoTrashPlayer.EnterRecentlyThrownAwayItems(caughtItem);
        SoundEngine.PlaySound(SoundID.Grab);

        return true;
    }
}
