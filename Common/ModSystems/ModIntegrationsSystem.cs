using ImproveGame.Common.Configs;
using ImproveGame.Common.ModPlayers;
using ImproveGame.Content.Functions;
using ImproveGame.Content.Functions.ChainedAmmo;
using ImproveGame.Content.Functions.HomeTeleporting;
using ImproveGame.Content.Items;
using ImproveGame.Content.Items.Coin;
using ImproveGame.Content.Items.Globes;
using ImproveGame.Content.Items.IconDummies;
using ImproveGame.Content.Items.ItemContainer;
using ImproveGame.Content.Items.Placeable;
using ImproveGame.Packets;
using ImproveGame.Packets.NetAutofisher;
using ImproveGame.UI.ModernConfig;
using ImproveGame.UI.PlayerStats;
using System.Collections;
using System.Reflection;
using Terraria.DataStructures;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;
using Terraria.ModLoader.Default;
using TEAutofisher = ImproveGame.Content.Tiles.TEAutofisher;

namespace ImproveGame.Common.ModSystems;

public record FishingStat(int Power = 0, float SpeedMultiplier = 1f, bool TackleBox = false, bool LavaFishing = false);

/// <summary>
/// 处理几乎所有跨模组的集成
/// </summary>
public class ModIntegrationsSystem : ModSystem
{
    /// <summary>
    /// 储存独立添加支持的模组放置类Buff（也就是只需要1堆叠即可生效），由于这个是加载后执行的，直接存ID即可
    /// <br>Key为物品ID，Value为BuffID</br>
    /// </summary>
    internal static Dictionary<int, List<int>> ModdedPlaceableItemBuffs = new();

    /// <summary>
    /// 储存独立添加支持的模组药水类Buff（也就是需要30堆叠生效），由于这个是加载后执行的，直接存ID即可
    /// <br>Key为物品ID，Value为BuffID</br>
    /// </summary>
    internal static Dictionary<int, List<int>> ModdedPotionBuffs = new();

    /// <summary>
    /// A list of the items that don't apply infinite buffs
    /// </summary>
    internal static HashSet<int> ModdedInfBuffsIgnore = new();

    /// <summary>
    /// 储存即使在触发无限增益的情况下，也会正常消耗的物品列表
    /// </summary>
    internal static HashSet<int> ModdedInfBuffsConsume = new();

    /// <summary>
    /// 储存冲突Buff（当玩家拥有某个增益时，一个/些增益会被清除）列表
    /// <br>Key为BuffID，Value为会被清除的BuffIDs</br>
    /// </summary>
    internal static Dictionary<int, List<int>> ModdedBuffConflicts = new()
    {
        // 高等级的饱腹会覆盖低等级的
        {BuffID.WellFed3, [BuffID.WellFed2, BuffID.WellFed] },
        {BuffID.WellFed2, [BuffID.WellFed] }
    };

    /// <summary>
    /// 添加物品ID对应的一系列Tile
    /// <br>Key为物品ID，Value为一个TileID的列表</br>
    /// </summary>
    internal static Dictionary<int, List<int>> PortableStations = new();

    /// <summary>
    /// 添加物品ID对应的自动钓鱼属性
    /// <br>Key代表物品ID，Value是物品属性</br>
    /// </summary>
    internal static Dictionary<int, FishingStat> FishingStatLookup = new()
    {
        // 配饰为AnglerEarring可使钓鱼速度*200%
        // 配饰为AnglerTackleBag可使钓鱼速度*250%
        // 配饰为LavaproofTackleBag可使钓鱼速度*300%
        { ItemID.TackleBox, new FishingStat(TackleBox: true) }, // 钓具箱
        { ItemID.AnglerEarring, new FishingStat(10, 2f) }, // 渔夫耳环
        { ItemID.AnglerTackleBag, new FishingStat(10, 2.5f, TackleBox: true) }, // 渔夫渔具袋
        { ItemID.LavaFishingHook, new FishingStat(LavaFishing: true) }, // 防熔岩钓钩
        { ItemID.LavaproofTackleBag, new FishingStat(10, 3f, true, true) }, // 防熔岩渔具袋
        { ItemID.None, new FishingStat()}
    };

    internal static bool NoLakeSizePenaltyLoaded = false;
    internal static bool WMITFLoaded = false;
    internal static bool DialogueTweakLoaded = false;
    internal static bool LuiafkRebornLoaded = false;
    internal static int LuiafkTravellingMerchantType = NPCID.None;

    internal static int UnloadedItemType;

