using ImproveGame.Common;
using ImproveGame.Common.Configs;
using ImproveGame.Common.ModSystems;
using Terraria.Utilities.FileBrowser;

namespace ImproveGame.Content.Items;

public partial class CreateWand
{

    #region 建筑预览
    private static Dictionary<BuildingData, Texture2D> BuildingDataPreview_Internal { get; } = [];
    public static IReadOnlyDictionary<BuildingData, Texture2D> BuildingDataPreview => BuildingDataPreview_Internal;
    public static event Action<BuildingData> OnBuildingDataPreviewAdded;
    private static void SetDefaultPair() 
    {
        var pair = BuildingDataPreview.FirstOrDefault();
        CurrentBuildData = pair.Key;
        CurrentBuildPreview = pair.Value;
    }
    private static BuildingData CurrentBuildData 
    {
        get 
        {
            if (field == null)
                SetDefaultPair();
            return field;
        }
        set;
    }
    private static Texture2D CurrentBuildPreview 
    {
        get
        {
            if (field == null)
                SetDefaultPair();
            return field;
        }
        set;
    }
    public static void SetBuildingData(BuildingData data)
    {
        CurrentBuildData = data;
        if (BuildingDataPreview.TryGetValue(data, out var texture))
            CurrentBuildPreview = texture;
    }
    #endregion

    #region 导入建筑信息
    public static void AddNewPrisonStyle(Texture2D dataTexture, Texture2D previewTexture)
    {
        BuildingRegisterSystem._loadQueue.Enqueue(BuildingLoadData.FromDataMap(dataTexture));
    }
    public static void OpenDialogAndChooseDataMap()
    {
        // 筛选png文件
        ExtensionFilter[] extensions = [
             new ExtensionFilter("png files", "png")
        ];

        // 打开文件选择窗口
        string path = FileBrowser.OpenFilePanel("Select Datamap", extensions);

        if (path == null) return;

        // 进行注册
        BuildingRegisterSystem._loadQueue.Enqueue(BuildingLoadData.FromFilePath(path));
    }

    public static void RegisterFromQotStructureFile(string path)
    {
        BuildingRegisterSystem._loadQueue.Enqueue(BuildingLoadData.FromStrcutre(path));
    }
    #endregion

    #region 建筑材料检索
    private int _cacheRefreshTimer;
    private readonly Dictionary<int, (List<Item>, List<Item>)> _cachedMaterialSources = [];
    private void UpdateMaterialSource(Player player)
    {
        _cachedMaterialSources.Clear();
        var checker = CreateWandHelper.CheckersForItem;
        foreach (var item in BuildingMaterials)
        {
            if (item == null || item.IsAir) continue;
            for (int i = 0; i < 24; i++)
            {
                if (checker[i].Invoke(item))
                {
                    if (!_cachedMaterialSources.TryGetValue(i, out var pair))
                        pair = _cachedMaterialSources[i] = ([], []);
                    pair.Item1.Add(item);
                    break;
                }
            }
        }
        for (int n = 0; n < 50; n++)
        {
            var item = player.inventory[n];
            if (item == null || item.IsAir) continue;
            for (int i = 0; i < 24; i++)
            {
                if (checker[i].Invoke(item))
                {
                    if (!_cachedMaterialSources.TryGetValue(i, out var pair))
                        pair = _cachedMaterialSources[i] = ([], []);
                    pair.Item2.Add(item);
                    break;
                }
            }
        }
    }
    #endregion

