using ImproveGame.Common.Packets.NetStorager;
using ImproveGame.Interface.ExtremeStorage;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;

namespace ImproveGame.Content.Tiles
{
    public class TEExtremeStorage : ModTileEntity
    {
        private BitsByte _flags;
        public bool UseUnlimitedBuffs { get => _flags[0]; set => _flags[0] = value; }
        public bool UsePortableStations { get => _flags[1]; set => _flags[1] = value; }
        public bool UseForCrafting { get => _flags[2]; set => _flags[2] = value; }

        public override void Update()
        {
            if (Main.netMode is NetmodeID.Server && UseForCrafting)
            {
                UseForCrafting = false;
                SyncDataPacket.Get(ID).Send();
            }
        }

        #region ����TE����

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
            return !TryGetTileEntityAs<TEExtremeStorage>(point.X, point.Y, out var te) ? null : te;
        }

        #endregion

        // ����һ�� 17x17 �ķ�Χ�ڵ���������
        // ����������������������������������
        // ����������������������������������
        // ����������������������������������
        // ����������������������������������
        // ����������������������������������
        // ����������������������������������
        // ����������������������������������
        // ����������������������������������
        // ����������������������������������
        // ����������������������������������
        // ����������������������������������
        // ����������������������������������
        // ����������������������������������
        // ����������������������������������
        // ����������������������������������
        // ����������������������������������
        // ����������������������������������
        public List<int> FindAllNearbyChests()
        {
            var chestIndexes = new List<int>();

            for (int i = 0; i < Main.maxChests; i++)
            {
                var chest = Main.chest[i];
                if (chest is null)
                    continue;

                bool inRangeX = Math.Abs(chest.x - Position.X) <= 7 || Math.Abs(chest.x - (Position.X + 2)) <= 7;
                bool inRangeY = Math.Abs(chest.y - Position.Y) <= 7 || Math.Abs(chest.y - (Position.Y + 2)) <= 7;
                if (inRangeX && inRangeY)
                    chestIndexes.Add(i);
            }

            return chestIndexes;
        }

        public Item[] GetAllItemsByGroup(ItemGroup group)
        {
            // ����������Ӧ������
            var chestIndexes = FindAllNearbyChestsWithGroup(group);

            // ������Ʒ�б�
            var itemList = new List<Item>();
            chestIndexes.ForEach(i => itemList.AddRange(Main.chest[i].item));

            return itemList.ToArray();
        }

        public List<int> FindAllNearbyChestsWithGroup(ItemGroup group) => FindAllNearbyChests().FindAll(i =>
            !string.IsNullOrEmpty(Main.chest[i].name) && Main.chest[i].name[0] == (char)group);

        /// <summary>
        /// ����Ʒ�ѵ����������ӣ�ָ��������ֻ���ڵ���ģʽ
        /// ����ģʽ��: <see cref="InvToChestPacket"/>
        /// </summary>
        public Item StackToNearbyChests(Item item, ItemGroup group)
        {
            // ����������Ӧ������
            var chestIndexes = FindAllNearbyChestsWithGroup(group);

            var allChestItems = new List<Item[]>();
            chestIndexes.ForEach(i => allChestItems.Add(Main.chest[i].item));

            // ��������Ʒ��ͬ��
            foreach (var chestItems in allChestItems)
            {
                for (int i = 0; i < chestItems.Length; i++)
                {
                    item = ItemStackToInventoryItem(chestItems, i, item, false);
                    if (item.IsAir) return item;
                }
            }

            // ������λ
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