    public override void PostSetupContent()
    {
        DoImproveGameModIntegration(); // 诶！更好的体验也是模组，也算处理跨模组集成！
        DoCalamityModIntegration();
        DoThoriumModIntegration();
        DoFargowiltasIntegration();
        DoGensokyoIntegration();
        DoRecipeBrowserIntegration();
        DoLuiafkRebornIntegration();
        DoDialogueTweakIntegration();
        DoModLoaderIntegration();
        DoShopLookupIntegration();
        DoMechTransferIntegration();
        NoLakeSizePenaltyLoaded = ModLoader.HasMod("NoLakeSizePenalty");
        WMITFLoaded = ModLoader.HasMod("WMITF");
    }

    static void NotFindContentWarn(string methodName, string modName, string name)
        => ImproveGame.Instance.Logger.Warn($"{methodName} : Not found {name} in {modName}, the Integration might not take effect.");

    static bool TryFindContent<T>(Mod mod, string name, out T value, string methodName/*, bool warn = true*/) where T : IModType
    {
        bool flag = mod.TryFind(name, out value);
        if (!flag /*&& warn*/)
            NotFindContentWarn(methodName, $"{mod.Name} {mod.Version}", name);
        return flag;
    }

    private static void DoRecipeBrowserIntegration()
    {
        if (!ModLoader.TryGetMod("RecipeBrowser", out Mod recipeBrowser))
            return;

        // 给合成表的计量材料数量功能添加储存管理器、大背包、药水袋和旗帜盒支持
        var providers = recipeBrowser.Code.GetTypes()
            .Where(t => !t.IsAbstract && !t.ContainsGenericParameters)
            .Where(t => t.IsAssignableTo(typeof(UIElement)));

        var detourMethod = providers.First(i => i.Name is "UITrackIngredientSlot")
            .GetMethod("CountItemGroups", BindingFlags.Public | BindingFlags.Instance);
        var itemField = providers.First(i => i.Name is "UIItemSlot")
            .GetField("item", BindingFlags.Public | BindingFlags.Instance);

        if (detourMethod is null || itemField is null)
        {
            return;
        }

        MonoModHooks.Add(detourMethod, (Func<object, Player, Recipe, int, int, int> orig,
            object ingredientSlot, Player player, Recipe recipe, int type, int stopCountingAt) =>
        {
            int count = orig(ingredientSlot, player, recipe, type, stopCountingAt);
            var item = itemField.GetValue(ingredientSlot) as Item;

            if (type == 0 || item is null)
            {
                return count;
            }

            var items = new List<Item>();
            if (player.TryGetModPlayer<DataPlayer>(out var dataPlayer))
                items.AddRange(dataPlayer.AddMaterialsForCrafting(out _) ?? new List<Item>());
            if (player.TryGetModPlayer<StorageMaterialConsumer>(out var storagePlayer))
                items.AddRange(storagePlayer.AddMaterialsForCrafting(out _) ?? new List<Item>());

            foreach (var currentItem in from i in items where !i.IsAir select i)
            {
                if (recipe.AcceptedByItemGroups(currentItem.type, item.type))
                {
                    count += currentItem.stack;
                }
                else if (currentItem.type == type)
                {
                    count += currentItem.stack;
                }
            }

            return count >= stopCountingAt ? stopCountingAt : count;
        });
    }

    private static void DoImproveGameModIntegration()
    {
        FishingStatLookup[ModContent.ItemType<DevMark>()] = new FishingStat(0, 100, false, false);
    }

