using ImproveGame.Common.Configs;
using ImproveGame.Common.GlobalItems;
using ImproveGame.Common.ModSystems;
using ImproveGame.Components;
using ImproveGame.Content.Items.ItemContainer;
using ImproveGame.Content.Tiles;
using ImproveGame.Packets;
using ImproveGame.UI.ExtremeStorage;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;

namespace ImproveGame.Modules.InfiniteBuff;

/// <summary>
/// 无限 Buff 的玩家侧核心系统。
/// </summary>
/// <remarks>
/// 主流程分两段：
/// <br/>1) 每帧从已缓存物品应用 Buff；
/// <br/>2) 每隔 <see cref="SetupBuffListCooldownTime"/> 帧重建一次可用物品缓存。
/// </remarks>
public class InfiniteBuffModPlayer : ModPlayer
{
    public bool[] ActivationFlags => _activationFlags;
    private bool[] _activationFlags = new bool[BuffLoader.BuffCount];

    private readonly TickTimer _rebuild;
    public InfiniteBuffModPlayer() => _rebuild = new TickTimer(1d, RecollectBuffItems);

    /// <summary>
    /// 判断当前 Buff 标记表是否满足激活组合。
    /// </summary>
    /// <param name="types">按 Buff ID 索引的布尔标记表。</param>
    /// <returns>组合内所有 Buff 都存在时返回 <see langword="true"/>。</returns>
    public bool MeetsBattlerCombination()
    {
        var flags = _activationFlags.AsSpan();

        foreach (var type in InfiniteBuffSets.BattlerCombination)
        {
            if (!flags[type]) return false;
        }

        return true;
    }

    /// <summary>
    /// 黑名单
    /// </summary>
    public BuffKeySet Blacklist { get; } = new();

    /// <summary>
    /// 收藏
    /// </summary>
    public BuffKeySet Favorites { get; } = new();

    /// <summary>
    /// 自身来源
    /// </summary>
    public List<Item> PlayerBuffItems { get; } = [];

    public List<Item> BuffItemsCache { get; } = [];

    /// <summary>
    /// 储存系统来源
    /// </summary>
    public List<Item> ExStorageBuffItems { get; } = [];

    /// <summary>
    /// 合并来源
    /// </summary>
    public List<Item> BuffItems { get; } = [];

    /// <summary>
    /// 幸运药水带来的本帧幸运增量。
    /// </summary>
    public float LuckPotionBoost { get; set; }

    public override void OnEnterWorld() => RecollectBuffItems();

    /// <summary>
    /// 注入本帧累计的幸运值加成（来自无限幸运药水），并重置缓存值。
    /// </summary>
    public override void ModifyLuck(ref float luck)
    {
        luck += LuckPotionBoost; LuckPotionBoost = 0;
    }

    /// <summary>
    /// 重建可用物品缓存（玩家来源 + 储存来源）。
    /// </summary>
    /// <remarks>
    /// 该方法是较重路径，因此由 <see cref="PostUpdateBuffs"/> 按间隔触发。
    /// </remarks>
    private void RecollectBuffItems()
    {
        // 1) 统计玩家自身来源。
        PlayerBuffItems.Clear();
        CollectAvailableItems(PlayerBuffItems, MyUtils.GetAllInventoryItemsList(Main.LocalPlayer));

        // 2) 统计储存系统来源（仅启用了无限 Buff 的储存）。
        ExStorageBuffItems.Clear();
        foreach ((var _, var tileEntity) in TileEntity.ByID)
        {
            if (tileEntity is not TEExtremeStorage { UseUnlimitedBuffs: true } storage) continue;

            var alchemyItems = storage.FindAllNearbyChestsWithGroup(ItemGroup.Alchemy);
            foreach (var alchemyItem in alchemyItems)
            {
                CollectAvailableItems(ExStorageBuffItems, Main.chest[alchemyItem].item);
            }
        }

        // 3) 合并为快速判定集合。
        BuffItems.Clear();
        BuffItems.AddRange([.. PlayerBuffItems, .. ExStorageBuffItems]);
        UpdateActivationFlags();

        // 仅在列表发生变化时同步，避免无效网络包。
        if (!BuffItemsCache.SequenceEqual(BuffItems))
        {
            BuffItemsCache.Clear();
            BuffItemsCache.AddRange(BuffItems);
            InfiniteBuffPacket.GetInstance(Player.whoAmI, BuffItems, _activationFlags).Send(ignoreClient: Main.myPlayer);
        }
    }

    private void UpdateActivationFlags()
    {
        if (_activationFlags.Length != BuffLoader.BuffCount)
            Array.Resize(ref _activationFlags, BuffLoader.BuffCount);
        Array.Clear(_activationFlags);

        var player = Main.LocalPlayer;
        //HideGlobalBuff.HidedBuffCountThisFrame = 0;

        // 1) 本地玩家可用物品
        SetupActivationFlags(BuffItems);

        // 2) 队友可共享物品 (开启时)
        if (Main.netMode == NetmodeID.MultiplayerClient && ImproveConfigs.Instance.ShareInfBuffs)
        {
            PlayerHelper.ForEachTeammate(player.whoAmI,
                (player) => SetupActivationFlags(player.GetModPlayer<InfiniteBuffModPlayer>().PlayerBuffItems));
        }
    }

