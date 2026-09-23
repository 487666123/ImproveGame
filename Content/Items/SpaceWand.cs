using ImproveGame.Common.Conditions;
using ImproveGame.Common.GlobalItems;
using ImproveGame.Common.ModSystems;
using ImproveGame.Common.ModSystems.MarqueeSystem;
using ImproveGame.Content.Packets;
using ImproveGame.Core;
using ImproveGame.Packets;
using ImproveGame.UIFramework;
using Terraria.ModLoader.IO;

namespace ImproveGame.Content.Items;

public enum PlaceType : byte
{
    Platform, Soild, Rope, Rail, GrassSeed, PlantPot
}

public enum ShapeType : byte
{
    Line, Corner, SquareEmpty, SquareFilled, CircleEmpty, CircleFilled
}

public partial class SpaceWand : ModItem, IMarqueeItem, IConditionItem
{
    public Condition UseCondition => ConfigCondition.AvailableSpaceWandC;
    public enum Direction
    {
        Right, Down, Left, Up, RightDown, LeftDown, LeftUp, RightUp
    }

    #region IMarqueeItem 一些基础参数

    private bool _shouldDrawing;
    private Rectangle _marquee = new();
    private Color _backgroundColor;
    private Color _borderColor;

    bool IMarqueeItem.ShouldDraw
    {
        get => _shouldDrawing;
        set => _shouldDrawing = value;
    }

    Rectangle IMarqueeItem.Marquee => _marquee;
    Color IMarqueeItem.BorderColor => _borderColor;
    Color IMarqueeItem.BackgroundColor => _backgroundColor;

    #endregion

    #region Item 基础设置

    public override bool AltFunctionUse(Player player) => true;

    public override void SetStaticDefaults()
    {
        Item.staff[Type] = true;
    }

    public override void SetDefaults()
    {
        Item.SetBaseValues(30, 30, ItemRarityID.Red, Item.sellPrice(0, 2, 50));
        Item.SetUseValues(ItemUseStyleID.Shoot, SoundID.Item1, 12, 12);
        Item.channel = true;
    }

    #endregion

    private readonly CoroutineRunner _syncRunner = new();
    private CoroutineHandle _handle;
    private bool _lastControlLeft; // 上一次按的是向左还是向右
    private static string _dataText; // 要显示在鼠标旁边的文本
    public PlaceType PlaceType;
    public BlockType BlockType;
    public ShapeType ShapeType;

    public Vector2 StartingPoint;
    public static Vector2 MousePosition => Main.MouseWorld.ToTileCoordinates().ToVector2() * 16f;

