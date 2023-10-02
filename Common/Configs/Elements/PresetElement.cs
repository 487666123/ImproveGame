using ReLogic.Graphics;
using System.Diagnostics;
using Terraria.ModLoader.Config.UI;
using Terraria.ModLoader.UI;
using Terraria.UI.Chat;

namespace ImproveGame.Common.Configs.Elements;

public class FukMeCalamityElement : PresetElement
{
    protected override void SetPreset(ImproveConfigs config)
    {
        config.SuperVoidVault = true;
        config.SmartVoidVault = true;
        config.SuperVault = true;
        config.ItemMaxStack = 9999;
        config.GrabDistance = 5;
        config.NoConsume_SummonItem = true;
        config.ExtraToolSpeed = 0.5f;
        config.ModifyPlayerPlaceSpeed = true;
        config.PortableCraftingStation = true;
        config.NoPlace_BUFFTile_Banner = true;
        config.NoConsume_Potion = true;
        config.NoConsume_Ammo = true;
        config.ImprovePrefix = true;
        config.MiddleEnableBank = true;
        config.FasterExtractinator = true;
        config.TownNPCHome = true;
        config.TownNPCGetTFIntoHouse = true;
        config.NPCLiveInEvil = true;
        config.TownNPCSpawnSpeed = 12;
        config.NoCD_FishermanQuest = true;
        config.NPCCoinDropRate = 8;
        config.ModifyNPCHappiness = true;
        config.NPCHappiness = 75;
        config.LavalessLavaSlime = true;
        config.TravellingMerchantStay = true;
        config.TravellingMerchantRefresh = true;
        config.BestiaryQuickUnlock = true;
        config.AlchemyGrassGrowsFaster = true;
        config.AlchemyGrassAlwaysBlooms = true;
        config.StaffOfRegenerationAutomaticPlanting = true;
        config.NoBiomeSpread = true;
        config.RespawnWithFullHP = true;
        config.DontDeleteBuff = true;
        config.ExtraPlayerBuffSlots = 99;
        config.TreeGrowFaster = true;
        config.ShakeTreeFruit = true;
        config.GemTreeAlwaysDropGem = true;
        config.NoLakeSizePenalty = true;
        config.MostTreeMin = 22;
        config.MostTreeMax = 36;
        config.PalmTreeMin = 22;
        config.PalmTreeMax = 36;
        config.GemTreeMin = 20;
        config.GemTreeMax = 30;
    }
}

public class ILoveBalanceElement : PresetElement
{
    protected override void SetPreset(ImproveConfigs config)
    {
        config.SuperVoidVault = false;
        config.SmartVoidVault = false;
        config.SuperVault = false;
        config.ItemMaxStack = 9999;
        config.GrabDistance = 5;
        config.NoConsume_SummonItem = false;
        config.ExtraToolSpeed = 0.125f;
        config.ModifyPlayerPlaceSpeed = true;
        config.PortableCraftingStation = true;
        config.NoPlace_BUFFTile_Banner = false;
        config.NoConsume_Potion = false;
        config.NoConsume_Ammo = true;
        config.ImprovePrefix = false;
        config.MiddleEnableBank = true;
        config.FasterExtractinator = true;
        config.TownNPCHome = true;
        config.TownNPCGetTFIntoHouse = false;
        config.NPCLiveInEvil = true;
        config.TownNPCSpawnSpeed = -1;
        config.NoCD_FishermanQuest = true;
        config.NPCCoinDropRate = 1;
        config.ModifyNPCHappiness = false;
        config.NPCHappiness = 75;
        config.LavalessLavaSlime = true;
        config.TravellingMerchantStay = false;
        config.TravellingMerchantRefresh = true;
        config.BestiaryQuickUnlock = false;
        config.AlchemyGrassGrowsFaster = false;
        config.AlchemyGrassAlwaysBlooms = false;
        config.StaffOfRegenerationAutomaticPlanting = true;
        config.NoBiomeSpread = false;
        config.RespawnWithFullHP = true;
        config.DontDeleteBuff = true;
        config.ExtraPlayerBuffSlots = 99;
        config.TreeGrowFaster = false;
        config.ShakeTreeFruit = false;
        config.GemTreeAlwaysDropGem = false;
        config.NoLakeSizePenalty = true;
        config.MostTreeMin = 10;
        config.MostTreeMax = 20;
        config.PalmTreeMin = 10;
        config.PalmTreeMax = 20;
        config.GemTreeMin = 7;
        config.GemTreeMax = 12;
    }
}

public abstract class PresetElement : LargerPanelElement
{
    public override void LeftClick(UIMouseEvent evt) {
        base.LeftClick(evt);
        
        if (Item is not ImproveConfigs config) return;
        SetPreset(config);
        Terraria.ModLoader.UI.Interface.modConfig.SetPendingChanges();
    }

    protected abstract void SetPreset(ImproveConfigs config);
}