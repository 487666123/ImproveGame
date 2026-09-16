using ImproveGame.Common.GlobalNPCs;
using ImproveGame.Common.GlobalProjectiles;

namespace ImproveGame.Common.Configs.Elements;

public class FukMeCalamityElement : PresetElement
{
    protected override void SetPreset(ImproveConfigs config)
    {
        config.SuperVoidVault = true;
        config.SmartVoidVault = true;
        config.SuperVault = true;
        config.ItemMaxStack = 99999;
        config.GrabDistance = 5;
        config.NoConsume_SummonItem = true;
        config.ExtraToolSpeed = 0.5f;
        config.ModifyPlayerPlaceSpeed = true;
        config.PortableCraftingStation = true;
        config.NoPlace_BUFFTile = true;
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
        config.NoCD_FishermanQuest = FishQuestResetType.NotResetFish;
        config.NPCCoinDropRate = 8;
        config.ModifyNPCHappiness = true;
        config.NPCHappiness = 75;
        config.SlimeExDrop = true;
        config.LavalessLavaSlime = true;
        config.TravellingMerchantStay = true;
        config.TravellingMerchantRefresh = true;
        config.BestiaryQuickUnlock = true;
        config.AlchemyGrassGrowsFaster = true;
        config.AlchemyGrassAlwaysBlooms = true;
        config.PumpkinGrowsFaster = true;
        config.LifeFruitGrowsFaster = true;
        config.LifeFruitLimit = 30;
        // config.StaffOfRegenerationAutomaticPlanting = true;
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
        config.BedTimeRate = 80;
        config.BedEverywhere = true;
        config.NoSleepRestrictions = true;
        config.WandMaterialNoConsume = true;
        config.MinimapMark = true;
        config.BedOnlyOne = true;
        config.QuestFishStack = true;
        config.BombsNotDamage = BombsNotDamageType.Item;
        config.NoConditionTP = true;
        config.DisableNonPlayerBombsExplosions = DisableNonPlayerBombsExplosionsType.FTW;
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
        config.NoPlace_BUFFTile = true;
        config.NoPlace_BUFFTile_Banner = false;
        config.NoConsume_Potion = false;
        config.NoConsume_Ammo = true;
        config.ImprovePrefix = true;
        config.MiddleEnableBank = true;
        config.FasterExtractinator = true;
        config.TownNPCHome = true;
        config.TownNPCGetTFIntoHouse = false;
        config.NPCLiveInEvil = true;
        config.TownNPCSpawnSpeed = 1;
        config.NoCD_FishermanQuest = FishQuestResetType.NotResetFish;
        config.NPCCoinDropRate = 1;
        config.ModifyNPCHappiness = false;
        config.NPCHappiness = 75;
        config.SlimeExDrop = false;
        config.LavalessLavaSlime = true;
        config.TravellingMerchantStay = false;
        config.TravellingMerchantRefresh = true;
        config.BestiaryQuickUnlock = false;
        config.AlchemyGrassGrowsFaster = false;
        config.AlchemyGrassAlwaysBlooms = false;
        config.PumpkinGrowsFaster = false;
        config.LifeFruitGrowsFaster = false;
        config.LifeFruitLimit = 30;
        // config.StaffOfRegenerationAutomaticPlanting = true;
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
        config.BedTimeRate = 10;
        config.BedEverywhere = true;
        config.NoSleepRestrictions = true;
        config.WandMaterialNoConsume = false;
        config.MinimapMark = true;
        config.BedOnlyOne = false;
        config.QuestFishStack = true;
        config.BombsNotDamage = BombsNotDamageType.Item;
        config.NoConditionTP = false;
        config.DisableNonPlayerBombsExplosions = DisableNonPlayerBombsExplosionsType.FTW;
    }
}

public class AllOffElement : PresetElement
{
    protected override void SetPreset(ImproveConfigs config)
    {
        config.SuperVoidVault = false;
        config.SmartVoidVault = false;
        config.SuperVault = false;
        config.ItemMaxStack = 9999;
        config.GrabDistance = 0;
        config.NoConsume_SummonItem = false;
        config.ExtraToolSpeed = 0f;
        config.ModifyPlayerPlaceSpeed = false;
        config.ModifyPlayerTileRange = 0;
        config.PortableCraftingStation = false;
        config.NoPlace_BUFFTile = false;
        config.NoPlace_BUFFTile_Banner = false;
        config.NoConsume_Potion = false;
        config.NoConsume_Ammo = false;
        config.NoConsume_Wire = false;
        config.ImprovePrefix = false;
        config.MiddleEnableBank = false;
        config.FasterExtractinator = false;
        config.TownNPCHome = false;
        config.TownNPCGetTFIntoHouse = false;
        config.NPCLiveInEvil = false;
        config.TownNPCSpawnSpeed = 1;
        config.NoCD_FishermanQuest = 0;
        config.NPCCoinDropRate = 1;
        config.ModifyNPCHappiness = false;
        config.NPCHappiness = 100;
        config.SlimeExDrop = false;
        config.LavalessLavaSlime = false;
        config.TravellingMerchantStay = false;
        config.TravellingMerchantRefresh = false;
        config.BestiaryQuickUnlock = false;
        config.AlchemyGrassGrowsFaster = false;
        config.AlchemyGrassAlwaysBlooms = false;
        config.PumpkinGrowsFaster = false;
        config.LifeFruitGrowsFaster = false;
        config.LifeFruitLimit = -1;
        // config.StaffOfRegenerationAutomaticPlanting = false;
        config.NoBiomeSpread = false;
        config.RespawnWithFullHP = false;
        config.DontDeleteBuff = false;
        config.ExtraPlayerBuffSlots = 0;
        config.TreeGrowFaster = false;
        config.ShakeTreeFruit = false;
        config.GemTreeAlwaysDropGem = false;
        config.NoLakeSizePenalty = false;
        config.MostTreeMin = 10;
        config.MostTreeMax = 20;
        config.PalmTreeMin = 10;
        config.PalmTreeMax = 20;
        config.GemTreeMin = 7;
        config.GemTreeMax = 12;
        config.BedTimeRate = 10;
        config.BedEverywhere = false;
        config.NoSleepRestrictions = false;
        config.BedTimeRate = 5;
        config.WandMaterialNoConsume = false;
        config.MinimapMark = false;
        config.BedOnlyOne = false;
        config.QuestFishStack = false;
        config.RedPotionEverywhere = false;
        config.InfiniteRedPotion = false;
        config.QuickNurse = false;
        config.BOSSBattleResurrectionTimeShortened = 0;
        config.BanTombstone = false;
        config.LongerExpertDebuff = true;
        config.LightNotBlocked = false;
        config.TeamAutoJoin = false;
        config.BombsNotDamage = BombsNotDamageType.Disabled;
        config.NoConditionTP = false;
        config.DisableNonPlayerBombsExplosions = DisableNonPlayerBombsExplosionsType.Disabled;
    }
}

public abstract class PresetElement : LargerPanelElement
{
    public override void LeftClick(UIMouseEvent evt)
    {
        base.LeftClick(evt);

        if (Item is not ImproveConfigs config) return;
        SetPreset(config);
        Terraria.ModLoader.UI.Interface.modConfig.SetPendingChanges();
    }

    protected abstract void SetPreset(ImproveConfigs config);
}