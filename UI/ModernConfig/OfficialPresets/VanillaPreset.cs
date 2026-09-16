using ImproveGame.Common.Configs;
using ImproveGame.Common.GlobalProjectiles;

namespace ImproveGame.UI.ModernConfig.OfficialPresets;

public class VanillaPreset : OfficialPreset
{
    public override void ApplyPreset(ImproveConfigs modConfig, UIConfigs uiConfig,
        AvailableModItemConfigs modItemConfig)
    {
        modConfig.SuperVoidVault = false;
        modConfig.SmartVoidVault = false;
        modConfig.SuperVault = false;
        modConfig.GrabDistance = 0;
        modConfig.NoConsume_SummonItem = false;
        modConfig.ExtraToolSpeed = 0f;
        modConfig.ModifyPlayerPlaceSpeed = false;
        modConfig.ModifyPlayerTileRange = 0;
        modConfig.PortableCraftingStation = false;
        modConfig.NoPlace_BUFFTile = false;
        modConfig.NoPlace_BUFFTile_Banner = false;
        modConfig.NoConsume_Potion = false;
        modConfig.NoConsume_Ammo = false;
        modConfig.NoConsume_Wire = false;
        modConfig.ImprovePrefix = false;
        modConfig.MiddleEnableBank = false;
        modConfig.FasterExtractinator = false;
        modConfig.TownNPCHome = false;
        modConfig.TownNPCGetTFIntoHouse = false;
        modConfig.NPCLiveInEvil = false;
        modConfig.TownNPCSpawnSpeed = 1;
        modConfig.NoCD_FishermanQuest = 0;
        modConfig.NPCCoinDropRate = 1;
        modConfig.ModifyNPCHappiness = false;
        modConfig.NPCHappiness = 100;
        modConfig.SlimeExDrop = false;
        modConfig.LavalessLavaSlime = false;
        modConfig.TravellingMerchantStay = false;
        modConfig.TravellingMerchantRefresh = false;
        modConfig.BestiaryQuickUnlock = false;
        modConfig.AlchemyGrassGrowsFaster = false;
        modConfig.AlchemyGrassAlwaysBlooms = false;
        modConfig.PumpkinGrowsFaster = false;
        modConfig.LifeFruitGrowsFaster = false;
        modConfig.LifeFruitLimit = -1;
        // modConfig.StaffOfRegenerationAutomaticPlanting = false;
        modConfig.NoBiomeSpread = false;
        modConfig.RespawnWithFullHP = false;
        modConfig.DontDeleteBuff = false;
        modConfig.TreeGrowFaster = false;
        modConfig.ShakeTreeFruit = false;
        modConfig.GemTreeAlwaysDropGem = false;
        modConfig.NoLakeSizePenalty = false;
        modConfig.MostTreeMin = 10;
        modConfig.MostTreeMax = 20;
        modConfig.PalmTreeMin = 10;
        modConfig.PalmTreeMax = 20;
        modConfig.GemTreeMin = 7;
        modConfig.GemTreeMax = 12;
        modConfig.BedTimeRate = 10;
        modConfig.BedEverywhere = false;
        modConfig.NoSleepRestrictions = false;
        modConfig.BedTimeRate = 5;
        modConfig.WandMaterialNoConsume = false;
        modConfig.MinimapMark = false;
        modConfig.BedOnlyOne = false;
        modConfig.QuestFishStack = false;
        modConfig.RedPotionEverywhere = false;
        modConfig.InfiniteRedPotion = false;
        modConfig.QuickNurse = false;
        modConfig.BOSSBattleResurrectionTimeShortened = 0;
        modConfig.BanTombstone = false;
        modConfig.LongerExpertDebuff = true;
        modConfig.LightNotBlocked = false;
        modConfig.TeamAutoJoin = false;
        modConfig.BombsNotDamage = BombsNotDamageType.Disabled;
        modConfig.NoConditionTP = false;
        modConfig.WeatherControl = false;
        modConfig.WorldFeaturePanel = false;
        modConfig.DisableNonPlayerBombsExplosions = DisableNonPlayerBombsExplosionsType.Disabled;
        modConfig.PylonPlaceNoRestriction = false;
        modConfig.PylonTeleNoBiome = false;
        modConfig.PylonTeleNoDanger = false;
        modConfig.PylonTeleNoNear = false;
        modConfig.PylonTeleNoNPC = false;

        uiConfig.ShowShimmerInfo = false;
        uiConfig.PlyInfo = UIConfigs.PAPDisplayMode.NotDisplayed;
        uiConfig.QoLAutoTrash = false;
        uiConfig.RecipeSearch = false;
        uiConfig.KeepFocus = false;
        uiConfig.InfernoTransparency = 1f;
        uiConfig.InvisibleTransparency = 0f;
        uiConfig.MagicMirrorInstantTp = false;
        uiConfig.AutoSummon = false;
        uiConfig.RemoveGraveyardMist = false;
        uiConfig.RemoveGraveyardMusic = false;
        uiConfig.RemoveGraveyardVisual = false;

        /*
        // 全部不可获取
        foreach (var fieldInfo in modItemConfig.GetType().GetFields()) {
            if (!fieldInfo.Name.StartsWith("Available") || fieldInfo.FieldType != typeof(bool))
                continue;
            
            fieldInfo.SetValue(modItemConfig, false);
        }*/
    }
}