    #region 放置函数
    /// <summary>
    /// 为了避免代码过长和减少重复工作，设置的放置判断“总开关”
    /// </summary>
    /// <param name="storedItem">指向仓库物品，即可能的存储物</param>
    /// <param name="player">玩家，一般应该是<see cref="Main.LocalPlayer"></param>
    /// <param name="x">放置目标X坐标</param>
    /// <param name="y">放置目标Y坐标</param>
    /// <param name="tryMethod">进行放置尝试的方法，只有符合条件的才会放置</param>
    private static bool TryPlace(Item item, Player player, int x, int y)
    {
        return MyUtils.BongBongPlace(x, y, item, player, true, true, false);
    }
    private static bool TryPlaceWall(Item item, Player player, int x, int y)
    {
        if (item.createWall > -1)
        {
            MyUtils.TryKillTile(x, y, player);
            if (UIConfigs.Instance.ExplosionEffect)
            {
                MyUtils.BongBong(new Vector2(x, y) * 16f, 16, 16);
            }

            WorldGen.KillWall(x, y);
            if (Main.tile[x, y].WallType == WallID.None)
            {
                WorldGen.PlaceWall(x, y, item.createWall, true);
                return true;
            }
        }

        return false;
    }
    private bool TryPlaceBuilding(Player player, BuildingData data)
    {
        for (int i = 0; i < 24; i++)
        {
            var consume = data.MaterialConsume[i];
            int stack = 0;
            if (_cachedMaterialSources.TryGetValue(i, out var pair))
                stack = pair.Item1.Sum(item => item.stack) + pair.Item2.Sum(item => item.stack);
            if (stack < consume)
                return false;
        }

        static Item FindFirstItemInSource((List<Item>, List<Item>) source)
        {
            foreach (var item in source.Item1)
                if (item.stack > 0)
                    return item;
            foreach (var item in source.Item2)
                if (item.stack > 0)
                    return item;
            return null;
        }


        Point position = Main.MouseWorld.ToTileCoordinates() - new Point(data.Width / 2, data.Height / 2);

        List<TileData> tileDatas = [];

        for (int i = 0; i < data.TileInfos.Count; i++) // 不会放置椅子和工作台
        {
            int x = position.X + i % data.Width; // 物块在图片中的 X 坐标
            int y = position.Y + i / data.Width; // Y 坐标

            TileInfo tileSort = data.TileInfos[i];

            // 墙体
            if (tileSort.HasWall)
            {
                var wallSource = _cachedMaterialSources[23];

                var item = FindFirstItemInSource(wallSource);
                if (item == null) return false;

                if (TryPlaceWall(item, player, x, y))
                    MyUtils.TryConsumeItem(ref item, player);
            }

            switch (tileSort.Sort)
            {
                case TileSort.Block:
                case TileSort.Platform:
                    var itemSource = _cachedMaterialSources[(int)tileSort.Sort - 1];
                    var item = FindFirstItemInSource(itemSource);
                    if (item == null) return false;
                    if (TryPlace(item, player, x, y))
                        MyUtils.TryConsumeItem(ref item, player);
                    break;
                case TileSort.None:
                    break;
                default:
                    tileDatas.Add(new(tileSort, x, y));
                    break;
            }
        }

        for (int i = 0; i < tileDatas.Count; i++)
        {
            int x = tileDatas[i].X;
            int y = tileDatas[i].Y;
            if (Main.tile[x, y].HasTile)
                continue;
            var info = tileDatas[i].Info;


            var itemSource = _cachedMaterialSources[(int)info.Sort - 1];
            var item = FindFirstItemInSource(itemSource);
            if (item == null) return false;
            if (TryPlace(item, player, x, y))
                MyUtils.TryConsumeItem(ref item, player);

            // 朝向特殊处理，后续看怎么加入对其它方向的支持吧
            switch (info.Sort)
            {
                case TileSort.Chair:
                case TileSort.Toilet:
                    if (info.Flip)
                    {
                        Main.tile[tileDatas[i].X, tileDatas[i].Y].TileFrameX += 18;
                        Main.tile[tileDatas[i].X, tileDatas[i].Y - 1].TileFrameX += 18;
                    }

                    break;
                case TileSort.Bathtub:
                case TileSort.Bed:
                    if (!info.Flip)
                    {
                        for (int u = -1; u < 3; u++)
                            for (int v = -1; v < 1; v++)
                            {
                                var tile = Main.tile[tileDatas[i].X + u, tileDatas[i].Y + v];
                                tile.TileFrameX -= 72;
                            }
                    }
                    break;
            }
        }

        if (Main.netMode == NetmodeID.MultiplayerClient)
            NetMessage.SendTileSquare(player.whoAmI, position.X, position.Y, data.Width, data.Height);

        return true;
    }
    #endregion

