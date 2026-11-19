using ImproveGame.Common.Configs;
using ImproveGame.Common.ModPlayers;
using ImproveGame.UI.AutoTrash;
using ImproveGame.UIFramework.Common;

namespace ImproveGame.Common.GlobalItems;

// 两个单词
// Grab 抓住（游戏内指代使物品飞向玩家的操作）
// Pickup 捡起

public class GrabAndPickup : GlobalItem
{
    // 抓取距离
    public override void GrabRange(Item item, Player player, ref int grabRange) => grabRange += ImproveConfigs.Instance.GrabDistance * 16;

    public override void Load()
    {
        // 已废弃
        // On_Player.PickupItem += PickupItem;
        On_Player.GetItem += On_Player_GetItem;
    }

    /// <summary>
    /// 修改抓取速度 <br/>
    /// 禁用原版吸附速度返回: <see langword="true"/> <br/>
    /// 允许原版吸附速度返回: <see langword="false"/>
    /// </summary>
    public override bool GrabStyle(Item item, Player player)
    {
        if (ImproveConfigs.Instance.GrabDistance > 0)
        {
            var toPlayer = player.Center - item.Center;

            // 物品与玩家中心重合时为零向量，直接归一化会产生 NaN，保留原速度等待拾取
            if (toPlayer != Vector2.Zero)
            {
                var dir = Vector2.Normalize(toPlayer) * 25f;
                item.velocity = (item.velocity * 9 + dir) / 10;
            }
        }

        return false;
    }

    private Item On_Player_GetItem(On_Player.orig_GetItem orig, Player self, int plr, Item source, GetItemSettings settings)
    {
        source = orig.Invoke(self, plr, source, settings);

        if (source.IsAir)
        {
            return source;
        }

        if (settings.LongText == false && settings.NoText == false && settings.CanGoIntoVoidVault == true)
        {
            Item cloneItem = source.Clone();

            // 背包溢出堆叠至其他容器
            if (!source.IsACoin)
            {
                // 大背包
                if (ImproveConfigs.Instance.SuperVault && self.GetModPlayer<UIPlayerSetting>().SuperVault_GrabItemsWhenOverflowing)
                {
                    source.StackToArray(self.GetModPlayer<DataPlayer>().SuperVault);
                }

                if (source.IsAir) goto Finish;

                // 猪猪 保险箱 ...
                if (ImproveConfigs.Instance.SuperVoidVault && self.TryGetModPlayer(out ImprovePlayer improvePlayer))
                {
                    if (improvePlayer.HasPiggyBank)
                    {
                        source.StackToArray(self.bank.item);
                    }

                    if (source.IsAir) goto Finish;

                    if (improvePlayer.HasSafe)
                    {
                        source.StackToArray(self.bank2.item);
                    }

                    if (source.IsAir) goto Finish;

                    if (improvePlayer.HasDefendersForge)
                    {
                        source.StackToArray(self.bank3.item);
                    }
                }
            }

            // 标签
            Finish: PickupPopupText(cloneItem, source);

            return source;
        }

        return source;
    }

    /*/// <summary>
    /// 拾取的物品溢出背包后, 已废弃
    /// </summary>
    private static Item PickupItem(On_Player.orig_PickupItem orig, Player player, int playerIndex, int worldItemArrayIndex, Item itemToPickUp)
    {
        itemToPickUp = orig(player, playerIndex, worldItemArrayIndex, itemToPickUp);

        if (itemToPickUp.IsAir)
        {
            return itemToPickUp;
        }

        Item cloneItem = itemToPickUp.Clone();

        // 背包溢出堆叠至其他容器
        if (!itemToPickUp.IsACoin)
        {
            // 大背包
            if (Config.SuperVault && player.GetModPlayer<UIPlayerSetting>().SuperVault_OverflowGrab)
            {
                itemToPickUp.StackToArray(player.GetModPlayer<DataPlayer>().SuperVault);
            }

            if (itemToPickUp.IsAir) goto Finish;

            // 猪猪 保险箱 ...
            if (Config.SuperVoidVault && player.TryGetModPlayer(out ImprovePlayer improvePlayer))
            {
                if (improvePlayer.HasPiggyBank)
                {
                    itemToPickUp.StackToArray(player.bank.item);
                }

                if (itemToPickUp.IsAir) goto Finish;

                if (improvePlayer.HasSafe)
                {
                    itemToPickUp.StackToArray(player.bank2.item);
                }

                if (itemToPickUp.IsAir) goto Finish;

                if (improvePlayer.HasDefendersForge)
                {
                    itemToPickUp.StackToArray(player.bank3.item);
                }
            }
        }

        Finish:

        if (itemToPickUp.stack < cloneItem.stack)
        {
            SoundEngine.PlaySound(SoundID.Grab);
        }

        Main.item[worldItemArrayIndex] = itemToPickUp;
        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            NetMessage.SendData(MessageID.SyncItem, -1, -1, null, worldItemArrayIndex);
        }
        return itemToPickUp;
    }
*/

