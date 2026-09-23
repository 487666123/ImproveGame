using ImproveGame.Common.Conditions;
using ImproveGame.Common.Configs;
using ImproveGame.Common.GlobalItems;
using Terraria.ModLoader.IO;

namespace ImproveGame.Content.Items;

public class ShellShipInBottle : ModItem, IConditionItem
{
    public Condition UseCondition => ConfigCondition.EnableQuickShimmerC;
    string IConditionItem.ConfigKey => "QuickShimmer";
    public override void SetStaticDefaults()
    {
        ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<ShellShipInBottle_Shimmered>();
        base.SetStaticDefaults();
    }

    public override void SetDefaults()
    {
        Item.maxStack = Item.CommonMaxStack;
        Item.rare = ItemRarityID.Blue;
        Item.Size = new Vector2(42, 20);
        Item.value = Item.sellPrice(silver: 20);
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.Bottle)
            .AddIngredient(ItemID.Seashell, 5)
            .AddRecipeGroup(RecipeGroups.Wood, 10)
            .AddIngredient(ItemID.FallenStar, 1)
            .AddTile(TileID.WorkBenches)
            .Register();
    }
}

public class ShellShipInBottle_Shimmered : ModItem
{
    public override void SetDefaults()
    {
        Item.CloneDefaults(ItemID.CombatBook);
        Item.Size = new Vector2(42, 20);
        Item.useStyle = ItemUseStyleID.RaiseLamp; // 用4的话物品会错位不在手上
        Item.value = Item.sellPrice(silver: 25);
    }

    public override bool? UseItem(Player player)
    {
        if (player.whoAmI != Main.myPlayer)
            return null;

        // 以50个一组获取可以微光转化的物品，测试用
        /*int c = 0;
        if (player.itemAnimation == 1)
            for (int n = 0; n < ItemLoader.ItemCount; n++)
                if (ItemID.Sets.ShimmerTransformToItem[n] > 0)
                {
                    if (c >= 200)
                        player.QuickSpawnItem(Item.GetSource_FromThis(), n);
                    c++;
                    if (c >= 250)
                        break;
                }*/
        if (QuickShimmerSystem.Unlocked)
        {
            if (player.itemAnimation == player.itemAnimationMax)
                MyUtils.AddNotification(MyUtils.GetText("UI.QuickShimmer.AlreadyUnlocked"), Color.Pink);
            return null;
        }

        QuickShimmerUnlockPacket.Unlock();
        WorldGen.BroadcastText(NetworkText.FromKey("Mods.ImproveGame.UI.QuickShimmer.Unlocked"), Color.Pink);
        return true;
    }
}

public class QuickShimmerSystem : ModSystem
{
    public static bool Unlocked;
    public static bool Enabled => Unlocked && ImproveConfigs.Instance.QuickShimmer;

    public override void SaveWorldData(TagCompound tag)
    {
        if (Unlocked) tag.Add("unlocked", true);
        base.SaveWorldData(tag);
    }

    public override void LoadWorldData(TagCompound tag)
    {
        if (tag.ContainsKey("unlocked")) Unlocked = tag.GetBool("unlocked");

        base.LoadWorldData(tag);
    }

    public override void NetSend(BinaryWriter writer)
    {
        var states = new BitsByte(Unlocked);
        writer.Write(states);
    }

    public override void NetReceive(BinaryReader reader)
    {
        var states = (BitsByte)reader.ReadByte();
        Unlocked = states[0];
    }

    public override void ClearWorld()
    {
        Unlocked = false;
    }
}

public class QuickShimmerUnlockPacket : NetModule
{
    /// <summary>
    /// 这个包只要一发，就会解锁快捷微光功能，所以“解锁”就是发个包罢了
    /// </summary>
    public static void Unlock()
    {
        var module = NetModuleLoader.Get<QuickShimmerUnlockPacket>();
        module.Send(runLocally: true);
    }

    public override void Receive()
    {
        QuickShimmerSystem.Unlocked = true;

        if (Main.netMode is NetmodeID.Server)
            Send(-1, Sender);
    }
}