using ImproveGame.Common;
using ImproveGame.Content.Packets;
using ImproveGame.Core;
using System.Collections;
using System.Threading;

namespace ImproveGame.Content.Items;

/// <summary>
/// 选区物品做成一个基类，方便批量生产
/// </summary>
public abstract class SelectorItem : ModItem
{
    public override void SetStaticDefaults()
    {
        Item.staff[Type] = true;
    }

    private readonly CoroutineRunner _syncRunner = new();
    private CoroutineHandle _handle;
    private Point start;
    private Point end;
    protected Point SelectRange;
    protected int MaxTilesPerFrame = 30;
    protected bool UseNewThread = false;
    protected bool RunOnServer = false;

    protected Rectangle TileRect => new((int)MathF.Min(start.X, end.X), (int)MathF.Min(start.Y, end.Y),
        (int)MathF.Abs(start.X - end.X) + 1, (int)MathF.Abs(start.Y - end.Y) + 1);

    /// <summary>
    /// 封装了原来的SetDefaults()，现在用这个，在里面应该设置SelectRange
    /// </summary>
    public virtual void SetItemDefaults() { }

    public sealed override void SetDefaults()
    {
        // 基本属性
        Item.SetBaseValues(28, 28, 0, 0);
        Item.SetUseValues(ItemUseStyleID.Shoot, SoundID.Item1, 18, 18, true);
        SetItemDefaults();
    }

    /// <summary>
    /// CanUseItem的替代
    /// </summary>
    public virtual bool StartUseItem(Player player) => true;

    /// <summary>
    /// 是否应该使用自由选区功能，比如说某个模式开了才能自由选区之类的
    /// </summary>
    public virtual bool CanUseSelector(Player player) => true;

    public override bool CanUseItem(Player player)
    {
        bool flag = StartUseItem(player);
        _syncRunner.StopAll();
        if (player.itemAnimationMax != 0)
            return true;
        if (!flag || !CanUseSelector(player))
        {
            return flag;
        }
        unCancelled = true;
        start = Main.MouseWorld.ToTileCoordinates();
        return true;
    }

    /// <summary>
    /// 右键取消选区的实现
    /// </summary>
    private bool unCancelled;

    /// <summary>
    /// 在对框选物块执行操作后的方法，
    /// </summary>
    public virtual void PostModifyTiles(Player player, int minI, int minJ, int maxI, int maxJ) { }

    /// <summary>
    /// 对被框选的物块进行修改的方法，如果返回值为false，则会立刻终止后面的操作，默认为true
    /// </summary>
    public virtual bool ModifySelectedTiles(Player player, int i, int j) => true;

    /// <summary>
    /// 修改Box和文本的颜色
    /// </summary>
    /// <param name="cancelled">是否取消选中</param>
    /// <returns>颜色</returns>
    public virtual Color ModifyColor(bool cancelled)
    {
        return !cancelled ? new(255, 0, 0) : Color.GreenYellow;
    }

    public override bool? UseItem(Player player)
    {
        if (CanUseSelector(player))
        {
            player.itemAnimation = 8;
            if (Main.netMode is not NetmodeID.Server && player.whoAmI == Main.myPlayer)
                DoSelector(player);
        }
        else if (Main.myPlayer == player.whoAmI && player.ItemAnimationJustStarted)
        {
            ItemRotation(player);
        }

        player.SetCompositeArmFront(enabled: true, Player.CompositeArmStretchAmount.Full,
            player.itemRotation - player.direction * MathHelper.ToRadians(90f));

        return base.UseItem(player);
    }

    private void DoSelector(Player player)
    {
        if (Main.netMode is NetmodeID.MultiplayerClient)
            _syncRunner.Update(1);

        if (Main.mouseRight && unCancelled)
        {
            unCancelled = false;
        }

        end = ModifySize(start, Main.MouseWorld.ToTileCoordinates(), SelectRange.X, SelectRange.Y);
        Color color = ModifyColor(!unCancelled);
        GameRectangle.Create(this, IsNeedKill, start, end, color * 0.35f, color, TextDisplayType.All);
        if (Main.mouseLeft)
        {
            player.itemAnimation = 8;
            ItemRotation(player, false);

            // Runner用来实现间隔为(player.itemAnimationMax - 6f)帧的rotation同步
            if (!_handle.IsRunning && Main.netMode is NetmodeID.MultiplayerClient)
                _handle = _syncRunner.Run(player.itemAnimationMax - 6f, ItemRotationCoroutines(player));
        }
        else
        {
            player.itemAnimation = 0;
            ItemRotation(player);
            if (unCancelled)
            {
                if (!UseNewThread)
                {
                    if (RunOnServer && Main.netMode is NetmodeID.MultiplayerClient)
                        SelectorItemOperation.Proceed(start, end);
                    else
                        CoroutineSystem.TileRunner.Run(ModifyTiles(player, TileRect));
                }
                else
                    new Thread(() =>
                    {
                        Rectangle tileRect = TileRect;
                        int minI = tileRect.X;
                        int maxI = tileRect.X + tileRect.Width - 1;
                        int minJ = tileRect.Y;
                        int maxJ = tileRect.Y + tileRect.Height - 1;
                        for (int j = minJ; j <= maxJ; j++)
                            for (int i = minI; i <= maxI; i++)
                                if (WorldGen.InWorld(i, j) && !ModifySelectedTiles(player, i, j))
                                    PostModifyTiles(player, minI, minJ, i, j);
                        PostModifyTiles(player, minI, minJ, maxI, maxJ);
                    }).Start();
            }
        }
    }

    public virtual bool IsNeedKill() => true;

    internal IEnumerator ModifyTiles(Player player, Rectangle tileRect)
    {
        int countTiles = 0;
        int minI = tileRect.X;
        int maxI = tileRect.X + tileRect.Width - 1;
        int minJ = tileRect.Y;
        int maxJ = tileRect.Y + tileRect.Height - 1;
        for (int j = minJ; j <= maxJ; j++)
        {
            for (int i = minI; i <= maxI; i++)
            {
                countTiles++;
                if (WorldGen.InWorld(i, j) && !ModifySelectedTiles(player, i, j))
                {
                    PostModifyTiles(player, minI, minJ, i, j);
                    yield break;
                }

                if (countTiles >= MaxTilesPerFrame)
                {
                    countTiles = 0;
                    yield return 0;
                }
            }
        }

        yield return 0;
        PostModifyTiles(player, minI, minJ, maxI, maxJ);
    }
}