    #region 原版重写函数

    public override bool? UseItem(Player player)
    {
        if (!Main.dedServ && Main.myPlayer == player.whoAmI && player.altFunctionUse == 0)
        {
            // 强制刷新一次来检测材料是否足够
            UpdateMaterialSource(player);
            if (TryPlaceBuilding(player, CurrentBuildData))
            {
                // 重新刷新合成配方，这样如果一个物品没了就可以把它的合成配方刷新掉
                // Recipe.FindRecipes();
                // 刷新材料来源字典以显示消耗后的量
                UpdateMaterialSource(player);
                if (UIConfigs.Instance.ExplosionEffect)
                    SoundEngine.PlaySound(SoundID.Item14, Main.MouseWorld);
            }
            else
            {
                CombatText.NewText(
                    player.getRect(),
                    new Color(225, 0, 0),
                    MyUtils.GetText("CombatText.Item.CreateWand_NotEnough"),
                    true);
            }
        }
        return true;
    }

    public override void HoldItem(Player player)
    {
        if (Main.dedServ
            || Main.myPlayer != player.whoAmI 
            || CurrentBuildData is not { } buildingData) 
            return;

        Point point = Main.MouseWorld.ToTileCoordinates() - new Point(buildingData.Width / 2, buildingData.Height / 2); // 鼠标位置
        int boxIndex = GameRectangle.Create(this, () => false,
            new Rectangle(point.X, point.Y, buildingData.Width, buildingData.Height), Color.Yellow * 0f, Color.Yellow * 0f);
        if (GameRectangleSystem.GameRectangles.IndexInRange(boxIndex))
        {
            GameRectangle box = GameRectangleSystem.GameRectangles[boxIndex];
            box.Texture2D = CurrentBuildPreview;
        }
    }

    private void ModifyTooltipLine_MaterialInfo(List<TooltipLine> tooltips)
    {
        if (CurrentBuildData is not { } data) return;
        if (_cacheRefreshTimer % 60 == 0)
        {
            _cacheRefreshTimer = 0;
            if (!Main.gameMenu)
                UpdateMaterialSource(Main.LocalPlayer);
        }
        _cacheRefreshTimer++;
        for (int n = 0; n < 24; n++)
        {
            int count = data.MaterialConsume[n];
            if (count <= 0) continue;

            string key = n == 23 ? "Wall" : ((TileSort)(n + 1)).ToString();

            int stackInWand = 0;
            int stackInInventory = 0;
            if (_cachedMaterialSources.TryGetValue(n, out var pair))
            {
                stackInWand = pair.Item1.Sum(item => item.stack);
                stackInInventory = pair.Item2.Sum(item => item.stack);
            }

            int total = stackInWand + stackInInventory;
            string hex = count > total ? "ff0000" : "ffff00";
            string hex2 = count > total ? "ff0000" : "00a7df";

            string neededText = $"[c/{hex}:{MyUtils.GetText($"Architecture.{key}")}: {count}]";
            string hasText =
                $"[c/{hex2}:{MyUtils.GetTextWith("Architecture.StoredMaterials", new { MaterialCountTotal = total, MaterialCountWand = stackInWand, MaterialCountInventory = stackInInventory })}]";

            tooltips.Add(new(Mod, $"MaterialConsume.{key}", $"{neededText}   {hasText}"));
        }
    }

    #endregion

}