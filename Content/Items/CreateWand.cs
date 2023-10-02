using ImproveGame.Common.Configs;
using ImproveGame.Common.ModHooks;
using ImproveGame.Common.ModSystems;
using ImproveGame.Entitys;
using ImproveGame.Interface.Common;
using ImproveGame.Interface.GUI;
using Microsoft.Xna.Framework.Input;
using Terraria.GameInput;
using Terraria.ModLoader.IO;
using static ImproveGame.Entitys.TileData;
using TileData = ImproveGame.Entitys.TileData;

namespace ImproveGame.Content.Items;

public class CreateWand : ModItem, IItemOverrideHover, IItemMiddleClickable
{
    public override bool IsLoadingEnabled(Mod mod) => Config.LoadModItems.CreateWand;
    public override bool AltFunctionUse(Player player) => true;

    private static Texture2D[] _prisons;
    private static Texture2D[] _prisonsPreView;
    private static Color[][] _colors;

    private static bool _colorsLoaded = false;
    private static int _styleIndex = 0;

    public static Texture2D Prison => _prisons[_styleIndex];
    public static Texture2D PrisonsPreView => _prisonsPreView[_styleIndex];
    public static Color[] Colors => _colors[_styleIndex];

    public override void Load()
    {
        if (!Main.dedServ)
        {
            _colorsLoaded = false;

            // 把读取放到主线程上
            Main.QueueMainThreadAction(() =>
                {
                    if (_colorsLoaded)
                        return;

                    _prisons = new[]
                    {
                        ModAsset.Prison1.Value, ModAsset.Prison2.Value, ModAsset.Prison3.Value
                    };

                    _prisonsPreView = new[]
                    {
                        ModAsset.PrisonPreview1.Value, ModAsset.PrisonPreview2.Value, ModAsset.PrisonPreview3.Value
                    };

                    _colors = new[] {GetColors(_prisons[0]), GetColors(_prisons[1]), GetColors(_prisons[2])};
                    _colorsLoaded = true;
                }
            );
        }
        else
        {
            _colorsLoaded = true;
        }
    }

    public override void Unload()
    {
        if (!Main.dedServ)
        {
            _prisons = null;
            _prisonsPreView = null;
            _colors = null;
        }

        _colorsLoaded = false;
    }

    // 切换样式
    public static void NextStyle()
    {
        _styleIndex++;
        _styleIndex %= _prisons.Length;
    }

    [CloneByReference] public Item Block = new Item();
    [CloneByReference] public Item Wall = new Item();
    [CloneByReference] public Item Platform = new Item();
    [CloneByReference] public Item Torch = new Item();
    [CloneByReference] public Item Chair = new Item();
    [CloneByReference] public Item Workbench = new Item();
    [CloneByReference] public Item Bed = new Item();

    public override void SaveData(TagCompound tag)
    {
        tag[nameof(Block)] = Block;
        tag[nameof(Wall)] = Wall;
        tag[nameof(Platform)] = Platform;
        tag[nameof(Torch)] = Torch;
        tag[nameof(Chair)] = Chair;
        tag[nameof(Workbench)] = Workbench;
        tag[nameof(Bed)] = Bed;
    }

    public override void LoadData(TagCompound tag)
    {
        tag.TryGet(nameof(Block), out Block);
        tag.TryGet(nameof(Wall), out Wall);
        tag.TryGet(nameof(Platform), out Platform);
        tag.TryGet(nameof(Torch), out Torch);
        tag.TryGet(nameof(Chair), out Chair);
        tag.TryGet(nameof(Workbench), out Workbench);
        tag.TryGet(nameof(Bed), out Bed);
    }