    /// <summary>
    /// 根据物品集合更新隐藏标记表。
    /// </summary>
    private void SetupActivationFlags(IEnumerable<Item> items)
    {
        foreach (var item in items)
        {
            if (item is null || item.IsAir) continue;

            // 展开药水袋
            if (item.ModItem is PotionBag bag && bag.ItemContainer is { Count: > 0 } container)
            {
                foreach (var potion in container)
                {
                    if (item is null || item.IsAir) continue;
                    if (item.stack < ImproveConfigs.Instance.NoConsume_PotionRequirement) continue;
                    _activationFlags[potion.buffType] = true;
                }

                continue;
            }

            foreach (var buffType in ApplyBuffItem.GetItemBuffTypes(item))
            {
                if (buffType == -1) continue;
                _activationFlags[buffType] = true;
            }
        }
    }

    /// <summary>
    /// 无限 Buff 的每帧执行入口。
    /// </summary>
    /// <remarks>
    /// 与 <see cref="HideBuffSystem.PostDrawInterface"/> 的数据来源顺序保持一致：
    /// 本地玩家 -> 队友共享（可选）-> 储存系统。
    /// </remarks>
    public override void PostUpdateBuffs()
    {
        var player = Player;
        if (player.whoAmI != Main.myPlayer || Main.netMode == NetmodeID.Server) return;

        _rebuild?.Tick(Main.gameTimeCache);

        // 先重置 “随身增益站” 状态，再由本帧命中的 Buff 回填。
        UniqueBoostFlags.ResetState();

        // 1) 应用本地玩家来源。
        ApplyAvailableBuffs(PlayerBuffItems);

        if (ImproveConfigs.Instance.ShareInfBuffs)
        {
            // 2) 应用同队共享来源（含距离规则，见 CheckTeamPlayers）。
            PlayerHelper.ForEachTeammate(player.whoAmI, (teammate) =>
            {
                ApplyAvailableBuffs(teammate.GetModPlayer<InfiniteBuffModPlayer>().PlayerBuffItems);
            }, requireAlive: false);
        }

        // 3) 应用储存系统来源（TE）。
        ApplyAvailableBuffs(ExStorageBuffItems);

        // 清理黑名单中的 Buff
        CleanupBuffByBlacklist();
    }

    /// <summary>
    /// 清理黑名单中的 Buff
    /// </summary>
    private void CleanupBuffByBlacklist()
    {
        var player = Player;
        if (player.whoAmI != Main.myPlayer) return;

        var count = Player.MaxBuffs;
        for (int i = 0; i < count; i++)
        {
            var type = player.buffType[i];
            if (type <= 0) continue;
            if (!Blacklist.ContainsByType(type)) continue;

            player.DelBuff(i--);
        }
    }

    /// <summary>
    /// 根据给定物品集合应用无限 Buff，并处理冲突与放行集合。
    /// </summary>
    /// <remarks>
    /// 该方法会：
    /// <br/>1) 汇总可提供的 BuffType；
    /// <br/>2) 应用可启用 Buff；
    /// <br/>3) 回填随身增益站标记；
    /// <br/>4) 清理冲突 Buff；
    /// <br/>5) 更新 <see cref="ClearBuffBan"/>。
    /// </remarks>
    private static void ApplyAvailableBuffs(IEnumerable<Item> items)
    {
        if (!Main.LocalPlayer.TryGetModPlayer<InfiniteBuffModPlayer>(out var infinitePlayer)) return;

        // 可用的 BuffType
        var buffTypes = new HashSet<int>();

        foreach (Item item in items)
        {
            // 侏儒
            if (item.createTile is TileID.GardenGnome) UniqueBoostFlags.HasGardenGnome = true;

            ApplyBuffItem.GetItemBuffTypes(item).ForEach(buffType => buffTypes.Add(buffType));

            // 幸运药水取更高档位，统一在 ModifyLuck 中结算。
            infinitePlayer.LuckPotionBoost = item.type switch
            {
                ItemID.LuckPotion => Math.Max(infinitePlayer.LuckPotionBoost, 0.1f),
                ItemID.LuckPotionGreater => Math.Max(infinitePlayer.LuckPotionBoost, 0.2f),
                _ => infinitePlayer.LuckPotionBoost
            };
        }

        // 收集清扫集合。
        var cleaningMapping = ModIntegrationsSystem.ModdedBuffConflicts;
        var cleaningCollection = new HashSet<int>();
        foreach (var type in buffTypes.Where(type => !infinitePlayer.Blacklist.ContainsByType(type)))
        {
            if (!cleaningMapping.TryGetValue(type, out var list)) continue;
            cleaningCollection.UnionWith(list);
        }

        foreach (var buffType in buffTypes.Where(type => !infinitePlayer.Blacklist.ContainsByType(type)))
        {
            if (cleaningCollection.Contains(buffType)) continue;

            switch (buffType)
            {
                case -1:
                    break;
                default:
                    Main.LocalPlayer.AddBuff(buffType, 30);
                    break;
            }

            // 未开启“随身增益站”时不回填场景标记。
            if (!ImproveConfigs.Instance.NoPlace_BUFFTile) continue;

            UniqueBoostFlags.UpdateStateByType(buffType);
        }

        // 二次清理：移除可能在本帧前半段已存在的冲突 Buff。
        foreach (var buffType in cleaningCollection)
        {
            Main.LocalPlayer.ClearBuff(buffType);
        }
    }

