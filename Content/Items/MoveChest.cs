using ImproveGame.Common;
using ImproveGame.Common.Conditions;
using ImproveGame.Common.GlobalItems;
using ImproveGame.Packets.Items;
using ImproveGame.Packets.NetChest;
using System.Collections.ObjectModel;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;

namespace ImproveGame.Content.Items;

public class MoveChest : ModItem, IConditionItem
{
    public Condition UseCondition => ConfigCondition.AvailableMoveChestC;
    /// <summary> 强制冷却，放置过快反复与服务器交换出bug </summary>
    public const ushort MaxForceCoolDown = 60;

    /// <summary> 箱子名称 </summary>
    private string _chestName = null;

    /// <summary> 箱子的类型 </summary>
    private ushort _chestType = 0;

    /// <summary> 是否有箱子 </summary>
    private bool _hasChest = false;

    /// <summary> 箱子中的物品 </summary>
    private Item[] _items = null;

    /// <summary> 模组箱子的全名 </summary>
    private string _modChestName = null;

    /// <summary> 箱子的样式 </summary>
    private int _style = 0;

    /// <summary> 强制冷却 </summary>
    private ushort _forceCoolDown = 0;

    /// <summary> 重写了Clone </summary>
    public override bool IsCloneable => true;

    private void SetCooldown(bool clear)
    {
        _forceCoolDown = clear ? (ushort)0 : MaxForceCoolDown;
    }

    private bool CheckCanUse(Player player)
    {
        if (player.noBuilding) return false;

        if (_forceCoolDown > 0) return false;

        var coord = Main.MouseWorld.ToTileCoordinates();

        // 必须在可操作范围内
        if (!player.IsInTileInteractionRange(coord.X, coord.Y, TileReachCheckSettings.Simple)) return true;

        // 放置箱子
        return !_hasChest || TileObject.CanPlace(coord.X, coord.Y, _chestType, _style, 1, out _, true);
    }

    private void TryPlaceChest(Player player)
    {
        var coord = Main.MouseWorld.ToTileCoordinates();

        if (Main.netMode is NetmodeID.SinglePlayer)
        {
            int index = WorldGen.PlaceChest(coord.X, coord.Y, _chestType, false, _style);
            if (index == -1)
            {
                Mod.Logger.Error("Unexpected Error: Unable to Place Chest");
                return;
            }

            SoundEngine.PlaySound(SoundID.Dig, player.position);
            var chest = Main.chest[index];
            chest.item = _items;
            chest.name = _chestName ?? "";

            Reset();
        }
        else
        {
            _items ??= new Item[Chest.DefaultMaxItems];
            _chestName ??= string.Empty;
            AskPlacementPacket.Get(coord, _chestType, _style, _items, _chestName,
                new ItemPosition((byte)player.whoAmI, Array.IndexOf(player.inventory, Item))).Send();

            SetCooldown(false);
        }
    }

    private void TryTakeChest(Player player)
    {
        var coord = Main.MouseWorld.ToTileCoordinates();
        AskTakePacket.Get(new ItemPosition((byte)player.whoAmI, Array.IndexOf(player.inventory, Item)), coord)
            .Send(runLocally: true);

        if (Main.netMode is NetmodeID.MultiplayerClient)
        {
            SetCooldown(false);
        }
    }

