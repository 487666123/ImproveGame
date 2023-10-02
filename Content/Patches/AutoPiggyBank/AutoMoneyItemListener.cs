using ImproveGame.Content.BuilderToggles;
using Terraria.ModLoader.IO;

namespace ImproveGame.Content.Patches.AutoPiggyBank;

public class AutoMoneyItemListener : GlobalItem
{
    public override bool OnPickup(Item item, Player player)
    {
        if (item.type is ItemID.PiggyBank or ItemID.ChesterPetItem or ItemID.MoneyTrough &&
            player.TryGetModPlayer<AutoMoneyPlayerListener>(out var listener) && !listener.AutoSaveUnlocked)
            listener.AutoSaveUnlocked = true;

        if (!PiggyToggle.AutoSaveEnabled)
            return base.OnPickup(item, player);

        // 没存成就执行原来的Pickup
        return !TryDepositACoin(item, player);
    }

    /// <summary>
    /// 存一个钱币物品
    /// </summary>
    /// <param name="item">物品</param>
    /// <param name="player">玩家</param>
    /// <returns>是否成功</returns>
    public static bool TryDepositACoin(Item item, Player player)
    {
        // 无空位
        if (!player.bank.item.Any(i =>
            {
                // 铂金币和护卫奖章 - 有对应钱币且不到最大堆叠
                if (item.type is ItemID.PlatinumCoin or ItemID.DefenderMedal)
                {
                    if (item.type == i.type && i.stack < i.maxStack)
                        return true;
                }
                // 非铂金币 - 有对应钱币就行
                else if (item.IsACoin)
                {
                    if (item.type == i.type)
                        return true;
                }

                return i.IsAir;
            }))
            return false;

        int type = item.type;

        if (type is ItemID.DefenderMedal)
        {
            PopupText.NewText(PopupTextContext.RegularItemPickup, item, item.stack);
            SoundEngine.PlaySound(SoundID.Grab, player.position);
            item.StackToArray(player.bank.item);
            return item.IsAir;
        }

        if (!item.IsACoin)
            return false;

        ulong totalMoney = CoinUtils.CalculateCoinValue(type, (uint)item.stack);
        totalMoney = player.bank.item.Aggregate(totalMoney,
            (current, bItem) => current + CoinUtils.CalculateCoinValue(bItem.type, (uint)bItem.stack));

        List<Item> toPlace = CoinUtils.ConvertCopperValueToCoins(totalMoney);
        CoinUtils.ReplaceOrPlaceIntoChest(player.bank, toPlace);

        // 看看有没有没放进去的，重新生成
        toPlace.ForEach(coinLeft =>
        {
            if (coinLeft.IsAir) return;
            player.QuickSpawnItem(player.GetSource_DropAsItem(), coinLeft, coinLeft.stack);
        });

        PopupText.NewText(PopupTextContext.RegularItemPickup, item, item.stack);
        SoundEngine.PlaySound(SoundID.CoinPickup, player.position);
        return true;
    }
}

// 试存钱币槽
public class AutoMoneyPlayerListener : ModPlayer
{
    internal bool AutoSaveUnlocked;
    private int _detectCd;

    public override void SaveData(TagCompound tag)
    {
        tag["unlocked"] = AutoSaveUnlocked;
    }

    public override void LoadData(TagCompound tag)
    {
        AutoSaveUnlocked = tag.GetBool("unlocked");
    }

    public override void PostUpdate()
    {
        _detectCd++;

        // 看看能不能解锁自动存钱
        if (!AutoSaveUnlocked && _detectCd % 90 == 0)
            AutoSaveUnlocked = HasItem(GetAllInventoryItemsList(Player), ItemID.PiggyBank,
                ItemID.ChesterPetItem, ItemID.MoneyTrough);

        if (Main.myPlayer != Player.whoAmI || !PiggyToggle.AutoSaveEnabled)
            return;

        // 10帧检测一次，仅存钱币槽
        if (_detectCd % 10 == 0)
            DetectCoins();
        // 60帧检测一次，护卫奖章
        if (_detectCd % 60 == 0)
            DetectDefenderMedal();
        // 30帧更新一下铂金最大堆叠
        if (_detectCd % 30 == 0)
            CoinUtils.PlatinumMaxStack = new Item(ItemID.PlatinumCoin).maxStack;
    }

    private void DetectCoins()
    {
        for (var i = 50; i <= 53; i++)
        {
            var item = Player.inventory[i];
            if (!item.IsAir && item.IsACoin && AutoMoneyItemListener.TryDepositACoin(item, Player))
            {
                item.TurnToAir();
            }
        }
    }

    private void DetectDefenderMedal()
    {
        for (var i = 0; i <= 49; i++)
        {
            var item = Player.inventory[i];
            if (!item.IsAir && item.type is ItemID.DefenderMedal && AutoMoneyItemListener.TryDepositACoin(item, Player))
            {
                item.TurnToAir();
            }
        }
    }
}