    private static void DoCalamityModIntegration()
    {
        if (!ModLoader.TryGetMod("CalamityMod", out Mod calamityMod))
            return;

        if (calamityMod.Version is { Minor: 0, Build: <= 4 })
        {
            AddBuffIntegration(calamityMod, "WeightlessCandle", true, "CirrusBlueCandleBuff");
            AddBuffIntegration(calamityMod, "VigorousCandle", true, "CirrusPinkCandleBuff");
            AddBuffIntegration(calamityMod, "SpitefulCandle", true, "CirrusYellowCandleBuff");
            AddBuffIntegration(calamityMod, "ResilientCandle", true, "CirrusPurpleCandleBuff");

            AddBuffConflicts(calamityMod, "CirrusBlueCandleBuff", "CirrusPinkCandleBuff", "CirrusYellowCandleBuff", "CirrusPurpleCandleBuff");
            AddBuffConflicts(calamityMod, "CirrusPinkCandleBuff", "CirrusYellowCandleBuff", "CirrusPurpleCandleBuff", "CirrusBlueCandleBuff");
            AddBuffConflicts(calamityMod, "CirrusYellowCandleBuff", "CirrusPurpleCandleBuff", "CirrusBlueCandleBuff", "CirrusPinkCandleBuff");
            AddBuffConflicts(calamityMod, "CirrusPurpleCandleBuff", "CirrusBlueCandleBuff", "CirrusPinkCandleBuff", "CirrusYellowCandleBuff");
        }
        else
        {
            AddBuffIntegration(calamityMod, "WeightlessCandle", true, "BlueCandleBuff");
            AddBuffIntegration(calamityMod, "VigorousCandle", true, "PinkCandleBuff");
            AddBuffIntegration(calamityMod, "SpitefulCandle", true, "YellowCandleBuff");
            AddBuffIntegration(calamityMod, "ResilientCandle", true, "PurpleCandleBuff");

            AddBuffConflicts(calamityMod, "BlueCandleBuff", "PinkCandleBuff", "YellowCandleBuff", "PurpleCandleBuff");
            AddBuffConflicts(calamityMod, "PinkCandleBuff", "YellowCandleBuff", "PurpleCandleBuff", "BlueCandleBuff");
            AddBuffConflicts(calamityMod, "YellowCandleBuff", "PurpleCandleBuff", "BlueCandleBuff", "PinkCandleBuff");
            AddBuffConflicts(calamityMod, "PurpleCandleBuff", "BlueCandleBuff", "PinkCandleBuff", "YellowCandleBuff");
        }

        AddBuffIntegration(calamityMod, "ChaosCandle", true, "ChaosCandleBuff");
        AddBuffIntegration(calamityMod, "TranquilityCandle", true, "TranquilityCandleBuff");
        AddBuffIntegration(calamityMod, "EffigyOfDecay", true, "EffigyOfDecayBuff");
        AddBuffIntegration(calamityMod, "CrimsonEffigy", true, "CrimsonEffigyBuff");
        AddBuffIntegration(calamityMod, "CorruptionEffigy", true, "CorruptionEffigyBuff");

        AddFishingAccIntegration(calamityMod, "EnchantedPearl", 2f, 10, false, false);
        AddFishingAccIntegration(calamityMod, "AlluringBait", 2f, 30, false, false);
        AddFishingAccIntegration(calamityMod, "SupremeBaitTackleBoxFishingStation", 5f, 80, true, true);
        PlayerStatsSystem.CalamityIntegration(calamityMod);
    }

    private static void DoThoriumModIntegration()
    {
        if (!ModLoader.TryGetMod("ThoriumMod", out Mod thoriumMod))
            return;

        AddBuffIntegration(thoriumMod, "Altar", true, "AltarBuff");
        AddBuffIntegration(thoriumMod, "ConductorsStand", true, "ConductorsStandBuff");
        AddBuffIntegration(thoriumMod, "Mistletoe", true, "MistletoeBuff");
        AddBuffIntegration(thoriumMod, "NinjaRack", true, "NinjaBuff");
        AddHomeTpIntegration(thoriumMod, "WishingGlass", false, false);
        PlayerStatsSystem.ThoriumIntegration(thoriumMod);
    }

    private static void DoFargowiltasIntegration()
    {
        if (!ModLoader.TryGetMod("Fargowiltas", out Mod fargowiltas))
            return;

        AddBuffIntegration(fargowiltas, "Omnistation", true, "Omnistation");
        AddBuffIntegration(fargowiltas, "Omnistation2", true, "Omnistation");

        if (!ModLoader.TryGetMod("FargowiltasSouls", out Mod fargowiltasSouls))
            return;

        AddFishingAccIntegration(fargowiltasSouls, "AnglerEnchant", 5f, 10, true, true);
        AddFishingAccIntegration(fargowiltasSouls, "TrawlerSoul", 5f, 60, true, true);
        AddFishingAccIntegration(fargowiltasSouls, "DimensionSoul", 5f, 60, true, true);
        AddFishingAccIntegration(fargowiltasSouls, "EternitySoul", 5f, 60, true, true);
    }