    public override void SetStaticDefaults()
    {
        Item.staff[Type] = true;
        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults()
    {
        Item.SetBaseValues(42, 42, ItemRarityID.Red, Item.sellPrice(0, 1));
        Item.SetUseValues(ItemUseStyleID.Swing, SoundID.Item1, 16, 16);
    }

    public override void HoldItem(Player player)
    {
        if (player.itemAnimation == 0)
            Item.mana = 40;
        if (!Main.dedServ && Main.myPlayer == player.whoAmI)
        {
            Point point = Main.MouseWorld.ToTileCoordinates() - (Prison.Size() / 2f).ToPoint(); // 鼠标位置
            int boxIndex = GameRectangle.Create(this, () => false,
                new Rectangle(point.X, point.Y, Prison.Width, Prison.Height), Color.Yellow * 0f, Color.Yellow * 0f);
            if (GameRectangleSystem.GameRectangles.IndexInRange(boxIndex))
            {
                GameRectangle box = GameRectangleSystem.GameRectangles[boxIndex];
                box.Texture2D = PrisonsPreView;
            }
        }
    }

    public override void ModifyManaCost(Player player, ref float reduce, ref float mult)
    {
        if (player.altFunctionUse == 2)
        {
            reduce = 0;
        }
    }

    public override bool CanUseItem(Player player)
    {
        if (player.noBuilding)
            return false;
        if (player.altFunctionUse == 2 && !Main.dedServ && player.whoAmI == Main.myPlayer)
        {
            if (!ArchitectureGUI.Visible)
                UISystem.Instance.ArchitectureGUI.Open(this);
            else
                UISystem.Instance.ArchitectureGUI.Close();
            return false;
        }

        return true;
    }

    /// <summary>
    /// 从<see cref="string"/>类型的名称获取对应的物品实例，为了方便而设置
    /// </summary>
    /// <param name="itemType">物品实例名称</param>
    /// <param name="item">物品实例</param>
    internal void GetStoredItemInstance(string itemType, out Item item)
    {
        item = new Item(ItemID.None);
        switch (itemType)
        {
            case nameof(Block):
                item = Block;
                return;
            case nameof(Platform):
                item = Platform;
                return;
            case nameof(Wall):
                item = Wall;
                return;
            case nameof(Torch):
                item = Torch;
                return;
            case nameof(Workbench):
                item = Workbench;
                return;
            case nameof(Chair):
                item = Chair;
                return;
            case nameof(Bed):
                item = Bed;
                return;
        }
    }

    /// <summary>
    /// 设置物品，用于UI和物品存储数据间的同步
    /// </summary>
    /// <param name="itemType">物品存储类型</param>
    /// <param name="item">物品实例</param>
    internal void SetItem(string itemType, Item item)
    {
        switch (itemType)
        {
            case nameof(Block):
                Block = item;
                break;
            case nameof(Platform):
                Platform = item;
                break;
            case nameof(Wall):
                Wall = item;
                break;
            case nameof(Torch):
                Torch = item;
                break;
            case nameof(Workbench):
                Workbench = item;
                break;
            case nameof(Chair):
                Chair = item;
                break;
            case nameof(Bed):
                Bed = item;
                break;
        }
    }

    /// <summary>
    /// 就为了实现一个“如果不放东西就没爆炸声音”的功能
    /// </summary>
    private static bool _playedSound = false;

    public override bool? UseItem(Player player)
    {
        if (!_colorsLoaded || _colors is null)
        {
            ImproveGame.Instance.Logger.Error("Create Wand Colors didn't load. Please report to mod developers.");
            return base.UseItem(player);
        }

        if (!Main.dedServ && Main.myPlayer == player.whoAmI && player.altFunctionUse == 0)
        {
            Point position = Main.MouseWorld.ToTileCoordinates() - (Prison.Size() / 2f).ToPoint();

            List<TileData> tileDatas = new();

            for (int i = 0; i < Colors.Length; i++) // 不会放置椅子和工作台
            {
                int x = position.X + i % Prison.Width; // 物块在图片中的 X 坐标
                int y = position.Y + i / Prison.Width; // Y 坐标

                TileSort tileSort = Color2TileSort(Colors[i]);

                // 墙体
                if (ShouldPlaceWall(tileSort))
                {
                    if (Wall.IsAir || Wall.createWall <= WallID.None)
                    {
                        PickItemInInventory(player, (item) => TryPlaceWall(item, player, x, y), true, out _);
                    }
                    else if (TryPlaceWall(Wall, player, x, y))
                    {
                        TryConsumeItem(ref Wall, player);
                    }
                }

                switch (tileSort)
                {
                    case TileSort.Block:
                        TryPlace(ref Block, player, x, y, TryPlaceTile);
                        break;
                    case TileSort.Platform:
                        TryPlace(ref Platform, player, x, y, TryPlacePlatform);
                        break;
                }

                if (tileSort != TileSort.None) // 物块
                {
                    if (tileSort == TileSort.Torch || tileSort == TileSort.Chair ||
                        tileSort == TileSort.Workbench || tileSort == TileSort.Table ||
                        tileSort == TileSort.Bed) // 火把，椅子，工作台，桌子，床
                    {
                        tileDatas.Add(new(tileSort, x, y));
                    }
                }
            }

            for (int i = 0; i < tileDatas.Count; i++) // 火把，椅子，工作台，桌子，床
            {
                int x = tileDatas[i].x;
                int y = tileDatas[i].y;
                if (Main.tile[x, y].HasTile)
                {
                    continue;
                }

                // 进行其他的放置尝试
                switch (tileDatas[i].tileSort)
                {
                    case TileSort.Torch:
                        TryPlace(ref Torch, player, x, y,
                            (Item item) => item.createTile >= TileID.Dirt && TileID.Sets.Torch[item.createTile]);
                        break;
                    case TileSort.Workbench:
                        TryPlace(ref Workbench, player, x, y, (Item item) => item.createTile == TileID.WorkBenches);
                        break;
                    case TileSort.Chair:
                        TryPlace(ref Chair, player, x, y, (Item item) => item.createTile == TileID.Chairs);
                        Main.tile[tileDatas[i].x, tileDatas[i].y].TileFrameX += 18;
                        Main.tile[tileDatas[i].x, tileDatas[i].y - 1].TileFrameX += 18;
                        break;
                    // 目前似乎没有桌子的需求，而且我整UI的时候也没给桌子整
                    //case TileSort.Table:
                    //    TryPlace(ref Table, player, x, y, (Item item) => item.createTile == TileID.Tables);
                    //    break;
                    case TileSort.Bed:
                        TryPlace(ref Bed, player, x, y, (Item item) => item.createTile == TileID.Beds);
                        break;
                }
            }

            if (Main.netMode == NetmodeID.MultiplayerClient)
                NetMessage.SendTileSquare(player.whoAmI, position.X, position.Y, Prison.Width, Prison.Height);

            // 重新刷新合成配方，这样如果一个物品没了就可以把它的合成配方刷新掉
            Recipe.FindRecipes();
            // 同步UI物品
            UISystem.Instance.ArchitectureGUI.RefreshSlots(this);
        }

        if (!_playedSound && player.altFunctionUse == 0)
            CombatText.NewText(player.getRect(), new Color(225, 0, 0), GetText("CombatText.Item.CreateWand_NotEnough"),
                true);

        _playedSound = false;
        return true;
    }

    /// <summary>
    /// 为了避免代码过长和减少重复工作，设置的放置判断“总开关”
    /// </summary>
    /// <param name="storedItem">指向仓库物品，即可能的存储物</param>
    /// <param name="player">玩家，一般应该是<see cref="Main.LocalPlayer"></param>
    /// <param name="x">放置目标X坐标</param>
    /// <param name="y">放置目标Y坐标</param>
    /// <param name="tryMethod">进行放置尝试的方法，只有符合条件的才会放置</param>
    private static void TryPlace(ref Item storedItem, Player player, int x, int y, Func<Item, bool> tryMethod)
    {
        // 没有存储物品，在物品栏里面找
        if (storedItem.IsAir || storedItem.createTile < TileID.Dirt)
        {
            PickItemInInventory(player, item =>
                    item is not null && tryMethod(item) &&
                    BongBongPlace(x, y, item, player, true, true, !_playedSound),
                true, out int index);
            if (index != -1)
            {
                _playedSound = true;
            }
        }
        // 进行存储物品的放置尝试
        else if (storedItem is not null && tryMethod(storedItem) &&
                 BongBongPlace(x, y, storedItem, player, true, true, !_playedSound))
        {
            TryConsumeItem(ref storedItem, player);
            _playedSound = true;
        }
    }

    private static bool TryPlacePlatform(Item item) =>
        item.createTile >= TileID.Dirt && TileID.Sets.Platforms[item.createTile];

    private static bool TryPlaceTile(Item item) =>
        item.createTile >= TileID.Dirt && Main.tileSolid[item.createTile] && !Main.tileSolidTop[item.createTile];

    private static bool TryPlaceWall(Item item, Player player, int x, int y)
    {
        if (item.createWall > -1)
        {
            TryKillTile(x, y, player);
            if (UIConfigs.Instance.ExplosionEffect)
            {
                BongBong(new Vector2(x, y) * 16f, 16, 16);
            }

            WorldGen.KillWall(x, y);
            if (Main.tile[x, y].WallType == 0)
            {
                WorldGen.PlaceWall(x, y, item.createWall, true);
                return true;
            }
        }

        return false;
    }

    private readonly Dictionary<TileSort, int> MaterialConsume = new()
    {
        {TileSort.None, 0},
        {TileSort.Block, 0},
        {TileSort.Wall, 0},
        {TileSort.Platform, 0},
        {TileSort.Torch, 0},
        {TileSort.Chair, 0},
        {TileSort.Table, 0},
        {TileSort.Workbench, 0},
        {TileSort.Bed, 0},
        {TileSort.NoWall, 0}
    };

    // 计算消耗
    private void CalculateConsume()
    {
        foreach (var item in MaterialConsume)
        {
            MaterialConsume[item.Key] = 0;
        }

        if (!_colorsLoaded || _colors is null)
        {
            ImproveGame.Instance.Logger.Error("Create Wand Colors didn't load. Please report to mod developers.");
            return;
        }

        TileSort tileSort;
        for (int i = 0; i < Colors.Length; i++)
        {
            tileSort = Color2TileSort(Colors[i]);
            MaterialConsume[tileSort]++;
            if (tileSort != TileSort.Block && tileSort != TileSort.Platform && tileSort != TileSort.NoWall)
            {
                MaterialConsume[TileSort.Wall]++;
            }
        }
    }

    public override void NetSend(BinaryWriter writer)
    {
        ItemIO.Send(Block, writer, true, true);
        ItemIO.Send(Wall, writer, true, true);
        ItemIO.Send(Platform, writer, true, true);
        ItemIO.Send(Torch, writer, true, true);
        ItemIO.Send(Chair, writer, true, true);
        ItemIO.Send(Workbench, writer, true, true);
        ItemIO.Send(Bed, writer, true, true);
    }

    public override void NetReceive(BinaryReader reader)
    {
        Block = ItemIO.Receive(reader, true, true);
        Wall = ItemIO.Receive(reader, true, true);
        Platform = ItemIO.Receive(reader, true, true);
        Torch = ItemIO.Receive(reader, true, true);
        Chair = ItemIO.Receive(reader, true, true);
        Workbench = ItemIO.Receive(reader, true, true);
        Bed = ItemIO.Receive(reader, true, true);
    }

    public bool OverrideHover(Item[] inventory, int context, int slot)
    {
        ((IItemMiddleClickable)this).HandleHover(inventory, context, slot);

        return false;
    }

    public void OnMiddleClicked(Item item)
    {
        if (!ArchitectureGUI.Visible)
            UISystem.Instance.ArchitectureGUI.Open(this);
        else
            UISystem.Instance.ArchitectureGUI.Close();
    }

    public void ManageHoverTooltips(Item item, List<TooltipLine> tooltips)
    {
        // 决定文本显示的是“开启”还是“关闭”
        string text = ArchitectureGUI.Visible ? "Off" : "On";
        tooltips.Add(new TooltipLine(Mod, "CreateWand", GetText($"Tips.CreateWand{text}"))
            {OverrideColor = Color.LightGreen});
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        ((IItemMiddleClickable)this).HandleTooltips(Item, tooltips);

        CalculateConsume();
        tooltips.Add(new(Mod, "MaterialConsume", $"[c/ffff00:{GetText("Architecture.MaterialsRequired")}]"));
        foreach (var item in MaterialConsume)
        {
            if (item.Key != TileSort.None && item.Value > 0)
            {
                GetStoredItemInstance(item.Key.ToString(), out var storedItem);
                int stack = 0;
                if (storedItem is not null && !storedItem.IsAir)
                {
                    stack = storedItem.stack;
                }

                string neededText = $"[c/ffff00:{GetText($"Architecture.{item.Key}")}: {MaterialConsume[item.Key]}]";
                string hasText =
                    $"[c/00a7df:{GetTextWith($"Architecture.StoredMaterials", new {MaterialCount = stack})}]";

                tooltips.Add(new(Mod, $"MaterialConsume.{item.Key}", $"{neededText}   {hasText}"));
            }
        }
    }

    /// <summary>
    /// 颜色对应的物块类型
    /// </summary>
    /// <param name="color"></param>
    /// <returns></returns>
    private static TileSort Color2TileSort(Color color)
    {
        if (color == Color.Red)
            return TileSort.Block; // 实体块
        if (color == Color.Black)
            return TileSort.Platform; // 平台
        if (color == Color.White)
            return TileSort.Torch; // 火把
        if (color == Color.Yellow)
            return TileSort.Chair; // 椅子
        if (color == FenSe)
            return TileSort.Table; // 桌子
        if (color == Color.Blue)
            return TileSort.Workbench; // 工作台
        if (color == ZiSe)
            return TileSort.Bed; // 床
        if (color == QingSe)
            return TileSort.NoWall; // 禁止放置墙体
        return TileSort.None; // 没有任何
    }

    private static readonly Color ZiSe = new(127, 0, 255);
    private static readonly Color QingSe = new(0, 255, 255);
    private static readonly Color FenSe = new(255, 0, 255);

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddRecipeGroup(RecipeGroupID.Wood, 24)
            .AddRecipeGroup(RecipeSystem.AnyGoldBar, 12)
            .AddIngredient(ItemID.FallenStar, 8)
            .Register();
    }
}