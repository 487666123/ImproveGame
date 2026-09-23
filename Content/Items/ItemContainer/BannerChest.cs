using ImproveGame.Common.Conditions;
using ImproveGame.Common.Configs;
using ImproveGame.Common.GlobalItems;
using ImproveGame.Common.ModHooks;
using ImproveGame.Common.ModSystems;
using ImproveGame.UI.ItemContainer;
using Terraria.ModLoader.IO;
using Terraria.UI.Chat;

namespace ImproveGame.Content.Items.ItemContainer;

// ReSharper disable once ClassNeverInstantiated.Global
public class BannerChest : ModItem, IItemOverrideLeftClick, IItemOverrideHover, IItemContainer, IItemMiddleClickable, IConditionItem
{
    public Condition UseCondition => ConfigCondition.AvailableBannerChestC;

    string IItemContainer.Name => Item.Name;
    public List<Item> ItemContainer { get; private set; } = [];
    public bool AutoStorage { get; set; }
    public bool AutoSort { get; set; }
    public bool Synthesis { get; set; }

    public void ItemIntoContainer(Item item) => ItemIntoContainer(item, true);

    public bool MeetEntryCriteria(Item item) => MyUtils.ItemToBanner(item) != -1;

    // 克隆内容不克隆引用
    public override ModItem Clone(Item newEntity)
    {
        BannerChest clone = (BannerChest)base.Clone(newEntity);
        clone.ItemContainer = new List<Item>(ItemContainer);
        return clone;
    }

    public override bool CanRightClick() => ItemContainer != null;

    public override void RightClick(Player player)
    {
        if (ItemContainerGUI.Instace.Enabled && ItemContainerGUI.Instace.Container == this &&
            ItemContainerGUI.Instace.StartTimer.AnyOpen)
            ItemContainerGUI.Instace.Close();
        else
            ItemContainerGUI.Instace.Open(this);

        //player.QuickSpawnItem(player.GetSource_OpenItem(Type), storedBanners[^1], storedBanners[^1].stack);
        //storedBanners.RemoveAt(storedBanners.Count - 1);
    }

    public override bool ConsumeItem(Player player) => false;

    /// <summary>
    /// 只有在这些地方才可以放旗帜进去
    /// </summary>
    private static readonly List<int> AvailableContexts =
    [
        ItemSlot.Context.InventoryItem,
        ItemSlot.Context.ChestItem,
        114514
    ];

    public bool OverrideLeftClick(Item[] inventory, int context, int slot)
    {
        // 很多的条件
        int bannerID = MyUtils.ItemToBanner(Main.mouseItem);
        if (ItemSlot.ShiftInUse || ItemSlot.ControlInUse || !AvailableContexts.Contains(context) ||
            Main.mouseItem.IsAir || !Main.mouseItem.consumable || bannerID == -1)
        {
            return false;
        }

        ItemIntoContainer(Main.mouseItem);
        if (context != 114514 && Main.netMode == NetmodeID.MultiplayerClient)
        {
            NetMessage.SendData(MessageID.SyncEquipment, -1, -1, null, Main.myPlayer, slot, inventory[slot].prefix);
        }

        return true;
    }

    public void ItemIntoContainer(Item item, bool sort)
    {
        // 清除 Air 和 堆叠物品
        for (int i = 0; i < ItemContainer.Count; i++)
        {
            if (ItemContainer[i].IsAir)
            {
                ItemContainer.RemoveAt(i--);
                continue;
            }

            if (ItemContainer[i].type == item.type &&
                ItemContainer[i].stack < ItemContainer[i].maxStack &&
                ItemLoader.TryStackItems(ItemContainer[i], item, out _))
            {
                SoundEngine.PlaySound(SoundID.Grab);
                if (item.stack <= 0)
                {
                    item.TurnToAir();
                }
            }
        }

        // 堆叠过后仍有剩余物品, 且有空间, 就 Add
        if (!item.IsAir && ItemContainer.Count < 500)
        {
            ItemContainer.Add(item.Clone());
            item.TurnToAir();

            SoundEngine.PlaySound(SoundID.Grab);
        }

        if (sort && AutoSort)
        {
            SortContainer();
        }
    }

    public void SortContainer()
    {
        ItemContainer.Sort((a, b) =>
        {
            return a.type.CompareTo(b.type) + a.stack.CompareTo(b.stack) * 10;
        });
    }