    private static void DoGensokyoIntegration()
    {
        if (!ModLoader.TryGetMod("Gensokyo", out Mod gensokyo))
            return;


        AddBuffIntegration(gensokyo, "ButterflyPheromones", true, "Buff_ButterflyPheromones");
        AddBuffIntegration(gensokyo, "OniSake", true, "Buff_SakeBoth");
        AddBuffIntegration(gensokyo, "HoshigumaDish", true, "Debuff_SakeHoshiguma");
        AddBuffIntegration(gensokyo, "IbarakiBox", true, "Buff_SakeIbaraki");
        AddBuffIntegration(gensokyo, "EagleRaviProvisions", false, "Buff_DangoPower1", "Buff_DangoPower2",
            "Buff_DangoPower3", "Buff_DangoPower4", "Buff_DangoPower5");
        AddInfBuffsConsume(gensokyo, "JellyStone");
        AddBuffConflicts(gensokyo, "Buff_SakeBoth", BuffID.Tipsy, "Buff_SakeIbaraki", "Debuff_SakeHoshiguma");
        AddBuffConflicts(gensokyo, "Buff_SakeIbaraki", BuffID.Tipsy, "Debuff_SakeHoshiguma");
        AddBuffConflicts(gensokyo, "Debuff_SakeHoshiguma", BuffID.Tipsy);
        AddBuffConflicts(gensokyo, "Buff_DangoPower5", "Buff_DangoPower4", "Buff_DangoPower3", "Buff_DangoPower2",
            "Buff_DangoPower1");
        AddBuffConflicts(gensokyo, "Buff_DangoPower4", "Buff_DangoPower3", "Buff_DangoPower2", "Buff_DangoPower1");
        AddBuffConflicts(gensokyo, "Buff_DangoPower3", "Buff_DangoPower2", "Buff_DangoPower1");
        AddBuffConflicts(gensokyo, "Buff_DangoPower2", "Buff_DangoPower1");
    }

    private static void DoDialogueTweakIntegration()
    {
        if (!ModLoader.TryGetMod("DialogueTweak", out Mod dialogueTweak))
            return;

        DialogueTweakLoaded = true;

        List<int> travellingMerchants = [NPCID.TravellingMerchant];
        if (LuiafkRebornLoaded)
            travellingMerchants.Add(LuiafkTravellingMerchantType);

        dialogueTweak.Call("AddButton",
            travellingMerchants, // NPC IDs
            () => RefreshTravelShopSystem.DisplayText, // 文本
            "DialogueTweak/Interfaces/Assets/Icon_Help", // 显示的icon
            ClickAction, // 点击操作
            () => ImproveConfigs.Instance.TravellingMerchantRefresh // 什么时候可用
            );


        static void ClickAction()
        {
            if (Main.mouseLeft && !RefreshTravelShopSystem.OldMouseLeft)
            {
                RefreshShopPacket.Get().Send(runLocally: true);
            }
        }
    }

    private static void DoModLoaderIntegration() => UnloadedItemType = ModContent.ItemType<UnloadedItem>();

    private static void DoMechTransferIntegration()
    {
        if (!ModLoader.TryGetMod("MechTransfer", out Mod mechTansfer))
        {
            return;
        }
        AutoFisherAdapter adapter = new AutoFisherAdapter();
        mechTansfer.Call("RegisterAdapterReflection", adapter, new int[] { ModContent.TileType<Content.Tiles.Autofisher>() });
    }

    private static void AddCraftStationIntegration(Mod mod, string itemName, List<int> tileIDs)
    {
        if (TryFindContent<ModItem>(mod, itemName, out var item, nameof(AddCraftStationIntegration)))
            PortableStations[item.Type] = tileIDs;
    }

    private static void AddBuffIntegration(Mod mod, string itemName, bool isPlaceable, params string[] buffNames)
    {
        List<int> buffs = [];
        foreach (string buffName in buffNames)
            if (TryFindContent<ModBuff>(mod, buffName, out var buff, nameof(AddBuffIntegration)))
                buffs.Add(buff.Type);

        if (!TryFindContent<ModItem>(mod, itemName, out var item, nameof(AddBuffIntegration))) return;
        if (isPlaceable)
            ModdedPlaceableItemBuffs[item.Type] = buffs;
        else
            ModdedPotionBuffs[item.Type] = buffs;
    }

    private static void AddInfBuffsConsume(Mod mod, string itemName)
    {
        if (TryFindContent<ModItem>(mod, itemName, out var item, nameof(AddInfBuffsConsume)))
            ModdedInfBuffsConsume.Add(item.Type);
    }

    /// <param name="BuffIDOrName">填int（原版）或string（Mod）</param>
    /// <param name="removeBuffIDOrName">填int（原版）或string（Mod）</param>
    public static void AddBuffConflicts(Mod mod, object BuffIDOrName, params object[] removeBuffIDOrName)
    {
        List<int> buffs = [];
        foreach (object obj in removeBuffIDOrName)
        {
            if (obj is int removeBuffID) buffs.Add(removeBuffID);
            if (obj is string removeBuffName && TryFindContent<ModBuff>(mod, removeBuffName, out var rbuff, nameof(AddBuffConflicts))) buffs.Add(rbuff.Type);
        }

        if (BuffIDOrName is int buffID) ModdedBuffConflicts[buffID] = buffs;
        if (BuffIDOrName is string buffName && TryFindContent<ModBuff>(mod, buffName, out var buff, nameof(AddBuffConflicts))) ModdedBuffConflicts[buff.Type] = buffs;
    }

