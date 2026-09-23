using ImproveGame.Common.Configs;
using ImproveGame.Content.Items.ItemContainer;
using ImproveGame.Packets;
using ImproveGame.UIFramework.Common;
using Terraria.ModLoader.IO;

namespace ImproveGame.Common.ModPlayers;

public class DataPlayer : ModPlayer
{
    internal static int RefreshTimer;
    internal static bool RefreshRecipes;
    public static bool TryGet(Player player, out DataPlayer modPlayer) => player.TryGetModPlayer(out modPlayer);
    public static DataPlayer Get(Player player) => player.GetModPlayer<DataPlayer>();

    // 保存的物品前缀，哥布林重铸栏
    public int ReforgeItemPrefix = 0;
    private readonly int[] oldSuperVaultStack = new int[100]; // 上一帧的SuperVault的stack情况
    public Item[] SuperVault = new Item[100];
    public bool SuperVaultVisable;
    public Vector2 SuperVaultPos;

    // 液体法杖
    public const int LiquidCap = 255 * 400;
    public int LiquidWandWater;
    public int LiquidWandLava;
    public int LiquidWandHoney;

    public override void Initialize()
    {
        for (int i = 0; i < SuperVault.Length; i++)
        {
            SuperVault[i] ??= new Item();
        }
    }

    public override void LoadData(TagCompound tag)
    {
        // 大背包 Item 数据以及旧版兼容
        if (tag.TryGet<Item[]>("SuperVault", out var superVault))
        {
            for (int i = 0; i < SuperVault.Length && i < superVault.Length; i++)
            {
                SuperVault[i] = superVault[i];
            }
        }

        for (int i = 0; i < SuperVault.Length; i++)
            if (tag.ContainsKey($"SuperVault_{i}"))
                SuperVault[i] = tag.Get<Item>($"SuperVault_{i}");

        LiquidWandWater = tag.GetInt(nameof(LiquidWandWater));
        LiquidWandLava = tag.GetInt(nameof(LiquidWandLava));
        LiquidWandHoney = tag.GetInt(nameof(LiquidWandHoney));
    }

    public override void SaveData(TagCompound tag)
    {
        tag["SuperVault"] = SuperVault;
        tag[nameof(LiquidWandWater)] = LiquidWandWater;
        tag[nameof(LiquidWandLava)] = LiquidWandLava;
        tag[nameof(LiquidWandHoney)] = LiquidWandHoney;
    }

    public override void PostUpdate()
    {
        if (Main.myPlayer != Player.whoAmI)
            return;

        // 侦测stack，如果有变化就发包
        for (int i = 0; i < 100; i++)
        {
            if (SuperVault[i] is null)
            {
                SuperVault[i] = new();
                continue;
            }

            if (SuperVault[i].stack != oldSuperVaultStack[i])
            {
                RefreshRecipes = true;
                if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    var packet = BigBagSlotPacket.Get(this, i);
                    packet.Send(runLocally: false);
                }
            }

            oldSuperVaultStack[i] = SuperVault[i].stack;
            if (SuperVault[i].IsAir)
                oldSuperVaultStack[i] = 0;
        }

        if (RefreshRecipes && RefreshTimer % 30 == 0)
        {
            RefreshRecipes = false;
            // Recipe.FindRecipes();
        }

        RefreshTimer++;
    }

    public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
    {
        for (int i = 0; i < 100; i++)
        {
            oldSuperVaultStack[i] = SuperVault[i].stack;
            if (SuperVault[i].IsAir)
                oldSuperVaultStack[i] = 0;
        }

        // 按照Example的写法 - 直接写就完了！
        BigBagAllSlotsPacket.Get(this).Send(toWho, fromWho, false);
        LiquidStoragePacket.Send(toWho, fromWho, this);
    }

    public override void CopyClientState(ModPlayer targetCopy)
    {
        DataPlayer clone = (DataPlayer)targetCopy;
        clone.SuperVault = SuperVault;
        clone.LiquidWandWater = LiquidWandWater;
        clone.LiquidWandLava = LiquidWandLava;
        clone.LiquidWandHoney = LiquidWandHoney;
    }

    public override void SendClientChanges(ModPlayer clientPlayer)
    {
        DataPlayer clone = (DataPlayer)clientPlayer;

        if (LiquidWandWater != clone.LiquidWandWater || LiquidWandLava != clone.LiquidWandLava ||
            LiquidWandHoney != clone.LiquidWandHoney)
            LiquidStoragePacket.Send(-1, Main.myPlayer, this);
    }

    public override IEnumerable<Item> AddMaterialsForCrafting(out ItemConsumedCallback itemConsumedCallback)
    {
        var items = new List<Item>();
        bool superVaultParticipateSynthesis = ImproveConfigs.Instance.SuperVault && Main.LocalPlayer.GetModPlayer<UIPlayerSetting>().SuperVault_ParticipateSynthesis && SuperVault is not null;

        foreach (Item item in MyUtils.GetAllInventoryItemsList(Main.LocalPlayer, superVaultParticipateSynthesis ? "" : "mod", estimatedCapacity: 260))
        {
            if (item is null || item.IsAir)
                continue;

            if (item.ModItem is BannerChest bannerChest && bannerChest.Synthesis && bannerChest.ItemContainer.Count > 0)
            {
                items.AddRange(bannerChest.ItemContainer);
                continue;
            }

            if (item.ModItem is PotionBag potionBag && potionBag.Synthesis && potionBag.ItemContainer.Count > 0)
            {
                items.AddRange(potionBag.ItemContainer);
                continue;
            }
        }

        if (superVaultParticipateSynthesis)
        {
            items.AddRange(SuperVault);
        }

        if (items.Count > 0)
        {
            itemConsumedCallback = null;
            return items;
        }

        return base.AddMaterialsForCrafting(out itemConsumedCallback);
    }
}