    public override bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset)
    {
        if (line.Mod == Mod.Name && line.Name == "BannerList" && Main.SettingsEnabled_OpaqueBoxBehindTooltips)
        {
            var font = FontAssets.MouseText.Value;
            var position = new Vector2(line.X, line.Y);
            var color = line.Color;
            var snippets = ChatManager.ParseMessage(line.Text, color);
            ChatManager.ConvertNormalSnippets(snippets);
            ChatManager.DrawColorCodedString(Main.spriteBatch, font, snippets, position, Color.White, 0f,
                Vector2.Zero, Vector2.One, out _, -1);
            return false;
        }

        return base.PreDrawTooltipLine(line, ref yOffset);
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        ((IItemMiddleClickable)this).HandleTooltips(Item, tooltips);

        // 显示当前绑定的特殊交互按键
        MyUtils.TryGetKeybindString(KeybindSystem.ItemInteractKeybind, out string keybind);
        foreach (TooltipLine tooltip in tooltips)
            if (tooltip.Text == "{TooltipByKeybind}")
                tooltip.Text = MyUtils.GetTextWith($"Items.{Name}.TooltipByKeybind", new { KeybindName = keybind });

        if (!ImproveConfigs.Instance.NoPlace_BUFFTile_Banner)
        {
            tooltips.Add(new TooltipLine(Mod, "BannerChestUseless", MyUtils.GetText("Tips.BannerChestUseless"))
            {
                Color = Color.SkyBlue
            });
        }

        if (ItemContainer is not null && ItemContainer.Count > 0)
        {
            string storeText = ItemContainer.Count >= 500
                ? MyUtils.GetText("Tips.BannerChestCurrentFull")
                : MyUtils.GetTextWith("Tips.BannerChestCurrent", new { StoredCount = ItemContainer.Count });
            tooltips.Add(new TooltipLine(Mod, "BannerChestCurrent", storeText)
            {
                Color = Color.LightGreen
            });

            string cachedText = string.Empty;
            for (int i = 0; i < ItemContainer.Count; i++)
            {
                var banner = ItemContainer[i];
                string text = $"[i/s{banner.stack}:{banner.type}]";
                cachedText += text;
                if ((i + 1) % 20 != 0)
                {
                    continue;
                }

                tooltips.Add(new TooltipLine(Mod, "BannerList", cachedText));
                cachedText = string.Empty;
            }

            if (!string.IsNullOrEmpty(cachedText))
            {
                tooltips.Add(new TooltipLine(Mod, "BannerList", cachedText));
            }
        }
        else
        {
            tooltips.Add(new TooltipLine(Mod, "BannerChestNone", MyUtils.GetText("Tips.BannerChestNone"))
            {
                Color = Color.SkyBlue
            });
        }
    }

    public override void SetDefaults()
    {
        Item.SetBaseValues(48, 42, ItemRarityID.Red, Item.sellPrice(0, 0, 50));
        Item.expert = false;
    }

    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 3;

    public override void LoadData(TagCompound tag)
    {
        IItemContainer.LoadData(tag, this);

        if (tag.TryGet<List<Item>>("banners", out var banners))
        {
            ItemContainer = banners;
        }

        // 旧版迁移
        if (tag.ContainsKey("storedBanners"))
        {
            List<Item> list = [];
            foreach (var entry in tag.GetList<TagCompound>("storedBanners"))
            {
                if (!entry.TryGet("banner", out Item banner) || banner.IsAir)
                {
                    continue;
                }

                list.Add(banner);
            }

            ItemContainer = list;
        }
    }

    public override void SaveData(TagCompound tag)
    {
        IItemContainer.SaveData(tag, this);

        tag["banners"] = ItemContainer;
    }

    public override void NetSend(BinaryWriter writer)
    {
        writer.Write(ItemContainer.ToArray());
    }

    public override void NetReceive(BinaryReader reader)
    {
        ItemContainer = new List<Item>(reader.ReadItemArray());
    }
    public override void AddRecipes()
    {
        CreateRecipe()
            .AddRecipeGroup(RecipeGroups.IronBar, 12)
            .AddTile(TileID.Anvils)
            .Register();
    }

    public bool OverrideHover(Item[] inventory, int context, int slot)
    {
        ((IItemMiddleClickable)this).HandleHover(inventory, context, slot);

        return false;
    }

    public void OnMiddleClicked(Item item)
    {
        var items = MyUtils.GetAllInventoryItemsList(Main.LocalPlayer, estimatedCapacity: 260);
        foreach (var sourceItem in items.Where(sourceItem => !sourceItem.IsAir && MeetEntryCriteria(sourceItem)))
        {
            ItemIntoContainer(sourceItem, false);
        }

        // 对光标位物品的特殊处理（inventory[58]是通过mouseItem.Clone来的，引用不是同一个，故需要特殊处理）
        Main.mouseItem.stack = Main.LocalPlayer.inventory[58].stack;
        if (Main.mouseItem.stack <= 0)
        {
            Main.mouseItem.TurnToAir();
        }

        SortContainer();
    }
}