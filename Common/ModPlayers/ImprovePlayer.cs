using ImproveGame.Common.Configs;
using ImproveGame.Common.ModSystems;
using ImproveGame.Content.Items;
using ImproveGame.Content.Items.ItemContainer;
using ImproveGame.Core;
using ImproveGame.Modules.InfiniteBuff.UserInterface;
using ImproveGame.UI;
using ImproveGame.UI.AutoTrash;
using ImproveGame.UI.ExtremeStorage;
using ImproveGame.UI.GrabBagInfo;
using ImproveGame.UI.ItemContainer;
using ImproveGame.UI.MasterControl;
using ImproveGame.UI.OpenBag;
using ImproveGame.UI.QuickShimmer;
using ImproveGame.UIFramework;
using SilkyUIFramework;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.GameInput;

namespace ImproveGame.Common.ModPlayers;

public class ImprovePlayer : ModPlayer
{
    /// <summary>
    /// 有猪猪钱罐
    /// </summary>
    public bool HasPiggyBank;

    /// <summary>
    /// 有保险库
    /// </summary>
    public bool HasSafe;

    /// <summary>
    /// 有护卫熔炉
    /// </summary>
    public bool HasDefendersForge;

    public IItemContainer BannerChest;
    public IItemContainer PotionBag;

    public bool ShouldUpdateTeam;

    public override void OnEnterWorld()
    {
        if (ImproveConfigs.Instance.TeamAutoJoin && Main.netMode is NetmodeID.MultiplayerClient)
        {
            ShouldUpdateTeam = true;
        }
    }

    public override void PostUpdate()
    {
        if (Main.gameMenu)
            return;

        if (Main.myPlayer == Player.whoAmI && _oldItemSelected is not -1 && !Player.ItemAnimationActive)
        {
            Player.selectedItemState = Player.selectedItemState with { selected = _oldItemSelected };
            _oldItemSelected = -1;
        }

        if (ImproveConfigs.Instance.JourneyResearch)
        {
            foreach (var item in from i in Player.inventory where !i.IsAir select i)
            {
                // 旅行自动研究
                if (Main.netMode is not NetmodeID.Server && item.favorited &&
                    Main.LocalPlayer.difficulty is PlayerDifficultyID.Creative &&
                    CreativeItemSacrificesCatalog.Instance.TryGetSacrificeCountCapToUnlockInfiniteItems(item.type,
                        out int amountNeeded))
                {
                    int sacrificeCount =
                        Main.LocalPlayerCreativeTracker.ItemSacrifices.GetSacrificeCount(item.type);
                    if (amountNeeded - sacrificeCount > 0 && item.stack >= amountNeeded - sacrificeCount)
                    {
                        Item itemClone = item.Clone();
                        Main.CreativeMenu.SacrificeItem(ref itemClone, out _);
                        SoundEngine.PlaySound(SoundID.Research);
                        SoundEngine.PlaySound(SoundID.ResearchComplete);
                    }
                }
            }
        }
    }

    public override void ResetEffects()
    {
        HasPiggyBank = false;
        HasSafe = false;
        HasDefendersForge = false;

        if (ImproveConfigs.Instance.SuperVoidVault)
        {
            if (!Player.IsVoidVaultEnabled)
            {
                Player.IsVoidVaultEnabled = InventoryHasItemFast(Player, ItemID.VoidVault);
            }

            // 激活猪猪钱罐的条件：猪猪钱罐，铅笔槽，眼骨
            HasPiggyBank = InventoryHasItemFast(Player, ItemID.PiggyBank, ItemID.ChesterPetItem, ItemID.MoneyTrough);
            HasSafe = InventoryHasItemFast(Player, ItemID.Safe);
            HasDefendersForge = InventoryHasItemFast(Player, ItemID.DefendersForge);
        }

        BannerChest = null;
        PotionBag = null;

        // 从 玩家背包 寻找第一个旗帜盒和药水包
        foreach (var item in from i in Player.inventory where !i.IsAir select i)
        {
            if (BannerChest is null && item.ModItem is BannerChest bannerChest)
            {
                BannerChest = bannerChest;
            }

            if (PotionBag is null && item.ModItem is PotionBag potionBag)
            {
                PotionBag = potionBag;
            }
        }

        // 从 大背包 寻找第一个旗帜盒和药水包
        Item[] superVault = Player.GetModPlayer<DataPlayer>().SuperVault;
        foreach (var item in from i in superVault where !i.IsAir select i)
        {
            if (BannerChest is null && item.ModItem is BannerChest bannerChest)
            {
                BannerChest = bannerChest;
            }

            if (PotionBag is null && item.ModItem is PotionBag potionBag)
            {
                PotionBag = potionBag;
            }
        }

        if (Player.whoAmI == Main.myPlayer)
        {
            if (ShouldUpdateTeam)
            {
                Player.team = 1;
                NetMessage.SendData(MessageID.TeamChange, -1, -1, null, Player.whoAmI);
                ShouldUpdateTeam = false;
            }
        }
    }