    private static void AddHomeTpIntegration(Mod mod, string itemName, bool isPotion, bool isComebackItem)
    {
        // 属于是自我测试了
        if (TryFindContent<ModItem>(mod, itemName, out var item, nameof(AddHomeTpIntegration)))
            Call("AddHomeTpItem", item.Type, isPotion, isComebackItem);
    }

    private static void AddFishingAccIntegration(Mod mod, string itemName, float speed, int power, bool tackleBox,
        bool lavaFishing)
    {
        if (TryFindContent<ModItem>(mod, itemName, out var item, nameof(AddFishingAccIntegration)))
            FishingStatLookup[item.Type] = new FishingStat(power, speed, tackleBox, lavaFishing);
    }

    private static void DoShopLookupIntegration()
    {
        if (!ModLoader.TryGetMod("ShopLookup", out Mod mod))
            return;

        mod.Call(3, ImproveGame.Instance, "Wand", TextureAssets.Item[ModContent.ItemType<StarburstWand>()].Value,
            new NPCShop(-1, "Wand")
                .Add<CreateWand>(gold: 10)
                .Add<MagickWand>(gold: 10)
                .Add<PaintWand>(gold: 5)
                .Add<MoveChest>(gold: 15)
                .Add<WallPlace>(Item.buyPrice(gold: 8), Condition.DownedKingSlime)
                .Add<SpaceWand>(Item.buyPrice(gold: 12, silver: 50), Condition.DownedKingSlime)
                .Add<LiquidWand>(Item.buyPrice(gold: 20), Condition.DownedEowOrBoc)
                .Add<StarburstWand>(Item.buyPrice(gold: 50), Condition.Hardmode)
                .Add<ConstructWand>(Item.buyPrice(gold: 30), Condition.Hardmode));
        mod.Call(3, ImproveGame.Instance, "Locator", TextureAssets.Item[ModContent.ItemType<AetherGlobe>()].Value,
            new NPCShop(-1, "Locator")
                .Add<FloatingIslandGlobe>(gold: 8)
                .Add<PyramidGlobe>(gold: 16)
                .Add<AetherGlobe>(gold: 30)
                .Add<DungeonGlobe>(silver: 80)
                .Add<GraniteCaveGlobe>(gold: 1)
                .Add<MarbleCaveGlobe>(gold: 1)
                .Add<HiveGlobe>(gold: 1)
                .Add<EnchantedSwordGlobe>(gold: 8)
                .Add<PlanteraGlobe>(Item.buyPrice(gold: 16), Condition.Hardmode)
                .Add<TempleGlobe>(Item.buyPrice(gold: 20), Condition.DownedPlantera));
        mod.Call(3, ImproveGame.Instance, "Other", TextureAssets.Item[ModContent.ItemType<ExtremeStorage>()].Value,
            new NPCShop(-1, "Other")
                .Add<BannerChest>(silver: 50)
                .Add<ExtremeStorage>(gold: 12)
                .Add<Autofisher>(gold: 4)
                .Add<DetectorDrone>(silver: 20)
                .Add<StorageCommunicator>(Item.buyPrice(gold: 50), Condition.Hardmode)
                .Add<BaitSupplier>(gold: 30)
                .Add<PotionBag>(silver: 50)
                .Add<Dummy>(silver: 50)
                .Add<ShellShipInBottle>(gold: 5)
                .Add<WeatherBook>(Item.buyPrice(gold: 25, silver: 60), Condition.DownedEowOrBoc));
    }

    private static void DoLuiafkRebornIntegration()
    {
        if (!ModLoader.TryGetMod("miningcracks_take_on_luiafk", out Mod mod))
            return;

        LuiafkRebornLoaded = true;
        if (TryFindContent<ModNPC>(mod, "TravellingMerchant", out var npc, nameof(DoLuiafkRebornIntegration)))
            LuiafkTravellingMerchantType = npc.Type;
    }

    public override void Unload()
    {
        ModdedPlaceableItemBuffs = null;
        ModdedPotionBuffs = null;
        NoLakeSizePenaltyLoaded = false;
    }

