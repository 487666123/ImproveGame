﻿using ImproveGame.Common.ModSystems;
using ImproveGame.Content.Items;
using ImproveGame.Interface.Common;
using ImproveGame.Interface.GUI;
using Microsoft.Xna.Framework.Input;
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

    public BannerChest BannerChest;
    public PotionBag PotionBag;

    public bool ShouldUpdateTeam;

    public override void OnEnterWorld()
    {
        if (Config.TeamAutoJoin && Main.netMode is NetmodeID.MultiplayerClient)
        {
            ShouldUpdateTeam = true;
        }
    }

    public override void PostUpdate()
    {
        if (Main.gameMenu)
            return;

        if (Config.JourneyResearch)
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
                        CreativeUI.SacrificeItem(item.Clone(), out _);
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
        if (Config.SuperVoidVault)
        {
            if (!Player.IsVoidVaultEnabled)
            {
                Player.IsVoidVaultEnabled = Player.HasItem(ItemID.VoidVault);
            }

            // 激活猪猪钱罐的条件：猪猪钱罐，铅笔槽，眼骨
            HasPiggyBank = Player.inventory.HasOne(ItemID.PiggyBank, ItemID.MoneyTrough, ItemID.ChesterPetItem);
            HasSafe = Player.HasItem(ItemID.Safe);
            HasDefendersForge = Player.HasItem(ItemID.DefendersForge);
        }

        BannerChest = null;
        PotionBag = null;
        // 玩家背包
        foreach (var item in from i in Player.inventory where !i.IsAir select i)
        {
            if (Config.LoadModItems.BannerChest)
            {
                if (BannerChest is null && item.ModItem is BannerChest chest)
                {
                    BannerChest = chest;
                }
            }

            if (Config.LoadModItems.PotionBag)
            {
                if (PotionBag is null && item.ModItem is PotionBag bag)
                {
                    PotionBag = bag;
                }
            }
        }

        // 大背包
        Item[] SuperVault = Player.GetModPlayer<DataPlayer>().SuperVault;
        foreach (var item in from i in SuperVault where !i.IsAir select i)
        {
            if (Config.LoadModItems.BannerChest)
            {
                if (BannerChest is null && item.ModItem is BannerChest chest)
                {
                    BannerChest = chest;
                }
            }

            if (Config.LoadModItems.PotionBag)
            {
                if (PotionBag is null && item.ModItem is PotionBag bag)
                {
                    PotionBag = bag;
                }
            }
        }

        if (Player.whoAmI == Main.myPlayer)
        {
            if (ShouldUpdateTeam)
            {
                Player.team = 1;
                NetMessage.SendData(MessageID.PlayerTeam, -1, -1, null, Player.whoAmI);
                ShouldUpdateTeam = false;
            }
        }

        if (Config.NoCD_FishermanQuest)
        {
            if (Main.anglerQuestFinished || Main.anglerWhoFinishedToday.Contains(Name))
            {
                Main.anglerQuestFinished = false;
                Main.anglerWhoFinishedToday.Clear();
                Main.NewText(Language.GetTextValue($"Mods.ImproveGame.Tips.AnglerQuest"), ItemRarityID.Pink);
            }
        }

        if (Player.whoAmI == Main.myPlayer)
        {
            Player.tileRangeX += Config.ModifyPlayerTileRange;
            Player.tileRangeY += Config.ModifyPlayerTileRange;

            if (Player.HeldItem.IsAir || !Config.ModifyPlayerPlaceSpeed)
            {
                return;
            }

            string internalName = ItemID.Search.GetName(Player.HeldItem.type).ToLower(); // 《英文名》因为没法在非英语语言获取英文名，只能用内部名了
            string currentLanguageName = Lang.GetItemNameValue(Player.HeldItem.type).ToLower();

            if (Config.TileSpeed_Blacklist.Any(str => internalName.Contains(str) || currentLanguageName.Contains(str)))
            {
                return;
            }

            // 是特判捏嘿嘿
            if (Player.HeldItem.ModItem is MoveChest)
            {
                return;
            }

            Player.tileSpeed = 3f;
            Player.wallSpeed = 3f;
        }
    }

    // 重生加速
    public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
    {
        // 判断有没有存活的 Boss
        bool hasBoss = false;
        for (int i = 0; i < Main.npc.Length; i++)
        {
            if (Main.npc[i].active && Main.npc[i].boss)
            {
                hasBoss = true;
                break;
            }
        }
        // 计算要缩短多少时间
        float TimeShortened = Player.respawnTimer * MathHelper.Clamp(hasBoss ? Config.BOSSBattleResurrectionTimeShortened : Config.ResurrectionTimeShortened, 0f, 100f) / 100f;
        if (TimeShortened > 0f)
        {
            int ct = CombatText.NewText(Player.getRect(), new(25, 255, 25), GetTextWith("CombatText.Commonds.ResurrectionTimeShortened", new
            {
                Name = Player.name,
                Time = MathF.Round(TimeShortened / 60)
            }));
            if (Main.combatText.IndexInRange(ct))
            {
                Main.combatText[ct].lifeTime *= 3;
            }
        }
        Player.respawnTimer -= (int)TimeShortened;
    }

    private bool _cacheSwitchSlot;

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
        if (KeybindSystem.HotbarSwitchKeybind.JustPressed || _cacheSwitchSlot)
            PressHotbarSwitchKeybind();
    }

    private static void PressSuperVaultKeybind()
    {
        if (!Config.SuperVault) return;

        if (BigBagGUI.Visible)
            UISystem.Instance.BigBagGUI.Close();
        else
            UISystem.Instance.BigBagGUI.Open();
    }

    private static void PressBuffTrackerKeybind()
    {
        if (BuffTrackerGUI.Visible)
            UISystem.Instance.BuffTrackerGUI.Close();
        else
            UISystem.Instance.BuffTrackerGUI.Open();
    }

    private static void PressGrabBagKeybind()
    {
        if (Main.HoverItem is not { } item || item.IsAir) return;

        // ItemLoader.CanRightClick 里面只要 Main.mouseRight == false 就直接返回了，不知道为什么
        // 所以这里必须伪装成右键点击
        bool oldRight = Main.mouseRight;
        Main.mouseRight = true;

        bool hasLoot = Main.ItemDropsDB.GetRulesForItemID(item.type).Count > 0;
        hasLoot &= CollectHelper.ItemCanRightClick[Main.HoverItem.type] || ItemLoader.CanRightClick(Main.HoverItem);
        Main.mouseRight = oldRight;

        if (GrabBagInfoGUI.Visible && (GrabBagInfoGUI.ItemID == item.type || item.IsAir || !hasLoot))
            UISystem.Instance.GrabBagInfoGUI.Close();
        else if (hasLoot)
            UISystem.Instance.GrabBagInfoGUI.Open(Main.HoverItem.type);
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
                    NetMessage.SendData(MessageID.SyncEquipment, number: Main.myPlayer, number2: i, number3: Main.LocalPlayer.inventory[i].prefix);
                    NetMessage.SendData(MessageID.SyncEquipment, number: Main.myPlayer, number2: i + 10, number3: Main.LocalPlayer.inventory[i].prefix);
                }
            }
            _cacheSwitchSlot = false;
        }
        else
        {
            _cacheSwitchSlot = true;
        }
    }
}