    public override bool CanUseItem(Player player)
    {
        if (!player.noBuilding)
        {
            // 右键
            if (player.altFunctionUse == 2)
            {
                UISystem.Instance.SpaceWandGUI.ProcessRightClick(this);
            }
            else
            {
                MyUtils.ItemCount(player.inventory, GetConditions(), out int count);

                if (count > 0)
                {
                    if (Main.myPlayer == player.whoAmI)
                        MyUtils.ItemRotation(player);
                    _syncRunner.StopAll();
                    CanPlaceTiles = true;
                    StartingPoint = Main.MouseWorld.ToTileCoordinates().ToVector2() * 16f;
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// 能否放置
    /// </summary>
    private bool CanPlaceTiles;

    public override bool? UseItem(Player player)
    {
        player.SetCompositeArmFront(enabled: true, Player.CompositeArmStretchAmount.Full,
            player.itemRotation - player.direction * MathHelper.ToRadians(90f));

        player.itemAnimation = player.itemAnimationMax;

        if (Main.dedServ || Main.myPlayer != player.whoAmI)
            return true;

        UseItem_PreUpdate(player);

        if (!Main.mouseLeft)
        {
            TryPlaceTiles(player);
            return true;
        }

        return false;
    }

    public void UseItem_PreUpdate(Player player)
    {
        _shouldDrawing = true;

        MyUtils.ItemRotation(player, false);

        UseItem_HandleCoroutines(player);

        // 用于Corner模式的形状判定
        if (player.controlLeft || player.controlRight)
            _lastControlLeft = player.controlLeft;

        if (Main.mouseRight && CanPlaceTiles)
        {
            CanPlaceTiles = false;
            CombatText.NewText(player.getRect(), new Color(250, 40, 80), MyUtils.GetText("CombatText.Item.SpaceWand_Cancel"));
        }

        Color color = CanPlaceTiles ? new Color(135, 0, 180) : new Color(250, 40, 80);
        _borderColor = color;
        _backgroundColor = color * 0.35f;
    }

    private void UseItem_HandleCoroutines(Player player)
    {
        if (Main.netMode is not NetmodeID.MultiplayerClient)
            return;

        _syncRunner.Update(1);

        // Runner用来实现间隔为8帧的rotation同步
        if (!_handle.IsRunning)
            _handle = _syncRunner.Run(8, MyUtils.ItemRotationCoroutines(player));
    }

    public override void HoldItem(Player player)
    {
        int oneIndex = MyUtils.EnoughItem(player, GetConditions());
        if (oneIndex == -1) return;

        player.cursorItemIconEnabled = true;
        player.cursorItemIconID = player.inventory[oneIndex].type;
        player.cursorItemIconPush = 26;
    }

    // 学单词
    // Judge v: 法官、判断、判定。n: 法官、裁判、裁判员

    // 学单词
    // Enough: 足够的
    // Enough!: 够了！

    public void TryPlaceTiles(Player player)
    {
        if (!CanPlaceTiles)
            return;

        if (Main.netMode is NetmodeID.MultiplayerClient)
        {
            SpaceWandOperation.Proceed(StartingPoint, MousePosition, ShapeType, BlockType, PlaceType, _lastControlLeft);
            return;
        }

        bool playSound = false;
        var tiles = GetSelectedTiles(ShapeType, StartingPoint, MousePosition, _lastControlLeft);
        var sortedPoints = tiles.OrderByDescending(p => p.Y); // Y轴降序排序，从下放到上，保证沙子不出问题
        var tilesHashSet = tiles.ToHashSet();
        MyUtils.ForeachTile(sortedPoints, (x, y) =>
        {
            OperateTile(player, x, y, tilesHashSet, PlaceType, BlockType, ref playSound, []);
        });

        if (playSound)
            PlaySoundPacket.PlaySound(LegacySoundIDs.Dig, StartingPoint);
    }

    void IMarqueeItem.PostDrawMarquee(Rectangle marquee, Color backgroundColor, Color borderColor)
    {
        MyUtils.DrawString(Main.MouseScreen + new Vector2(18f), _dataText, Color.White, _borderColor);
    }

    public void PreDrawMarquee(ref bool shouldDraw, Rectangle marquee, Color backgroundColor, Color borderColor)
    {
        MarqueeSystem.DrawSelectedTiles(GetSelectedTiles(ShapeType, StartingPoint, MousePosition, _lastControlLeft),
            borderColor, backgroundColor);
        shouldDraw = false;
    }

    /// <summary>
    /// 获取使用的条件
    /// </summary>
    public Func<Item, bool> GetConditions() => GetConditions(PlaceType);

    /// <summary>
    /// 获取使用的条件
    /// </summary>
    public static Func<Item, bool> GetConditions(PlaceType placeType)
    {
        return placeType switch
        {
            PlaceType.Platform => item => item.createTile > -1 && TileID.Sets.Platforms[item.createTile],
            PlaceType.Soild => item =>
                item.createTile > -1 && !Main.tileSolidTop[item.createTile] &&
                Main.tileSolid[item.createTile] && !GrassSeeds.Contains(item.createTile) &&
                TileID.ClosedDoor != item.createTile,
            PlaceType.Rope => item => item.createTile > -1 && Main.tileRope[item.createTile],
            PlaceType.Rail => item => item.createTile == TileID.MinecartTrack,
            PlaceType.GrassSeed => item => GrassSeeds.Contains(item.createTile),
            PlaceType.PlantPot => item => item.createTile == TileID.PlanterBox,
            _ => _ => false,
        };
    }

    public override void LoadData(TagCompound tag)
    {
        PlaceType = (PlaceType)tag.GetByte("PlaceType");
        ShapeType = (ShapeType)tag.GetByte("ShapeType");
    }

    public override void SaveData(TagCompound tag)
    {
        tag.Add("PlaceType", (byte)PlaceType);
        tag.Add("ShapeType", (byte)ShapeType);
    }
    public override void AddRecipes()
    {
        CreateRecipe()
            .AddRecipeGroup(RecipeGroups.Wood, 24)
            .AddRecipeGroup(RecipeSystem.AnyDemoniteBar, 8)
            .AddIngredient(ItemID.Amethyst, 8)
            .AddTile(TileID.WorkBenches)
            .Register();
    }
}