    /// <summary>
    /// 从一组物品中收集可用于“无限 Buff”的物品。
    /// </summary>
    /// <remarks>
    /// 会展开 <see cref="PotionBag"/> 内部物品，并对每个物品调用
    /// <see cref="CollectAvailableItemsFromItem"/> 进行判定与收集。
    /// </remarks>
    private static List<Item> CollectAvailableItems(List<Item> availableItems, IEnumerable<Item> targetItems)
    {
        if (targetItems is null) return availableItems;

        var rpActivated = false;

        foreach (var item in targetItems)
        {
            if (item is null || item.IsAir) continue;

            CollectAvailableItemsFromItem(item, availableItems, ref rpActivated);

            if (!item.IsAir && item.ModItem is PotionBag potionBag && potionBag.ItemContainer.Count > 0)
            {
                foreach (var p in potionBag.ItemContainer)
                {
                    CollectAvailableItemsFromItem(p, availableItems, ref rpActivated);
                }
            }
        }

        return availableItems;
    }

    /// <summary>
    /// 判断单个物品是否可用于 “无限 Buff”，并将其 (或其等效项) 加入列表。
    /// </summary>
    private static void CollectAvailableItemsFromItem(Item item, List<Item> availableItems, ref bool rpActivated)
    {
        // 常规可提供 Buff 的物品（含花园侏儒）。
        if (ApplyBuffItem.GetItemBuffTypes(item).Count > 0 || item.createTile is TileID.GardenGnome)
        {
            availableItems.Add(item);
        }

        // 红药扩展：把扩展效果拆解为多种标准药水以复用后续逻辑（仅一次）。
        if (rpActivated || !item.CanActivateRedPotionExtension()) return;

        rpActivated = true;

        availableItems.Add(item);
        availableItems.Add(new Item(ItemID.ObsidianSkinPotion, 9999));
        availableItems.Add(new Item(ItemID.RegenerationPotion, 9999));
        availableItems.Add(new Item(ItemID.SwiftnessPotion, 9999));
        availableItems.Add(new Item(ItemID.IronskinPotion, 9999));
        availableItems.Add(new Item(ItemID.ManaRegenerationPotion, 9999));
        availableItems.Add(new Item(ItemID.MagicPowerPotion, 9999));
        availableItems.Add(new Item(ItemID.FeatherfallPotion, 9999));
        availableItems.Add(new Item(ItemID.SpelunkerPotion, 9999));
        availableItems.Add(new Item(ItemID.ArcheryPotion, 9999));
        availableItems.Add(new Item(ItemID.HeartreachPotion, 9999));
        availableItems.Add(new Item(ItemID.HunterPotion, 9999));
        availableItems.Add(new Item(ItemID.EndurancePotion, 9999));
        availableItems.Add(new Item(ItemID.LifeforcePotion, 9999));
        availableItems.Add(new Item(ItemID.InfernoPotion, 9999));
        availableItems.Add(new Item(ItemID.MiningPotion, 9999));
        availableItems.Add(new Item(ItemID.RagePotion, 9999));
        availableItems.Add(new Item(ItemID.WrathPotion, 9999));
        availableItems.Add(new Item(ItemID.TrapsightPotion, 9999));
    }

    /// <summary>
    /// 新玩家加入时同步当前无限 Buff 物品状态。
    /// </summary>
    public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
    {
        InfiniteBuffPacket.GetInstance(Player.whoAmI, PlayerBuffItems, _activationFlags).Send(toWho, fromWho);
    }

    /// <summary>
    /// 读取玩家的黑名单与收藏数据。
    /// </summary>
    public override void LoadData(TagCompound tag)
    {
        if (tag.TryGet<TagCompound>(nameof(Blacklist), out var blacklist))
        {
            Blacklist.LoadData(blacklist);
        }

        if (tag.TryGet<TagCompound>(nameof(Favorites), out var favorites))
        {
            Favorites.LoadData(favorites);
        }
    }

    /// <summary>
    /// 保存玩家的黑名单与收藏数据。
    /// </summary>
    public override void SaveData(TagCompound tag)
    {
        tag[nameof(Blacklist)] = Blacklist.GetData();
        tag[nameof(Favorites)] = Favorites.GetData();
    }
}