    /// <summary>
    /// 重生加速
    /// </summary>
    public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
    {
        // 判断有没有存活的 Boss
        bool hasBoss = CurrentFrameProperties.AnyActiveBoss;

        // 计算要缩短多少时间
        float timeShortened = Player.respawnTimer *
            MathHelper.Clamp(hasBoss ? ImproveConfigs.Instance.BOSSBattleResurrectionTimeShortened : ImproveConfigs.Instance.ResurrectionTimeShortened,
                0f, 100f) / 100f;
        if (timeShortened > 0f)
        {
            int ct = CombatText.NewText(Player.getRect(), new(25, 255, 25), GetTextWith(
                "CombatText.Commonds.ResurrectionTimeShortened", new
                {
                    Name = Player.name,
                    Time = MathF.Round(timeShortened / 60)
                }));
            if (Main.combatText.IndexInRange(ct))
            {
                Main.combatText[ct].lifeTime *= 3;
            }
        }

        Player.respawnTimer -= (int)timeShortened;
        Player.respawnTimer = Math.Max(Player.respawnTimer, 90);
    }

    public override void UpdateDead()
    {
        // 非Boss战时快速复活
        // 计算要缩短多少时间 15*60=900
        int minRemainingTime = (int)(900 * MathHelper.Clamp(100f - ImproveConfigs.Instance.ResurrectionTimeShortened, 0f, 100f) / 100f);
        if (ImproveConfigs.Instance.ResurrectionTimeShortened is 0 || Player.respawnTimer <= minRemainingTime || CurrentFrameProperties.AnyActiveBoss)
            return;
        Player.respawnTimer = minRemainingTime;
    }

    private bool _cacheSwitchSlot;
    private int _oldItemSelected;

    /// <summary>
    /// 快捷键
    /// </summary>
    public override void ProcessTriggers(TriggersSet triggersSet)
    {
        if (KeybindSystem.SuperVaultKeybind.JustPressed)
            PressSuperVaultKeybind();
        if (KeybindSystem.BuffTrackerKeybind.JustPressed)
            PressBuffTrackerKeybind();
        if (KeybindSystem.GrabBagKeybind.JustPressed)
            PressGrabBagKeybind();
        if (KeybindSystem.OpenBagKeybind.JustPressed)
            PressOpenBagKeybind();
        if (KeybindSystem.QuickShimmerKeybind.JustPressed)
            PressQuickShimmerKeybind();
        if (KeybindSystem.HotbarSwitchKeybind.JustPressed || _cacheSwitchSlot)
            PressHotbarSwitchKeybind();
        if (KeybindSystem.AutoTrashKeybind.JustPressed)
            PressAutoTrashKeybind();
        if (KeybindSystem.MasterControlKeybind.JustPressed)
            PressMasterControlKeybind();
        if (KeybindSystem.ExtremeStorageSearch.JustPressed)
            PressExtremeStorageSearchKeybind();

        // 下面是操作类快捷键
        if (Player.dead) return;
        if (KeybindSystem.DiscordRodKeybind.JustPressed)
            PressDiscordKeybind();
    }

    private static void PressSuperVaultKeybind()
    {
        if (!ImproveConfigs.Instance.SuperVault) return;

        if (BigBagGUI.Instance.Enabled && BigBagGUI.Instance.StartTimer.AnyOpen)
        {
            BigBagGUI.Instance.Close();
        }
        else
        {
            bool oldInventory = Main.playerInventory;

            BigBagGUI.Instance.Open();

            // 假设也按了物品栏快捷键...
            if (PlayerInput.Triggers.JustPressed.Inventory)
                OperateInventory(oldInventory);
        }
    }

