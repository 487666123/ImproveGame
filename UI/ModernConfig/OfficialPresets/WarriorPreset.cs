using ImproveGame.Common.Configs;
using ImproveGame.Common.GlobalNPCs;
using ImproveGame.Common.GlobalProjectiles;

namespace ImproveGame.UI.ModernConfig.OfficialPresets;

public class WarriorPreset : OfficialPreset
{
    public override void ApplyPreset(ImproveConfigs modConfig, UIConfigs uiConfig,
        AvailableModItemConfigs modItemConfig)
    {
        modConfig.SuperVoidVault = true;
        modConfig.SmartVoidVault = true;
        modConfig.SuperVault = true;
        modConfig.GrabDistance = 5;
        modConfig.NoConsume_SummonItem = true;
        modConfig.ExtraToolSpeed = 0.5f;
        modConfig.ModifyPlayerPlaceSpeed = true;
        modConfig.PortableCraftingStation = true;
        modConfig.NoPlace_BUFFTile = true;
        modConfig.NoPlace_BUFFTile_Banner = true;
        modConfig.NoConsume_Potion = true;
        modConfig.NoConsume_Ammo = true;
        modConfig.ImprovePrefix = true;
        modConfig.MiddleEnableBank = true;
        modConfig.FasterExtractinator = true;
        modConfig.TownNPCHome = true;
        modConfig.TownNPCGetTFIntoHouse = true;
        modConfig.NPCLiveInEvil = true;
        modConfig.TownNPCSpawnSpeed = 12;
        modConfig.NoCD_FishermanQuest = FishQuestResetType.NotResetFish;
        modConfig.NPCCoinDropRate = 8;
        modConfig.ModifyNPCHappiness = true;
        modConfig.NPCHappiness = 75;
        modConfig.SlimeExDrop = true;
        modConfig.LavalessLavaSlime = true;
        modConfig.TravellingMerchantStay = true;
        modConfig.TravellingMerchantRefresh = true;
        modConfig.BestiaryQuickUnlock = true;
        modConfig.AlchemyGrassGrowsFaster = true;
        modConfig.AlchemyGrassAlwaysBlooms = true;
        modConfig.PumpkinGrowsFaster = true;
        modConfig.LifeFruitGrowsFaster = true;
        modConfig.LifeFruitLimit = 30;
        // modConfig.StaffOfRegenerationAutomaticPlanting = true;
        modConfig.NoBiomeSpread = true;
        modConfig.RespawnWithFullHP = true;
        modConfig.DontDeleteBuff = true;
        modConfig.TreeGrowFaster = true;
        modConfig.ShakeTreeFruit = true;
        modConfig.GemTreeAlwaysDropGem = true;
        modConfig.NoLakeSizePenalty = true;
        modConfig.MostTreeMin = 22;
        modConfig.MostTreeMax = 36;
        modConfig.PalmTreeMin = 22;
        modConfig.PalmTreeMax = 36;
        modConfig.GemTreeMin = 20;
        modConfig.GemTreeMax = 30;
        modConfig.BedTimeRate = 80;
        modConfig.BedEverywhere = true;
        modConfig.NoSleepRestrictions = true;
        modConfig.WandMaterialNoConsume = true;
        modConfig.MinimapMark = true;
        modConfig.BedOnlyOne = true;
        modConfig.QuestFishStack = true;
        modConfig.BombsNotDamage = BombsNotDamageType.Item;
        modConfig.NoConditionTP = true;
        modConfig.WeatherControl = true;
        modConfig.WorldFeaturePanel = true;
        modConfig.DisableNonPlayerBombsExplosions = DisableNonPlayerBombsExplosionsType.FTW;

        uiConfig.ShowShimmerInfo = true;
        uiConfig.QoLAutoTrash = true;
        uiConfig.AutoSummon = true;
        uiConfig.PlyInfo = UIConfigs.PAPDisplayMode.AlwaysDisplayed;
    }
}