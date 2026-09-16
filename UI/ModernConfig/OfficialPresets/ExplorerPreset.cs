using ImproveGame.Common.Configs;
using ImproveGame.Common.GlobalNPCs;
using ImproveGame.Common.GlobalProjectiles;

namespace ImproveGame.UI.ModernConfig.OfficialPresets;

public class ExplorerPreset : OfficialPreset
{
    public override void ApplyPreset(ImproveConfigs modConfig, UIConfigs uiConfig,
        AvailableModItemConfigs modItemConfig)
    {
        modConfig.SuperVoidVault = false;
        modConfig.SmartVoidVault = false;
        modConfig.SuperVault = false;
        modConfig.GrabDistance = 5;
        modConfig.NoConsume_SummonItem = false;
        modConfig.ExtraToolSpeed = 0.125f;
        modConfig.ModifyPlayerPlaceSpeed = true;
        modConfig.PortableCraftingStation = true;
        modConfig.NoPlace_BUFFTile = true;
        modConfig.NoPlace_BUFFTile_Banner = false;
        modConfig.NoConsume_Potion = false;
        modConfig.NoConsume_Ammo = true;
        modConfig.ImprovePrefix = true;
        modConfig.MiddleEnableBank = true;
        modConfig.FasterExtractinator = true;
        modConfig.TownNPCHome = true;
        modConfig.TownNPCGetTFIntoHouse = false;
        modConfig.NPCLiveInEvil = true;
        modConfig.TownNPCSpawnSpeed = 1;
        modConfig.NoCD_FishermanQuest = FishQuestResetType.NotResetFish;
        modConfig.NPCCoinDropRate = 1;
        modConfig.ModifyNPCHappiness = false;
        modConfig.NPCHappiness = 75;
        modConfig.SlimeExDrop = false;
        modConfig.LavalessLavaSlime = true;
        modConfig.TravellingMerchantStay = false;
        modConfig.TravellingMerchantRefresh = true;
        modConfig.BestiaryQuickUnlock = false;
        modConfig.AlchemyGrassGrowsFaster = false;
        modConfig.AlchemyGrassAlwaysBlooms = false;
        modConfig.PumpkinGrowsFaster = false;
        modConfig.LifeFruitGrowsFaster = false;
        modConfig.LifeFruitLimit = 30;
        // modConfig.StaffOfRegenerationAutomaticPlanting = true;
        modConfig.NoBiomeSpread = false;
        modConfig.RespawnWithFullHP = true;
        modConfig.DontDeleteBuff = true;
        modConfig.TreeGrowFaster = false;
        modConfig.ShakeTreeFruit = false;
        modConfig.GemTreeAlwaysDropGem = false;
        modConfig.NoLakeSizePenalty = true;
        modConfig.MostTreeMin = 10;
        modConfig.MostTreeMax = 20;
        modConfig.PalmTreeMin = 10;
        modConfig.PalmTreeMax = 20;
        modConfig.GemTreeMin = 7;
        modConfig.GemTreeMax = 12;
        modConfig.BedTimeRate = 10;
        modConfig.BedEverywhere = true;
        modConfig.NoSleepRestrictions = true;
        modConfig.WandMaterialNoConsume = false;
        modConfig.MinimapMark = true;
        modConfig.BedOnlyOne = false;
        modConfig.QuestFishStack = true;
        modConfig.BombsNotDamage = BombsNotDamageType.Item;
        modConfig.NoConditionTP = false;
        modConfig.WeatherControl = false;
        modConfig.WorldFeaturePanel = false;
        modConfig.DisableNonPlayerBombsExplosions = DisableNonPlayerBombsExplosionsType.FTW;

        uiConfig.ShowShimmerInfo = true;
        uiConfig.QoLAutoTrash = true;
        uiConfig.AutoSummon = true;
        uiConfig.PlyInfo = UIConfigs.PAPDisplayMode.AlwaysDisplayed;
    }
}