    public static object Call(params object[] args)
    {
        try
        {
            if (args is null)
            {
                throw new ArgumentNullException(nameof(args), "Arguments cannot be null!");
            }

            if (args.Length == 0)
            {
                throw new ArgumentException("Arguments cannot be empty!");
            }

            if (args[0] is string msg)
            {
                switch (msg)
                {
                    case "IgnoreInfItem":
                    {
                        List<int> ignores = AsListOfInt(args[1]); // Potion IDs
                        foreach (int ignore in ignores)
                        {
                            ModdedInfBuffsIgnore.Add(ignore);
                        }

                        return true;
                    }
                    case "AddPotion":
                    {
                        int itemType = Convert.ToInt32(args[1]); // Item ID
                        List<int> buffTypes = AsListOfInt(args[2]); // Buff IDs
                        ModdedPotionBuffs[itemType] = buffTypes;
                        return true;
                    }
                    case "ConsumePotion":
                    {
                        List<int> consumes = AsListOfInt(args[1]); // Potion IDs
                        foreach (int consume in consumes)
                        {
                            ModdedInfBuffsConsume.Add(consume);
                        }

                        return true;
                    }
                    case "BuffConflict":
                    {
                        int buffType = Convert.ToInt32(args[1]); // Buff ID
                        List<int> ClearedBuffTypes = AsListOfInt(args[2]); // Cleared Buff IDs
                        ModdedBuffConflicts[buffType] = ClearedBuffTypes;
                        return true;
                    }
                    case "AddStation":
                    {
                        int itemType = Convert.ToInt32(args[1]); // Item ID
                        List<int> buffTypes = AsListOfInt(args[2]); // Buff IDs
                        ModdedPlaceableItemBuffs[itemType] = buffTypes;
                        return true;
                    }
                    case "AddPortableCraftingStation":
                    {
                        int itemType = Convert.ToInt32(args[1]); // Item ID
                        List<int> tileIDs = AsListOfInt(args[2]); // Tile IDs
                        PortableStations[itemType] = tileIDs;
                        return true;
                    }
                    case "AddFishingAccessory":
                    {
                        int itemType = Convert.ToInt32(args[1]); // Item ID
                        float speed = Convert.ToSingle(args[2]); // Fishing Speed Bonus
                        int power = Convert.ToInt32(args[3]); // Fishing Power
                        bool tackleBox = Convert.ToBoolean(args[4]); // Tackle Box
                        bool lavaFishing = Convert.ToBoolean(args[5]); // Lava Fishing
                        FishingStatLookup[itemType] = new FishingStat(power, speed, tackleBox, lavaFishing);
                        return true;
                    }
                    // 添加属性类别
                    case "AddStatCategory":
                    {
                        string category = Convert.ToString(args[1]);
                        Texture2D texture = (Texture2D)args[2];
                        string nameKey = Convert.ToString(args[3]);
                        Texture2D modIcon = (Texture2D)args[4];

                        if (PlayerStatsSystem.Instance.StatsCategories.ContainsKey(category))
                        {
                            return false;
                        }

                        PlayerStatsSystem.Instance.StatsCategories.Add(category,
                            new BaseStatsCategory(texture, nameKey, true, modIcon));

                        return true;
                    }
                    // 添加属性到指定类别
                    case "AddStat":
                    {
                        string category = Convert.ToString(args[1]);
                        string nameKey = Convert.ToString(args[2]);
                        Func<string> value = (Func<string>)args[3];

                        if (PlayerStatsSystem.Instance.StatsCategories.TryGetValue(category,
                                out BaseStatsCategory proCat))
                        {
                            proCat.BaseProperties.Add(new BaseStat(proCat, nameKey, value, true));
                            return true;
                        }

                        return false;
                    }
                    // 添加回家物品
                    case "AddHomeTpItem":
                    {
                        List<int> items = AsListOfInt(args[1]); // Item IDs
                        bool
                            isPotion = Convert
                                .ToBoolean(args[
                                    2]); // Whether the item should meet the Infinite Potion requirement (stacked over 30 by default and can be changed via mod config)
                        bool isComebackItem = Convert.ToBoolean(args[3]); // Potion of Return like item

                        foreach (int item in items)
                        {
                            HomeTeleportingPlayer.HomeTeleportingItems.Add(
                                new HomeTeleportingItem(item, isPotion, isComebackItem));
                        }

                        return false;
                    }
                    // 获取弹药链序列
                    case "GetAmmoChainSequence":
                    {
                        Item item = (Item)args[1];
                        if (item is null || item.IsAir ||
                            !item.TryGetGlobalItem<AmmoChainGlobalItem>(out var globalItem) ||
                            globalItem.Chain is null)
                            return null;
                        return globalItem.Chain.SerializeData();
                    }
                    // 获取大背包物品
                    case "GetBigBagItems":
                    {
                        Player player = (Player)args[1];
                        return GetAllInventoryItemsList(player, "portable, inv", 110);
                    }
                    // 获取“任意弹药”物品的ID
                    case "GetUniversalAmmoId":
                    {
                        return ModContent.ItemType<UniversalAmmoIcon>();
                    }
                    // 获取钓鱼机物品
                    case "GetFisherItems":
                    {
                        // Item[] Call(string, Point16)
                        // or Item[] Call(string, AutoFisher)
                        // or Item[] Call(string, int, int);
                        Point16 location = default;
                        TEAutofisher fisher = null;
                        bool ok = false;
                        int argIndex = 1;
                        object arg = args[argIndex++];
                        if (arg is TEAutofisher autofisher)
                        {
                            fisher = autofisher;
                            ok = TileEntity.ByID[autofisher.ID] == autofisher;
                        }
                        else
                        {
                            if (arg is Point16 point)
                            {
                                location = point;
                                ok = true;
                            }
                            else if (arg is int x && args[argIndex] is int y)
                            {
                                location = new Point16(x, y);
                                ok = true;
                            }
                            if (ok)
                            {
                                ok = AutoFisherAdapter.TryGetTEAutofisher(location.X, location.Y, out fisher);
                            }
                        }
                        if (ok)
                        {
                            Item[] items = [.. fisher.fish, fisher.fishingPole, fisher.bait, fisher.accessory];
                            return items;
                        }
                        return Array.Empty<Item>();
                    }
                    // 同步钓鱼机物品
                    case "SyncFisherItems":
                    {
                        // bool Call(string, Point16, int, int)
                        // or bool Item[] Call(string, AutoFisher, int, int)
                        // or bool Call(string, int, int, int, int)
                        Point16 location = default;
                        bool ok = false;
                        int argIndex = 1;
                        object arg = args[argIndex++];
                        TEAutofisher fisher = null;
                        if (arg is TEAutofisher autofisher)
                        {
                            fisher = autofisher;
                            ok = TileEntity.ByID[autofisher.ID] == autofisher;
                        }
                        else
                        {
                            if (arg is Point16 point)
                            {
                                location = point;
                                ok = true;
                            }
                            else if (arg is int x && args[argIndex] is int y)
                            {
                                location = new Point16(x, y);
                                ok = true;
                            }
                            if (ok)
                            {
                                ok = AutoFisherAdapter.TryGetTEAutofisher(location.X, location.Y, out fisher);
                            }
                        }
                        if (ok)
                        {
                            int index = (int)args[argIndex++];
                            if (index is > 0 and < ItemSyncPacket.All)
                            {
                                int amount = (int)args[argIndex];
                                if (Main.netMode == NetmodeID.MultiplayerClient)
                                {
                                    // TODO: Send packet when in multiplayer client
                                    return false;
                                }
                                AutoFisherAdapter.SyncItem(fisher, (byte)index, amount);
                                return true;
                            }
                        }
                        return false;
                    }
                    // 注册分类卡
                    case "RegisterCategory":
                    {
                        int length = args.Length;
                        CategorySidePanel.RegisterCategory(args[1] as Mod, args[2] as List<KeyValuePair<string, Terraria.ModLoader.Config.ModConfig>>,
                            length > 3 ? (int)args[3] : 0,
                            length > 4 ? args[4] as Func<Texture2D> : null,
                            length > 5 ? args[5] as Func<string> : null,
                            length > 6 ? args[6] as Func<string> : null);
                        return true;
                    }
                    // 注册“关于”页面
                    case "SetAboutPage":
                    {
                        int length = args.Length;
                        CategorySidePanel.SetAboutPage(args[1] as Mod, args[2] as Func<string>,
                            length > 3 ? (int)args[3] : 0,
                            length > 4 ? args[4] as Func<Texture2D> : null,
                            length > 5 ? args[5] as Func<string> : null,
                            length > 6 ? args[6] as Func<string> : null);
                        return true;
                    }
                    // 移除模组注册的所有分类卡
                    case "RemoveCategory":
                    {
                        CategorySidePanel.RemoveCategory(args[1] as Mod);
                        return true;
                    }
                    // 移除模组注册的“关于”页面
                    case "RemoveAboutPage":
                    {
                        CategorySidePanel.RemoveAboutPage(args[1] as Mod);
                        return true;
                    }
                    // 设置模组的配置中心在模组设置入口处的文本标题
                    case "AddModernConfigTitle":
                    {
                        CategorySidePanel.ModdedTitle[args[1] as Mod] = args[2] as LocalizedText;
                        return true;
                    }
                    // 注册预览绘制
                    case "RegisterPreview":
                    {
                        CategorySidePanel.ModdedPreviews[args[1] as PropertyFieldWrapper] = new PreviewDrawing(args[2] as Action<UIElement, ModConfig, PropertyFieldWrapper, object, IList, int>);
                        return true;
                    }
                    // 添加全局预览绘制
                    case "OnGlobalConfigPreview":
                    {
                        TooltipPanel.GlobalDrawing += new PreviewDrawing(args[1] as Action<UIElement, ModConfig, PropertyFieldWrapper, object, IList, int>);
                        return true;
                    }
                    // 这个本来是LSL用来注册在UI层开启Render的条件的
                    // 但是仔细研究之后发现LSL那一套和现在的主页毛玻璃插入的内容可以兼容，就没必要搬运过来了
                    //case "AddRenderOnCondition": 
                    //    {
                    //        ModernConfigDetours.RenderOnConditions.Add(args[1] as Func<bool>);
                    //        break;
                    //    }
                    // 添加新的监狱
                    case "AddPrison":
                    {
                        CreateWand.AddNewPrisonStyle(args[1] as Texture2D, args[2] as Texture2D);
                        return true;
                    }
                    // 注册钓鱼事件
                    case "RegisterFishingEvent":
                    {
                        if (args[1] is Delegate del &&
                            Delegate.CreateDelegate(typeof(Content.Tiles.FishingEventHandler), del.Target, del.Method, false) is Content.Tiles.FishingEventHandler eventHandler)
                        {
                            TEAutofisher.FishingEventHandler += eventHandler;
                            return true;
                        }
                        return false;
                    }
                    // 移除钓鱼事件
                    case "UnregisterFishingEvent":
                    {
                        if (args[1] is Delegate del &&
                            Delegate.CreateDelegate(typeof(Content.Tiles.FishingEventHandler), del.Target, del.Method, false) is Content.Tiles.FishingEventHandler eventHandler)
                        {
                            // CLR 判断是委托是否相同是通过 Target 与 Method
                            // 所以注册的委托的 Target 与 Method 相同即可正常卸载

                            TEAutofisher.FishingEventHandler -= eventHandler;
                            return true;
                        }
                        return false;
                    }
                    // 注册一个气候控制项
                    case "RegisterWeatherControl":
                        return Content.Functions.WeatherControl.WeatherControlCrossMod.RegisterControl(args);
                    // 移除一个气候控制项
                    case "UnregisterWeatherControl":
                        return Content.Functions.WeatherControl.WeatherControlCrossMod.UnregisterControl(args);
                    // 注册一个自定义内容栏
                    case "RegisterWeatherControlGroup":
                        return Content.Functions.WeatherControl.WeatherControlCrossMod.RegisterGroup(args);
                    // 移除一个自定义内容栏
                    case "UnregisterWeatherControlGroup":
                        return Content.Functions.WeatherControl.WeatherControlCrossMod.UnregisterGroup(args);
                    // 查询某控制项的当前档位，未知返回 -1
                    case "QueryWeatherControlStage":
                        return Content.Functions.WeatherControl.WeatherControlCrossMod.QueryStage(args[1] as string);
                    // 派发档位变更
                    case "SetWeatherControlStage":
                        return Content.Functions.WeatherControl.WeatherControlCrossMod.DispatchStage(args[1] as string, Convert.ToInt32(args[2]));
                    // 查询某控制项是否锁定
                    case "IsWeatherControlLocked":
                        return Content.Functions.WeatherControl.WeatherControlCrossMod.QueryLocked(args[1] as string);
                    // 派发锁定状态变更
                    case "SetWeatherControlLocked":
                        return Content.Functions.WeatherControl.WeatherControlCrossMod.DispatchLocked(args[1] as string, Convert.ToBoolean(args[2]));
                    default:
                        ImproveGame.Instance.Logger.Error($"Replacement type \"{msg}\" not found.");
                        return false;
                }
            }
        }
        catch (Exception e)
        {
            ImproveGame.Instance.Logger.Error($"{e.StackTrace} {e.Message}");
        }

        static List<int> AsListOfInt(object data) =>
            data as List<int> ?? new List<int> { Convert.ToInt32(data) };

        return false;
    }
}