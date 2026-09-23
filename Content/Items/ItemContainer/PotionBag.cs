using ImproveGame.Common.Conditions;
using ImproveGame.Common.Configs;
using ImproveGame.Common.GlobalItems;
using ImproveGame.Common.ModHooks;
using ImproveGame.Common.ModSystems;
using ImproveGame.Modules.InfiniteBuff;
using ImproveGame.UI.ItemContainer;
using Terraria.GameContent.Creative;
using Terraria.ModLoader.IO;
using Terraria.UI.Chat;

namespace ImproveGame.Content.Items.ItemContainer;

public class PotionBag : ModItem, IItemOverrideLeftClick, IItemOverrideHover, IItemContainer, IItemMiddleClickable, IConditionItem
{
    public Condition UseCondition => ConfigCondition.AvailablePotionBagC;
    string IItemContainer.Name => Item.Name;
    public List<Item> ItemContainer { get; private set; } = [];
    public bool AutoStorage { get; set; }
    public bool AutoSort { get; set; }
    public bool Synthesis { get; set; }
    public bool MeetEntryCriteria(Item item) => item.buffType > 0 && item.consumable;

    public void ItemIntoContainer(Item item) => ItemIntoContainer(item, true);

    // 克隆内容不克隆引用
    public override ModItem Clone(Item newEntity)
    {
        PotionBag clone = (PotionBag)base.Clone(newEntity);
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

        // player.QuickSpawnItem(player.GetSource_OpenItem(Type), storedPotions[^1], storedPotions[^1].stack);
        // storedPotions.RemoveAt(storedPotions.Count - 1);
    }

    public override bool ConsumeItem(Player player) => false;

    /// <summary>
    /// 只有在这些地方才可以放药水进去
    /// </summary>
    private static readonly List<int> availableContexts = new()
    {
        ItemSlot.Context.InventoryItem,
        ItemSlot.Context.ChestItem,
        114514
    };

    public bool OverrideLeftClick(Item[] inventory, int context, int slot)
    {
        // 很多的条件
        if (ItemSlot.ShiftInUse || ItemSlot.ControlInUse || !availableContexts.Contains(context) ||
            Main.mouseItem.IsAir || !Main.mouseItem.consumable || Main.mouseItem.buffType <= 0)
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
                ItemContainer.RemoveAt(i);
                i--;
                continue;
            }