    // 只在单个客户端运行的 Shoot，针对客户端写放箱子代码即可
    // 放箱子工作流程：
    // 1. 玩家按下鼠标左键
    // 2. 在检查可以放置后向服务器传输放箱包，魔杖清除箱子基本信息，CD设为最大
    // 3. 服务器收到放箱包，在服务器放下箱子
    // 4. 服务器向所有客户端广播箱子同步包
    // 5. 在服务器设置物品属性，向其他所有客户端广播物品同步包
    // 拿箱子工作流程：
    // 1. 玩家按下鼠标左键
    // 2. 向服务器发送取箱包，CD设为最大
    // 3. 服务器收到取箱包，在服务器清除箱子
    // 4. 服务器广播箱子同步包
    // 5. 在服务器设置物品属性，广播物品同步包
    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity,
        int type,
        int damage, float knockback)
    {
        if (!CheckCanUse(player)) return false;

        // 放箱子
        if (_hasChest)
        {
            TryPlaceChest(player);
        }
        // 拿箱子
        else
        {
            TryTakeChest(player);
        }

        return false;
    }

    /// <summary>
    /// 对Item数组的浅拷贝<br/>
    /// 因为当物品拿在手上时会疯狂触发Clone，深复制套娃物品开销巨大<br/>
    /// </summary>
    /// <param name="newEntity"></param>
    /// <returns></returns>
    public override ModItem Clone(Item newEntity)
    {
        var clone = base.Clone(newEntity) as MoveChest;
        clone._items = _items?.ToArray();
        return clone;
    }

    public override void HoldItem(Player player)
    {
        if (_forceCoolDown > 0)
        {
            _forceCoolDown--;
        }

        if (player.itemAnimation is 0)
        {
            //为了显示放置预览
            Item.createTile = _hasChest ? _chestType : -1;
            Item.placeStyle = _style;
        }

        if (_hasChest)
            ModifyPlayerSpeed(player);
    }

    #region 物品属性多人传输

    public override void NetReceive(BinaryReader reader)
    {
        _forceCoolDown = reader.ReadUInt16();
        _hasChest = reader.ReadBoolean();

        if (!_hasChest)
            return;

        _chestType = reader.ReadUInt16();
        _style = reader.ReadByte();
        if (_chestType >= TileID.Count)
        {
            _modChestName = reader.ReadString();
        }

        _chestName = reader.ReadString();
        _items = reader.ReadItemArray();
    }

    public override void NetSend(BinaryWriter writer)
    {
        writer.Write(_forceCoolDown);
        writer.Write(_hasChest);

        if (!_hasChest)
            return;

        writer.Write(_chestType);
        writer.Write((byte)_style);
        if (_chestType >= TileID.Count)
        {
            writer.Write(_modChestName);
        }

        writer.Write(_chestName);
        writer.Write(_items);
    }

    #endregion

    #region 数据存储和读取

    public void Reset()
    {
        _hasChest = false;
        _items = null;
        _chestName = null;
        _modChestName = null;
        _chestType = 0;
        _style = 0;
        SetCooldown(true);
    }

    public void SetChest(Item[] items, ushort type, string name, int style, string modChestName)
    {
        _hasChest = true;
        _items = items;
        _chestName = name;
        _modChestName = modChestName;
        _chestType = type;
        _style = style;
        SetCooldown(true);
    }

    /// <summary>
    /// 读取包括物品在内的数据
    /// </summary>
    /// <param name="tag"></param>
    public override void LoadData(TagCompound tag)
    {
        if (tag.Count == 0)
        {
            Reset();
            return;
        }

        _hasChest = true;
        _chestType = (ushort)tag.GetAsShort("chest");
        _style = tag.GetAsInt("style");
        if (_chestType >= TileID.Count)
        {
            _modChestName = tag.GetString("mod");
            _chestType = ModContent.TryFind<ModTile>(_modChestName, out var tile) ? tile.Type : TileID.Containers;
        }

        _chestName = tag.GetString("name");
        _items = tag.Get<Item[]>("items");
    }

    public override void SaveData(TagCompound tag)
    {
        if (!_hasChest)
        {
            return;
        }

        tag.Add("chest", (short)_chestType);
        tag.Add("style", _style);
        if (_chestType >= TileID.Count)
        {
            tag.Add("mod", _modChestName);
        }

        tag["name"] = _chestName;
        if (_items is not null)
            tag["items"] = _items;
    }

    #endregion

    #region 基础属性设置

    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;

    public override void SetDefaults()
    {
        Item.width = 1;
        Item.height = 1;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTime = 15;
        Item.useAnimation = 15;
        Item.rare = ItemRarityID.Red;
        Item.autoReuse = true;
        Item.tileBoost = 6;
        Item.shoot = ProjectileID.PurificationPowder; // 占位符
        Item.value = Item.sellPrice(gold: 3);
    }

    public override void UpdateInventory(Player player)
    {
        if (_hasChest && player.HeldItem != Item)
        {
            ModifyPlayerSpeed(player);
        }
    }

    /// <summary>
    /// 当携带箱子时降低玩家移动速度，增加坠落速度
    /// </summary>
    /// <param name="player"></param>
    private static void ModifyPlayerSpeed(Player player)
    {
        player.accRunSpeed -= 0.5f;
        player.maxRunSpeed = 0.5f;
        player.jumpSpeedBoost *= 0.5f;
        player.maxFallSpeed += 0.5f;
        player.moveSpeed -= 0.1f;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        if (_hasChest)
        {
            tooltips.Add(new(Mod, "TooltipHeavy", MyUtils.GetText("MoveChest.Heavy")));
        }
    }

    /// <summary>
    /// 显示其内箱子的物品
    /// </summary>
    public override bool PreDrawTooltip(ReadOnlyCollection<TooltipLine> lines, ref int x, ref int y)
    {
        if (!_hasChest)
            return base.PreDrawTooltip(lines, ref x, ref y);

        List<TooltipLine> list = new();

        for (int i = 0; i < 4; i++)
        {
            string line = "";
            for (int j = 0; j <= 9; j++)
            {
                line += BgItemTagHandler.GenerateTag(_items[i * 10 + j]);
            }

            list.Add(new(Mod, $"ChestItemLine_{i}", line));
        }

        TagItem.DrawTooltips(lines, list, x, y);

        return base.PreDrawTooltip(lines, ref x, ref y);
    }
    public override void AddRecipes()
    {
        CreateRecipe()
            .AddRecipeGroup(RecipeGroups.IronBar, 6)
            .AddRecipeGroup(RecipeGroups.Wood, 18)
            .AddIngredient(ItemID.Diamond)
            .AddTile(TileID.Anvils)
            .Register();
    }

    #endregion
}