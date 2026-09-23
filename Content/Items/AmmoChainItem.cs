using ImproveGame.Common.Configs;
using ImproveGame.Content.Functions.ChainedAmmo;
using ImproveGame.Core;
using ImproveGame.UI.AmmoChainPanel;
using Terraria.ModLoader.IO;

namespace ImproveGame.Content.Items;

public class AmmoChainItem : ModItem
{
    public string ChainName = "";
    public AmmoChain Chain = new();
    public string PlayerName = "";

    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 0;
    }

    public override void SetDefaults()
    {
        Item.width = 24;
        Item.height = 34;
        Item.noUseGraphic = true;
        Item.autoReuse = false;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTime = 20;
        Item.useAnimation = 20;
        Item.rare = ItemRarityID.Quest;
        Item.UseSound = SoundID.ResearchComplete;
        // 考虑了一下要不要消耗，由于一般用完之后留着也没用，所以最后选择做成消耗的
        Item.consumable = true;
    }

    public override bool? UseItem(Player player)
    {
        // 后面那个服务器判断保险用，一般来说第一个就跳了
        if (player.whoAmI != Main.myPlayer || Main.netMode is NetmodeID.Server)
            return true;

        var name = ChainSaver.SaveAsFile(Chain, ChainName);
        var successInfo = this.GetLocalization("Added").WithFormatArgs(name);
        MyUtils.AddNotification(successInfo.Value, itemIconType: Type);
        AmmoChainUI.Instance?.RefreshWeaponPage();
        return true;
    }

    /// <summary>
    /// 获取一个带弹药链信息的物品
    /// </summary>
    public static Item GetItemWithChainData(AmmoChain chain, string chainName, Player player)
    {
        var item = new Item(ModContent.ItemType<AmmoChainItem>());
        var modItem = ((AmmoChainItem)item.ModItem);
        modItem.Chain = chain;
        modItem.ChainName = chainName;
        modItem.PlayerName = player.name;
        item.color = chain.Color;
        return item;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        foreach (var line in tooltips.Where(line => line.Name is not "ItemName" and not "Tooltip0" and not "Tooltip1"))
        {
            line.Visible = false;
        }

        tooltips.Find(t => t.Name is "ItemName").Text += $" - {ChainName}";

        var fromPlayerInfo = this.GetLocalization("FromPlayer").WithFormatArgs(PlayerName);
        tooltips.Add(new TooltipLine(Mod, "AmmoChainFromPlayer", fromPlayerInfo.Value));

        AddAmmoChainTooltips(Mod, Chain, tooltips);
    }

    public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor,
        Color itemColor,
        Vector2 origin, float scale)
    {
        Item.color = Chain.Color;
        return base.PreDrawInInventory(spriteBatch, position, frame, drawColor, itemColor, origin, scale);
    }
    public override bool PreDrawInWorld(WorldItem item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation,
        ref float scale,
        int whoAmI)
    {
        Item.color = Chain.Color;
        return base.PreDrawInWorld(item, spriteBatch, lightColor, alphaColor, ref rotation, ref scale, whoAmI);
    }

    public static void AddAmmoChainTooltips(Mod mod, AmmoChain ammoChain, List<TooltipLine> tooltips)
    {
        var tooltipLines = new List<TooltipLine>();

        if (!ImproveConfigs.Instance.AmmoChain)
        {
            tooltipLines.Add(new TooltipLine(mod, "AmmoChainDisabled", MyUtils.GetText("Tips.AmmoChainDisabled"))
            {
                Color = Color.Yellow
            });
            AddToTooltip();
            return;
        }

        var ammoChainLineText = MyUtils.GetText("Tips.AmmoChain");
        // 如果少于或等于9个，就是很少，直接一行显示得了
        if (ammoChain.Chain.Count <= 9)
        {
            string theText = "";
            foreach ((ItemTypeData itemData, int times) in ammoChain.Chain)
            {
                string text = $"[centeritem/s{times}:{itemData.Item.type}]";
                theText += text;
            }

            ammoChainLineText += theText;
            tooltipLines.Add(new TooltipLine(mod, "AmmoChain", ammoChainLineText)
            {
                Color = Color.Yellow
            });

            // 这里return，强调一下
            AddToTooltip();
            return;
        }

        tooltipLines.Add(new TooltipLine(mod, "AmmoChain", ammoChainLineText)
        {
            Color = Color.Yellow
        });

        string cachedText = "";
        int count = 0;
        int line = 1;
        foreach ((ItemTypeData itemData, int times) in ammoChain.Chain)
        {
            // 用 centeritem 没有 maxStack 限制
            string text = $"[centeritem/s{times}:{itemData.Item.type}]";
            cachedText += text;
            count++;

            if (count < 12)
                continue;

            count = 0;
            tooltipLines.Add(new TooltipLine(mod, $"AmmoChainL{line}", cachedText));
            cachedText = "";
            line++;
        }

        if (!string.IsNullOrWhiteSpace(cachedText))
        {
            tooltipLines.Add(new TooltipLine(mod, $"AmmoChainL{line}", cachedText));
        }

        AddToTooltip();
        return;

        void AddToTooltip()
        {
            int fromModIndex = tooltips.FindIndex(i => i.Name == "FromModTip" && i.Mod == ImproveGame.Instance.Name);
            if (fromModIndex != -1)
                tooltips.InsertRange(fromModIndex, tooltipLines);
            else
                tooltips.AddRange(tooltipLines);
        }
    }

    public override void SaveData(TagCompound tag)
    {
        tag["plrName"] = PlayerName;
        tag["name"] = ChainName;
        tag["chain"] = Chain;
    }

    public override void LoadData(TagCompound tag)
    {
        PlayerName = tag.GetString("plrName");
        ChainName = tag.GetString("name");
        Chain = tag.Get<AmmoChain>("chain") ?? new AmmoChain();
    }

    public override void NetSend(BinaryWriter writer)
    {
        Chain ??= new AmmoChain();
        writer.Write(PlayerName);
        writer.Write(ChainName);
        writer.Write(Chain);
    }

    public override void NetReceive(BinaryReader reader)
    {
        PlayerName = reader.ReadString();
        ChainName = reader.ReadString();
        Chain = reader.ReadAmmoChain();
    }

    public override ModItem Clone(Item newEntity)
    {
        var clone = base.Clone(newEntity) as AmmoChainItem;
        clone.Chain = (AmmoChain)Chain.Clone();
        clone.ChainName = ChainName;
        return clone;
    }
}