            if (ItemContainer[i].type == item.type && ItemContainer[i].stack < ItemContainer[i].maxStack &&
                ItemLoader.TryStackItems(ItemContainer[i], item, out _))
            {
                SoundEngine.PlaySound(SoundID.Grab);
                if (item.stack <= 0)
                    item.TurnToAir();
            }
        }

        // 堆叠过后仍有剩余物品, 且有空间, 就 Add
        if (!item.IsAir && ItemContainer.Count < 200)
        {
            ItemContainer.Add(item.Clone());
            item.TurnToAir();
            SoundEngine.PlaySound(SoundID.Grab);
        }

        if (AutoSort && sort)
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

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        ((IItemMiddleClickable)this).HandleTooltips(Item, tooltips);

        // 显示当前绑定的特殊交互按键
        MyUtils.TryGetKeybindString(KeybindSystem.ItemInteractKeybind, out string keybind);
        foreach (TooltipLine tooltip in tooltips)
            if (tooltip.Text == "{TooltipByKeybind}")
                tooltip.Text = MyUtils.GetTextWith($"Items.{Name}.TooltipByKeybind", new { KeybindName = keybind });

        if (ItemContainer is not null && ItemContainer.Count > 0)
        {
            string storeText = ItemContainer.Count >= 200
                ? MyUtils.GetText("Tips.PotionBagCurrentFull")
                : MyUtils.GetTextWith("Tips.PotionBagCurrent", new { StoredCount = ItemContainer.Count });
            tooltips.Add(new(Mod, "PotionBagCurrent", storeText)
            {
                Color = Color.LightGreen
            });

            // 20+类药水时不显示详细信息
            int cow = 0;
            if (ItemContainer.Count > 20)
            {
                string cachedText = string.Empty;
                for (int i = 0; i < ItemContainer.Count; i++)
                {
                    Item potion = ItemContainer[i];
                    int stackDisplayed = potion.stack >= 99 ? 99 : potion.stack;
                    string text = $"[i/s{stackDisplayed}:{potion.type}]";
                    cachedText += text;
                    if ((i + 1) % 20 == 0)
                    {
                        tooltips.Add(new(Mod, $"PotionBagP{++cow}", cachedText));
                        cachedText = string.Empty;
                    }
                }

                if (!string.IsNullOrEmpty(cachedText))
                {
                    tooltips.Add(new(Mod, "PotionBagPX", cachedText));
                }
            }
            // 药水少于20类时显示详细信息
            else
            {
                for (int i = 0; i < ItemContainer.Count; i++)
                {
                    var potion = ItemContainer[i];
                    var color = Color.SkyBlue;
                    bool available = potion.stack >= ImproveConfigs.Instance.NoConsume_PotionRequirement;
                    string text = $"[i/s{potion.stack}:{potion.type}] [{Lang.GetItemNameValue(potion.type)}]";
                    // 有30个
                    if (available)
                    {
                        if (!ImproveConfigs.Instance.NoConsume_Potion || Main.LocalPlayer.GetModPlayer<InfiniteBuffModPlayer>().Blacklist.ContainsByType(potion.buffType))
                        {
                            // 被禁用了
                            text += $"  {MyUtils.GetText("Tips.PotionBagDisabled")}";
                        }
                        else
                        {
                            text += $"  {MyUtils.GetText("Tips.PotionBagAvailable")}";
                            color = Color.LightGreen;
                        }
                    }
                    // 没有30个
                    else
                    {
                        text +=
                            $"  {MyUtils.GetText("Tips.PotionBagUnavailable")} ({potion.stack}/{ImproveConfigs.Instance.NoConsume_PotionRequirement})";
                    }

                    tooltips.Add(new(Mod, $"PotionBagP{i}", text)
                    {
                        Color = color
                    });
                }
            }
        }
        else
        {
            tooltips.Add(new(Mod, "PotionBagNone", MyUtils.GetText("Tips.PotionBagNone"))
            {
                Color = Color.SkyBlue
            });
        }
    }

    public override bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset)
    {
        if (line.Mod == Mod.Name && line.Name.StartsWith("PotionBagP") &&
            Main.SettingsEnabled_OpaqueBoxBehindTooltips)
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

    public override void SetDefaults()
    {
        Item.CloneDefaults(ItemID.EyeOfCthulhuBossBag);
        Item.consumable = false;
        Item.maxStack = 1;
        Item.rare = ItemRarityID.Red;
        Item.expert = false;
        Item.width = 48;
        Item.height = 42;
        Item.value = Item.sellPrice(silver: 30);
    }

    public override void SetStaticDefaults() =>
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 3;

    public override void LoadData(TagCompound tag)
    {
        IItemContainer.LoadData(tag, this);

        if (tag.TryGet<List<Item>>("potions", out var potions))
        {
            ItemContainer = potions;
        }

        // 旧版迁移
        if (tag.ContainsKey("storedPotions"))
        {
            List<Item> list = [];
            foreach (var entry in tag.GetList<TagCompound>("storedPotions"))
            {
                if (!entry.TryGet("potion", out Item potion) || potion.IsAir)
                {
                    continue;
                }

                list.Add(potion);
            }

            ItemContainer = list;
        }
    }

    public override void SaveData(TagCompound tag)
    {
        IItemContainer.SaveData(tag, this);

        tag["potions"] = ItemContainer;
    }

    public override void NetSend(BinaryWriter writer)
    {
        writer.Write(ItemContainer.ToArray());
    }

    public override void NetReceive(BinaryReader reader)
    {
        ItemContainer = new(reader.ReadItemArray());
    }
    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.Silk, 8)
            .AddTile(TileID.WorkBenches)
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