    public override bool ItemSpace(Item item, Player player)
    {
        if (item.IsACoin)
        {
            return false;
        }

        // 大背包
        if (ImproveConfigs.Instance.SuperVault &&
            player.TryGetModPlayer(out UIPlayerSetting setting) &&
            player.TryGetModPlayer(out DataPlayer dataPlayer))
        {
            if ((setting.SuperVault_GrabItemsWhenOverflowing && item.CanStackToArray(dataPlayer.SuperVault)) ||
                (setting.SuperVault_PrioritizeGrabbing && item.CanStackToArray(dataPlayer.SuperVault) && item.TheArrayHas(dataPlayer.SuperVault)))
            {
                return true;
            }
        }

        if (ImproveConfigs.Instance.SuperVoidVault && player.TryGetModPlayer<ImprovePlayer>(out var improvePlayer))
        {
            if (improvePlayer.HasPiggyBank = improvePlayer.HasPiggyBank && item.CanStackToArray(player.bank.item))
            {
                return true;
            }

            if (improvePlayer.HasSafe = improvePlayer.HasSafe && item.CanStackToArray(player.bank2.item))
            {
                return true;
            }

            if (improvePlayer.HasDefendersForge = improvePlayer.HasDefendersForge && item.CanStackToArray(player.bank3.item))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 允许你在玩家捡到一项物品时做一些特殊的事情 <br/>
    /// 返回 <see langword="false"/> 会阻止物品进入玩家的 inventoy，默认情况下返回 true。
    /// </summary>
    public override bool OnPickup(Item source, Player player)
    {
        if (UIConfigs.Instance.QoLAutoTrash &&
            player.TryGetModPlayer(out AutoTrashPlayer autoTrashPlayer) && true &&
            autoTrashPlayer.ThrowAwayItems.Any(adItem => adItem.type == source.type))
        {
            autoTrashPlayer.EnterRecentlyThrownAwayItems(source);
            SoundEngine.PlaySound(SoundID.Grab);
            return false;
        }

        if (!player.TryGetModPlayer(out ImprovePlayer improvePlayer))
        {
            return true;
        }

        // 旗帜盒
        if (improvePlayer.BannerChest is not null && improvePlayer.BannerChest.AutoStorage && ItemToBanner(source) != -1)
        {
            Item cloneItem = source.Clone();
            improvePlayer.BannerChest.ItemIntoContainer(source);
            PickupPopupText(cloneItem, source);
        }

        if (source.IsAir) return false;

        // 药水袋
        if (improvePlayer.PotionBag is not null && improvePlayer.PotionBag.AutoStorage && source.buffType > 0 && source.consumable)
        {
            Item item = source.Clone();
            improvePlayer.PotionBag.ItemIntoContainer(source);
            PickupPopupText(item, source);
        }

        if (source.IsAir) return false;

        // 大背包
        if (ImproveConfigs.Instance.SuperVault &&
            player.TryGetModPlayer(out UIPlayerSetting setting) && player.TryGetModPlayer(out DataPlayer dataPlayer) &&
            setting.SuperVault_PrioritizeGrabbing && source.TheArrayHas(dataPlayer.SuperVault))
        {
            Item cloneItem = source.Clone();
            source.StackToArray(dataPlayer.SuperVault);
            PickupPopupText(cloneItem, source);
        }

        if (source.IsAir) return false;

        // 虚空保险库 之 智能收纳
        if (ImproveConfigs.Instance.SmartVoidVault && !source.IsACoin)
        {
            // 虚空保险库
            if (player.IsVoidVaultEnabled && source.TheArrayHas(player.bank4.item))
            {
                Item cloneItem = source.Clone();
                source.StackToArray(player.bank4.item);
                PickupPopupText(cloneItem, source);
            }

            if (source.IsAir) return false;

            // 猪猪 保险箱 ...
            if (ImproveConfigs.Instance.SuperVoidVault)
            {
                if (improvePlayer.HasPiggyBank && source.TheArrayHas(player.bank.item))
                {
                    Item cloneItem = source.Clone();
                    source.StackToArray(player.bank.item);
                    PickupPopupText(cloneItem, source);
                }

                if (source.IsAir) return false;

                if (improvePlayer.HasSafe && source.TheArrayHas(player.bank2.item))
                {
                    Item cloneItem = source.Clone();
                    source.StackToArray(player.bank2.item);
                    PickupPopupText(cloneItem, source);
                }

                if (source.IsAir) return false;

                if (improvePlayer.HasDefendersForge && source.TheArrayHas(player.bank3.item))
                {
                    Item cloneItem = source.Clone();
                    source.StackToArray(player.bank3.item);
                    PickupPopupText(cloneItem, source);
                }

                if (source.IsAir) return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 物品拾取后提示
    /// </summary>
    private static void PickupPopupText(Item cloneItem, Item self)
    {
        if (self.stack < cloneItem.stack)
        {
            SoundEngine.PlaySound(SoundID.Grab);
            PopupText.NewText(PopupTextContext.ItemPickupToVoidContainer, cloneItem, cloneItem.stack - self.stack);
        }
    }
}