    private static void PressBuffTrackerKeybind()
    {
        if (!UISceneManager.Instance.TryGetInstance<InfiniteBuffController>(out var controller)) return;

        controller.Enabled = !controller.Enabled;

        // [old]
        //if (BuffTrackerGUI.Visible)
        //    UISystem.Instance.BuffTrackerGUI.Close();
        //else
        //    UISystem.Instance.BuffTrackerGUI.Open();
    }

    private static void PressOpenBagKeybind()
    {
        var ui = OpenBagGUI.Instance;
        if (ui is null) return;

        if (ui.Enabled && ui.StartTimer.AnyOpen)
            ui.Close();
        else
            ui.Open();
    }
    private static void PressQuickShimmerKeybind()
    {
        var ui = QuickShimmerGUI.Instance;
        if (ui is null || !ImproveConfigs.Instance.QuickShimmer || !QuickShimmerSystem.Unlocked) return;

        if (ui.Enabled && ui.StartTimer.AnyOpen)
            ui.Close();
        else
            ui.Open();
    }

    private static void PressGrabBagKeybind()
    {
        if (Main.HoverItem is not { } item || item.IsAir || GrabBagInfoGUI.Instance is null) return;

        bool hasLoot = ItemLoader.CanRightClick(Main.HoverItem);
        hasLoot &= Main.ItemDropsDB.GetRulesForItemID(item.type).Count > 0;

        if (GrabBagInfoGUI.Instance.Enabled && GrabBagInfoGUI.Instance.StartTimer.AnyOpen &&
            (GrabBagInfoGUI.ItemID == item.type || item.IsAir || !hasLoot))
            GrabBagInfoGUI.Instance.Close();
        else if (hasLoot)
            GrabBagInfoGUI.Instance.Open(Main.HoverItem.type);
    }

    private void PressHotbarSwitchKeybind()
    {
        if (Main.LocalPlayer.ItemTimeIsZero && Main.LocalPlayer.itemAnimation is 0)
        {
            for (int i = 0; i <= 9; i++)
            {
                (Player.inventory[i], Player.inventory[i + 10]) = (Player.inventory[i + 10], Player.inventory[i]);
                if (Main.netMode is NetmodeID.MultiplayerClient)
                {
                    NetMessage.SendData(MessageID.SyncEquipment, number: Main.myPlayer, number2: i,
                        number3: Main.LocalPlayer.inventory[i].prefix);
                    NetMessage.SendData(MessageID.SyncEquipment, number: Main.myPlayer, number2: i + 10,
                        number3: Main.LocalPlayer.inventory[i].prefix);
                }
            }

            _cacheSwitchSlot = false;
        }
        else
        {
            _cacheSwitchSlot = true;
        }
    }

    private void PressAutoTrashKeybind()
    {
        InventoryTrashGUI.Hidden = !InventoryTrashGUI.Hidden;
    }

    private void PressMasterControlKeybind()
    {
        MasterControlGUI.Opened = !MasterControlGUI.Opened;
    }

    private void PressDiscordKeybind()
    {
        if (Player.ItemAnimationActive) return;

        int itemType = ItemID.None;
        var items = GetAllInventoryItemsList(Player);
        foreach (var item in items)
        {
            if (item.type == ItemID.RodOfHarmony)
            {
                itemType = ItemID.RodOfHarmony;
                break; // 最高优先级
            }

            if (item.type == ItemID.RodofDiscord)
                itemType = ItemID.RodofDiscord;
        }

        if (itemType is ItemID.None)
            return;

        _oldItemSelected = Player.selectedItem;
        UseItemByType(Player, itemType);
    }

    private void PressExtremeStorageSearchKeybind()
    {
        if (Main.HoverItem is not { } item || item.IsAir || UISystem.Instance.ExtremeStorageGUI is null) return;

        if (ExtremeStorageGUI.Visible)
            ExtremeStorageGUI.SetSearchContent(item.Name);
    }
}