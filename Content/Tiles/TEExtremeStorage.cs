using ImproveGame.Common.Configs;
using ImproveGame.Packets.NetStorager;
using ImproveGame.UI.ExtremeStorage;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;

namespace ImproveGame.Content.Tiles
{
    public class TEExtremeStorage : ModTileEntity
    {
        private BitsByte _flags = new(true, true, true, true);
        public bool UseUnlimitedBuffs { get => _flags[0]; set => _flags[0] = value; }
        public bool UsePortableStations { get => _flags[1]; set => _flags[1] = value; }
        public bool UseForCrafting { get => _flags[2]; set => _flags[2] = value; }
        public bool UsePortableBanner { get => _flags[3]; set => _flags[3] = value; }

        #region 基本TE内容

        public override void NetSend(BinaryWriter writer)
        {
            writer.Write(_flags);
        }

        public override void NetReceive(BinaryReader reader)
        {
            _flags = reader.ReadByte();
        }

        public override void SaveData(TagCompound tag)
        {
            tag["flags"] = (byte)_flags;
        }

        public override void LoadData(TagCompound tag)
        {
            _flags = tag.GetByte("flags");
        }

        public override bool IsTileValidForEntity(int x, int y)
        {
            Tile tile = Main.tile[x, y];
            return tile.HasTile && tile.TileType == ModContent.TileType<ExtremeStorage>();
        }

        public override int Hook_AfterPlacement(int i, int j, int placeType, int style, int direction, int alternate)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                NetMessage.SendTileSquare(Main.myPlayer, i - 1, j - 2, 3, 3);
                NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, i - 1, j - 2, Type);
                return -1;
            }

            int placedEntity = Place(i - 1, j - 2);
            return placedEntity;
        }

        public override void OnNetPlace()
        {
            NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y);
        }

        public static bool TryGet(out TEExtremeStorage storage, Point16 point)
        {
            storage = Get(point);
            if (storage is not null)
            {
                return true;
            }

            storage = new();
            return false;
        }

        public static TEExtremeStorage Get(Point16 point)
        {
            // if (!IsAutofisherOpened)
            //     return null;
            Tile tile = Main.tile[point.ToPoint()];
            if (!tile.HasTile)
                return null;
            return !MyUtils.TryGetTileEntityAs<TEExtremeStorage>(point.X, point.Y, out var te) ? null : te;
        }

        #endregion

        // 上下左右 11 格内的所有箱子即视为在范围内
        public bool ChestInRange(int x, int y)
        {
            int distance = ImproveConfigs.Instance.ExStorageSearchDistance;
            bool inRangeX = Math.Abs(x - Position.X) <= distance + 1 || Math.Abs(x - (Position.X + 2)) <= distance;
            bool inRangeY = Math.Abs(y - Position.Y) <= distance + 1 || Math.Abs(y - (Position.Y + 2)) <= distance;
            return inRangeX && inRangeY;
        }

        public bool ChestInRange(Chest chest) => ChestInRange(chest.x, chest.y);

        public bool ChestInRange(int chestIndex)
        {
            var chest = Main.chest[chestIndex];
            return chest is not null && ChestInRange(chest);
        }

        public List<int> FindAllNearbyChests()
        {
            var chestIndexes = new List<int>();

            for (int i = 0; i < Main.maxChests; i++)
                if (ChestInRange(i))
                    chestIndexes.Add(i);

            return chestIndexes;
        }

        public Item[] GetAllItemsByGroup(ItemGroup group)
        {
            // 查找名字相应的箱子
            var chestIndexes = FindAllNearbyChestsWithGroup(group);

            // 建立物品列表
            var itemList = new List<Item>();
            chestIndexes.ForEach(i => itemList.AddRange(Main.chest[i].item));

            return itemList.ToArray();
        }

        public List<int> FindAllNearbyChestsWithGroup(ItemGroup group) => FindAllNearbyChests().FindAll(i =>
            !string.IsNullOrEmpty(Main.chest[i].name) && Main.chest[i].RedirectChestToGroup() == group);

        /// <summary>
        /// 将物品堆叠到附近箱子，指定组别，这个只用于单人模式
        /// 多人模式在: <see cref="InvToChestPacket"/>
        /// </summary>
        public Item StackToNearbyChests(Item item, ItemGroup group)
        {
            // 查找名字相应的箱子
            var chestIndexes = FindAllNearbyChestsWithGroup(group);

            var allChestItems = new List<Item[]>();
            chestIndexes.ForEach(i => allChestItems.Add(Main.chest[i].item));

            // 先填充和物品相同的
            foreach (var chestItems in allChestItems)
            {
                for (int i = 0; i < chestItems.Length; i++)
                {
                    item = MyUtils.ItemStackToInventoryItem(chestItems, i, item, false);
                    if (item.IsAir) return item;
                }
            }

            // 后填充空位
            foreach (var chestItems in allChestItems)
            {
                for (int i = 0; i < chestItems.Length; i++)
                {
                    if (chestItems[i] is null || chestItems[i].IsAir)
                    {
                        chestItems[i] = item;
                        return new Item();
                    }
                }
            }

            return item;
        }
    }
}